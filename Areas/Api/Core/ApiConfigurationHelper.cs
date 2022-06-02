namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Api;
    using Cyara.Shared.Web.Settings;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Core.Api;

    using Newtonsoft.Json;

    public static class ApiConfigurationHelper
    {
        public static void SetupWebApiConfiguration(HttpConfiguration configuration)
        {
            RegisterBasicAuthenticationForApi(configuration);
            RegisterApiServices(configuration);
            RegisterApiFilters(configuration);
            RegisterApiFormatters(configuration);
        }

        private static void RegisterBasicAuthenticationForApi(HttpConfiguration configuration)
        {
            ILogger logger = DependencyResolver.Current.GetService<ILogger>();
            var settings = DependencyResolver.Current.GetService<WebSiteSettings>();
            configuration.MessageHandlers.Add(new BasicAuthenticationHandler(logger));
        }

        private static void RegisterApiFormatters(HttpConfiguration configuration)
        {
            // don't use the DataContractorSerializer its a PIA with inbound types
            configuration.Formatters.XmlFormatter.UseXmlSerializer = true;
            configuration.Formatters.XmlFormatter.Indent = true;

            var index = configuration.Formatters.IndexOf(configuration.Formatters.JsonFormatter);
            configuration.Formatters[index] = new JsonCamelCaseFormatter(Formatting.Indented, DefaultValueHandling.Include, DateTimeZoneHandling.RoundtripKind);
        }

        private static void RegisterApiServices(HttpConfiguration configuration)
        {
            configuration.Services.Replace(typeof(IHttpControllerSelector), new VersionedApiHttpControllerSelector(configuration));
            configuration.Services.Replace(typeof(IHttpActionSelector), new ActionNotFoundExceptionActionSelector());
        }

        private static void RegisterApiFilters(HttpConfiguration configuration)
        {
            configuration.Filters.Add(new HandleApiException());
            configuration.Filters.Add(new ClientCacheControlAttribute());
        }
    }
}