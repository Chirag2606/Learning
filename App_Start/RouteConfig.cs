namespace Cyara.Web.Portal
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Resource;
    using Cyara.Web.Portal.Core.Angular;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.IgnoreRoute("admin/elmah.axd/{*pathInfo}");

            routes.LowercaseUrls = true;

            routes.Add(
                "ClassifiedResultsSummary",
                new Route(
                    "{routeAccountId}/report/ClassifiedResultsSummary/{runId}",
                    new RouteValueDictionary(new { controller = "Report", action = "ClassifiedResultsSummary" }),
                    null,
                    new RouteValueDictionary(new { namespaces = new[] { "Cyara.Web.Portal.Controllers" } }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "ClassifiedResultsSummaryToAngular",
                new Route(
                    "{routeAccountId}/report/{campaignId}/resultssummary/{runId}",
                    new RouteValueDictionary(new { controller = "Report", action = "ClassifiedResultsSummaryToAngular" }),
                    null,
                    new RouteValueDictionary(new { namespaces = new[] { "Cyara.Web.Portal.Controllers" } }),
                    new ReadOnlySessionRouteHandler()));

            // This route is used in SchedulerStatusViewModelExtensions
            routes.MapRoute(
                name: "CampaignWithMedia",
                url: "{routeAccountId}/campaign/{action}/{id}/{media}",
                defaults: new { controller = "Campaign", action = "Edit" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.MapRoute(
                name: "TempAudio",
                url: "{routeAccountId}/media/scratch/{testResultId}/{stepNo}/{resource}",
                defaults: new { controller = "Media", action = "Scratch" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.Add(
                "CallRecordingsAccessCheck",
                new Route(
                    "{routeAccountId}/media/recordings/accesscheck/{testResultId}/{*resource}",
                    new RouteValueDictionary(new { controller = "Media", action = "RecordingAccessible" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "CallRecordings",
                new Route(
                    "{routeAccountId}/media/recordings/{testResultId}/{*resource}",
                    new RouteValueDictionary(new { controller = "Media", action = "Recordings" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "CCMRealtimeCampaignBrowserSnapshotData",
                new Route(
                    "{routeAccountId}/report/CCMRealtimeCampaignBrowserSnapshotData/{id}",
                    new RouteValueDictionary(new { controller = "Report", action = "CCMRealtimeCampaignBrowserSnapshotData" }),
                    null,
                    new RouteValueDictionary(new { namespaces = new[] { "Cyara.Web.Portal.Controllers" } }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "SaveDraft",
                new Route(
                    "{routeAccountId}/{controller}/savedraft/{id}",
                    new RouteValueDictionary(new { controller = "TestCase", action = "SaveDraft" }),
                    new RouteValueDictionary(new { controller = "^(TestCase|Block)$" }),
                    new ReadOnlySessionRouteHandler()));

            routes.MapRoute(
                name: "StepRecordings",
                url: "{routeAccountId}/media/steprecordings/{testResultId}/{stepNo}/{*resource}",
                defaults: new { controller = "Media", action = "Recordings" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.Add(
                "AudioData",
                new Route(
                    "{routeAccountId}/media/audiodata/{testResultId}/{sampleSize}/{*resource}",
                    new RouteValueDictionary(new { controller = "Media", action = "AudioData" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "ScratchAudioData",
                new Route(
                    "{routeAccountId}/media/ScratchAudioData/{testResultId}/{stepNo}/{sampleSize}/{*resource}",
                    new RouteValueDictionary(new { controller = "Media", action = "ScratchAudioData" }),
                    new ReadOnlySessionRouteHandler()));

            routes.MapRoute(
                name: "AccountAudio",
                url: "{routeAccountId}/media/audio/{*resource}",
                defaults: new { controller = "Media", action = "Audio" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.Add(
                "Attachment",
                new Route(
                    "{routeAccountId}/media/attach",
                    new RouteValueDictionary(new { controller = "Media", action = "Attachment" }),
                    new ReadOnlySessionRouteHandler()));

            routes.MapRoute(
                name: "BlockEditFromTestCase",
                url: "{routeAccountId}/block/edit/{id}/{draftId}",
                defaults: new { controller = "Block", action = "Edit" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.MapRoute(
                name: "AccountChat",
                url: "{routeAccountId}/media/chat/{id}/{*resource}",
                defaults: new { controller = "Media", action = "Chat" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.MapRoute(
                name: "ChatScreenshots",
                url: "{routeAccountId}/media/screenshot/{id}/{*resource}",
                defaults: new { controller = "Media", action = "Screenshot" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.MapRoute(
                name: "RealtimeAudio",
                url: "{routeAccountId}/media/realtime/{*resource}",
                defaults: new { controller = "Media", action = "Realtime" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // View CX Model route
            routes.MapRoute("CxModel_View", "Crawler/{routeAccountId}/Model/View/{modelId}");

            // angular mapping to various folders
            routes.Add(new Route("app/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new AngularResourceRouteHandler()));
            routes.Add(new Route("app/assets/images/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new AngularResourceRouteHandler()));
            routes.Add(new Route("app/assets/css/icons/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new AngularResourceRouteHandler()));
            routes.Add(new Route("app/assets/css/images/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new AngularResourceRouteHandler()));
            routes.Add(new Route("app/assets/css/jquery/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new AngularResourceRouteHandler()));
            routes.Add(new Route("app/assets/css/jquery/images/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new AngularResourceRouteHandler()));

            routes.Add(new Route("embedded/{resource}", new RouteValueDictionary(new { }), new RouteValueDictionary(new { }), new EmbeddedResourceRouteHandler()));

            // ajax requests that only require read-only session (stops the locks)
            routes.Add(
                "TestCaseSearchFolder",
                new Route(
                    "{routeAccountId}/testcase/searchfolder",
                    new RouteValueDictionary(new { controller = "TestCase", action = "SearchFolder" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "FolderOpen",
                new Route(
                    "{routeAccountId}/folder/open",
                    new RouteValueDictionary(new { controller = "Folder", action = "Open" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "BlockGetBlockFull",
                new Route(
                    "{routeAccountId}/block/getblockfull",
                    new RouteValueDictionary(new { controller = "Block", action = "GetBlockFull" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "TestCaseValidationStatus",
                new Route(
                    "{routeAccountId}/testcase/validationstatus",
                    new RouteValueDictionary(new { controller = "TestCase", action = "ValidationStatus" }),
                    new ReadOnlySessionRouteHandler()));

            routes.Add(
                "Dataset",
                new Route(
                    "{routeAccountId}/dataset",
                    new RouteValueDictionary(new { controller = "Dataset", action = "Index" }),
                    new ReadOnlySessionRouteHandler()));

            // NOTE, the public, resources, and error controllers are required to work without the routeAccountId
            // public
            routes.MapRoute(
                name: "public",
                url: "public/{action}/{id}",
                defaults: new { controller = "Public", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // resources
            routes.MapRoute(
                name: "resources",
                url: "resources/{version}/all.json",
                defaults: new { controller = "Resources", action = "Get" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // errors
            routes.MapRoute(
                name: "ErrorExplicit",
                url: "error/{action}/{id}",
                defaults: new { controller = "Error", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // sms faux rest api
            routes.MapRoute(
                name: "SmsRestApi",
                url: "sms/{action}/{id}",
                defaults: new { controller = "Sms", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // verify setup
            routes.MapRoute(
                name: "VerifySetup",
                url: "VerifySetup",
                defaults: new { controller = "VerifySetup", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // angular stuff hacking stuff
            routes.MapRoute(
                name: "Angular",
                url: "Angular/{action}/{*message}",
                defaults: new { controller = "Angular" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // mobile
            routes.MapRoute(
                name: "mobile",
                url: "mobile/{action}/{id}",
                defaults: new { controller = "Mobile", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // default
            routes.MapRoute(
                name: "Default",
                url: "{routeAccountId}/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // default
            routes.MapRoute(
                name: "AccountRoot",
                url: "{routeAccountId}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { routeAccountId = @"^[0-9]+$" },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            // default
            routes.MapRoute(
                name: "Root",
                url: string.Empty,
                defaults: new { controller = "Home", action = "Bounce", id = UrlParameter.Optional },
                namespaces: new[] { "Cyara.Web.Portal.Controllers" });

            routes.MapRoute(
                "Error",
                 "{*url}",
                 new { controller = "Error", action = "NotFound" });   // 404s
        }
    }
}
