namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class CallEngineViewData
    {
        public string AddressAndPort { get; set; }

        public string UniqueCallEngineId { get; set; }

        public int TotalPorts { get; set; }

        public string VoiceSchedulerId { get; set; }

        public string MaintenanceMode { get; set; }
    }
}