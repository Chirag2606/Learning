namespace Cyara.Web.Portal.Areas.Apps.Controllers
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Licensing;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;

    [SecuredResource(RequiresAccount = false)]
    public class CxInsightsController : BaseController
    {
        public async Task<ActionResult> Index(int routeAccountId = 0)
        {
            int selectedAccountId = routeAccountId;
            var session = new SessionFacade(HttpContext);

            if (selectedAccountId == 0)
            {
                var accountService = DependencyResolver.Current.GetService<IAccountService>();
                var user = await accountService.UserGetAsync(new GenericRequest<Guid>(session.User.UserId) { User = session.User });
                user.ExceptionIfError();

                selectedAccountId = user.Value.Properties.DefaultAccount ?? 0;
            }

            var configurationService = DependencyResolver.Current.GetService<IConfigurationService>();

            if (selectedAccountId != 0)
            {
                // the only people who won't have a selected account could be a platform admin, so if they don't, let them through.
                if (!configurationService.IsLicensed(LicensedFeature.CxInsights, selectedAccountId))
                {
                    return new HttpNotFoundResult();
                }
            }

            var insightsUrl = configurationService.Get(ConfigurationKey.CxInsightsUrl.Key);

            return Redirect(Regex.Replace(insightsUrl, @"\{accountId\}", selectedAccountId.ToString(), RegexOptions.IgnoreCase));
        }
    }
}