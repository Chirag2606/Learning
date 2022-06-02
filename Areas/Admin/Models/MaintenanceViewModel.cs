namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;

    using Cyara.Domain.Types.Maintenance;
    using Cyara.Shared.Web.Models;

    public class MaintenanceViewModel : BaseViewModel
    {
        public bool MaintenanceModeForPortal { get; set; }

        public ComponentMaintenanceViewModel VoiceScheduler { get; set; }

        public ComponentMaintenanceViewModel OmniScheduler { get; set; }

        public Dictionary<string, InstanceViewData> VoiceSchedulerInstances { get; set; }

        public Dictionary<string, InstanceViewData> CallEngineInstances { get; set; }

        public Dictionary<string, string> VoiceSchedulerFeatures { get; set; }

        public string CallEngineChangesJson { get; set; }

        public CallEngineChangesModel CallEngineChanges { get; set; }

        public string PageWarning { get; set; }

        public class CallEngineChangesModel
        {
            public Dictionary<string, MaintenanceMode> Changed { get; set; }

            public string[] Deleted { get; set; }
        }
    }
}