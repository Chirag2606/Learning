namespace Cyara.Web.Portal.Areas.Report.Controllers
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Roles;
    using Cyara.Domain.Types.TestResult;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Core.IO;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Reports;
    using Cyara.Shared.Types.ReportSchedule;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Script;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Reports;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Messaging.Types.Command;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    using MediatR;

    using Shared.Business.ReportSchedule;

    [SecuredResource(new[] { StaticRoles.Reporting })]
    public class CustomController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IReportsService _reportsService;
        private readonly ICampaignService _campaignService;
        private readonly IAccountService _accountService;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _logger;

        public CustomController(
            IReportsService reportsService,
            ICampaignService campaignService,
            IAccountService accountService,
            IConfigurationService configurationService,
            IMediator mediator,
            ILogger logger)
        {
            _reportsService = reportsService;
            _campaignService = campaignService;
            _accountService = accountService;
            _configurationService = configurationService;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Fake action for Site Map
        /// </summary>
        /// <returns>Empty result</returns>
        public ActionResult Home()
        {
            return new EmptyResult();
        }

        public async Task<ActionResult> Index(int routeAccountId)
        {
            var model = new CustomReportViewModel();
            await model.Prime(routeAccountId, _mediator, null, true);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> List(int routeAccountId, PaginatedView paging)
        {
            var model = new CustomReportViewModel();
            await model.Prime(routeAccountId, _mediator, paging, false);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<CustomReportViewData>
                {
                    CollectionSize = model.Reports.CollectionSize ?? 0,
                    TotalPages = model.Reports.TotalPages,
                    List = model.Reports.Collection
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int routeAccountId, int id)
        {
            var session = new SessionFacade(HttpContext);

            var command = CustomReportDeleteCommand.Construct(session);
            command.ReportId = id;

            var response = await _mediator.Send(command);

            return new JsonCamelCaseResult
                       {
                           Data = new ScriptResult
                                   {
                                       IsSuccess = response.IsSuccess,
                                       Message =
                                           response.IsSuccess
                                               ? Messages.CustomReport_Deleted
                                               : response.ErrorResult.GetDisplayMessage()
                                   }
                       };
        }

        public async Task<ActionResult> Create(int routeAccountId)
        {
            var session = new SessionFacade(HttpContext);

            var command = CustomReportCreateCommand.Construct(session);
            command.CustomReport = new CustomReportEntity { MediaType = MediaType.Voice };

            var response = await _mediator.Send(command);
            response.ExceptionIfError();

            return RedirectToAction("Edit", new { id = response.Value });
        }

        public async Task<ActionResult> Edit(int routeAccountId, int id)
        {
            var session = new SessionFacade(HttpContext);

            var model = await new ExecuteCustomReportViewModel().Prime(true, id, routeAccountId, session.UserTimezone, _mediator, _accountService, _configurationService, applyDefaults: true);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(int routeAccountId, int id, ExecuteCustomReportViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                await SaveFilters(routeAccountId, id, model, session);
                return View(model);
            }

            return View(await model.Prime(false, id, routeAccountId, session.UserTimezone, _mediator, _accountService, _configurationService, model.TestCases));
        }

        [HttpPost]
        public async Task<ActionResult> SummaryList(int routeAccountId, int id, long timestamp, PaginatedView paging)
        {
            var session = new SessionFacade(HttpContext);

            var testCaseSummaryRequest = new CustomReportTestCaseSummaryPaginatedQuery
            {
                AccountId = routeAccountId,
                ReportId = id,
                PageNo = paging.PageNumber,
                PageSize = paging.PageSize,
                SortAscending = paging.SortAscending,
                SortField = paging.SortColumn,
                BaseTimestamp = new DateTime(timestamp, DateTimeKind.Utc),
                Timezone = session.UserTimezone
            };

            var paginatedResponse = await _mediator.Send(testCaseSummaryRequest);

            var paginatedView = new PaginatedView<ExecuteCustomReportViewData>().FromPaginatedResponse(paginatedResponse);

            // set the report id here used for links in grid
            paginatedView.Collection = paginatedView.Collection.ToArray();
            paginatedView.Collection.ForEach(x => x.Id = id);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<ExecuteCustomReportViewData>
                {
                    CollectionSize = paginatedView.CollectionSize ?? 0,
                    TotalPages = paginatedView.TotalPages,
                    List = paginatedView.Collection
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> UpdateReportName(int routeAccountId, int id, EditCustomReportNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState[ReflectOn<EditCustomReportNameViewModel>.GetProperty(p => p.ReportName).Name].Errors;
                if (errors != null && errors.Count > 0)
                {
                    return new JsonCamelCaseResult
                    {
                        Data = new ScriptResult
                        {
                            IsSuccess = false,
                            Message = errors.First().ErrorMessage
                        }
                    };
                }
            }

            var session = new SessionFacade(HttpContext);

            var command = CustomReportUpdateNameCommand.Construct(session);
            command.ReportId = id;
            command.Name = model.ReportName;

            var response = await _mediator.Send(command);

            return new JsonCamelCaseResult
            {
                Data = new ScriptResult
                {
                    IsSuccess = response.IsSuccess,
                    Message = response.ErrorResult.GetDisplayMessage()
                }
            };
        }

        [HttpPost]
        public ActionResult GetDateRange(int routeAccountId, CustomDateRange dateRange)
        {
            var session = new SessionFacade(HttpContext);

            var calculatedRange = dateRange != CustomDateRange.Custom ?
                dateRange.ToDateTimeInterval().ConstructRange(session.UserTimezone)
                : CustomDateRange.Last24Hours.ToDateTimeInterval().ConstructRange(session.UserTimezone);

            return new JsonCamelCaseResult
            {
                Data = new ScriptResult
                {
                    Data = new
                    {
                        from = calculatedRange.Item1.FormatToUserLocalDateTime(),
                        to = calculatedRange.Item2.FormatToUserLocalDateTime(),
                        range = dateRange.ToString()
                    },
                    IsSuccess = true
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> PopoverData(int routeAccountId, int id, string resultType, long timestamp)
        {
            var session = new SessionFacade(HttpContext);

            var requestedResultType = (ResultType)Enum.Parse(typeof(ResultType), resultType.RemoveWhitespace());

            var response = await new ExecuteCustomReportViewModel { ReportId = id }.GetPopoverData(session, requestedResultType, _mediator, routeAccountId, new DateTime(timestamp, DateTimeKind.Utc));

            // add the links and success localized text
            string popoverUrl = Url.ActionForEnumerable("ResultsSummaryDetails", routeValues: new { id, resultTypeId = (int)requestedResultType, timeStamp = timestamp });

            string success = string.Empty;
            if (requestedResultType == ResultType.Success)
            {
                success = LocalisationHelpers.GetCommonResource("Success");
            }

            response.SetupLinksAndWriteSuccessLabel(popoverUrl, success);

            return new JsonCamelCaseResult
            {
                Data = response
            };
        }

        [HttpPost]
        public async Task<ActionResult> PopoverDataForCategory(int routeAccountId, int id, ResultType resultType, string testResultCategory, long timestamp)
        {
            var session = new SessionFacade(HttpContext);

            TestResultCategory trc = testResultCategory.ToTestResultCategory();

            var response = await new ExecuteCustomReportViewModel { ReportId = id }.GetPopoverCategoryData(session, trc, resultType, _mediator, routeAccountId, new DateTime(timestamp, DateTimeKind.Utc));

            return new JsonCamelCaseResult
                       {
                           Data = response
                       };
        }

        #region ResultSummaryDetails

        [HttpGet]
        public async Task<ActionResult> SuccessfulCalls(int routeAccountId, int id)
        {
            return await DonutBreakdown(routeAccountId, id, null, ResultType.Success);
        }

        [HttpPost]
        public async Task<ActionResult> SuccessfulCalls(int routeAccountId, int id, ExecuteCustomReportViewModel model)
        {
            return await DonutBreakdown(routeAccountId, id, model, ResultType.Success);
        }

        [HttpGet]
        public async Task<ActionResult> FailedCalls(int routeAccountId, int id)
        {
            return await DonutBreakdown(routeAccountId, id, null, ResultType.Failed);
        }

        [HttpPost]
        public async Task<ActionResult> FailedCalls(int routeAccountId, int id, ExecuteCustomReportViewModel model)
        {
            return await DonutBreakdown(routeAccountId, id, model, ResultType.Failed);
        }

        [HttpGet]
        public async Task<ActionResult> SatisfactoryCalls(int routeAccountId, int id)
        {
            return await DonutBreakdown(routeAccountId, id, null, ResultType.Satisfactory);
        }

        [HttpPost]
        public async Task<ActionResult> SatisfactoryCalls(int routeAccountId, int id, ExecuteCustomReportViewModel model)
        {
            return await DonutBreakdown(routeAccountId, id, model, ResultType.Satisfactory);
        }

        [HttpGet]
        public async Task<ActionResult> AbortedCalls(int routeAccountId, int id)
        {
            return await DonutBreakdown(routeAccountId, id, null, ResultType.Aborted);
        }

        [HttpPost]
        public async Task<ActionResult> AbortedCalls(int routeAccountId, int id, ExecuteCustomReportViewModel model)
        {
            return await DonutBreakdown(routeAccountId, id, model, ResultType.Aborted);
        }

        [HttpGet]
        public async Task<ActionResult> InternalErrorCalls(int routeAccountId, int id)
        {
            return await DonutBreakdown(routeAccountId, id, null, ResultType.InternalError);
        }

        [HttpPost]
        public async Task<ActionResult> InternalErrorCalls(int routeAccountId, int id, ExecuteCustomReportViewModel model)
        {
            return await DonutBreakdown(routeAccountId, id, model, ResultType.InternalError);
        }

        public ActionResult ListCampaignRunTestResultsByDetailedResult(MediaType mediaType, PaginatedCampaignRunTestCasesByDetailedResultViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                var response =
                    _reportsService.CampaignRunTestCaseDetailedResultGetRange(
                        new PaginatedRequest<ResultsSummaryDetailsRequest>(new ResultsSummaryDetailsRequest
                        {
                            RunIds = model.RunIds,
                            DetailedResult = model.DetailedResult,
                            ResultType = model.ResultType,
                            TestCaseName = model.TestCaseName,
                            FolderPath = FileUtils.GetPathWithoutLeadingSlash(model.FolderPath),
                            MediaType = mediaType
                        })
                        {
                            AccountId = session.SelectedAccount.Id,
                            PageSize = model.PageSize,
                            CurrentPage = model.PageNumber,
                            SortAscending = model.SortAscending,
                            SortField = model.SortColumn,
                            User = session.User
                        });
                response.ExceptionIfError();

                var paginatedView = new PaginatedView<CampaignRunTestResultViewData>().FromPaginatedResponse(response);

                if (mediaType == MediaType.Voice)
                {
                    paginatedView.Collection = paginatedView.Collection.ToList();
                    paginatedView.Collection.ForEach(
                        vr =>
                            {
                                var origRec = response.Collection.First(x => x.TestResultId == vr.TestResultId);
                                vr.RecordingNotAvailable = !_configurationService.IsRecordingAvailable(origRec.ActualStartDate, session.User);
                            });
                }

                return new JsonCamelCaseResult
                {
                    Data = new AjaxPaginatedResponse<CampaignRunTestResultViewData>
                    {
                        CollectionSize = response.CollectionSize,
                        TotalPages = response.TotalPages,
                        List = paginatedView.Collection
                    }
                };
            }

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<CampaignRunTestResultViewData>
                {
                    Error = ModelState.FirstErrorMessage()
                }
            };
        }

        public async Task<ActionResult> ListCampaignRunTestResultDetails(int routeAccountId, int id, long timestamp, PaginatedCampaignRunTestCasesByDetailedResultViewModel model)
        {
            if (ModelState.IsValid)
            {
                var session = new SessionFacade(HttpContext);
                var ret = await new PaginatedView<ResultSummaryDetailsTestCaseViewData>().PrimePaginatedGrid(_mediator, routeAccountId, id, timestamp, session.UserTimezone, model.ResultType, model);

                return new JsonCamelCaseResult
                {
                    Data = new AjaxPaginatedResponse<ResultSummaryDetailsTestCaseViewData>
                    {
                        CollectionSize = ret.CollectionSize ?? 0,
                        TotalPages = ret.TotalPages,
                        List = ret.Collection
                    }
                };
            }

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<CampaignRunTestResultViewData>
                {
                    Error = ModelState.FirstErrorMessage()
                }
            };
        }

        public ActionResult ResultDetails(int? testResultId, int? callNo, int? testCaseId, Guid? ticket, MediaType? mediaType)
        {
            // reuse view from Report controller
            return RedirectToAction("ResultDetails", "Report", new { area = string.Empty, testResultId, callNo, testCaseId, ticket, mediaType });
        }

        #endregion ResultSummaryDetails

        public async Task<ActionResult> Schedule(int routeAccountId, int id)
        {
            var form = new AjaxFormResult();

            var model = new ScheduleViewModel();
            model = await model.PrimeEdit(_mediator, routeAccountId, id);

            form.AddPartial(string.Empty, ControllerContext, null, model);
            return new JsonCamelCaseResult { Data = form, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult HasUnavailableSchedule(int routeAccountId, ScheduleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var session = new SessionFacade(HttpContext);

                var schedule = Mapper.Map<ScheduleViewModel, Schedule>(model);

                return new JsonCamelCaseResult { Data = ScriptResponse.Success(schedule.HasUnavailableRuns(session.UserTimezone, _logger)) };
            }

            return new JsonCamelCaseResult { Data = ScriptResponse.Success(false) };
        }

        [HttpPost]
        public async Task<ActionResult> Schedule(int routeAccountId, ScheduleViewModel model)
        {
            if (!model.IsReloading && !model.IsRefreshing)
            {
                if (ModelState.IsValid)
                {
                    var session = new SessionFacade(HttpContext);

                    var schedule = Mapper.Map<ScheduleViewModel, Schedule>(model);

                    var command = CustomReportUpdateScheduleCommand.Construct(session);
                    command.ReportId = model.ReportId;
                    command.Schedule = schedule.ToJson();
                    command.NextRun = schedule.GetNextRun(session.UserTimezone, default(DateTime), _logger);

                    var result = await _mediator.Send(command);
                    if (result.IsSuccess)
                    {
                        return new JsonCamelCaseResult
                                   {
                                       Data = new ScriptResult
                                                  {
                                                      IsSuccess = true,
                                                      Data = new NextRunViewData
                                                      {
                                                            NextRun = command.NextRun?.FormatToUserLocalDateTime(),
                                                            IsActive = schedule.Active
                                                      }
                                                  },
                                       JsonRequestBehavior = JsonRequestBehavior.AllowGet
                                   };
                    }

                    model.Message = new MessageViewData { Severity = Severity.ValidationError, Text = new HtmlString(result.ErrorResult.GetDisplayMessage()) };
                }
            }

            if (model.IsReloading)
            {
                foreach (var key in ModelState.Keys)
                {
                    ModelState[key].Errors.Clear();
                }

                // we don't want to use POSTed value but rather use one that comes from Prime methods
                var scheduleName = ReflectOn<ScheduleMonthlyViewData>.GetProperty(m => m.ScheduleUnavailable).Name;
                var scheduleKey = ModelState.Keys.FirstOrDefault(x => x.Contains(scheduleName));
                if (scheduleKey != null)
                {
                    ModelState.Remove(scheduleKey);
                }
            }

            if (!model.IsRefreshing)
            {
                var form = AjaxFormSubmission.Failed(ControllerContext, model.Prime(routeAccountId, false));
                return new JsonCamelCaseResult { Data = form, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            if (!model.Period.Validate(new ValidationContext(model.Period)).Any())
            {
                return new JsonCamelCaseResult { Data = new { Summary = model.Period.ToString(), CounterId = model.RefreshCounterId, isSuccess = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            return new JsonCamelCaseResult { Data = new { Summary = string.Empty, CounterId = model.RefreshCounterId, isSuccess = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task<ActionResult> Export(int routeAccountId, int id, CustomReportExportType type, ResultType? resultType, long? timestamp, bool? includeTestRuns)
        {
            var session = new SessionFacade(HttpContext);

            switch (type)
            {
                case CustomReportExportType.Xls:
                    if (resultType.HasValue)
                    {
                        var request = new CustomDetailsReportGetExportCsvQuery
                        {
                            AccountId = routeAccountId,
                            ReportId = id,
                            Timezone = session.UserTimezone,
                            ResultType = resultType.Value,
                            BaseTimestamp = new DateTime(timestamp ?? DateTime.UtcNow.Ticks, DateTimeKind.Utc),
                            TestRunsOnly = false
                        };

                        var response = _mediator.Send(request);
                        return new CsvActionResult
                        {
                            Name = Labels.CustomDetailsPulseReportExportCsv,
                            Content = await response
                        };
                    }
                    else
                    {
                        var request = new CustomReportGetExportCsvQuery
                        {
                            AccountId = routeAccountId,
                            ReportId = id,
                            Timezone = session.UserTimezone,
                            BaseTimestamp = new DateTime(timestamp ?? DateTime.UtcNow.Ticks, DateTimeKind.Utc),
                        };

                        var response = _mediator.Send(request);
                        return new CsvActionResult
                        {
                            Name = Labels.CustomPulseReportExportCsv,
                            Content = await response
                        };
                    }

                case CustomReportExportType.Pdf:
                    if (resultType.HasValue)
                    {
                        var pdfResult =
                            _mediator.Send(
                                new CustomDetailsReportGetExportPdfQuery()
                                    {
                                        Timezone = session.UserTimezone,
                                        AccountId = routeAccountId,
                                        ReportId = id,
                                        ResultType = resultType.Value,
                                        RelativeViewUrl = "~/Areas/Report/Views/Custom/EmailDetails.cshtml",
                                        BaseTimestamp = new DateTime(timestamp ?? DateTime.UtcNow.Ticks, DateTimeKind.Utc),
                                        IncludeTestRuns = false
                                });
                        return new PdfActionResult
                        {
                            Name = Labels.CustomDetailsPulseReportExportPdf,
                            Content = await pdfResult
                        };
                    }
                    else
                    {
                        var pdfResult =
                            _mediator.Send(
                                new CustomReportGetExportPdfQuery
                                    {
                                        Timezone = session.UserTimezone,
                                        AccountId = routeAccountId,
                                        ReportId = id,
                                        RelativeViewUrl = "~/Areas/Report/Views/Custom/EmailSummary.cshtml",
                                        BaseTimestamp = new DateTime(timestamp ?? DateTime.UtcNow.Ticks, DateTimeKind.Utc),
                                });
                          return new PdfActionResult
                        {
                            Name = Labels.CustomPulseReportExportPdf,
                            Content = await pdfResult
                        };
                    }

                default:
                    throw new Exception("Unrecognized Export option: {0}".FormatWith(type));
            }
        }

        public async Task<ActionResult> ExportTestCaseBreakdown(int routeAccountId, int id, CustomReportExportType type, string name, string path, long? timestamp, ResultFilter? filter)
        {
            var session = new SessionFacade(HttpContext);
            name = HttpUtility.UrlDecode(name);
            var model = await new TestCaseBreakdownViewModel { ResultFilter = filter ?? ResultFilter.All }.Prime(
                            _mediator,
                            _configurationService,
                            routeAccountId,
                            id,
                            name,
                            path,
                            timestamp,
                            session.UserTimezone,
                            new PaginatedView { PageNumber = 1, PageSize = int.MaxValue, SortAscending = false, SortColumn = Columns.StartDate },
                            session);
            model.ResultFilter = filter ?? ResultFilter.All;
            model.SelectedAccountName = _accountService.AccountGet(new GenericRequest<int>(routeAccountId)).Value.Name;

            switch (type)
            {
                case CustomReportExportType.Xls:
                    return new CsvActionResult
                    {
                        Name = Labels.CustomTestCaseBreakdownPulseReportExportCsv,
                        Content = await model.ToCsv(_mediator, session.UserTimezone)
                    };
                case CustomReportExportType.Pdf:
                    var pdfResult =
                      await _mediator.Send(
                            new ExportPdfQuery
                            {
                                Timezone = session.UserTimezone,
                                RelativeViewUrl = "~/Areas/Report/Views/Custom/EmailTestCaseBreakdown.cshtml",
                                Model = model,
                                SelectedAccountName = model.SelectedAccountName,
                            });

                    return new PdfActionResult
                        {
                            Name = Labels.CustomTestCaseBreakdownPulseReportExportPdf,
                            Content = pdfResult
                        };
                default:
                    throw new Exception("Unrecognized Export option: {0}".FormatWith(type));
            }
        }

        public async Task<ActionResult> TestCaseBreakdown(int routeAccountId, int id, string name, string path)
        {
            var session = new SessionFacade(HttpContext);
            name = HttpUtility.UrlDecode(name);
            var model =
                await
                new TestCaseBreakdownViewModel().Prime(
                    _mediator,
                    _configurationService,
                    routeAccountId,
                    id,
                    name,
                    path,
                    null,
                    session.UserTimezone,
                    null,
                    session);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> TestCaseBreakdown(int routeAccountId, int id, string name, string path, TestCaseBreakdownViewModel model)
        {
            var session = new SessionFacade(HttpContext);
            name = HttpUtility.UrlDecode(name);
            model = await model.Prime(_mediator, _configurationService, routeAccountId, id, name, path, null, session.UserTimezone, null, session);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> TestCaseBreakdownPopoverData(int routeAccountId, int id, string name, string path, string resultType, long timestamp)
        {
            var session = new SessionFacade(HttpContext);
            name = HttpUtility.UrlDecode(name);
            var requestedResultType = (ResultType)Enum.Parse(typeof(ResultType), resultType.RemoveWhitespace());

            var response = await new TestCaseBreakdownViewModel { ReportId = id }.GetPopoverData(
                session,
                name,
                path,
                requestedResultType,
                _mediator,
                routeAccountId,
                new DateTime(timestamp, DateTimeKind.Utc));

            var success = string.Empty;
            if (requestedResultType == ResultType.Success)
            {
                success = LocalisationHelpers.GetCommonResource("Success");
            }

            response.SetupLinksAndCaptions(string.Empty, success);

            return new JsonCamelCaseResult
            {
                Data = response
            };
        }

        [HttpPost]
        public async Task<ActionResult> TestCaseBreakdownList(int routeAccountId, int id, TestCaseBreakdownViewModel model, PaginatedView view)
        {
            var session = new SessionFacade(HttpContext);

            var ret = await new TestCaseBreakdownViewModel { ReportId = id }.PrimePaginatedGrid(_mediator, _configurationService, routeAccountId, id, model.TestCaseName, model.FolderPath, model.ViewLoaded, model.ResultFilter, session, view);

            return new JsonCamelCaseResult
                       {
                           Data = new AjaxPaginatedResponse<PulseTestResultsViewData>
                                      {
                                          CollectionSize = ret.CollectionSize ?? 0,
                                          TotalPages = ret.TotalPages,
                                          List = ret.Collection
                                      }
                       };
        }

        private async Task<ActionResult> DonutBreakdown(int routeAccountId, int id, ExecuteCustomReportViewModel model, ResultType result)
        {
            var session = new SessionFacade(HttpContext);

            if (model != null)
            {
                if (ModelState.IsValid)
                {
                    await SaveFilters(routeAccountId, id, model, session);
                }
            }
            else
            {
                model = await new ExecuteCustomReportViewModel().Prime(true, id, routeAccountId, session.UserTimezone, _mediator, _accountService, _configurationService, applyDefaults: true);
            }

            CustomReportSummaryDetailsViewModel viewModel =
                await new CustomReportSummaryDetailsViewModel { Filter = model }.ApplyDefaults(result)
                    .Prime(
                        _reportsService,
                        _campaignService,
                        _accountService,
                        _mediator,
                        id,
                        routeAccountId,
                        model.ViewLoaded,
                        new SessionFacade(HttpContext).UserTimezone,
                        result);

            return View("ResultsSummaryDetails", viewModel);
        }

        private async Task SaveFilters(int routeAccountId, int id, ExecuteCustomReportViewModel model, SessionFacade session)
        {
            // reset the timestamp
            model.ViewLoaded = DateTime.UtcNow.Ticks;

            var command = CustomReportUpdateCommand.Construct(session);
            command.ReportId = id;
            command.MediaType = model.MediaType;
            command.DateRange = model.DateRange;
            command.CustomFrom = model.DateRange == CustomDateRange.Custom
                ? model.From.FromLocal(session.UserTimezone)
                : (DateTime?)null;
            command.CustomTo = model.DateRange == CustomDateRange.Custom
                ? model.To.FromLocal(session.UserTimezone)
                : (DateTime?)null;
            command.TestCaseFilter =
                (model.TestCaseFilterSelection ?? string.Empty).Split(' ')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
            command.CampaignFilter =
                (model.CampaignFilterSelection ?? string.Empty).Split(' ')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
            command.FailureReasonFilter = (model.FailureReasonSelection ?? string.Empty).Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => Convert.ToInt32(x)).ToArray();

            var response = await _mediator.Send(command);
            response.ExceptionIfError();

            await model.Prime(ModelState.IsValid, id, routeAccountId, session.UserTimezone, _mediator, _accountService, _configurationService, model.TestCases, model.ViewLoaded);

            if (!response.IsSuccess)
            {
                model.Message = new MessageViewData
                {
                    Text = new HtmlString(response.ErrorResult.GetDisplayMessage()),
                    Severity = Severity.PageFatal
                };
            }
        }
    }
}
