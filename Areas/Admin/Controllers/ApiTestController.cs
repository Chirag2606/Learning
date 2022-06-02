namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Web.Portal.Areas.Api.Models.V2;

    [SecuredResource(new[] { StaticRoles.PlatformAdmin }, false)]
    public class ApiTestController : BaseController
    {
        // GET: /ApiTestV2/
        public ActionResult Index(float? version)
        {
            if (!version.HasValue)
            {
                return RedirectToAction("Index", new { version = 2.5 });
            }

            return View($"IndexV{version}", new TestViewModel { Username = "admin", Password = "password" });
        }

        [HttpPost]
        public ActionResult Index(float version, TestViewModel model)
        {
            if (model == null)
            {
                model = new TestViewModel { Username = "admin", Password = "password" };
            }

            return View($"IndexV{version}", model);
        }
    }
}
