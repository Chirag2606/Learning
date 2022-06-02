namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class VoicePulsePlanEditViewModel : VoicePlanBaseWithCaps
    {
        [Display(Name = "MinCallFrequency", ResourceType = typeof(Resources.Labels))]
        public int MinCallFrequency { get; set; }

        public List<SelectListItem> CallFrequencies { get; set; }

        [Display(Name = "Premium", ResourceType = typeof(Resources.Labels))]
        public bool Premium { get; set; }
    }
}