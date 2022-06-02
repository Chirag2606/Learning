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

    [Serializable]
    public class ScheduleMonthlyViewData : ScheduleDetailWithEndRepeatViewData, ISchedulePeriod
    {
        /// <summary>
        /// Every month ( = 1 ) or skip some, i.e. every second month ( = 2 ), etc.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Every_Invalid")]
        [Display(Name = "Every", ResourceType = typeof(Labels))]
        public int EveryMonth { get; set; }

        /// <summary>
        /// select by day of month or by day of specific week within month
        /// </summary>
        public MonthlyScheduleRepeatOption MonthlyOption { get; set; }

        /// <summary>
        /// comma delimited list of selected days within month from 1 to 31, e.g. "1,3,6,9,31,"
        /// </summary>
        [RequiredIf("MonthlyOption", MonthlyScheduleRepeatOption.DayWithinMonth, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "DaySelection_Invalid")]
        [NotMarkup(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [Display(Name = "Each", ResourceType = typeof(Labels))]
        public string EachParticularDayOfMonth { get; set; }

        /// <summary>
        /// E.g. first, second, last
        /// </summary>
        [RequiredIf("MonthlyOption", MonthlyScheduleRepeatOption.SpecialDayWithinMonth, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "DaySelection_Invalid")]
        [Display(Name = "OnThe", ResourceType = typeof(Labels))]
        public PeriodPositionWithinParentPeriodEnum? DayPosition { get; set; }

        public IEnumerable<SelectListItem> DayPositionOptions { get; set; }

        /// <summary>
        /// e.g. Monday, weekend, weekday
        /// </summary>
        [RequiredIf("MonthlyOption", MonthlyScheduleRepeatOption.SpecialDayWithinMonth, ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "DaySelection_Invalid")]
        public DaySelectionEnum? DaySelection { get; set; }

        public IEnumerable<SelectListItem> DaySelectionOptions { get; set; }

        public ScheduleUnavailableOption ScheduleUnavailable { get; set; }

        /// <summary>
        /// Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        /// A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The validation context.</param>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(EachParticularDayOfMonth))
            {
                bool error = false;
                foreach (var m in EachParticularDayOfMonth.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int day;
                    if (!int.TryParse(m, out day) || day < 1 || day > 31)
                    {
                        error = true;
                        break;
                    }
                }

                if (error)
                {
                    yield return
                        new ValidationResult(
                            ValidationMessages.MonthSelection_Invalid,
                            new[] { ReflectOn<ScheduleMonthlyViewData>.GetProperty(a => a.EachParticularDayOfMonth).Name });
                }
            }
        }

        public override string ToString()
        {
            return this.ToText();
        }
    }
}