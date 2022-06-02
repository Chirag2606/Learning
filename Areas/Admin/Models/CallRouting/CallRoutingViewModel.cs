namespace Cyara.Web.Portal.Areas.Admin.Models.CallRouting
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Cyara.Shared.Web.Models;
    using Cyara.Web.Resources;

    public class CallRoutingViewModel : BaseViewModel, IValidatableObject
    {
        public int CallRoutingId { get; set; }

        public string Hash { get; set; }

        public bool LastValid { get; set; }

        public CallRoutingStatusViewData LastStatus { get; set; }

        public int LastValidCallRoutingId { get; set; }

        [Display(Name = "LastValidCallRoutingRuleset", ResourceType = typeof(Labels))]
        public string LastValidCallRoutingRuleset { get; set; }

        public CallRoutingStatusViewData LastValidStatus { get; set; }

        [Display(Name = "RoutingRuleset", ResourceType = typeof(Labels))]
        public string RoutingRuleset { get; set; }

        [Display(Name = "SystemApproved", ResourceType = typeof(Labels))]
        public bool SystemApproved { get; set; }

        public IEnumerable<string> ValidationIssues { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}