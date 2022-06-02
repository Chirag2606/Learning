namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class SmsPulseOutboundPlanEditViewModel : SmsOutboundPlanEditViewModel
    {
        [Display(Name = "MinCallFrequency", ResourceType = typeof(Resources.Labels))]
        public int MinCallFrequency { get; set; }

        public List<SelectListItem> CallFrequencies { get; set; }
    }
}