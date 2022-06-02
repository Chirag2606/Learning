namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;

    using Cyara.Shared.Reflection;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Resources;

    [Serializable]
    public class ScheduleDailyViewData : ScheduleDetailWithEndRepeatViewData, ISchedulePeriod
    {
        [Required(ErrorMessage = " ")]
        [Display(Name = "UseMultipleTimes", ResourceType = typeof(Labels))]
        public bool UseCustomTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Every_Invalid")]
        [Display(Name = "Every", ResourceType = typeof(Labels))]
        public int EveryDay { get; set; }

        [RequiredIf("UseCustomTime", true, ErrorMessage = " ")]
        [NotMarkup(ErrorMessageResourceType = typeof(Resources.ValidationMessages), ErrorMessageResourceName = "Markup_NotAllowed")]
        [Display(Name = "CustomTime", ResourceType = typeof(Labels))]
        public string CustomTime { get; set; }

        public static IEnumerable<TimeSpan> ParseTimes(string customTimes)
        {
            if (string.IsNullOrWhiteSpace(customTimes))
            {
                return null;
            }

            var items = new List<TimeSpan>();

            var components = customTimes.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var component in components)
            {
                DateTime parsed;
                
                if (DateTime.TryParseExact(component, new[] { "h:mm tt", "H:mm" }, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
                {
                    items.Add(parsed.TimeOfDay);
                }
                else
                {
                    return null;
                }
            }

            return items;
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
            if (EveryDay <= 0)
            {
                yield return
                            new ValidationResult(
                                ValidationMessages.Every_Invalid,
                                new[] { ReflectOn<ScheduleDailyViewData>.GetProperty(m => m.EveryDay).Name });
            }

            if (UseCustomTime)
            {
                if (!string.IsNullOrWhiteSpace(this.CustomTime))
                {
                    var items = ParseTimes(this.CustomTime);
                    if (items == null)
                    {
                        yield return
                            new ValidationResult(
                                ValidationMessages.CustomTime_UnableToParse,
                                new[] { ReflectOn<ScheduleDailyViewData>.GetProperty(m => m.CustomTime).Name });
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