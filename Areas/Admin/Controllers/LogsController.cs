namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Web.Portal.Core.Mvc;

    [SecuredResource(StaticRoles.PlatformAdmin, false)]
    public class LogsController : BaseController
    {
        public ActionResult Index(string type)
        {
            return new ElmahResult(type);
        }
    }
}
