namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Resources;
    using Cyara.Web.Resources.Areas.Report.Views.Account;
    using JQChart.Web.Mvc;

    public static class AccountPeakPortsTabDataExtensions
    {
        private const int DefaultDayRange = 28;
        private const int WeekVsMonthDays = 7 * 12;

        public static async Task<AccountPeakPortsTabData> PrimeAsync(this AccountPeakPortsTabData tab, RestApiFacade webApi, string timezone, int accountId, bool applyDefaults, bool isModelValid)
        {
            DateTime from = tab.From ?? DateTime.UtcNow.Date.AddDays(-DefaultDayRange);
            DateTime to = tab.To ?? DateTime.UtcNow.Date;

            if (applyDefaults || (tab.DateRange == AccountUsageDateRange.Custom && (!tab.From.HasValue || !tab.To.HasValue)))
            {
                tab.DateRange = AccountUsageDateRange.Last12Months;
            }

            if (tab.DateRange != AccountUsageDateRange.Custom)
            {
                var ranges = tab.DateRange.ConstructRange(timezone);
                from = ranges.Item1;
                to = ranges.Item2; 
            }

            from = from.FromLocal(timezone);
            to = to.FromLocal(timezone);

            tab.From = from.ToLocal(timezone);
            tab.To = to.ToLocal(timezone);

            tab.DateRangePeriod = (to - from).TotalDays >= WeekVsMonthDays ? DateTimeIntervalType.Months : DateTimeIntervalType.Weeks;
            var group = tab.DateRangePeriod == DateTimeIntervalType.Months ? "Month" : "Week";
            tab.DateRangeFormat = tab.DateRangePeriod == DateTimeIntervalType.Months ? "mmm-yyyy" : "d-mmm-yyyy";

            var response = new CyaraWebApi.PaginatedResultOfDailyPortUsageAndDailyPortUsageModel();
            if (isModelValid)
            {
                response = await webApi.DailyPeaksApi.GetDailyPeaksForFilterAsync(accountId, from, to, tab.Channel.ToApi(), tab.Plan.ToApi(), group.ToApi(), null, true, 1000, 1);
            }

            tab.Data = response.Results?.ToList() ?? new System.Collections.Generic.List<CyaraWebApi.DailyPortUsageModel>();

            tab.DateRangeList = EnumHelper.EnumToList(typeof(AccountUsageDateRange), "AccountUsageDateRange", sort: false)
                                        .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 })
                                        .ToList();

            var validChannels = (new[] { MediaType.Email, MediaType.Sms, MediaType.Voice, MediaType.Chat })
                .Select(x => x.ToString())
                .ToList();

            tab.ChannelList = EnumHelper.EnumToList(typeof(MediaType), "MediaType", i => validChannels.All(x => x != i), false)
                    .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 })
                    .ToList();
            tab.ChannelList.Insert(0, new SelectListItem { Value = string.Empty, Text = Resources.Common.All });

            var validPlans = (new[] { PlanType.Cruncher, PlanType.CruncherLite, PlanType.Outbound, PlanType.Pulse, PlanType.PulseOutbound, PlanType.Replay, PlanType.Velocity })
                .Select(x => x.ToString())
                .ToList();

            tab.PlanList = EnumHelper.EnumToList(typeof(PlanType), "PlanType", i => validPlans.All(x => x != i), false)
                .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 })
                .ToList();
            tab.PlanList.Insert(0, new SelectListItem { Value = string.Empty, Text = Resources.Common.All });

            return tab;
        }

        public static string ExportPortUsageAsCsv(this AccountPeakPortsTabData tab)
        {
            var channel = tab.Channel.HasValue ? LocalisationHelpers.GetCommonResource($"MediaType_{(int)tab.Channel.Value}") : Common.All;
            var plan = tab.Plan.HasValue ? LocalisationHelpers.GetCommonResource($"PlanType_{(int)tab.Plan.Value}") : Common.All;
            var builder = new StringBuilder();
            builder.AppendLine($"{PeakPortUsageTab_cshtml.Title}");
            builder.AppendLine($"{Labels.PlanType}: {plan}");
            builder.AppendLine($"{Labels.MediaType}: {channel}");
            builder.AppendLine($"{Labels.DateRange}: {LocalisationHelpers.GetCommonResource($"AccountUsageDateRange_{(int)tab.DateRange}")}");
            builder.AppendLine($"{Labels.FromDateTime}: {tab.From?.FormatToPickerDate()}");
            builder.AppendLine($"{Labels.ToDateTime}: {tab.To?.FormatToPickerDate()}");
            builder.AppendLine();
            builder.AppendLine($"{Common.TableHeading_Date},{PeakPortUsageTab_cshtml.PortsUsedConcurrent},{PeakPortUsageTab_cshtml.PortsConfigured}");

            foreach (var d in tab.Data)
            {
                builder.AppendLine($"{d.Date.FormatToPickerDate()},{d.ConnectionsPeak},{d.ConnectionsLicensed}");
            }

            return builder.ToString();
        }
    }
}
