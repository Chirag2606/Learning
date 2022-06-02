namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_1
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;

    using MediatR;

    public class CampaignController : Cyara.Web.Portal.Areas.Api.Controllers.V2.CampaignController
    {
        public CampaignController(
            ILogger logger,
            IAccountService accountService,
            IAuthorisationManager authorisationManager,
            ICampaignService campaignService,
            ITestCaseService testCaseService,
            IStorageService storageService,
            IAgentService agentService,
            IMediator mediator)
            : base(logger, accountService, authorisationManager, campaignService, testCaseService, storageService, agentService, mediator)
        {
        }
    }
}
