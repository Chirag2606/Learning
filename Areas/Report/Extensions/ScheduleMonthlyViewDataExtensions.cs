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

    public static class ScheduleMonthlyViewDataExtensions
    {
        public static ScheduleMonthlyViewData Prime(this ScheduleMonthlyViewData value, bool applyDefaults)
        {
            ((ScheduleDetailWithEndRepeatViewData)value).Prime(applyDefaults);

            if (applyDefaults)
            {
                value.EveryMonth = 1; // every month

                // by day of month
                value.MonthlyOption = MonthlyScheduleRepeatOption.SpecialDayWithinMonth;
                value.EachParticularDayOfMonth = string.Empty;
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

        public static string ToText(this ScheduleMonthlyViewData value)
        {
            string ret = string.Empty;

            switch (value.MonthlyOption)
            {
                case MonthlyScheduleRepeatOption.DayWithinMonth:
                    if (string.IsNullOrEmpty(value.EachParticularDayOfMonth))
                    {
                        return ret;
                    }

                    ret += Common.Schedule_EveryDays.FormatWith(value.EachParticularDayOfMonth.TrimEnd(','));
                    break;
                case MonthlyScheduleRepeatOption.SpecialDayWithinMonth:
                    if (value.DayPosition.HasValue && value.DaySelection.HasValue)
                    {
                        ret += Common.Schedule_Every.FormatWith(
                            Common.ResourceManager.GetString("{0}_{1}".FormatWith("PeriodPositionWithinParentPeriodEnum", value.DayPosition)) +
                            " " +
                            Common.ResourceManager.GetString("{0}_{1}".FormatWith("DaySelectionEnum", value.DaySelection)));
                    }

                    break;
            }

            if (value.EndRepeat != EndRepeatType.After || !value.EndRepeatValue.HasValue || value.EndRepeatValue != 1)
            {
                if (value.EveryMonth > 1)
                {
                    ret += " " + Common.Schedule_EveryMonths.FormatWith(value.EveryMonth);
                }
                else if (value.EveryMonth == 1)
                {
                    ret += " " + Common.Schedule_EveryMonth;
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
