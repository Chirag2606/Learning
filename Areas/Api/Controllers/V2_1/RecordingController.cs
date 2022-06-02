namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_1
{
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Core.IO;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Foundation.MediaAccess.Core.Contracts;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.IO;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2;
    using Cyara.Web.Resources;

    [LogApiRequest]
    [V2ControllerConfiguration]
    [HandleApiException]
    public class RecordingController : BaseApiController
    {
        public RecordingController(
            ILogger logger,
            ITestCaseService testCaseService,
            IFileSystem fileSystem,
            IMediaAccess mediaAccess)
        {
            Logger = logger;
            TestCaseService = testCaseService;
            FileSystem = fileSystem;
            MediaAccess = mediaAccess;
            DataHelper = new DataHelper();
        }

        protected ILogger Logger { get; }

        protected ITestCaseService TestCaseService { get; }

        protected IFileSystem FileSystem { get; }

        protected IMediaAccess MediaAccess { get; }

        /// <summary>
        /// Get the recording for a test result or step
        /// </summary>
        /// <param name="account">account id</param>
        /// <param name="scope">voice scope</param>
        /// <param name="id">Test result id</param>
        /// <param name="step">optional step number to get recording of</param>
        [HttpGet, ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.View, Access.Execute }, report: new[] { Access.View })]
        public HttpResponseMessage Get(int account, string scope, int id, int? step = null)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            HttpContextBase httpContext = (HttpContextBase)Request.Properties["MS_HttpContext"];

            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            try
            {
                var recording = Recording.Prime(httpContext, id, step, TestCaseService, FileSystem, MediaAccess, session, Logger);

                // return a generic not found message so we don’t divulge if it was the account, test result id, step or there was no recording present.
                if (recording == null)
                {
                    return ApiResponse.Fails(HttpStatusCode.NotFound, ApiMessages.Recording_NotFound, ApiMessages.NotFound.FormatWith(ApiMessages.Entity_Recording)).Construct(this, Logger);
                }

                return ApiResponse.Succeeds(recording).Construct(this, Logger);
            }
            catch (TestRecordingHelper.OutsideAvailabilityRangeException)
            {
                Logger.LogDebug(typeof(RecordingController), $"Test result outside range of available days of retrieval. TestResultId:{id} AccountId:{account}");
                return ApiResponse.Fails(HttpStatusCode.Forbidden, ApiMessages.Recording_OutsideAvailabilityRange, ApiMessages.Recording_OutsideAvailabilityRange).Construct(this, Logger);
            }
        }

        [HttpGet, ActionName("List")]
        [AuthorizeArea(testCase: new[] { Access.View, Access.Execute }, report: new[] { Access.View })]
        public HttpResponseMessage List(int account, string scope, int id)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            HttpContextBase httpContext = (HttpContextBase)Request.Properties["MS_HttpContext"];

            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            var recordings = TestResultRecordings.Prime(httpContext, id, TestCaseService, session, Logger);

            // return a generic not found message so we don’t divulge if it was the account, test result id, step or there was no recording present.
            if (recordings == null)
            {
                return ApiResponse.Fails(HttpStatusCode.NotFound, ApiMessages.Recording_NotFound, ApiMessages.NotFound.FormatWith(ApiMessages.Entity_Recording)).Construct(this, Logger);
            }

            return ApiResponse.Succeeds(recordings).Construct(this, Logger);
        }
    }
}
