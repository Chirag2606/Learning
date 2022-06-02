namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_1
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;

    public class RunController : V2.RunController
    {
        public RunController(
            ILogger logger,
            IAccountService accountService,
            ICampaignService campaignService)
            : base(logger, accountService, campaignService)
        {
        }
    }
}