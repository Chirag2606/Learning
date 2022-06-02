namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;

    public enum AccountUsageDateRange
    {
        ThisMonth,
        LastMonth,
        ThisQuarter,
        LastQuarter,
        ThisYear,
        LastYear,
        Last12Months,
        Custom
    }

    public static class AccountUsageDateRangeExtensions
    {
        public static Tuple<DateTime, DateTime> ConstructRange(this AccountUsageDateRange value, string timezone, DateTime? baseTo = null)
        {
            if (baseTo.HasValue && baseTo.Value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Utc timestamp expected", nameof(baseTo));
            }

            DateTime from, to;

            var baseInUtc = baseTo ?? DateTime.UtcNow;

            var @base = baseInUtc.ToLocal(timezone);

            switch (value)
            {
                case AccountUsageDateRange.ThisMonth:
                    from = WrapFromLocal(new DateTime(@base.Year, @base.Month, 1), timezone);
                    to = baseInUtc;
                    break;
                case AccountUsageDateRange.LastMonth:
                    to = new DateTime(@base.Year, @base.Month, 1).AddSeconds(-1);
                    from = new DateTime(to.Year, to.Month, 1).FromLocal(timezone);
                    to = WrapFromLocal(to, timezone);
                    break;
                case AccountUsageDateRange.ThisQuarter:
                    GetCurrentQuarter(out to, out from, @base, timezone);
                    to = baseInUtc;
                    break;
                case AccountUsageDateRange.LastQuarter:
                    GetCurrentQuarter(out to, out from, @base.AddMonths(-3), timezone);
                    break;
                case AccountUsageDateRange.ThisYear:
                    from = WrapFromLocal(new DateTime(@base.Year, 1, 1), timezone);
                    to = baseInUtc;
                    break;
                case AccountUsageDateRange.LastYear:
                    from = WrapFromLocal(new DateTime(@base.Year - 1, 1, 1), timezone);
                    to = from.AddYears(1).AddSeconds(-1);
                    break;
                case AccountUsageDateRange.Last12Months:
                    from = WrapFromLocal(new DateTime(@base.Year, @base.Month, 1), timezone).AddMonths(-11);
                    to = baseInUtc;
                    break;
                default:
                    throw new ApplicationException("Problem constructing DateTimeInterval");
            }

            return Tuple.Create(from, to);
        }

        private static void GetCurrentQuarter(out DateTime to, out DateTime from, DateTime baseDate, string timezone)
        {
            int quarterNumber = ((baseDate.Month - 1) / 3) + 1;

            from = WrapFromLocal(new DateTime(baseDate.Year, ((quarterNumber - 1) * 3) + 1, 1), timezone);
            to = from.AddMonths(3).AddSeconds(-1);
        }

        private static DateTime WrapFromLocal(DateTime dt, string timezone)
        {
            try
            {
                return dt.FromLocal(timezone);
            }
            catch (Exception)
            {
                // ignore
            }

            return dt.SetUtcKind();
        }
    }
}
