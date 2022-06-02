namespace Cyara.Web.Portal.Areas.Api.Controllers.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2;
    using Cyara.Web.Resources;

    using CampaignRunSummary = Cyara.Shared.Types.Campaign.CampaignRunSummary;
    using ResultType = Cyara.Domain.Types.TestResult.ResultType;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class RunReportController : BaseApiController
    {
        public RunReportController(
            ILogger logger,
            ICampaignService campaignService,
            IAgentService agentService,
            IReportsService reportsService,
            ISchedulerService schedulerService,
            IAccountService accountService)
        {
            Logger = logger;
            CampaignService = campaignService;
            AgentService = agentService;
            ReportsService = reportsService;
            SchedulerService = schedulerService;
            AccountService = accountService;
            DataHelper = new DataHelper();
        }

        public ILogger Logger { get; }

        public ICampaignService CampaignService { get; }

        public IAgentService AgentService { get; }

        public IReportsService ReportsService { get; }

        public ISchedulerService SchedulerService { get; }

        public IAccountService AccountService { get; }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("Summary")]
        [AuthorizeAccount(StaticRoles.Reporting, StaticRoles.CCMReporting)]
        public virtual HttpResponseMessage Get(int account, string scope, int id)
        {
            // Check that the params are ok...
            MediaType? verifyScope = ApiModel.TargetForScope(scope);
            if (verifyScope == null || id == 0)
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
                if (!action.Equals("Summary", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ValidUrl_Pattern_V2Report, ApiMessages.InvalidUrl).Construct(this, Logger);
                }
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            if (verifyScope.Value == MediaType.Voice)
            {
                // Confirm the RunId is for the voice campaign for this account
                GenericResponse<CampaignRunSummary> campaignRun = CampaignService.CampaignRunGet(AccountRequest.Construct(id, session.User, account));
                campaignRun.ExceptionIfError();
                if (!campaignRun.IsSuccess || campaignRun.Value == null)
                {
                    return ApiResponse.NotFoundId<int>(ApiMessages.Entity_CampaignRun, id).Construct(this, Logger);
                }

                // Ensure the run is for the right area...
                GenericResponse<Plan> planResponse = AccountService.PlanGet(AccountRequest.Construct(campaignRun.Value.Plan.PlanId, session.User, account));
                planResponse.ExceptionIfError();
                if (!planResponse.IsSuccess || planResponse.Value == null || planResponse.Value.MediaType != MediaType.Voice)
                {
                    return ApiResponse.NotFoundId<int>(ApiMessages.Entity_CampaignRun, id).Construct(this, Logger);
                }

                // Get the resultSummary
                ResultsSummaryRequest resultRequest = new ResultsSummaryRequest { RunIds = new[] { id }, MediaType = MediaType.Voice };
                GenericResponse<IEnumerable<Tuple<ResultType, int>>> resultSummaryResult = ReportsService.CampaignRunTestResultsSummaryGet(AccountRequest.Construct(resultRequest, session.User, account));
                resultSummaryResult.ExceptionIfError();

                // get the testResult breakdown (all of them)
                PaginatedResponse<CampaignRunTestCaseBreakdown> testCaseBreakdownResult = ReportsService.CampaignRunTestCasesBreakdownGet(
                    new PaginatedRequest<ResultsSummaryRequest>(resultRequest)
                        {
                            PageSize = int.MaxValue,
                            CurrentPage = 1,
                            SortAscending = true,
                            SortField = null,
                            AccountId = account
                        });
                testCaseBreakdownResult.ExceptionIfError();

                return ApiResponse.Succeeds(CampaignRunSummaryResults.From(campaignRun.Value, testCaseBreakdownResult.Collection.OrderBy(x => x.TestCaseId).ToList(), resultSummaryResult.Value.OrderBy(x => x.Item1), DataHelper)).Construct(this, Logger);
            }

            if (verifyScope.Value == MediaType.AgentVoice)
            {
                AgentStatusResponse agentStatsResponse = ReportsService.CCMGetCampaignAgentStats(
                    new AgentStatusRequest
                        {
                            AccountId = account,
                            RunId = id,
                            PaginatedRequest = new PaginatedRequest { AccountId = account, PageSize = int.MaxValue, CurrentPage = 1 }
                        });
                agentStatsResponse.ExceptionIfError();

                if (!agentStatsResponse.IsSuccess || !string.IsNullOrEmpty(agentStatsResponse.ErrorMessage()))
                {
                    return ApiResponse.NotFoundId<int>(ApiMessages.Entity_CampaignRun, id).Construct(this, Logger);
                }

                return ApiResponse.Succeeds(CampaignRunSummaryResults.From(agentStatsResponse.CampaignDetails, agentStatsResponse.AgentStatusList, DataHelper)).Construct(this, Logger);
            }

            return ApiResponse.Fails(HttpStatusCode.BadRequest, "huh", "no idea what you wanted").Construct(this, Logger);
        }
    }
}
