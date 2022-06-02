namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;

    [Serializable]
    public class CustomReportViewData
    {
        public int CustomReportId { get; set; }

        public string Name { get; set; }

        public ReportScheduleViewData Schedule { get; set; }

        public string NextRun { get; set; }

        public string LastRun { get; set; }
    }
}