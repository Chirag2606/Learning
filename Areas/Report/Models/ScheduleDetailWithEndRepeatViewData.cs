namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Resources;

    public class ScheduleDetailWithEndRepeatViewData
    {
        public string DatePattern { get; set; }

        [Display(Name = "EndRepeat", ResourceType = typeof(Labels))]
        public EndRepeatType EndRepeat { get; set; }

        public IEnumerable<SelectListItem> EndRepeatOptions { get; set; }

        [RequiredIf("EndRepeat", EndRepeatType.After, ErrorMessage = " ")]
        [Integer(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "After_Invalid")]
        public int? EndRepeatValue { get; set; }

        [RequiredIf("EndRepeat", EndRepeatType.On, ErrorMessage = " ")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        public DateTime? EndRepeatDate { get; set; }
    }
}