namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;

    using Cyara.Domain.Types.Common;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Models;
    using Cyara.Web.Portal.Models;

    public class TestCaseBreakdownViewModel : BaseViewModel, IMediaTypeDiscriminator
    {
        public IEnumerable<CampaignRunTestResultSummaryViewData> TestResultSummaries { get; set; }

        [LoggingIgnore]
        public PaginatedView<PulseTestResultsViewData> TestResults { get; set; }

        public string ReportName { get; set; }

        public int ReportId { get; set; }

        public ResultFilter ResultFilter { get; set; }

        public MediaType MediaType { get; set; }

        public string TestCaseName { get; set; }

        public string TestCaseNameEncoded { get; set; }

        public string FolderPath { get; set; }

        public string DatePattern { get; set; }

        public string ChartTimePattern { get; set; }

        public string ChartDateTimePattern { get; set; }

        public bool ShowChart { get; set; }

        public string ChartTitle { get; set; }

        public long ViewLoaded { get; set; }
    }
}