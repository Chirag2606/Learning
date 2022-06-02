namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class ChatPulsePlanEditViewModel : ChatPlanBase
    {
        [Display(Name = "MinFrequency", ResourceType = typeof(Resources.Labels))]
        public int MinFrequency { get; set; }

        public List<SelectListItem> Frequencies { get; set; }
    }
}