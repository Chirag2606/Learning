namespace Cyara.Web.Portal.Areas.Apps.Controllers
{
    using System.Text.RegularExpressions;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Licensing;
    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;

    public class AgentEndpointRealtimeController : BaseController
    {
        // GET: Apps/AgentEndpointRealtime

        public ActionResult Index(int routeAccountId)
        {
            int selectedAccountId = routeAccountId;

            var configurationService = DependencyResolver.Current.GetService<IConfigurationService>();

            if (!configurationService.IsLicensed(LicensedFeature.AgentEndpoints, selectedAccountId))
            {
                return new HttpNotFoundResult();
            }

            var agentEndpointRealtimeUrl = configurationService.Get(ConfigurationKey.AgentEndpointRealtimeUrl.Key);

            return Redirect(Regex.Replace(agentEndpointRealtimeUrl, @"\{accountId\}", selectedAccountId.ToString(), RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// Fake action to hide the menu item from the navigation for non CCM Reporting users.
        /// </summary>
        [SecuredResource(StaticRoles.CCMReporting)]
        public ActionResult Endpoint()
        {
            return new HttpNotFoundResult();
        }
    }
}