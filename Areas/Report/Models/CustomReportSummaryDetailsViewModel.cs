namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.TestResult;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Models;
    using Cyara.Web.Portal.Models;
    using Cyara.Foundation.Logging.Types;

    public class CustomReportSummaryDetailsViewModel : BaseViewModel, IMediaTypeDiscriminator
    {
        public int ReportId { get; set; }

        public string ReportName { get; set; }

        public string ReportErrors { get; set; }

        public MediaType MediaType { get; set; }

        // this timestamp is used to sync data across all parts of the page and even on refresh, so it all shows the same data
        public long ViewLoaded { get; set; }

        public ResultType ResultType { get; set; }

        public int[] RunIds { get; set; }

        // we are truncating detailed results after some length in PDF exports only
        public string TruncationWarning { get; set; }

        [LoggingIgnore]
        public IEnumerable<CampaignRunDetailedResultByResultSummaryViewData> TestResultSummaries { get; set; }

        [LoggingIgnore]
        public PaginatedView<ResultSummaryDetailsTestCaseViewData> CampaignRunTestResultDetails { get; set; }

        [LoggingIgnore]
        public MediaPaginatedView<CampaignRunTestResultViewData> CampaignRunTestResults { get; set; }

        public string DetailedResult { get; set; }

        public string TestCaseName { get; set; }

        public string FolderPath { get; set; }
        
        public ExecuteCustomReportViewModel Filter { get; set; }
    }
}