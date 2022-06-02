namespace Cyara.Web.Portal.Areas.Apps.Controllers
{
    using System;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Licensing;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;

    [SecuredAreaAccess(agentLiveVQ: new[] { Access.View, Access.Edit, Access.Execute })]
    public class LiveVQController : BaseController
    {
        public ActionResult Index(int routeAccountId)
        {
            var configurationService = DependencyResolver.Current.GetService<IConfigurationService>();
            if (!configurationService.IsLicensed(LicensedFeature.LiveVQ, routeAccountId))
            {
                return new HttpNotFoundResult();
            }

            SessionFacade session = new SessionFacade(HttpContext);

            var accountService = DependencyResolver.Current.GetService<IAccountService>();
            var accountResponse = accountService.AccountGet(new GenericRequest<int>(routeAccountId) { User = session.User });
            accountResponse.ExceptionIfError();

            if (string.IsNullOrEmpty(accountResponse.Value.Properties.LiveVQPortalUrl))
            {
                throw new Exception($"Missing LiveVQPortalUrl property for account {routeAccountId}");
            }

            return Redirect(accountResponse.Value.Properties.LiveVQPortalUrl);
        }
    }
}