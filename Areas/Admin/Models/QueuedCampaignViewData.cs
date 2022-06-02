namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class QueuedCampaignViewData
    {
        public int AccountId { get; set; }

        public int CampaignId { get; set; }

        public string ConcurrentPorts { get; set; }

        public string JobName { get; set; }

        public string PlanType { get; set; }

        public string ScheduledDateTime { get; set; }

        public string VoiceSchedulerId { get; set; }

        public string LinkToCampaign { get; set; }
    }
}