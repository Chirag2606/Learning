namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using AutoMapper;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.TestResult;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Reports;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Messaging.Types.Data;
    using Cyara.Web.Messaging.Types.Exceptions;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;
    using MediatR;

    public static class TestCaseBreakdownViewModelExtensions
    {
        public static async Task<TestCaseBreakdownViewModel> Prime(
            this TestCaseBreakdownViewModel value,
            IMediator mediator,
            IConfigurationService configurationService,
            int accountId,
            int reportId,
            string testCaseName,
            string folderPath,
            long? timestamp,
            string userTimezone,
            PaginatedView paging,
            SessionFacade session)
        {
            value.ViewLoaded = timestamp ?? DateTime.UtcNow.Ticks;
            var maximumStart = new DateTime(value.ViewLoaded, DateTimeKind.Utc);

            var customReportTask = mediator.Send(new CustomReportGetQuery { AccountId = accountId, ReportId = reportId });

            var testCaseSummariesTask =
                mediator.Send(
                    new CustomReportTestCaseSummaryByNameAndPathQuery
                    {
                        AccountId = accountId,
                        ReportId = reportId,
                        BaseTimestamp = maximumStart,
                        Timezone = userTimezone,
                        TestCaseName = testCaseName,
                        FolderPath = folderPath
                    });

            var testCaseDetailPaginatedTask =
                mediator.Send(
                    new CustomReportTestCaseDetailPaginatedQuery
                    {
                        AccountId = accountId,
                        ReportId = reportId,
                        PageNo = paging?.PageNumber ?? 1,
                        PageSize = paging?.PageSize ?? MvcApplication.Settings.DefaultPageSize,
                        SortAscending = paging?.SortAscending ?? false,
                        SortField = paging?.SortColumn ?? Columns.StartDate,
                        BaseTimestamp = maximumStart,
                        Timezone = userTimezone,
                        TestCaseName = testCaseName,
                        FolderPath = folderPath,
                        ResultTypes = value.ResultFilter.ToResultTypeList()
                    });

            Func<TestCaseBreakdownViewModel> primeEmpty = () =>
            {
                value.TestResults = new PaginatedView<PulseTestResultsViewData> { Collection = Enumerable.Empty<PulseTestResultsViewData>() };
                value.Message = new MessageViewData().Prime("CustomReport_NotFound", Severity.PageFatal);
                return value;
            };

            try
            {
                var customReport = await customReportTask;
                value.SelectedAccountId = accountId;
                value.ReportId = reportId;
                value.ReportName = customReport.Name;
                value.TestCaseName = testCaseName;
                value.TestCaseNameEncoded = HttpUtility.UrlEncode(testCaseName);
                value.FolderPath = folderPath;
                value.MediaType = customReport.MediaType;

                var testCaseSummaries = await testCaseSummariesTask;
                value.TestResultSummaries = testCaseSummaries.Select(Mapper.Map<ResultCallAggregation, CampaignRunTestResultSummaryViewData>);

                var testCaseDetailPaginated = await testCaseDetailPaginatedTask;
                value.TestResults = new PaginatedView<PulseTestResultsViewData>().FromPaginatedResponse(testCaseDetailPaginated);

                if (value.MediaType == MediaType.Voice)
                {
                    value.TestResults.Collection = value.TestResults.Collection.ToList();

                    value.TestResults.Collection.ForEach(
                        vr =>
                            {
                                var origRec = testCaseDetailPaginated.Collection.First(x => x.TestResultId == vr.TestResultId);
                                vr.RecordingNotAvailable = !configurationService.IsRecordingAvailable(origRec.ActualStart, session.User);
                            });
                }

                return value;
            }
            catch (ReportNotFoundException)
            {
                return primeEmpty();
            }
        }

        public static async Task<IList<ReportSectionPopoverViewData>> GetPopoverData(
            this TestCaseBreakdownViewModel value,
            SessionFacade session,
            string testCaseName,
            string folderPath,
            ResultType resultType,
            IMediator mediator,
            int accountId,
            DateTime maximumStart)
        {
            var query = new CustomReportTestCaseDetailPopoverDataQuery
            {
                AccountId = accountId,
                ReportId = value.ReportId,
                TestCaseName = testCaseName,
                FolderPath = folderPath,
                Result = resultType,
                BaseTimestamp = maximumStart,
                Timezone = session.UserTimezone
            };

            var response = await mediator.Send(query);

            // map to Popover view data
            var items = Shared.Web.Mapping.Mapper.MapList<DetailedResultCallAggregation, ReportSectionPopoverViewData>(response).ToList();

            // give each item its percent of the total set
            var total = items.Sum(x => x.Value);
            items.ForEach(x => x.Percent = x.Value.ToPercent(total));

            string success = string.Empty;
            if (resultType == ResultType.Success)
            {
                success = LocalisationHelpers.GetCommonResource("Success");
            }

            items.SetupLinksAndCaptions(string.Empty, success);

            return items;
        }

        public static async Task<PaginatedView<PulseTestResultsViewData>> PrimePaginatedGrid(
            this TestCaseBreakdownViewModel value,
            IMediator mediator,
            IConfigurationService configurationService,
            int accountId,
            int reportId,
            string testCaseName,
            string folderPath,
            long timestamp,
            ResultFilter resultFilter,
            SessionFacade session,
            IPaginatedView view)
        {
            var testCaseDetail = await
                mediator.Send(
                    new CustomReportTestCaseDetailPaginatedQuery
                    {
                        AccountId = accountId,
                        ReportId = reportId,
                        PageNo = view.PageNumber,
                        PageSize = view.PageSize,
                        SortAscending = view.SortAscending,
                        SortField = view.SortColumn,
                        BaseTimestamp = new DateTime(timestamp, DateTimeKind.Utc),
                        Timezone = session.UserTimezone,
                        TestCaseName = testCaseName,
                        FolderPath = folderPath,
                        ResultTypes = resultFilter.ToResultTypeList()
                    });

            var result = new PaginatedView<PulseTestResultsViewData>().FromPaginatedResponse(testCaseDetail);

            if (testCaseDetail.IsSuccess && testCaseDetail.Collection != null && testCaseDetail.Collection.Count > 0)
            {
                if (testCaseDetail.Collection.First().MediaType == MediaType.Voice)
                {
                    result.Collection = result.Collection.ToList();

                    result.Collection.ForEach(
                        vr =>
                            {
                                var origRec = testCaseDetail.Collection.First(x => x.TestResultId == vr.TestResultId);
                                vr.RecordingNotAvailable = !configurationService.IsRecordingAvailable(origRec.ActualStart, session.User);
                            });
                }
            }

            return result;
        }

        public static async Task<string> ToCsv(this TestCaseBreakdownViewModel model, IMediator mediator, string timezone)
        {
            var filtersTaskAwaiter =
                mediator.Send(new CustomReportGetFilterSummaryQuery { AccountId = model.SelectedAccountId ?? 0, ReportId = model.ReportId, CountLimit = int.MaxValue });

            var customReportTaskAwaiter = mediator.Send(new CustomReportGetQuery { AccountId = model.SelectedAccountId ?? 0, ReportId = model.ReportId });

            CustomReportEntity customReport;
            FilterSummary filters;

            try
            {
                customReport = await customReportTaskAwaiter;
                filters = await filtersTaskAwaiter;
            }
            catch (ReportNotFoundException ex)
            {
                return ex.Message;
            }

            var dateRange =
                EnumHelper.EnumToList(typeof(CustomDateRange), "CustomDateRange", sort: false)
                    .Where(a => a.Item1.Equals(customReport.DateRange.ToString()))
                    .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 })
                    .FirstOrDefault();

            var builder = new StringBuilder();

            // headers
            builder.AppendFormat("{0}{1}{1}", customReport.Name.ToCsvValue(), Environment.NewLine);

            builder.AppendFormat("{0}: {1}{2}", Resources.Common.TableHeading_AccountName, model.SelectedAccountName.ToCsvValue(), Environment.NewLine);

            builder.AppendFormat(
                "{0}{1}",
                Resources.Common.CustomReport_PDF_Header_Left.FormatWith(DateTime.UtcNow.FormatToLocalDateTimeWithTextualMonth(timezone).ToCsvValue(), timezone),
                Environment.NewLine);

            builder.AppendFormat(
                "{0}: {1}{2}",
                Resources.Common.TableHeading_Media,
                customReport.MediaType == MediaType.Voice ? Resources.Common.MediaType_1 : Resources.Common.MediaType_2,
                Environment.NewLine);

            if (dateRange != null)
            {
                string from, to;
                if (customReport.DateRange != CustomDateRange.Custom)
                {
                    var ranges = customReport.DateRange.ToDateTimeInterval().ConstructRange(timezone, new DateTime(model.ViewLoaded, DateTimeKind.Utc));
                    from = ranges.Item1.FormatToUserLocalDateTime();
                    to = ranges.Item2.FormatToUserLocalDateTime();
                }
                else
                {
                    from = customReport.CustomFrom.FormatToLocalDateTime(timezone);
                    to = customReport.CustomTo.FormatToLocalDateTime(timezone);
                }

                builder.AppendFormat("{0}: {1}{2}", Resources.Common.Period, dateRange.Text.ToCsvValue(), Environment.NewLine);
                builder.AppendFormat("{0}: {1} - {2}{3}", Resources.Common.DateRange, from.ToCsvValue(), to.ToCsvValue(), Environment.NewLine);
            }

            builder.AppendFormat(
                "{0} ({1}){2}{3}",
                Resources.Common.Campaigns,
                filters.CampaignCount,
                filters.CampaignCount > 0 ? ": ," + string.Join(",", filters.Campaigns.ToCsvValues()) : string.Empty,
                Environment.NewLine);

            builder.AppendFormat(
                "{0} ({1}){2}{3}",
                Resources.Common.TestCases,
                filters.TestCasesCount,
                filters.TestCasesCount > 0 ? ": ," + string.Join(",", filters.TestCases.ToCsvValues()) : string.Empty,
                Environment.NewLine);

            builder.AppendFormat(
                "{0} ({1}){2}{3}{3}",
                Resources.Common.FailureReasons,
                filters.FailureReasonsCount,
                filters.FailureReasonsCount > 0 ? ": ," + string.Join(",", filters.FailureReasons) : string.Empty,
                Environment.NewLine);

            builder.AppendFormat(
                "{0}: {1}{2}",
                Resources.Common.TableHeading_TestCase,
                model.FolderPath.CombineFolderWithName(model.TestCaseName).ToCsvValue(),
                Environment.NewLine);

            builder.AppendFormat(
                "{0}: {1}{2}{2}",
                Resources.Common.TableHeading_Result,
                Resources.Common.ResourceManager.GetString(model.ResultFilter.ToString()),
                Environment.NewLine);

            builder.AppendFormat(
                "{0}: {1}{2}{2}",
                customReport.MediaType == MediaType.Voice ? Resources.Common.TotalCallsUppercase : Resources.Common.TotalSessionsUppercase,
                model.TestResults.CollectionSize ?? 0,
                Environment.NewLine);

            // body 2
            builder.AppendFormat(
                "{0},{1},{2},{3}{4}",
                Resources.Common.TableHeading_Date,
                Resources.Common.TableHeading_Duration,
                Resources.Common.TableHeading_DetailedResult,
                Resources.Common.TableHeading_Result,
                Environment.NewLine);

            model.TestResults.Collection.ForEach(
                testCaseResult =>
                    builder.AppendFormat(
                        "{0},{1},{2},{3}{4}",
                        testCaseResult.ActualStart.ToCsvValue(),
                        testCaseResult.Duration == Core.Constants.Common.Dash ? "--" : testCaseResult.Duration,
                        testCaseResult.DetailedResult.ToCsvValue(),
                        testCaseResult.ResultLabel,
                        Environment.NewLine));

            return builder.ToString();
        }
    }
}
