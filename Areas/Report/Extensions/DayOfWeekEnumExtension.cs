namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using Cyara.Shared.Types.ReportSchedule;

    public static class DayOfWeekEnumExtension
    {
        public static string ToLabel(this DaySelectionEnum value)
        {
            return Resources.Common.ResourceManager.GetString("DayOfWeek_" + (int)value);
        }

        public static string ToLabel(this DaySelectionEnum? value)
        {
            return value == null ? string.Empty : value.Value.ToLabel();
        }
    }
}
