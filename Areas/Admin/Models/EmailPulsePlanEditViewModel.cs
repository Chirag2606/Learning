namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Web.Resources;

    public class EmailPulsePlanEditViewModel : EmailBasePlanEditViewModel
    {
        [Display(Name = "MinFrequency", ResourceType = typeof(Labels))]
        public int MinFrequency { get; set; }

        public List<SelectListItem> Frequencies { get; set; }
    }
}