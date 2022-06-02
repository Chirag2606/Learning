namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Resources;

    public class ScheduleYearlyViewData : ScheduleDetailWithEndRepeatViewData, ISchedulePeriod
    {
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Every_Invalid")]
        [Display(Name = "Every", ResourceType = typeof(Labels))]
        public int EveryYear { get; set; }

        /// <summary>
        /// comma delimited list of selected months from 1 to 12, e.g. "1,3,6,9,12"
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "MonthSelection_Invalid")]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        public string EachParticularMonth { get; set; }

        [Display(Name = "SpecialDayWithinSpecifiedMonth", ResourceType = typeof(Labels))]
        public bool SpecialDayWithinSpecifiedMonth { get; set; }

        [RequiredIf("SpecialDayWithinSpecifiedMonth", true, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "DaySelection_Invalid")]
        [Display(Name = "OnThe", ResourceType = typeof(Labels))]
        public PeriodPositionWithinParentPeriodEnum? DayPosition { get; set; }

        public IEnumerable<SelectListItem> DayPositionOptions { get; set; }

        [RequiredIf("SpecialDayWithinSpecifiedMonth", true, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "DaySelection_Invalid")]
        public DaySelectionEnum? DaySelection { get; set; }

        public IEnumerable<SelectListItem> DaySelectionOptions { get; set; }

        public ScheduleUnavailableOption ScheduleUnavailable { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool error = false;
            if (!string.IsNullOrEmpty(EachParticularMonth))
            {
                foreach (var m in EachParticularMonth.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int month;
                    if (!int.TryParse(m, out month) || month < 1 || month > 12)
                    {
                        error = true;
                    }
                }
            }

            if (error)
            {
                yield return
                    new ValidationResult(
                        ValidationMessages.MonthSelection_Invalid,
                        new[] { ReflectOn<ScheduleYearlyViewData>.GetProperty(a => a.EachParticularMonth).Name });
            }
        }

        public override string ToString()
        {
            return this.ToText();
        }
    }
}