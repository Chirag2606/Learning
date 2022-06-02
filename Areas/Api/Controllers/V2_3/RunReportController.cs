// <copyright file="RunReportController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace

namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_3
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Settings;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;

    public class RunReportController : V2_2.RunReportController
    {
        public RunReportController(ILogger logger, ICampaignService campaignService, IAgentService agentService, IReportsService reportsService, ISchedulerService schedulerService, IAccountService accountService, ITestCaseService testCaseService, WebSiteSettings settings)
            : base(logger, campaignService, agentService, reportsService, schedulerService, accountService, testCaseService, settings)
        {
        }
    }
}