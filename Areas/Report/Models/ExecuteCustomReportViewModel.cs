namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Reports;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    [Serializable]
    public class ExecuteCustomReportViewModel : BaseViewModel, IValidatableObject
    {
        [NonSerialized]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Can't have NonSerialized attribute on a property")]
        public IEnumerable<SelectListItem> DateRangeTypes;

        public bool ForPrinting { get; set; }

        public int ReportId { get; set; }

        public string LastModified { get; set; }

        public string ModifiedBy { get; set; }

        public string ReportName { get; set; }

        [Display(Name = "Period", ResourceType = typeof(Labels))]
        [LoggingIgnore]
        public CustomDateRange DateRange { get; set; }

        [Display(Name = "PeriodFrom", ResourceType = typeof(Labels))]
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "FromDateTime_Required")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
       
        public DateTime From { get; set; }

        [Display(Name = "PeriodTo", ResourceType = typeof(Labels))]
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_Required")]
        [GreaterThan("From", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_LessThanFrom")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
       
        public DateTime To { get; set; }

        [LoggingIgnore]
        public string DatePattern { get; set; }
        
        public PaginatedView<ExecuteCustomReportViewData> TestCases { get; set; }

        [LoggingIgnore]
        public ListView<string> TestCasesFilter { get; set; }

        [LoggingIgnore]
        public ListView<string> CampaignsFilter { get; set; }

        [LoggingIgnore]
        public ListView<string> FailureReasonFilter { get; set; }

        [LoggingIgnore]
        public string FilterTree { get; set; }

        [LoggingIgnore]
        public string FailureReasonsTree { get; set; }

        [LoggingIgnore]
        public string TestCaseFilterSelection { get; set; }

        [LoggingIgnore]
        public string CampaignFilterSelection { get; set; }

        [LoggingIgnore]
        public string FailureReasonSelection { get; set; }

        [Display(Name = "MediaType", ResourceType = typeof(Labels))]
        [LoggingIgnore]
        public MediaType MediaType { get; set; }

        [LoggingIgnore]
        public IEnumerable<SelectListItem> MediaTypes { get; set; }

        [LoggingIgnore]
        public string NextScheduledReport { get; set; }

        [LoggingIgnore]
        public ScheduleStatusEnum ScheduleStatus { get; set; }

        [LoggingIgnore]
        public bool HasDeletedTestCases { get; set; }

        [LoggingIgnore]
        public IEnumerable<CampaignRunTestResultSummaryViewData> TestResultSummaries { get; set; }

        /// <summary>
        /// Timestamp used to ensure we get a uniformed set of results across the grid and donut chart.
        /// </summary>
        
        [LoggingIgnore]
        public long ViewLoaded { get; set; }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateRange == CustomDateRange.Custom)
            {
                var maxDays = RangeIncludeLeapYear(From, To) ? 366 : 365;
                if ((To - From).TotalDays > maxDays)
                {
                    yield return new ValidationResult(
                        ValidationMessages.CustomReport_DateRangeInvalid,
                        new[] { ReflectOn<ExecuteCustomReportViewModel>.GetProperty(p => p.From).Name });
                }
            }
        }
        
        private static bool RangeIncludeLeapYear(DateTime from, DateTime to)
        {
            return (from.Month <= 2 && DateTime.IsLeapYear(from.Year)) || (to.Month >= 2 && DateTime.IsLeapYear(to.Year));
        }
    }
}