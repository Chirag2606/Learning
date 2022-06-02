namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Web.Portal.Areas.Report.Models;

    public static class ScheduleDailyViewDataExtensions
    {
        public static ScheduleDailyViewData Prime(this ScheduleDailyViewData value, bool applyDefaults)
        {
            ((ScheduleDetailWithEndRepeatViewData)value).Prime(applyDefaults);

            if (applyDefaults)
            {
                value.CustomTime = string.Empty;

                value.EveryDay = 1;

                value.UseCustomTime = false;
            }

            return value;
        }

        public static string ToText(this ScheduleDailyViewData value)
        {
            string ret = string.Empty;

            Action addDayFn = () =>
                {
                    if (value.EveryDay > 1)
                    {
                        ret += Cyara.Web.Resources.Common.Schedule_EveryDays.FormatWith(value.EveryDay);
                    }
                    else if (value.EveryDay == 1)
                    {
                        ret += Cyara.Web.Resources.Common.Schedule_EveryDay;
                    }
                };

            if (value.UseCustomTime)
            {
                if (value.EndRepeat != EndRepeatType.After || !value.EndRepeatValue.HasValue
                    || value.EndRepeatValue != 1)
                {
                    addDayFn();
                    ret += " ";
                }

                ret += Cyara.Web.Resources.Common.Schedule_At.FormatWith(value.CustomTime);
            }
            else
            {
                if (value.EveryDay <= 0)
                {
                    return ret;
                }

                if (value.EndRepeat != EndRepeatType.After || !value.EndRepeatValue.HasValue || value.EndRepeatValue != 1)
                {
                    addDayFn();
                }
            }

            ScheduleDetailWithEndRepeatViewData baseValue = value;
            var until = baseValue.ToText();
            if (!string.IsNullOrEmpty(until) && !string.IsNullOrEmpty(ret))
            {
                ret += ", ";
            }

            return ret + until;
        }
    }
}
