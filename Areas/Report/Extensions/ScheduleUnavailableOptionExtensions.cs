namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;

    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.ReportSchedule;

    public static class ScheduleUnavailableOptionExtensions
    {
        public static string ToLabel(this ScheduleUnavailableOption value)
        {
            return Resources.Common.ResourceManager.GetString("ScheduleUnavailableOption_" + (int)value);
        }

        public static string ToScheduleText(this ScheduleUnavailableOption value)
        {
            switch (value)
            {
                case ScheduleUnavailableOption.AlwaysAvailable:
                    return string.Empty;

                case ScheduleUnavailableOption.Skip:
                    return Resources.Common.Schedule_UnavailableSkip;

                case ScheduleUnavailableOption.ClosestDayInMonth:
                    return Resources.Common.Schedule_UnavailableClosest;

                default:
                    throw new Exception("Unrecognized ScheduleUnavailableOption:{0}".FormatWith(value));
            }
        }
    }
}
