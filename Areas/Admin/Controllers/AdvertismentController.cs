namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;

    public class AdvertismentController : BaseController
    {
        [SecuredResource(new[] { StaticRoles.Marketing }, false)]
        public ActionResult Edit()
        {
            return View("~/Views/Angular.cshtml");
        }
    }
}