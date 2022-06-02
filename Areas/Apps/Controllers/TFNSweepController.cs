namespace Cyara.Web.Portal.Areas.Apps.Controllers
{
    using System.Text.RegularExpressions;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Licensing;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;

    [SecuredAreaAccess(testTools: new[] { Access.Edit, Access.Delete, Access.Execute })]
    public class TFNSweepController : BaseController
    {
        // GET: Apps/TFNSweep
        public ActionResult Index(int routeAccountId)
        {
            int selectedAccountId = routeAccountId;

            var configurationService = DependencyResolver.Current.GetService<IConfigurationService>();

            if (!configurationService.IsLicensed(LicensedFeature.TfnSweep, selectedAccountId))
            {
                return new HttpNotFoundResult();
            }

            var cyaraTfnSweepUrl = configurationService.Get(ConfigurationKey.CyaraTfnSweepUrl.Key);

            return Redirect(Regex.Replace(cyaraTfnSweepUrl, @"\{accountId\}", selectedAccountId.ToString(), RegexOptions.IgnoreCase));
        }
    }
}