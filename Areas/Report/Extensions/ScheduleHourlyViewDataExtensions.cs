namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Web.Portal.Areas.Report.Models;

    public static class ScheduleHourlyViewDataExtensions
    {
        public static ScheduleHourlyViewData Prime(this ScheduleHourlyViewData value, bool applyDefaults)
        {
            if (applyDefaults)
            {
                value.EveryHour = 1;
            }

            return value;
        }

        public static string ToText(this ScheduleHourlyViewData value)
        {
            string ret = string.Empty;
            if (value.EveryHour > 1)
            {
                ret += Cyara.Web.Resources.Common.Schedule_EveryHours.FormatWith(value.EveryHour);
            }
            else if (value.EveryHour == 1)
            {
                ret += Cyara.Web.Resources.Common.Schedule_EveryHour;
            }

            return ret;
        }
    }
}
