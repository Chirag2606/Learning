namespace Cyara.Web.Portal.Areas.Api
{
    using System.Web.Http;
    using System.Web.Mvc;

    public class OptionalInt : System.Web.Routing.IRouteConstraint, System.Web.Http.Routing.IHttpRouteConstraint
    {
        public bool Match(System.Web.HttpContextBase httpContext, System.Web.Routing.Route route, string parameterName, System.Web.Routing.RouteValueDictionary values, System.Web.Routing.RouteDirection routeDirection)
        {
            if (values.ContainsKey(parameterName))
            {
                var pv = values[parameterName];
                if (pv == UrlParameter.Optional)
                {
                    return route.Defaults.ContainsKey(parameterName);
                }

                int intValue;
                return int.TryParse(pv.ToString(), out intValue) && (intValue != int.MinValue) && (intValue != int.MaxValue);
            }

            return false;
        }

        public bool Match(System.Net.Http.HttpRequestMessage request, System.Web.Http.Routing.IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary<string, object> values, System.Web.Http.Routing.HttpRouteDirection routeDirection)
        {
            if (values.ContainsKey(parameterName))
            {
                var pv = values[parameterName];
                if (pv == RouteParameter.Optional)
                {
                    return route.Defaults.ContainsKey(parameterName);
                }

                int intValue;
                return int.TryParse(pv.ToString(), out intValue) && (intValue != int.MinValue) && (intValue != int.MaxValue);
            }

            return false;
        }
    }
}