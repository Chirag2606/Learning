// <copyright file="CampaignController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace

namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_3
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;

    using MediatR;

    public class CampaignController : V2_2.CampaignController
    {
        public CampaignController(ILogger logger, IAccountService accountService, IAuthorisationManager authorisationManager, ICampaignService campaignService, ITestCaseService testCaseService, IStorageService storageService, IAgentService agentService, IMediator mediator)
            : base(logger, accountService, authorisationManager, campaignService, testCaseService, storageService, agentService, mediator)
        {
        }
    }
}