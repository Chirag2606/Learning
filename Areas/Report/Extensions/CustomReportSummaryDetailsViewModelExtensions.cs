namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.TestResult;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Core.IO;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Messaging.Types.Projections;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;
    using MediatR;

    public static class CustomReportSummaryDetailsViewModelExtensions
    {
        public static CustomReportSummaryDetailsViewModel ApplyDefaults(
            this CustomReportSummaryDetailsViewModel value,
            ResultType resultType,
            bool forPrinting = false)
        {
            value.CampaignRunTestResultDetails = new PaginatedView<ResultSummaryDetailsTestCaseViewData>
            {
                PageNumber = 1,
                PageSize = forPrinting ? int.MaxValue : MvcApplication.Settings.DefaultPageSize,
                SortAscending = true,
                SortColumn = Columns.TestCase,
                CollectionSize = 0
            };

            value.TestResultSummaries = null;
            value.CampaignRunTestResults = new MediaPaginatedView<CampaignRunTestResultViewData>
            {
                PageNumber = 1,
                PageSize = forPrinting ? int.MaxValue : MvcApplication.Settings.DefaultPageSize,
                SortAscending = true,
                SortColumn = Columns.TestCase,
                CollectionSize = 0
            };

            value.DetailedResult = string.Empty;
            value.FolderPath = string.Empty;
            value.TestCaseName = string.Empty;
            return value;
        }

        public static async Task<CustomReportSummaryDetailsViewModel> Prime(
            this CustomReportSummaryDetailsViewModel value,
            IReportsService reportsService,
            ICampaignService campaignService,
            IAccountService accountService,
            IMediator mediator,
            int reportId,
            int accountId,
            long? baseTimestamp,
            string timezone,
            ResultType resultType)
        {
            value.ReportId = reportId;
            value.ResultType = resultType;
            value.ViewLoaded = baseTimestamp ?? DateTime.UtcNow.Ticks;

            // get report name and MediaType
            var customReportTask = mediator.Send(new CustomReportGetQuery { AccountId = accountId, ReportId = reportId });

            // get filtered (by TCs, Campaigns and Failure reasons) and sorted (by description) results (and RunIDs) for donut and donut's grid
            var query = new CustomReportDonutBreakdownFilteredDetailsDataQuery
            {
                AccountId = accountId,
                ReportId = value.ReportId,
                Result = resultType,
                BaseTimestamp = new DateTime(value.ViewLoaded, DateTimeKind.Utc),
                Timezone = timezone
            };
            var filteredResultsTask = mediator.Send(query);

            // populate paginated grid
            var paginatedViewTask = value.CampaignRunTestResultDetails.PrimePaginatedGrid(mediator, accountId, reportId, value.ViewLoaded, timezone, resultType, value.CampaignRunTestResultDetails);

            // get report result and populate with the report name and media type
            var customReport = await customReportTask;
            value.ReportName = customReport.Name;
            value.MediaType = customReport.MediaType;
            value.CampaignRunTestResults.MediaType = customReport.MediaType;

            // get filtered results now
            var filteredResults = await filteredResultsTask;

            // populate with the returned results and RunIds
            value.RunIds = filteredResults.Value.Item1;
            value.TestResultSummaries = Mapper.MapList<CampaignRunResultCategoryByResultSummary, CampaignRunDetailedResultByResultSummaryViewData>(filteredResults.Value.Item2).ToList();
            value.TestResultSummaries.ToList().PopulateDetailedDescriptionAndSwapNotSetCategoryForSuccess(resultType);
            var total = value.TestResultSummaries.Sum(x => Convert.ToInt32(x.Number));
            value.TestResultSummaries.ForEach(x => x.Percent = Convert.ToInt32(x.Number).ToPercent(total).ToString(CultureInfo.InvariantCulture)); // populate donut's percentages

            if (value.Filter != null && value.Filter.Message != null && (value.Message == null || value.Message.Severity == Severity.PageSuccess))
            {
                value.Message = value.Filter.Message; // pass error message along
            }

            // ensure this one is done before returning
            await paginatedViewTask;

            return value;
        }

        public static async Task<PaginatedView<ResultSummaryDetailsTestCaseViewData>> PrimePaginatedGrid(
            this PaginatedView<ResultSummaryDetailsTestCaseViewData> value,
            IMediator mediator,
            int accountId,
            int reportId,
            long baseTimestamp,
            string timezone,
            ResultType resultType,
            IPaginatedView view)
        {
            // get paginated list of test cases that have executed during selected time period (by applying filters)
            var query = new CustomReportDonutBreakdownFilteredAndGroupedTestCasesForPaginatedGridDataQuery
            {
                AccountId = accountId,
                ReportId = reportId,
                Result = resultType,
                BaseTimestamp = new DateTime(baseTimestamp, DateTimeKind.Utc),
                Timezone = timezone,
                PageSize = view.PageSize,
                PageNo = view.PageNumber,
                SortField = view.SortColumn,
                SortAscending = view.SortAscending
            };
            var response = await mediator.Send(query);
            PaginatedResponse<CampaignRunTestCaseByResultSummary> campaignRunTestResultDetailsResponse = response;

            // to each TC that we care about add the list of corresponding results (filtered from the list of all results this TC has produced during selected period)
            value = value.FromPaginatedResponse(campaignRunTestResultDetailsResponse);
            var collection = value.Collection.ToList();

            foreach (var resultSummaryDetailsTestCaseViewData in collection)
            {
                var folderPathForTestCase = FileUtils.GetPathWithoutLeadingSlash(resultSummaryDetailsTestCaseViewData.Folder);

                var query2 = new CustomReportDonutBreakdownFilteredResultsForTestCaseDataQuery
                {
                    AccountId = accountId,
                    ReportId = reportId,
                    Result = resultType,
                    BaseTimestamp = new DateTime(baseTimestamp, DateTimeKind.Utc),
                    Timezone = timezone,
                    TestCaseName = resultSummaryDetailsTestCaseViewData.TestCase,
                    TestCaseFolder = string.IsNullOrWhiteSpace(folderPathForTestCase) ? null : folderPathForTestCase,
                };
                var response2 = await mediator.Send(query2);
                var detailedResults3 = Mapper.MapList<CampaignRunDetailedResultByResultSummary, CampaignRunDetailedResultByResultSummaryViewData>(response2.Value).ToList();
                detailedResults3.PopulateDetailedDescriptionAndParameter(resultType);
                resultSummaryDetailsTestCaseViewData.DetailedResult = detailedResults3;
            }

            value.Collection = collection;
            return value;
        }
    }
}
