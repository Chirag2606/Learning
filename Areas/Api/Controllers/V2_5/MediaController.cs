namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_5
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Rules;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Types.Results;
    using Cyara.Shared.Types.Storage;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.IO;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_5;
    using Cyara.Web.Resources;

    [LogApiRequest]
    [V2ControllerConfiguration]
    [HandleApiException]
    public class MediaController : BaseApiController
    {
        private static readonly Type Me = MethodBase.GetCurrentMethod().DeclaringType;

        private readonly ILogger _logger;

        private readonly IScreenshotService _screenshotService;

        private readonly ITestCaseService _testCaseService;

        public MediaController(
            ILogger logger,
            IScreenshotService screenshotService,
            ITestCaseService testCaseService)
        {
            _logger = logger;
            _screenshotService = screenshotService;
            _testCaseService = testCaseService;
        }

        /// <summary>
        /// Get the media associated by the supplied id.
        /// api/{version}/account/{account}/web/screenshot/{id}/{step}?{count}=1
        /// </summary>
        /// <param name="account">account id</param>
        /// <param name="id">test result id</param>
        /// <param name="step">the optional step</param>
        /// <param name="count">the optional number of steps to return, from step</param>
        /// <param name="screenshotType">the optional type of screenshot</param>
        [HttpGet, ActionName("screenshot")]
        [AuthorizeArea(testCase: new[] { Access.View }, report: new[] { Access.View })]
        public async Task<HttpResponseMessage> Screenshot(int account, int id, int? step = null, int? count = null, ScreenshotType screenshotType = ScreenshotType.NotSet)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            
            var testResultResponse = _testCaseService.ResultGetById(
                new AccountRequest<Tuple<int, MediaType>>(Tuple.Create(id, MediaType.Chat)) { AccountId = account, User = session.User });
            testResultResponse.ExceptionIfError();

            var chatTestResult = testResultResponse.Value as ChatTestResult;

            if (testResultResponse.Value == null || chatTestResult == null)
            {
                return ApiResponse.Fails(HttpStatusCode.NotFound, ApiMessages.NotFound.FormatWith("Test Result Id"), ApiMessages.NotFound_Id.FormatWith(id)).Construct(this, _logger);
            }
            
            Guid fileId;
            if (!Guid.TryParse(chatTestResult.MediaFile, out fileId))
            {
                _logger.LogInfo(Me, $"Invalid file id passed to retrieve media. AccountId:{account} FileId:{id}");
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidArgument, ApiMessages.InvalidArgument).Construct(this, _logger);
            }

            HttpContextBase httpContext = (HttpContextBase)Request.Properties["MS_HttpContext"];
            var tempFile = TempFileHelper.GetTempFileName(MvcApplication.Settings, httpContext, ".mp4");

            var response = await _screenshotService.ScreenshotsGetAsync(
                new AccountRequest<Tuple<Guid, string, ScreenshotType>>(Tuple.Create(fileId, tempFile, screenshotType)) { AccountId = account },
                new Cyara.Domain.Types.Rules.Chat());

            response.ExceptionIfError();

            string resource = step == null ? null : Chat.ScreenshotImageName.Replace("%03d", step.Value.ToString("000"));

            if (response.Value == null || response.Value.Count == 0 || (resource != null && !response.Value.ContainsKey(resource)))
            {
                _logger.LogWarnWithFormat(Me, $"Cannot find requested media. FileId:{fileId} AccountId:{account} Resource:{resource}");

                return ApiResponse.Fails(HttpStatusCode.NotFound, ApiMessages.Media_NotFound, ApiMessages.Media_NotFound).Construct(this, _logger);
            }

            Func<string, byte[], MediaFile> createMedia = (r, b) => new MediaFile
                                                                    {
                                                                        Name = r,
                                                                        ContentType = "image/jpeg",
                                                                        Content = Convert.ToBase64String(b)
                                                                    };
            MediaFile[] files;

            if (resource != null)
            {
                if (count.HasValue)
                {
                    // range
                    var index = response.Value.Select((x, i) => new { Value = x, Index = i }).Single(x => x.Value.Key == resource).Index;
                    files = response.Value.Skip(index).Take(count.Value).Select(x => createMedia(x.Key, x.Value)).ToArray();
                    
                    _logger.LogWarnWithFormat(Me, $"Match range. FileId:{fileId} AccountId:{account} From:{files.First().Name} To:{files.Last().Name}");

                    return ApiResponse.Succeeds(files).Construct(this, _logger);
                }

                // single
                var file = createMedia(resource, response.Value[resource]);

                _logger.LogWarnWithFormat(Me, $"Match single. FileId:{fileId} AccountId:{account} File:{file.Name}");

                return ApiResponse.Succeeds(file).Construct(this, _logger);
            }

            // all
            files = response.Value.Select(x => createMedia(x.Key, x.Value)).ToArray();

            _logger.LogWarnWithFormat(Me, $"Match all. FileId:{fileId} AccountId:{account} From:{files.First().Name} To:{files.Last().Name}");

            return ApiResponse.Succeeds(files).Construct(this, _logger);
        }
    }
}
