namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.ComponentModel.DataAnnotations;

    public class AgentVoiceCCMPlanEditViewModel : PlanEditViewModel
    {
        [Required(ErrorMessage = " ")]
        [RegularExpression("([0-9]+)", ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Agents_Numeric")]
        [Range(1, Domain.Types.Rules.Plans.Agent.MaxAgents, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Agents_Range")]
        [Display(Name = "MaximumConcurrentAgents", ResourceType = typeof(Resources.Labels))]
        public int Agents { get; set; }
    }
}