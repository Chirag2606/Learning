namespace Cyara.Web.Portal.Areas.Report.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Shared;
    using Cyara.Domain.Types.TestResult;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.Reports;
    using Cyara.Shared.Types.Storage;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Globalization;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Common.Helpers;
    using Cyara.Web.Messaging.Types.Data;
    using Cyara.Web.Messaging.Types.Exceptions;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Translator;
    using Cyara.Web.Portal.Models;
    using MediatR;

    public static class ExecuteCustomReportViewModelExtensions
    {
        public static async Task<ExecuteCustomReportViewModel> Prime(
            this ExecuteCustomReportViewModel value,
            bool isModelValid,
            int reportId,
            int accountId,
            string timezone,
            IMediator mediator,
            IAccountService accountService,
            IConfigurationService configurationService,
            IPaginatedView paging = null,
            long? baseTimestamp = null,
            bool forPrinting = false,
            bool applyDefaults = false)
        {
            User user = new User().AsService();

            if (applyDefaults || paging == null)
            {
                paging = new PaginatedView
                {
                    PageNumber = 1,
                    PageSize = MvcApplication.Settings.DefaultPageSize,
                    SortAscending = true,
                    SortColumn = Columns.TestCase
                };
            }

            if (applyDefaults)
            {
                value.MediaType = MediaType.Voice;
            }

            value.ForPrinting = forPrinting;
            if (value.ForPrinting)
            {
                paging.PageSize = int.MaxValue;
            }

            value.SelectedAccountId = accountId;

            value.DateRangeTypes =
                EnumHelper.EnumToList(typeof(CustomDateRange), "CustomDateRange", sort: false)
                    .Select(x => new SelectListItem { Value = x.Item1, Text = x.Item2 })
                    .ToList();
            value.DatePattern = DateTimeFormat.GetPickerDateTimeFormat(MvcApplication.Settings);
            value.MediaTypes =
                ListGenerator.CreateVoiceMediaTypeList(configurationService)
                    .Select(mt => new SelectListItem { Value = mt.Item1, Text = mt.Item2 })
                    .ToList();
            value.ViewLoaded = baseTimestamp ?? DateTime.UtcNow.Ticks;
            var maximumStart = new DateTime(value.ViewLoaded, DateTimeKind.Utc);

            // send async request to get current test cases in filter
            var customReportTaskAwaiter =
                mediator.Send(new CustomReportGetQuery { AccountId = accountId, ReportId = reportId });

            var filterTreeTaskAwaiter =
                mediator.Send(new CustomReportFilterTreeQuery
                {
                    AccountId = accountId,
                    ReportId = reportId,
                    User = user
                });

            var testCasePaginatedSummariesTaskAwaiter =
                mediator.Send(
                    new CustomReportTestCaseSummaryPaginatedQuery
                    {
                        AccountId = accountId,
                        ReportId = reportId,
                        PageNo = paging.PageNumber,
                        PageSize = paging.PageSize,
                        SortAscending = paging.SortAscending,
                        SortField = paging.SortColumn,
                        BaseTimestamp = maximumStart,
                        Timezone = timezone
                    });

            var testCaseSummariesTaskAwaiter =
                mediator.Send(
                    new CustomReportTestCaseSummaryQuery
                    {
                        AccountId = accountId,
                        ReportId = reportId,
                        BaseTimestamp = maximumStart,
                        Timezone = timezone
                    });

            Func<ExecuteCustomReportViewModel> primeEmpty = () =>
            {
                value.CampaignsFilter = new ListView<string> { Collection = Enumerable.Empty<string>() };
                value.TestCasesFilter = new ListView<string> { Collection = Enumerable.Empty<string>() };
                value.FailureReasonFilter = new ListView<string> { Collection = Enumerable.Empty<string>() };
                value.TestCases = new PaginatedView<ExecuteCustomReportViewData> { Collection = Enumerable.Empty<ExecuteCustomReportViewData>() };
                value.Message = new MessageViewData().Prime("CustomReport_NotFound", Severity.PageFatal);
                return value;
            };

            CustomReportEntity customReport;
            FilterTree filterTree;
            PaginatedResponse<TestResultReasonSummary> testCasePaginatedSummaries;
            IEnumerable<ResultCallAggregation> testCaseSummaries;

            try
            {
                // extract results here individually, in case of exceptions, prime empty model
                customReport = await customReportTaskAwaiter;
                filterTree = await filterTreeTaskAwaiter;
                testCasePaginatedSummaries = await testCasePaginatedSummariesTaskAwaiter;
                testCaseSummaries = await testCaseSummariesTaskAwaiter;
            }
            catch (ReportNotFoundException)
            {
                return primeEmpty();
            }

            value.ScheduleStatus = customReport.GetStatus();

            if (customReport.NextRun != null)
            {
                value.NextScheduledReport = customReport.NextRun.Value.FormatToLocalDateTime(timezone);
            }
            else
            {
                value.NextScheduledReport = null;
            }

            value.ReportId = customReport.ReportId;
            value.MediaType = customReport.MediaType;
            value.ReportName = customReport.Name;
            value.LastModified = customReport.DateModified.FormatToLocalDateTimeLong(timezone);

            var accountResponse = await accountService.UserGetAsync(new GenericRequest<Guid>(customReport.ModifiedBy) { User = user });
            accountResponse.ExceptionIfError();

            value.ModifiedBy = accountResponse.Value == null ? Resources.Common.NotApplicable : "{0} {1}".FormatWith(accountResponse.Value.Firstname, accountResponse.Value.Lastname);
            value.SelectedAccountName = accountService.AccountGet(new GenericRequest<int>(accountId)).Value.Name;

            value.DateRange = customReport.DateRange;
            if (isModelValid)
            {
                if (customReport.DateRange == CustomDateRange.Custom)
                {
                    if (customReport.CustomFrom != null && customReport.CustomTo != null)
                    {
                        value.From = customReport.CustomFrom.Value.ToLocal(timezone);
                        value.To = customReport.CustomTo.Value.ToLocal(timezone);
                    }
                    else
                    {
                        // custom without values defaulting to last 24 hours
                        var ranges = DateTimeInterval.Last24Hours.ConstructRange(timezone);
                        value.From = ranges.Item1.ToLocal(timezone);
                        value.To = ranges.Item2.ToLocal(timezone);
                    }
                }
                else
                {
                    var ranges = value.DateRange.ToDateTimeInterval().ConstructRange(timezone, maximumStart);
                    value.From = ranges.Item1.ToLocal(timezone);
                    value.To = ranges.Item2.ToLocal(timezone);
                }
            }

            var tree = value.BuildTree(filterTree, value.ForPrinting);

            // add icon-campaign to all campaigns
            var treeViewModel = Mapper.MapList<Node, FilterFolderViewData>(tree).ToArray();
            var rootCampaign = treeViewModel.FirstOrDefault(x => x.EntityType == EntityType.Campaign && x.Key == EntityType.Campaign.GetRootKey());
            if (rootCampaign != null)
            {
                rootCampaign.Children.ForEach(x => x.AddClass = "icon-campaign");
            }

            // serialize
            value.FilterTree = treeViewModel.ToJson(toCamelCase: true);

            var failureReasonsTree = value.BuildFailureReasonsTree(filterTree, value.ForPrinting);
            value.FailureReasonsTree = Mapper.MapList<Node, FilterFolderViewData>(failureReasonsTree).ToJson(toCamelCase: true);

            value.TestCases = new PaginatedView<ExecuteCustomReportViewData>().FromPaginatedResponse(testCasePaginatedSummaries, paging);
            value.TestCases.SortAscending = paging.SortAscending;

            value.TestResultSummaries = testCaseSummaries.Select(Mapper.Map<ResultCallAggregation, CampaignRunTestResultSummaryViewData>);

            return value;
        }

        public static async Task<IList<ReportSectionPopoverViewData>> GetPopoverData(
            this ExecuteCustomReportViewModel value,
            SessionFacade session,
            ResultType resultType,
            IMediator mediator,
            int accountId,
            DateTime maximumStart)
        {
            var query = new CustomReportPopoverDataQuery
            {
                AccountId = accountId,
                ReportId = value.ReportId,
                Result = resultType,
                BaseTimestamp = maximumStart,
                Timezone = session.UserTimezone
            };

            var response = await mediator.Send(query);

            // map to Popover view data
            List<ReportSectionPopoverViewData> items = Mapper.MapList<ResultCategoryCallAggregation, ReportSectionPopoverViewData>(response.Value).ToList();

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

        public static async Task<IList<ReportSectionPopoverViewData>> GetPopoverCategoryData(
            this ExecuteCustomReportViewModel value,
            SessionFacade session,
            TestResultCategory testResultCategory,
            ResultType resultType,
            IMediator mediator,
            int accountId,
            DateTime maximumStart)
        {
            var query = new CustomReportPopoverCategoryDataQuery
                            {
                                AccountId = accountId,
                                ReportId = value.ReportId,
                                TestResultCategory = testResultCategory,
                                Result = resultType,
                                BaseTimestamp = maximumStart,
                                Timezone = session.UserTimezone
                            };

            var response = await mediator.Send(query);

            // map to Popover view data
            List<ReportSectionPopoverViewData> items = Mapper.MapList<DetailedResultCallAggregation, ReportSectionPopoverViewData>(response.Value).ToList();

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

        public static List<Folder> BuildFailureReasonsTree(this ExecuteCustomReportViewModel value, FilterTree filterTree, bool forPrinting = false)
        {
            var root = new Folder
            {
                EntityType = EntityType.TestResultCategory,
                Name = EntityType.TestResultCategory.ToLabel(),
                Key = EntityType.TestResultCategory.GetRootKey(),
                Attributes = new Dictionary<string, object> { { "HideCheckbox", true } },
                Children = filterTree.FailureReasons.ToList()
            };

            var roots = new List<Folder> { root };

            // create a list of all selected failure reasons
            var failureReasons = filterTree.FailureReasons.Where(x => x.Attributes.ContainsKey("Checked") && (bool)x.Attributes["Checked"]).Select(x => x.Name).OrderBy(x => x).ToArray();

            value.FailureReasonFilter = new ListView<string>
            {
                Collection = forPrinting ? failureReasons : failureReasons.Take(3),
                CollectionSize = failureReasons.Length
            };

            return roots;
        }

        public static List<Folder> BuildTree(this ExecuteCustomReportViewModel value, FilterTree filterTree, bool forPrinting = false)
        {
            // create a list of all the test cases that are included in a filter
            var root = new Folder { Children = filterTree.TestCases.ToList() };

            Func<IEnumerable<Node>, IEnumerable<string>, List<string>> getFiles = (files, exclude) => files.OfType<File>()
                    .Where(x => x.Attributes != null && x.Attributes.ContainsKey("Checked") && (bool)x.Attributes["Checked"])
                    .Where(x => !exclude.Contains(x.Key))
                    .Select(x => x.Name)
                    .ToList();

            // create a list of all the filtered campaigns
            var campaigns =
                filterTree.Campaigns.Where(x => x.Attributes.ContainsKey("Checked") && (bool)x.Attributes["Checked"])
                    .ToList();

            // get all the test cases on the root first
            var testCases = getFiles(filterTree.TestCases, Enumerable.Empty<string>());

            // and all the sub folders
            root.WalkTree(
                folder =>
                {
                    var files = getFiles(folder.Children, Enumerable.Empty<string>());
                    if (files.Count > 0)
                    {
                        var path = folder.FlattenPath();
                        files.ForEach(file => testCases.Add(path.CombineFolderWithName(file)));
                    }
                });

            // add the deleted test cases
            if (filterTree.Deleted != null)
            {
                testCases.AddRange(filterTree.Deleted.Where(x => x.EntityType == EntityType.TestCase).Select(x => x.Name));

                campaigns.AddRange(filterTree.Deleted.Where(x => x.EntityType == EntityType.Campaign));
            }

            value.TestCasesFilter = new ListView<string>
            {
                Collection = forPrinting ? testCases.OrderBy(x => x) : testCases.OrderBy(x => x).Take(3),
                CollectionSize = testCases.Count
            };

            value.CampaignsFilter = new ListView<string>
            {
                Collection = forPrinting ? campaigns.Select(x => x.Name) : campaigns.Take(3).Select(x => x.Name),
                CollectionSize = campaigns.Count
            };

            var roots = new List<Folder>();

            // create the Campaign root
            var entityType = EntityType.Campaign;
            var campaignRoot = new Folder
                                   {
                                       EntityType = entityType,
                                       Name = entityType.ToLabel(),
                                       Key = entityType.GetRootKey(),
                                       Attributes = new Dictionary<string, object> { { "HideCheckbox", true } },
                                       Children = filterTree.Campaigns.ToList()
                                   };
            roots.Add(campaignRoot);

            // and the test case root
            entityType = EntityType.TestCase;
            var testCaseRoot = new Folder
                                   {
                                       EntityType = entityType,
                                       Name = entityType.ToLabel(),
                                       Key = entityType.GetRootKey(),
                                       Attributes = new Dictionary<string, object> { { "HideCheckbox", true } },
                                       Children = filterTree.TestCases.ToList()
                                   };
            roots.Add(testCaseRoot);

            // and the deleted one if applicable
            if (filterTree.Deleted != null && filterTree.Deleted.Any())
            {
                filterTree.Deleted.ForEach(x => x.Attributes.Add("Unselectable", true));

                entityType = EntityType.TestCase;
                var deletedRoot = new Folder
                                      {
                                          EntityType = entityType,
                                          Name = Resources.Common.Deleted,
                                          Key = "DELETED",
                                          Attributes = new Dictionary<string, object> { { "HideCheckbox", true } },
                                          Children =
                                              new List<Node>
                                                  {
                                                      new Folder
                                                          {
                                                              Children =
                                                                  filterTree.Deleted.Where(x => x.EntityType == EntityType.Campaign)
                                                                      .ToList(),
                                                              Name = EntityType.Campaign.ToLabel(),
                                                              Key = EntityType.Campaign.GetRootKey(),
                                                              EntityType = EntityType.Campaign,
                                                              Attributes = new Dictionary<string, object> { { "HideCheckbox", true } },
                                                          },
                                                      new Folder
                                                          {
                                                              Children =
                                                                  filterTree.Deleted.Where(x => x.EntityType == EntityType.TestCase)
                                                                      .ToList(),
                                                              Name = EntityType.TestCase.ToLabel(),
                                                              Key = EntityType.TestCase.GetRootKey(),
                                                              EntityType = EntityType.TestCase,
                                                              Attributes = new Dictionary<string, object> { { "HideCheckbox", true } },
                                                          }
                                                  }
                                      };

                value.HasDeletedTestCases = true;

                roots.Add(deletedRoot);
            }

            return roots;
        }
    }
}
