namespace Cyara.Web.Portal.Areas.Admin
{
    using System;
    using System.Web.Mvc;
    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;

    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Admin";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_elmah",
                "Admin/logs/{type}",
                new { action = "Index", controller = "Logs", type = UrlParameter.Optional });

            context.MapRoute(
                "Admin_MediaType",
                "Admin/{routeAccountId}/Plan/Create/{media}/{plan}",
                new { controller = "plan", action = "create" },
                new { media = GetValidNames<MediaType>(), plan = GetValidNames<PlanType>() },
                new[] { "Cyara.Web.Portal.Areas.Admin.Controllers" });

            context.MapRoute(
                "ApiTest_default",
                "Admin/ApiTest/{version}",
                new { Controller = "ApiTest", action = "Index", version = UrlParameter.Optional },
                new { });

            context.MapRoute(
                "Admin_default",
                "Admin/{routeAccountId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Cyara.Web.Portal.Areas.Admin.Controllers" });
        }

        private static string GetValidNames<T>()
        {
            var names = Enum.GetNames(typeof(T));
            return string.Join("|", names);
        }
    }
}