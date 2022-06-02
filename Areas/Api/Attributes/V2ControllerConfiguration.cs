namespace Cyara.Web.Portal.Areas.Api.Attributes
{
    using System;
    using System.Web.Http.Controllers;

    public class V2ControllerConfiguration : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            // dont use the DataContractorSerializer its a PIA with inbound types
            // controllerSettings.Formatters.XmlFormatter.UseXmlSerializer = true;
            // controllerSettings.Formatters.XmlFormatter.Indent = true;

            /*
            var json = controllerSettings.Formatters.Where(x => x.SupportedMediaTypes.Any(m => m.MediaType == "application/json")).FirstOrDefault();
            if(json != null)
            {
                var index = controllerSettings.Formatters.IndexOf(json);
                controllerSettings.Formatters[index] = new JsonCamelCaseFormatter(Newtonsoft.Json.Formatting.Indented, Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate, Newtonsoft.Json.DateTimeZoneHandling.r);
            }
            else
            {
                controllerSettings.Formatters.Add(new JsonCamelCaseFormatter(Newtonsoft.Json.Formatting.Indented, Newtonsoft.Json.DefaultValueHandling.IgnoreAndPopulate, Newtonsoft.Json.DateTimeZoneHandling.Local));
            }*/

            controllerSettings.Services.RemoveAll(typeof(System.Web.Http.Filters.IFilterProvider), x => true);
            controllerSettings.Services.Add(typeof(System.Web.Http.Filters.IFilterProvider), new PriorityFilterProvider());
        }
    }
}