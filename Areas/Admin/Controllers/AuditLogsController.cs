namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Core.IO;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Journal;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Resources;

    using MediatR;

    [SecuredResource(StaticRoles.PlatformAdmin, false)]
    public class AuditLogsController : BaseController
    {
        public AuditLogsController(IJournalService journal, IAccountService accountService, IMediator mediator, IFileSystem fileSystem)
        {
            Journal = journal;
            AccountService = accountService;
            Mediator = mediator;
            FileSystem = fileSystem;
        }

        private IJournalService Journal { get; }

        private IAccountService AccountService { get; }

        private IMediator Mediator { get; }

        private IFileSystem FileSystem { get; }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var session = new SessionFacade(HttpContext);

            var model = await new AuditLogsViewModel().PrimeAsync(Journal, AccountService, Mediator, session.User);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(AuditLogsViewModel model)
        {
            var session = new SessionFacade(HttpContext);
            if (ModelState.IsValid)
            {
                if (model.Action == AuditLogsViewModel.SubmitAction.Export)
                {
                    var filePath = Path.Combine(
                        HttpContext.Server.MapPath(HttpContext.Request.ApplicationPath),
                        MvcApplication.Settings.GetTempDownloadFolder(session.SelectedAccount?.Id ?? 0, HttpContext),
                        $"{Guid.NewGuid()}.csv");

                    JournalLookup lookup = model.ViewModelToJournalLookup();
                    var result =
                        Journal.GetJournalRecordsEx(
                            new PaginatedRequest<JournalLookup>(lookup) { SortAscending = false, CurrentPage = 1, PageSize = int.MaxValue, User = session.User },
                            true);
                    return new CsvActionResult
                           {
                               Name = Labels.AuditLogCSV,
                               CsvFilePath = await model.ToCsvAsync(result, AccountService, session.User, FileSystem, filePath)
                           };
                }

                if (model.Action == AuditLogsViewModel.SubmitAction.Update)
                {
                    model.PageNumber = 1;
                    model.PageSize = MvcApplication.Settings.DefaultPageSize;
                    model.SortAscending = false;
                    model.SortColumn = Columns.Date;
                }
            }

            await model.PrimeAsync(Journal, AccountService, Mediator, session.User, false);
            
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ListJournalRecords(AuditLogsViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            await model.PrimeAsync(Journal, AccountService, Mediator, session.User, false);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<AuditLogViewData>
                {
                    CollectionSize = model.CollectionSize ?? 0,
                    TotalPages = model.TotalPages,
                    List = model.Collection
                }
            };
        }
    }
}
