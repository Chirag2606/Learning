namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.ComponentModel.DataAnnotations;

    using Cyara.Domain.Types.Rules;
    using Cyara.Web.Resources;

    public class SmsOutboundPlanEditViewModel : PlanEditViewModel
    {
        [Required(ErrorMessage = " ")]
        [RegularExpression("([0-9]+)", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Sessions_Numeric")]
        [Range(1, Plans.Sms.MaxSessions, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Sessions_Range")]
        [Display(Name = "Sessions", ResourceType = typeof(Labels))]
        public int Sessions { get; set; }
    }
}