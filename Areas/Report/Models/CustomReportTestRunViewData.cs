namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;

    public class CustomReportTestRunViewData
    {
        public string PhoneNumber { get; set; }

        public int TestResultId { get; set; }

        public float? Duration { get; set; }

        public DateTime ActualStart { get; set; }

        public string DetailedResult { get; set; }

        public string Name { get; set; }

        public string Folder { get; set; }
    }
}