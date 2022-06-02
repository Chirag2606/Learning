// <copyright file="RunController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace

namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_4
{
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;

    public class RunController : V2_3.RunController
    {
        public RunController(ILogger logger, IAccountService accountService, ICampaignService campaignService)
            : base(logger, accountService, campaignService)
        {
        }
    }
}