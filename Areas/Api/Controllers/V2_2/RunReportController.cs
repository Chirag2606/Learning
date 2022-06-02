// <copyright file="RunReportController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace

namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_2
{
    using System;
    using System.Collections.Specialized;
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
    using Cyara.Web.Portal.Areas.Api.Extensions.V2_2;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_2;
    using Cyara.Web.Resources;

    using CampaignRunRequest = Cyara.Web.Portal.Areas.Api.Models.V2_2.CampaignRunRequest;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class RunReportController : V2_1.RunReportController
    {
        public RunReportController(ILogger logger, ICampaignService campaignService, IAgentService agentService, IReportsService reportsService, ISchedulerService schedulerService, IAccountService accountService, ITestCaseService testCaseService, WebSiteSettings settings)
            : base(logger, campaignService, agentService, reportsService, schedulerService, accountService)
        {
            Settings = settings;
            TestCaseService = testCaseService;
        }

        public WebSiteSettings Settings { get; set; }

        public ITestCaseService TestCaseService { get; }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("CampaignRunTestResults")]
        [AuthorizeAccount(StaticRoles.Reporting)]
        public virtual HttpResponseMessage GetCampaignRunTestResults(int account, string scope, [FromUri] CampaignRunRequest request)
        {
            // Check that the params are ok...
            MediaType? verifyScope = ApiModel.TargetForScope(scope);
            if (verifyScope == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // make sure we dont pickup the default route.
            // i.e. http://localhost/portaltrunk/API/2.0/account/2/report/agent/run/1162
            IHttpRouteData routeData = Request.GetRouteData();
            if (routeData != null)
            {
                var action = (routeData.Values["action"] ?? string.Empty).ToString();
                if (!action.Equals("CampaignRunTestResults", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ValidUrl_Pattern_V2Report, ApiMessages.InvalidUrl).Construct(this, Logger);
                }
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var sessionId = ControllerContext.Request.GetCorrelationId().ToString();

            if (verifyScope.Value == MediaType.Voice)
            {
                ApiResponse<CampaignRunResults> result = request.GetCampaignRunTestResults(sessionId, session.User, account, Logger, CampaignService, SchedulerService, ReportsService, AccountService, DataHelper);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.AgentVoice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_AgentNotSupported, ApiMessages.InvalidUrl_AgentNotSupported).Construct(this, Logger);
            }

            return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl, ApiMessages.InvalidUrl).Construct(this, Logger);
        }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("CampaignRunTestStepResults")]
        [AuthorizeAccount(StaticRoles.Reporting)]
        public virtual HttpResponseMessage GetCampaignRunTestStepResults(int account, string scope, [FromUri] CampaignRunRequest request)
        {
            // Check that the params are ok...
            MediaType? verifyScope = ApiModel.TargetForScope(scope);
            if (verifyScope == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // make sure we dont pickup the default route.
            // i.e. http://localhost/portaltrunk/API/2.0/account/2/report/agent/run/1162
            IHttpRouteData routeData = Request.GetRouteData();
            if (routeData != null)
            {
                var action = (routeData.Values["action"] ?? string.Empty).ToString();
                if (!action.Equals("CampaignRunTestStepResults", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ValidUrl_Pattern_V2Report, ApiMessages.InvalidUrl).Construct(this, Logger);
                }
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var sessionId = ControllerContext.Request.GetCorrelationId().ToString();

            if (verifyScope.Value == MediaType.Voice)
            {
                NameValueCollection query = Request.RequestUri.ParseQueryString();
                ApiResponse<CampaignRunStepResults> result = request.GetCampaignRunTestStepResults(query, sessionId, session.User, account, Logger, CampaignService, SchedulerService, ReportsService, TestCaseService, this, MediaType.Voice, AccountService, DataHelper);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.AgentVoice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_AgentNotSupported, ApiMessages.InvalidUrl_AgentNotSupported).Construct(this, Logger);
            }

            return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl, ApiMessages.InvalidUrl).Construct(this, Logger);
        }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("TestResultsFeed")]
        [AuthorizeAccount(StaticRoles.Reporting)]
        public HttpResponseMessage GetTestResultsFeed(int account, string scope, [FromUri] TestResultsFeedRequest request)
        {
            request.AccountId = account;

            // Check that the params are ok...
            MediaType? verifyScope = ApiModel.TargetForScope(scope);
            if (verifyScope == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // make sure we dont pickup the default route.
            // i.e. http://localhost/portaltrunk/API/2.0/account/2/report/agent/run/1162
            IHttpRouteData routeData = Request.GetRouteData();
            if (routeData != null)
            {
                var action = (routeData.Values["action"] ?? string.Empty).ToString();
                if (!action.Equals("TestResultsFeed", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ValidUrl_Pattern_V2Report, ApiMessages.InvalidUrl).Construct(this, Logger);
                }
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var sessionId = ControllerContext.Request.GetCorrelationId().ToString();

            if (verifyScope.Value == MediaType.Voice)
            {
                NameValueCollection query = Request.RequestUri.ParseQueryString();

                ApiResponse<TestResultFeed> result = request.GetTestResultsFeedWithAccount(query, sessionId, session.User, account, Logger, ReportsService, AccountService, DataHelper, Settings, this);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.AgentVoice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_AgentNotSupported, ApiMessages.InvalidUrl_AgentNotSupported).Construct(this, Logger);
            }

            return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl, ApiMessages.InvalidUrl).Construct(this, Logger);
        }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("TestResult")]
        [AuthorizeAccount(StaticRoles.Reporting)]
        public virtual async Task<HttpResponseMessage> GetTestResult(int account, string scope, [FromUri] TestResultRequest request)
        {
            // Check that the params are ok...
            MediaType? verifyScope = ApiModel.TargetForScope(scope);
            if (verifyScope == null || request == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // make sure we dont pickup the default route.
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
                ApiResponse<TestCaseResult2WithCampaignInfo> result = await request.GetTestResults(sessionId, session.User, account, Logger, TestCaseService, SchedulerService, DataHelper);
                return result.Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.AgentVoice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_AgentNotSupported, ApiMessages.InvalidUrl_AgentNotSupported).Construct(this, Logger);
            }

            return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl, ApiMessages.InvalidUrl).Construct(this, Logger);
        }
    }
}