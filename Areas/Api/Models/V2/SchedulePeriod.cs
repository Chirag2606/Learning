namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Linq;

    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class SchedulePeriod
    {
        private static DayOfWeekMap[] dayOfWeekMaps = 
            new DayOfWeekMap[] 
            {
                new DayOfWeekMap { ApiDay = Weekday.Sunday, ModelDay = DayOfWeek.Sunday },
                new DayOfWeekMap { ApiDay = Weekday.Monday, ModelDay = DayOfWeek.Monday },
                new DayOfWeekMap { ApiDay = Weekday.Tuesday, ModelDay = DayOfWeek.Tuesday },
                new DayOfWeekMap { ApiDay = Weekday.Wednesday, ModelDay = DayOfWeek.Wednesday },
                new DayOfWeekMap { ApiDay = Weekday.Thursday, ModelDay = DayOfWeek.Thursday },
                new DayOfWeekMap { ApiDay = Weekday.Friday, ModelDay = DayOfWeek.Friday },
                new DayOfWeekMap { ApiDay = Weekday.Saturday, ModelDay = DayOfWeek.Saturday }
            };

        public static SchedulePeriod FromVal(Cyara.Domain.Types.Shared.Schedule schedule, IDataHelper dataHelper)
        {
            return new SchedulePeriod
            {
                Day = dayOfWeekMaps.First(x => x.ModelDay == schedule.From.DayOfWeek).ApiDay,
                From = dataHelper.OutputString(schedule.From),
                To = dataHelper.OutputString(schedule.To)
            };
        }

        private struct DayOfWeekMap
        {
            public Weekday ApiDay;
            public DayOfWeek ModelDay;
        }
    }
}