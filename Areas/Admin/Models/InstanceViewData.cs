namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using Cyara.Domain.Types.Maintenance;
    using Cyara.Web.Portal.Core;

    using Newtonsoft.Json;

    public class InstanceViewData
    {
        public string InstanceName { get; set; }

        public string LastUpdated { get; set; }

        public long LastUpdatedTicks { get; set; }

        [JsonConverter(typeof(ForceDefaultJsonConverter))]
        public MaintenanceMode MaintenanceMode { get; set; }
    }
}