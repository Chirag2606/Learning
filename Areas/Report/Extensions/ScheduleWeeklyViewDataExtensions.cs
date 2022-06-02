namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Web.Portal.Areas.Report.Models;

    public static class ScheduleWeeklyViewDataExtensions
    {
        private static readonly string[] DayOfWeekNames =
        {
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_0,
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_1,
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_2,
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_3,
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_4,
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_5,
            Cyara.Web.Resources.Common.DayOfWeekAbbrev_6
        };

        public static ScheduleWeeklyViewData Prime(this ScheduleWeeklyViewData value, bool applyDefaults)
        {
            ((ScheduleDetailWithEndRepeatViewData)value).Prime(applyDefaults);

            if (applyDefaults)
            {
                value.EveryWeek = 1; // every week
                value.RepeatOn = string.Empty;
            }

            return value;
        }

        public static string ToText(this ScheduleWeeklyViewData value)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(value.RepeatOn))
            {
                return ret;
            }

            string listOfDaysOfWeek = string.Empty;
            var weeks = value.RepeatOn.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var m in weeks)
            {
                listOfDaysOfWeek += DayOfWeekNames[int.Parse(m)] + ",";
            }

            listOfDaysOfWeek = listOfDaysOfWeek.TrimEnd(',');
            ret += Cyara.Web.Resources.Common.Schedule_Every.FormatWith(listOfDaysOfWeek);

            if (value.EndRepeat != EndRepeatType.After || !value.EndRepeatValue.HasValue || value.EndRepeatValue != 1)
            {
                if (value.EveryWeek > 1)
                {
                    ret += " " + Cyara.Web.Resources.Common.Schedule_EveryWeeks.FormatWith(value.EveryWeek);
                }
                else if (value.EveryWeek == 1)
                {
                    ret += " " + Cyara.Web.Resources.Common.Schedule_EveryWeek;
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
