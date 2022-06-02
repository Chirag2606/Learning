namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.DataAnnotations;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Validation;
    using Cyara.Web.Resources;
    using JQChart.Web.Mvc;

    public class AccountPeakPortsTabData
    {
        public PlanType? Plan { get; set; }

        public IList<SelectListItem> PlanList { get; set; }

        public MediaType? Channel { get; set; }

        public IList<SelectListItem> ChannelList { get; set; }

        [Display(Name = "DateRange", ResourceType = typeof(Labels))]
        public AccountUsageDateRange DateRange { get; set; } = AccountUsageDateRange.Last12Months;

        public DateTimeIntervalType DateRangePeriod { get; set; }

        public string DateRangeFormat { get; set; }

        public IList<SelectListItem> DateRangeList { get; set; }

        [RequiredIfEqual(nameof(DateRange), AccountUsageDateRange.Custom, ClientPropertyName = "PeakPorts_DateRange", ErrorMessage = " ")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        [Display(Name = "FromDateTime", ResourceType = typeof(Labels))]
        public DateTime? From { get; set; }

        public string FromPicker => From?.FormatToPickerDate() ?? string.Empty;

        [RequiredIfEqual(nameof(DateRange), AccountUsageDateRange.Custom, ClientPropertyName = "PeakPorts_DateRange", ErrorMessage = " ")]
        [GreaterThan(nameof(From), ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_LessThanFrom")]
        [DateSpan(nameof(From), 400, "days", ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ToDateTime_Within400Days")]
        [ValidLocalDate(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "ValidLocalDate")]
        [Display(Name = "ToDateTime", ResourceType = typeof(Labels))]
        public DateTime? To { get; set; }

        public string ToPicker => To?.FormatToPickerDate() ?? string.Empty;

        public List<CyaraWebApi.DailyPortUsageModel> Data { get; set; }

        public DateTime AxisMinimum => Data != null && Data.Count > 0 ? Data.Min(x => x.Date) : DateTime.UtcNow.Date;

        public DateTime AxisMaximum => Data != null && Data.Count > 0 ? Data.Max(x => x.Date) : DateTime.UtcNow.Date;

        /// <summary>
        /// Calculate the vertical interval which will have at most 20 horizontal lines showing in the graph
        /// </summary>
        public int PortsAxisLabelsInterval => (int)Math.Ceiling(PortsAxisMaximum / 20.0m);

        /// <summary>
        /// Calculate the highest value to show on the graph - ensure it is at least one more than the highest number, minimum of 10 (well 11)
        /// </summary>
        public int PortsAxisMaximum => Data != null && Data.Count > 0 ? (int)(1 + Data?.Max(x => x.ConnectionsLicensed)) : 10;
    }
}