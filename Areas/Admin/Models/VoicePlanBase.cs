namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Cyara.Domain.Types.Rules;
    using Cyara.Web.Resources;

    public class VoicePlanBase : PlanEditViewModel, IVoicePlanComponentInCountry
    {
        [Required(ErrorMessage = " ")]
        [RegularExpression("([0-9]+)", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Ports_Numeric")]
        [Range(1, Plans.Voice.MaxPorts, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Ports_Range")]
        [Display(Name = "Ports", ResourceType = typeof(Labels))]
        public int Ports { get; set; }

        public bool InCountryLicensed { get; set; }

        public bool InCountryEnabled { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var result in base.Validate(validationContext))
            {
                yield return result;
            }
        }
    }
}