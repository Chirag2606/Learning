namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;

    using Cyara.Shared.Web.Models;

    public class SchedulerStatusViewModel : BaseViewModel
    {
        public string LastRefreshed { get; set; }

        public IReadOnlyList<VoiceSchedulerStatusViewData> Status { get; set; } 

        public IReadOnlyList<CallEngineViewData> CallEngines { get; set; }

        public IReadOnlyList<QueuedCampaignViewData> QueuedCampaigns { get; set; }

        public IReadOnlyList<RunningCampaignViewData> RunningCampaigns { get; set; } 
    }
}