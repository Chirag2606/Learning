namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_5
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Settings;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Extensions;
    using Cyara.Web.Portal.Areas.Api.Extensions.V2_5;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_5;
    using Cyara.Web.Resources;

    public class RunReportController : V2_4.RunReportController
    {
        public RunReportController(
            ILogger logger,
            ICampaignService campaignService,
            IAgentService agentService,
            IReportsService reportsService,
            ISchedulerService schedulerService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            WebSiteSettings settings)
            : base(logger, campaignService, agentService, reportsService, schedulerService, accountService, testCaseService, settings)
        {
            DataHelper = new DataHelper();
        }

        [HttpGet]
        [ActionName("CampaignRunTestResults")]
        [AuthorizeAccount(StaticRoles.Reporting)]
        public override HttpResponseMessage GetCampaignRunTestResults(int account, string scope, [FromUri] Models.V2_2.CampaignRunRequest request)
        {
            var ret = base.GetCampaignRunTestResults(account, scope, request);
            var content = ret.ExtractContentFromHttpResponse<Models.V2_2.CampaignRunResults>();
            if (content != null)
            {
                content.CampaignRun2.EndDateSpecified = true;
            }

            return ret;
        }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("Summary")]
        [AuthorizeAccount(StaticRoles.Reporting, StaticRoles.CCMReporting)]
        public override HttpResponseMessage Get(int account, string scope, int id)
        {
            var ret = base.Get(account, scope, id);
            var content = ret.ExtractContentFromHttpResponse<Models.V2.CampaignRunSummaryResults>();
            if (content != null)
            {
                if (ApiModel.TargetForScope(scope) != MediaType.AgentVoice)
                {
                    content.EndDateSpecified = true;
                }
            }

            return ret;
        }

        [HttpGet]
        [ActionName("CampaignRunTestStepResults")]
        [AuthorizeAccount(StaticRoles.Reporting)]
        public override HttpResponseMessage GetCampaignRunTestStepResults(int account, string scope, [FromUri] Models.V2_2.CampaignRunRequest request)
        {
            var ret = base.GetCampaignRunTestStepResults(account, scope, request);
            var content = ret.ExtractContentFromHttpResponse<Models.V2_2.CampaignRunStepResults>();
            if (content != null && content.CampaignRun2 != null)
            {
                content.CampaignRun2.EndDateSpecified = true;
            }

            return ret;
        }
        
        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("TestResult")]
        [AuthorizeAccount(StaticRoles.Reporting, StaticRoles.ExecutiveDashboard, StaticRoles.DashboardAdmin, StaticRoles.BusinessExecutive)]
        public override async Task<HttpResponseMessage> GetTestResult(int account, string scope, [FromUri] Models.V2_2.TestResultRequest testResultRequest)
        {
            var testResult25Request = AutoMapper.Mapper.Map<Models.V2_2.TestResultRequest, TestResultRequest>(testResultRequest);

            // Check that the params are ok...
            MediaType? verifyScope = ApiModel.TargetForScope(scope, new[] { MediaType.AgentVoice, MediaType.Voice, MediaType.Email, MediaType.Chat, MediaType.Sms });
            if (verifyScope == null || testResult25Request == null)
            {
                // Couldn't verify our channel. Just log voice and agent as supported types until we officially support email and web
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // make sure we don't pickup the default route.  
            // i.e. http://localhost/portaltrunk/API/2.0/account/2/report/agent/run/1162
            IHttpRouteData routeData = Request.GetRouteData();
            if (routeData != null)
            {
                var action = (routeData.Values["action"] ?? string.Empty).ToString();
                if (!action.Equals("TestResult", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ValidUrl_Pattern_V2Report, ApiMessages.InvalidUrl).Construct(this, Logger);
                }
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var sessionId = ControllerContext.Request.GetCorrelationId().ToString();

            if (verifyScope.Value == MediaType.Voice)
            {
                ApiResponse<TestCaseResult2WithCampaignInfo> result = await testResult25Request.GetTestResults(sessionId, session.User, account, Logger, TestCaseService, SchedulerService, DataHelper);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.AgentVoice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_AgentNotSupported, ApiMessages.InvalidUrl_AgentNotSupported).Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.Chat)
            {
                var result = testResult25Request.GetChatTestResults(sessionId, session.User, account, Logger, TestCaseService, DataHelper);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.Email)
            {
                var result = testResult25Request.GetEmailTestResults(sessionId, session.User, account, Logger, TestCaseService, DataHelper);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.Sms)
            {
                var result = testResult25Request.GetSmsTestResults(sessionId, session.User, account, Logger, TestCaseService, DataHelper);
                return result.Construct(this, Logger);
            }

            return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl, ApiMessages.InvalidUrl).Construct(this, Logger);
        }
    }
}