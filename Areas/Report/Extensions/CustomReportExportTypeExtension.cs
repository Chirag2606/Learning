namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using Cyara.Shared.Types.ReportSchedule;

    public static class CustomReportExportTypeExtension
    {
        public static string ToLabel(this CustomReportExportType value)
        {
            return Resources.Common.ResourceManager.GetString("CustomReportExportType_" + (int)value);
        }

        public static string ToLabel(this CustomReportExportType? value)
        {
            return value == null ? string.Empty : value.Value.ToLabel();
        }
    }
}