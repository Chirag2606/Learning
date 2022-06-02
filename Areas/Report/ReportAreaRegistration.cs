namespace Cyara.Web.Portal.Areas.Report
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using Cyara.Shared.Web.Mvc;

    public class ReportAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Report";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var readOnlyRoute = new Route(
                "Report/{routeAccountId}/{controller}/{action}/{id}",
                new RouteValueDictionary(new { area = "Report", action = "Index", id = UrlParameter.Optional }),
                new RouteValueDictionary(new { action = "^(Index|CreateList|UpdateReportName|GetDateRange|PopoverData|Schedule|HasUnavailableSchedule|ResultsSummaryDetails)$" }),
                new ReadOnlySessionRouteHandler())
                                    {
                                        DataTokens = new RouteValueDictionary(
                                            new
                                            {
                                                Namespaces = new[] { "Cyara.Web.Portal.Areas.Report.Controllers" },
                                                area = "Report"
                                            })
                                    };

            context.Routes.Add("ReadOnlySessionState", readOnlyRoute);
                
            context.MapRoute(
                "Report_default",
                "Report/{routeAccountId}/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Cyara.Web.Portal.Areas.Report.Controllers" });
        }
    }
}
