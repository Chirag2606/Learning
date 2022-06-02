namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_1
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.Scheduler;

    public class RunReportController : V2.RunReportController
    {
        public RunReportController(
            ILogger logger,
            ICampaignService campaignService,
            IAgentService ccmService,
            IReportsService reportsService,
            ISchedulerService schedulerService,
            IAccountService accountService)
            : base(logger, campaignService, ccmService, reportsService, schedulerService, accountService)
        {
        }
    }
}