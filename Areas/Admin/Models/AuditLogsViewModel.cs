namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Web.Resources;

    using DateTimeExtensions = Cyara.Shared.Extensions.DateTimeExtensions;

    public class AuditLogsViewModel : PaginatedView<AuditLogViewData>, IValidatableObject
    {
        public enum SubmitAction
        {
            Update,
            Export    
        }

        public SubmitAction Action { get; set; } = SubmitAction.Update;

        public string DatePattern { get; set; }

        [Display(Name = "PeriodFromInUTC", ResourceType = typeof(Labels))]
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "FromDateTime_Required")]
        public DateTime From { get; set; }

        [Display(Name = "PeriodToInUTC", ResourceType = typeof(Labels))]
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_Required")]
        [GreaterThan("From", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_LessThanFrom")]
        [DateSpan("From", 180, "days", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_Within180Days")]
        public DateTime To { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Labels))]
        public string Category { get; set; }

        [Display(Name = "SubCategory", ResourceType = typeof(Labels))]
        public string SubCategory { get; set; }

        [Display(Name = "UserId", ResourceType = typeof(Labels))]
        public string UserId { get; set; }

        [Display(Name = "Username", ResourceType = typeof(Labels))]
        public string UserName { get; set; }

        [Display(Name = "Account", ResourceType = typeof(Labels))]
        public string AccountName { get; set; }

        public IEnumerable<SelectListItem> AllUsers { get; set; }

        public IEnumerable<SelectListItem> AllAccounts { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        public string AllCategoriesAsJson { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            DateTime earliestAllowed = DateTime.UtcNow.Date.AddDays(-180);
            if (earliestAllowed > From)
            {
                yield return
                    new ValidationResult(
                        ValidationMessages.Date_EarliestAllowed.FormatWith(earliestAllowed.ToString(DateTimeExtensions.DateFormatString)),
                        new[] { nameof(From) });
            }

            var latestAllowed = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);
            if (To > latestAllowed)
            {
                yield return
                    new ValidationResult(
                        ValidationMessages.Date_LatestAllowed.FormatWith(latestAllowed.FormatToPickerDateTime()), 
                        new[] { nameof(To) });
            }
        }
    }
}
