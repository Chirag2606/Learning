namespace Cyara.Web.Portal.Areas.Report.Models
{
    using Cyara.Domain.Types.TestResult;
    using Cyara.Shared.Types.Reports;

    public class DetailedResultMappingViewData
    {
        public string Name { get; set; }

        public CustomSeverityLevel Severity { get; set; }

        public TestResultCategory TestResultCategory { get; set; }
    }
}