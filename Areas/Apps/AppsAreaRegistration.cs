namespace Cyara.Web.Portal.Areas.Apps
{
    using System.Web.Mvc;

    public class AppsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Apps";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CXInsights_default_with_account",
                AreaName + "/{routeAccountId}/cxinsights/index",
                new { controller = "CXInsights", action = "Index" },
                new[] { "Cyara.Web.Portal.Areas.Apps.Controllers" });

            context.MapRoute(
                "CXInsights_default_no_account",
                AreaName + "/cxinsights/index",
                new { controller = "CXInsights", action = "Index" },
                new[] { "Cyara.Web.Portal.Areas.Apps.Controllers" });

            context.MapRoute(
                "Apps_default",
                AreaName + "/{routeAccountId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Cyara.Web.Portal.Areas.Apps.Controllers" });
        }
    }
}