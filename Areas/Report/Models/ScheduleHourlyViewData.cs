namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Resources;

    public class ScheduleHourlyViewData : ISchedulePeriod
    {
        /// <summary>
        /// Every hour ( = 1 ) or skip some, i.e. every second hour ( = 2 ), ...
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Every_Invalid")]
        [Display(Name = "Every", ResourceType = typeof(Labels))]
        public int EveryHour { get; set; }

        /// <summary>
        /// Determines whether the specified object is valid.
        /// </summary>
        /// <returns>
        /// A collection that holds failed-validation information.
        /// </returns>
        /// <param name="validationContext">The validation context.</param>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }

        public override string ToString()
        {
            return this.ToText();
        }
    }
}