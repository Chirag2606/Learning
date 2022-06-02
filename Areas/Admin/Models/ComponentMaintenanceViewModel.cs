namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using Cyara.Foundation.Core.Settings;
    using Cyara.Shared.Types.Maintenance;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Web.Resources;

    public class ComponentMaintenanceViewModel
    {
        public Component Component { get; set; }

        public DateTime? Updated { get; set; }

        public Guid? UpdatedBy { get; set; }

        [Display(Name = "LimitedAvailabilityAccounts", ResourceType = typeof(Labels))]
        [RegularExpression(@"^(\d|\s*,\s*)*$", ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "LimitedAvailabilityAccounts_Invalid")]
        public string LimitedAvailabilityAccounts { get; set; }
    }
}
