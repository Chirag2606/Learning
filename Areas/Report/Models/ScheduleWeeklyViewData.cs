namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Cyara.Shared.Reflection;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Resources;

    public class ScheduleWeeklyViewData : ScheduleDetailWithEndRepeatViewData, ISchedulePeriod
    {
        /// <summary>
        /// Every week ( = 1 ) or skip some, i.e. every second week ( = 2 ), etc.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Every_Invalid")]
        [Display(Name = "Every", ResourceType = typeof(Labels))]
        public int EveryWeek { get; set; }

        /// <summary>
        /// comma delimited list of selected days within week starting with 0 (Sunday=0) to 6 (Saturday=6), e.g. "0,3,6,"
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "DaySelection_Invalid")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        public string RepeatOn { get; set; }

        public static IEnumerable<DayOfWeek> ParseDayOfWeek(string daysOfWeek)
        {
            var ints = daysOfWeek.ParseCsvIntegers();
            return ints != null ? ints.Select(integer => (DayOfWeek)integer) : null;
        }

        /// <summary>
        /// Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        /// A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The validation context.</param>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(RepeatOn))
            {
                foreach (var m in RepeatOn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int day;
                    if (!int.TryParse(m, out day) || day < 0 || day > 6)
                    {
                        yield return
                            new ValidationResult(
                                ValidationMessages.WeekSelection_Invalid,
                                new[] { ReflectOn<ScheduleWeeklyViewData>.GetProperty(a => a.RepeatOn).Name });
                    }
                }
            }
        }

        public override string ToString()
        {
            return this.ToText();
        }
    }
}