namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Resources;

    public static class ScheduleYearlyViewDataExtensions
    {
        private static readonly string[] MonthNames =
            {
                Common.MonthAbbrev_1, 
                Common.MonthAbbrev_2,
                Common.MonthAbbrev_3, 
                Common.MonthAbbrev_4,
                Common.MonthAbbrev_5, 
                Common.MonthAbbrev_6,
                Common.MonthAbbrev_7, 
                Common.MonthAbbrev_8,
                Common.MonthAbbrev_9, 
                Common.MonthAbbrev_10,
                Common.MonthAbbrev_11, 
                Common.MonthAbbrev_12
            };

        public static ScheduleYearlyViewData Prime(this ScheduleYearlyViewData value, bool applyDefaults)
        {
            ((ScheduleDetailWithEndRepeatViewData)value).Prime(applyDefaults);

            if (applyDefaults)
            {
                value.EveryYear = 1; // every year

                // by day of month
                value.SpecialDayWithinSpecifiedMonth = false;
                value.EachParticularMonth = string.Empty;
                value.DayPosition = PeriodPositionWithinParentPeriodEnum.First;
                value.DaySelection = DaySelectionEnum.Day;
                value.ScheduleUnavailable = ScheduleUnavailableOption.AlwaysAvailable;
            }

            // by day of week
            value.DayPositionOptions = EnumHelper
                .EnumToList(typeof(PeriodPositionWithinParentPeriodEnum), "PeriodPositionWithinParentPeriodEnum", sort: false, useEnumNamesForResourceLookup: true)
                .Select(x => new SelectListItem { Text = x.Item2, Value = x.Item1 })
                .ToList();
            var days = EnumHelper
                .EnumToList(
                    "DaySelectionEnum",
                    new List<Enum>
                    {
                        DaySelectionEnum.Sunday, 
                        DaySelectionEnum.Monday, 
                        DaySelectionEnum.Tuesday, 
                        DaySelectionEnum.Wednesday, 
                        DaySelectionEnum.Thursday,
                        DaySelectionEnum.Friday, 
                        DaySelectionEnum.Saturday, 
                        DaySelectionEnum.Day,
                        //// DaySelectionEnum.Weekday,     // hiding for now
                        //// DaySelectionEnum.WeekendDay   // hiding for now
                    }, 
                    true)
                .Select(x => new SelectListItem { Text = x.Item2, Value = x.Item1 })
                .ToList();
            days.Insert(7, new SelectListItem() { Text = "~~~~~~~~~~~~~~~", Value = "Separator" });
            value.DaySelectionOptions = days;

            return value;
        }

        public static string ToText(this ScheduleYearlyViewData value)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(value.EachParticularMonth))
            {
                return ret;
            }

            if (value.SpecialDayWithinSpecifiedMonth && value.DayPosition.HasValue && value.DaySelection.HasValue)
            {
                ret += Common.Schedule_Every.FormatWith(
                    Common.ResourceManager.GetString("{0}_{1}".FormatWith("PeriodPositionWithinParentPeriodEnum", value.DayPosition)) +
                    " " +
                    Common.ResourceManager.GetString("{0}_{1}".FormatWith("DaySelectionEnum", value.DaySelection)));
            }

            string listOfMonths = string.Empty;
            var months = value.EachParticularMonth.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var m in months)
            {
                listOfMonths = listOfMonths + MonthNames[int.Parse(m) - 1] + ",";
            }

            listOfMonths = listOfMonths.TrimEnd(',');
            ret += string.IsNullOrEmpty(ret) ? Common.Schedule_Every.FormatWith(listOfMonths) : " " + Common.Schedule_OfEvery.FormatWith(listOfMonths);

            if (value.EndRepeat != EndRepeatType.After || !value.EndRepeatValue.HasValue || value.EndRepeatValue != 1)
            {
                if (value.EveryYear > 1)
                {
                    ret += " " + Common.Schedule_EveryYears.FormatWith(value.EveryYear);
                }
                else if (value.EveryYear == 1)
                {
                    ret += " " + Common.Schedule_EveryYear;
                }
            }

            ScheduleDetailWithEndRepeatViewData baseValue = value;
            var until = baseValue.ToText();
            if (!string.IsNullOrEmpty(until) && !string.IsNullOrEmpty(ret))
            {
                ret += ", ";
            }

            ret = ret + until;

            string unavailable = value.ScheduleUnavailable.ToScheduleText();

            if (!string.IsNullOrWhiteSpace(unavailable))
            {
                if (!string.IsNullOrEmpty(ret))
                {
                    ret += ", ";
                }

                ret = ret + unavailable;
            }

            return ret;
        }
    }
}
