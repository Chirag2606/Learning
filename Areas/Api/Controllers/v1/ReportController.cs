namespace Cyara.Web.Portal.Areas.Api.Controllers.V1
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Settings;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Portal.Models.Api.V1_0;

    using CampaignRunRequest = Cyara.Web.Portal.Models.Api.V1_0.CampaignRunRequest;

    public class ReportController : BaseApiController
    {
        private static readonly Type Me = typeof(ReportController);
        private readonly ILogger _logger;
        private readonly AccountResolver _accountResolver;
        private readonly ICampaignService _campaignService;
        private readonly ISchedulerService _schedulerService;
        private readonly IReportsService _reportsService;
        private readonly IAccountService _accountService;
        private readonly ITestCaseService _testCaseService;

        private readonly WebSiteSettings _settings;

        private IAuthorisationManager _authorisationManager;

        public ReportController(
            ILogger logger,
            ICampaignService campaignService,
            ISchedulerService schedulerService,
            IReportsService reportsService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            IAuthorisationManager authorisationManager,
            WebSiteSettings settings)
        {
            _logger = logger;
            _campaignService = campaignService;
            _reportsService = reportsService;
            _schedulerService = schedulerService;
            _accountService = accountService;
            _testCaseService = testCaseService;
            _settings = settings;
            _authorisationManager = authorisationManager;
            _accountResolver = new AccountResolver(_logger, _campaignService, _accountService, _testCaseService);
        }

        [HttpGet]
        [ActionName("CampaignRunTestResults")]
        [RestfulAuthorize(Roles = StaticRoles.Reporting)]
        public HttpResponseMessage GetCampaignRunTestResults([FromUri] CampaignRunRequest request)
        {
            ModelState.ExceptionIfError();

            return ActionWrapper.Wrap(
                this,
                _authorisationManager,
                (session, sessionId) =>
                    {
                        _logger.LogDebugWithFormat(
                            Me,
                            "Starting CampaignRunTestResults report. SessionId:{0} Query:{1}".FormatWith(sessionId, Request.RequestUri.Query));

                        var result = request.GetCampaignRunTestResults(sessionId, session.User, session.SelectedAccount.Id, _logger, _campaignService, _schedulerService, _reportsService);
                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    if (request.RunId.HasValue)
                    {
                        return _accountResolver.ByCampaignRun(request.RunId.Value, sessionId, user);
                    }

                    if (request.CampaignId.HasValue)
                    {
                        return _accountResolver.ByCampaign(request.CampaignId.Value, sessionId, user);
                    }

                    _logger.LogError(Me, "SessionId:{0} Unable to resolve account for PlatformUser as both RunId and CampaignId are empty".FormatWith(sessionId));
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(ApiMessageConstants.CampaignRunTestResultAtLeastOneOfRunIdCampaignId),
                        ReasonPhrase = ApiMessageConstants.InvalidArgumentReason
                    });
                },
                _logger);
        }

        [HttpGet]
        [ActionName("TestResult")]
        [RestfulAuthorize(Roles = StaticRoles.Reporting)]
        public async Task<HttpResponseMessage> GetTestResult([FromUri] TestResultRequest request)
        {
            ModelState.ExceptionIfError();

            return await ActionWrapper.WrapAsync(
                this,
                _authorisationManager,
                async (session, sessionId) =>
                {
                    _logger.LogDebugWithFormat(Me, "Starting TestResult report. SessionId:{0} Query:{1}".FormatWith(sessionId, Request.RequestUri.Query));

                    var result = await request.GetTestResults(sessionId, session.User, session.SelectedAccount.Id, _logger, _testCaseService, _schedulerService);
                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    if (request.TestResultId.HasValue)
                    {
                        return _accountResolver.ByTestResult(request.TestResultId.Value, sessionId, user);
                    }

                    if (request.Ticket != null)
                    {
                        return _accountResolver.ByTicket(request.Ticket.Value, sessionId, user);
                    }

                    _logger.LogError(Me, "SessionId:{0} Unable to resolve account for PlatformUser as both TestResultId and Ticket are empty".FormatWith(sessionId));

                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(ApiMessageConstants.TestResultAtLeastOneOfTestResultIdTicket),
                        ReasonPhrase = ApiMessageConstants.InvalidArgumentReason
                    });
                },
                _logger);
        }

        [HttpGet]
        [ActionName("CampaignRunTestStepResults")]
        [RestfulAuthorize(Roles = StaticRoles.Reporting)]
        public HttpResponseMessage GetCampaignRunTestStepResults([FromUri] CampaignRunRequest request)
        {
            ModelState.ExceptionIfError();

            return ActionWrapper.Wrap(
                this,
                _authorisationManager,
                (session, sessionId) =>
                    {
                        _logger.LogDebugWithFormat(
                            Me,
                            "Starting CampaignRunTestStepResults report. SessionId:{0} Query:{1}".FormatWith(sessionId, Request.RequestUri.Query));

                        NameValueCollection query = Request.RequestUri.ParseQueryString();

                    var result = request.GetCampaignRunTestStepResults(
                        query, sessionId, session.User, session.SelectedAccount.Id, _logger, _campaignService, _schedulerService, _reportsService, _testCaseService, this, MediaType.Voice);
                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    if (request.RunId.HasValue)
                    {
                        return _accountResolver.ByCampaignRun(request.RunId.Value, sessionId, user);
                    }

                    if (request.CampaignId.HasValue)
                    {
                        return _accountResolver.ByCampaign(request.CampaignId.Value, sessionId, user);
                    }

                    _logger.LogError(Me, "SessionId:{0} Unable to resolve account for PlatformUser as both RunId and CampaignId are empty".FormatWith(sessionId));
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(ApiMessageConstants.CampaignRunTestResultAtLeastOneOfRunIdCampaignId),
                        ReasonPhrase = ApiMessageConstants.InvalidArgumentReason
                    });
                },
                _logger);
        }

        [HttpGet]
        [ActionName("TestResultsFeed")]
        [RestfulAuthorize(Roles = StaticRoles.Reporting)]
        public HttpResponseMessage GetTestResultsFeed([FromUri] TestResultsFeedRequest request)
        {
            ModelState.ExceptionIfError();

            var apiVersion = this.Request.GetRouteData().Values["version"].ToString();

            return ActionWrapper.Wrap(
                this,
                _authorisationManager,
                (session, sessionId) =>
                    {
                        _logger.LogDebugWithFormat(
                            Me,
                            "Starting GetTestResultsFeed report. SessionId:{0} Query:{1}".FormatWith(sessionId, Request.RequestUri.Query));

                        NameValueCollection query = Request.RequestUri.ParseQueryString();

                        var result = request.GetTestResultsFeed(
                            query,
                            sessionId,
                            session.User,
                            session.SelectedAccount.Id,
                            _logger,
                            _reportsService,
                            _accountService,
                            _settings,
                            this,
                            _authorisationManager,
                            apiVersion);

                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    if (request.AccountId.HasValue && request.AccountId.Value > 0)
                    {
                        return _accountResolver.ByAccount(request.AccountId.Value, sessionId, user);
                    }

                    _logger.LogError(Me, "SessionId:{0} GetTestResultsFeed - Unable to resolve account for PlatformUser as AccountId is empty".FormatWith(sessionId));
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(ApiMessageConstants.ValidationTestResultsFeedAccountIdRequired),
                        ReasonPhrase = ApiMessageConstants.InvalidArgumentReason
                    });
                },
                _logger);
        }
    }
}
