namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using Cyara.Shared.Types.ReportSchedule;

    public static class MonthlyScheduleRepeatOptionExtensions
    {
        public static string ToLabel(this MonthlyScheduleRepeatOption value)
        {
            return Resources.Common.ResourceManager.GetString("MonthlyScheduleRepeatOption_" + (int)value);
        }

        public static string ToLabel(this MonthlyScheduleRepeatOption? value)
        {
            return value == null ? string.Empty : value.Value.ToLabel();
        }
    }
}