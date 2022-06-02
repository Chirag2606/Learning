namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Web.Resources;

    /// <summary>
    /// Borrowed from <see href="http://blogs.infosupport.com/asp-net-mvc-4-rc-getting-webapi-and-areas-to-play-nicely/"/>
    /// And adapted to include version as well as area
    /// </summary>
    public class VersionedApiHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string AreaRouteVariableName = "area";
        private const string VersionRouteVariableName = "version";
        private const string ControllerPostfixName = "controllerPostfix";

        private readonly HttpConfiguration _configuration;
        private readonly Lazy<ConcurrentDictionary<string, Type>> _apiControllerTypes;

        public VersionedApiHttpControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
            _apiControllerTypes = new Lazy<ConcurrentDictionary<string, Type>>(GetControllerTypes);
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            return this.GetApiController(request);
        }

        private static string GetAreaName(HttpRequestMessage request)
        {
            var data = request.GetRouteData();
            if (data.Route.DataTokens == null)
            {
                return null;
            }

            object areaName;
            return (data.Route.DataTokens.TryGetValue(AreaRouteVariableName, out areaName) && areaName != null) ? areaName.ToString() : null;
        }

        private static string GetControllerPostfix(HttpRequestMessage request)
        {
            var data = request.GetRouteData();
            if (data.Route.DataTokens == null)
            {
                return null;
            }

            object controllerPostfix;
            if (data.Route.DataTokens.TryGetValue(ControllerPostfixName, out controllerPostfix) && controllerPostfix != null)
            {
                return controllerPostfix.ToString();
            }

            return null;
        }

        private static decimal? GetVersion(HttpRequestMessage request)
        {
            var data = request.GetRouteData();

            decimal version;
            if (data.Values.ContainsKey("version"))
            {
                if (decimal.TryParse(data.Values["version"].ToString(), out version))
                {
                    return version;
                }
            }

            object versionObj;
            if (data.Route.DataTokens != null && data.Route.DataTokens.TryGetValue(VersionRouteVariableName, out versionObj))
            {
                if (decimal.TryParse(versionObj.ToString(), out version))
                {
                    return version;
                }
            }

            return null;
        }

        private ConcurrentDictionary<string, Type> GetControllerTypes()
        {
            IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();

            var types = controllersResolver.GetControllerTypes(assembliesResolver)
                .Where(ct => ct.IsSubclassOf(typeof(ApiController)))
                .ToDictionary(x => x.FullName, x => x);
            return new ConcurrentDictionary<string, Type>(types);
        }

        private HttpControllerDescriptor GetApiController(HttpRequestMessage request)
        {
            var areaName = GetAreaName(request);
            var version = GetVersion(request);
            var controllerName = GetControllerName(request) + (GetControllerPostfix(request) ?? string.Empty);
            var types = GetControllerType(areaName, version, controllerName);

            // If we can't find the controller - tell them the request was bad, and tell them the right formats to use
            if (types == null || types.Count < 1)
            {
                var loggerService = DependencyResolver.Current.GetService<ILogger>();
                loggerService?.LogError(GetType(), $"Api Call Error [{request.GetCorrelationId()}]: Controller not found : {request.RequestUri}");

                var res = request.CreateErrorResponse(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_ControllerNotFound + "\r\n" + ApiMessages.ValidUrl_Patterns);
                res.ReasonPhrase = ApiMessages.InvalidUrl;
                throw new HttpResponseException(res);
            }

            if (types.Count > 1)
            {
                throw new HttpResponseException(
                    request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Multiple controllers were found that match this request."));
            }

            return new HttpControllerDescriptor(_configuration, controllerName, types.First());
        }

        private List<Type> GetControllerType(string areaName, decimal? version, string controllerName)
        {
            var query = _apiControllerTypes.Value.AsEnumerable();

            query = string.IsNullOrEmpty(areaName) ? query.WithoutAreaName() : query.ByAreaName(areaName);

            if (version.HasValue)
            {
                query = query.ByVersion(version.Value);
            }

            return query
                .ByControllerName(controllerName)
                .Select(x => x.Value)
                .ToList();
        }
    }
}
