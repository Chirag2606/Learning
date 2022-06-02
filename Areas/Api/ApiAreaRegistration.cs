namespace Cyara.Web.Portal.Areas.Api
{
    using System.Web.Http;
    using System.Web.Mvc;

    using Cyara.Web.Common.Extensions;

    public class ApiAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Api";

        /// <summary>
        /// Register area
        /// </summary>
        /// <remarks>
        /// This is being done here, because if it is done in register area, it works for the web portal, but not for the testing as it has been implemented.
        /// It is important that the version parameters be constrained, so that the version routes don't interfere with one another.
        /// At some point it would be great to add the Web.Api 2.0 package from MS, and use the Routing Attributes to define the routes instead of this.
        /// Call me chicken, but I didn't want to risk it on a short time-frame.
        /// </remarks>
        public void Register(HttpConfiguration config)
        {
            const string Supported2xVersions = @"^2\.[012345]$";

            const string Supported25PlusVersions = @"^2\.[5]$";

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_0_CyaraXml",
                area: this,
                version: "2.0",
                controllerPostfix: null,
                routeTemplate: "api/{version}/account/{account}/{scope}/{controller}/{id}/block/{block}/data/{data}",
                defaults: new { action = "Default", entity = RouteParameter.Optional },
                constraints:
                    new
                        {
                            version = Supported2xVersions,
                            account = @"^[0-9]+$",
                            id = @"^[0-9]+$",
                            scope = @"^(voice)$",
                            block = @"^(include|exclude|flatten)$",
                            data = @"^(yes|no)$",
                            controller = "cyaraxml"
                        });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_5_CyaraXml",
                area: this,
                version: "2.5",
                controllerPostfix: null,
                routeTemplate: "api/{version}/account/{account}/{scope}/{controller}/{id}/block/{block}/data/{data}/service/{service}",
                defaults: new { action = "Default", entity = RouteParameter.Optional },
                constraints:
                new
                    {
                        version = Supported25PlusVersions,
                        account = @"^[0-9]+$",
                        id = @"^[0-9]+$",
                        scope = @"^(voice)$",
                        block = @"^(include|exclude|flatten)$",
                        data = @"^(yes|no)$",
                        service = @"^(yes|no)$",
                        controller = "cyaraxml"
                    });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_1_Recording",
                area: this,
                version: "2.1",
                controllerPostfix: null,
                routeTemplate: "api/{version}/account/{account}/{scope}/recording/{id}/{step}",
                defaults: new { controller = "Recording", action = "Default", step = RouteParameter.Optional },
                constraints: new { version = Supported2xVersions, account = @"^[0-9]+$", id = @"^[0-9]+$", scope = @"^(voice)$", step = @"^[0-9]+$" });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_5_Screenshot",
                area: this,
                version: "2.5",
                controllerPostfix: null,
                routeTemplate: "api/{version}/account/{account}/{scope}/screenshot/{id}/{step}",
                defaults: new { controller = "Media", action = "Screenshot", step = RouteParameter.Optional },
                constraints: new { version = Supported25PlusVersions, account = @"^[0-9]+$", scope = @"^(web)$" });
            
            config.Routes.MapHttpVersionRoute(
                name: "API_V2_0",
                area: this,
                version: "2.0",
                controllerPostfix: null,
                routeTemplate: "api/{version}/account/{account}/{scope}/{controller}/{id}/{action}/{entity}",
                defaults: new { id = RouteParameter.Optional, action = "Default", entity = RouteParameter.Optional },
                constraints:
                    new
                        {
                            version = Supported2xVersions,
                            account = @"^[0-9]+$",
                            id = new OptionalInt(),
                            scope = @"^(agent|voice)$"
                        });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_0_Report_Filtered",
                area: this,
                version: "2.0",
                controllerPostfix: "Report",
                routeTemplate: "api/{version}/account/{account}/report/{scope}/{controller}/{action}",
                constraints: new { version = Supported2xVersions, account = @"^[0-9]+$", scope = @"^(agent|voice)$" });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_5_Report_Filtered",
                area: this,
                version: "2.5",
                controllerPostfix: "Report",
                routeTemplate: "api/{version}/account/{account}/report/{scope}/{controller}/{action}",
                constraints: new { version = Supported25PlusVersions, account = @"^[0-9]+$", scope = @"^(agent|voice|email|web|sms)$" });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_5_User",
                area: this,
                version: "2.5",
                controllerPostfix: null,
                routeTemplate: "api/{version}/user/{action}",
                constraints: new { version = Supported25PlusVersions },
                defaults: new { controller = "User", id = RouteParameter.Optional });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_0_Report",
                area: this,
                version: "2.0",
                controllerPostfix: "Report",
                routeTemplate: "api/{version}/account/{account}/report/{scope}/{controller}/{id}/{action}/{entity}",
                defaults:
                    new
                        {
                            id = RouteParameter.Optional,
                            action = RouteParameter.Optional,
                            entity = RouteParameter.Optional
                        },
                constraints:
                    new
                        {
                            version = Supported2xVersions,
                            account = @"^[0-9]+$",
                            id = new OptionalInt(),
                            scope = @"^(agent|voice)$"
                        });

            config.Routes.MapHttpVersionRoute(
                name: "API_V1_0_Report",
                area: this,
                version: "1.0",
                controllerPostfix: null,
                routeTemplate: "api/{version}/report/{action}/{id}",
                defaults: new { controller = "Report", id = UrlParameter.Optional },
                constraints: new { version = @"^1\.0$" });

            config.Routes.MapHttpVersionRoute(
                name: "API_V1_0_CampaignRun",
                area: this,
                version: "1.0",
                controllerPostfix: null,
                routeTemplate: "api/{version}/campaignrun/{id}/{command}",
                defaults: new { controller = "CampaignRun", action = "Runner" },
                constraints: new { version = @"^1\.0$" });

            config.Routes.MapHttpVersionRoute(
                name: "API_V1_0_Default",
                area: this,
                version: "1.0",
                controllerPostfix: null,
                routeTemplate: "api/{version}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { version = @"^1\.0$" });

            config.Routes.MapHttpVersionRoute(
                name: "API_V2_0_Invalid_Report",
                area: this,
                version: "2.0",
                controllerPostfix: null,
                routeTemplate: "api/{version_str}/{account_spec}/{account_str}/report/{scope_str}/{controller_str}/{id_str}/{action_str}/{entity_str}/{*extra}",
                defaults: new
                {
                    controller = "InvalidUrl",
                    action = "ReasonV2",
                    version = "2.0",
                    urlpattern = 2,
                    controller_str = RouteParameter.Optional,
                    id_str = RouteParameter.Optional,
                    action_str = RouteParameter.Optional,
                    entity_str = RouteParameter.Optional,
                    extra = RouteParameter.Optional
                });

            // api/two/account/111/agent/Campaign/222/run
            config.Routes.MapHttpVersionRoute(
                name: "API_V2_0_Invalid",
                area: this,
                version: "2.0",
                controllerPostfix: null,
                routeTemplate: "api/{version_str}/{account_spec}/{account_str}/{scope_str}/{controller_str}/{id_str}/{action_str}/{entity_str}/{*extra}",
                defaults: new
                {
                    controller = "InvalidUrl",
                    action = "ReasonV2",
                    version = "2.0",
                    urlpattern = 1,
                    version_str = RouteParameter.Optional,
                    account_spec = RouteParameter.Optional,
                    account_str = RouteParameter.Optional,
                    scope_str = RouteParameter.Optional,
                    controller_str = RouteParameter.Optional,
                    id_str = RouteParameter.Optional,
                    action_str = RouteParameter.Optional,
                    entity_str = RouteParameter.Optional,
                    extra = RouteParameter.Optional
                });
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
        }
    }
}
