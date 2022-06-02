namespace Cyara.Web.Portal.Areas.Apps.Controllers
{
    using System.Text.RegularExpressions;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Licensing;
    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;

    [SecuredAreaAccess(agent: new[] { Access.All }, agentCampaign: new[] { Access.All })]
    public class AgentWebController : BaseController
    {
        // GET: Apps/Agent bulk upload
        public ActionResult Import(int routeAccountId)
        {
            int selectedAccountId = routeAccountId;

            var configurationService = DependencyResolver.Current.GetService<IConfigurationService>();

            // Add and condition with feature flag, feature flag should be false by default
            if (!configurationService.IsLicensed(LicensedFeature.AgentBulkUpload, selectedAccountId))
            {
                return new HttpNotFoundResult();
            }

            var agentWebUrl = configurationService.Get(ConfigurationKey.AgentWebAppUrl.Key);
            agentWebUrl = Regex.Replace(agentWebUrl, @"\{accountId\}", selectedAccountId.ToString(), RegexOptions.IgnoreCase);
            return Redirect(agentWebUrl + "/import");
        }

        // GET: Apps/Agent bulk upload
        public ActionResult EndPoints(int routeAccountId)
        {
            return GetEndPointURLResult(routeAccountId);
        }

        [SecuredResource(new[] { StaticRoles.PlatformAdmin, StaticRoles.PlatformUserAdmin }, false)]
        public ActionResult EndPointsAdmin(int routeAccountId)
        {
            return GetEndPointURLResult(routeAccountId,true);
        }

        private ActionResult GetEndPointURLResult(int routeAccountId, bool enableAgentEndpointsManagement = false)
        {
            int selectedAccountId = routeAccountId;

            var configurationService = DependencyResolver.Current.GetService<IConfigurationService>();

            // TODO: When Endpoint plan is available, should check in configuration service if account has endpoint plan

            switch (enableAgentEndpointsManagement)
            {
                case false:
                    if (!configurationService.IsLicensed(LicensedFeature.AgentEndpoints, selectedAccountId))
                    {
                        return new HttpNotFoundResult();
                    }
                    break;
                case true:
                    if (!configurationService.IsLicensed(LicensedFeature.AgentEndpoints))
                    {
                        return new HttpNotFoundResult();
                    }
                    break;
                
                default: return new HttpNotFoundResult();

            }
            
            var agentWebUrl = configurationService.Get(ConfigurationKey.AgentWebAppUrl.Key);
            agentWebUrl = Regex.Replace(agentWebUrl, @"\{accountId\}", enableAgentEndpointsManagement == true ? "admin" : selectedAccountId.ToString(), RegexOptions.IgnoreCase);
            return Redirect(agentWebUrl + "/endpoints");
        }
    }
}