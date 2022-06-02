namespace Cyara.Web.Portal.Areas.Report.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using AutoMapper;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models.Client;
    using Cyara.Web.Portal.Models.SchedulerStatistics;
    using Cyara.Web.Resources;

    using MediatR;

    // Must have reporting access in order to see any of the data, but must also either be a platform user, or have reporting + account feature turned on for some data.
    [SecuredResource(new[] { StaticRoles.Reporting, StaticRoles.PlatformAdmin })]
    public class AccountController : BaseController
    {
        public AccountController(ILogger logger, IMediator mediator, IAuthorisationManager authorisationManager, RestApiFacade webApi, RealTimePortUsageWebSettings realTimePortUsageWebSettings)
        {
            Logger = logger;
            Mediator = mediator;
            AuthorisationManager = authorisationManager;
            WebApi = webApi;
            RealTimePortUsageWebSettings = realTimePortUsageWebSettings;
        }

        public ILogger Logger { get; }

        public IMediator Mediator { get; }

        public IAuthorisationManager AuthorisationManager { get; }

        public RestApiFacade WebApi { get; }

        public RealTimePortUsageWebSettings RealTimePortUsageWebSettings { get; }

        #region Platform Port Usage

        [SecuredResource(new[] { StaticRoles.PlatformUser, StaticRoles.Reporting })]
        public async Task<ActionResult> PortUsage(MediaType id = MediaType.Voice)
        {
            var session = new SessionFacade(HttpContext);

            var model = await new AccountPortUsageViewModel().Prime(RealTimePortUsageWebSettings, Mediator, id, true, session, AuthorisationManager, WebApi, true);
            return View(model);
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.PlatformUser, StaticRoles.Reporting })]
        public async Task<ActionResult> PortUsage(AccountPortUsageViewModel model, MediaType id = MediaType.Voice)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid || model.PeakPorts.DateRange != AccountUsageDateRange.Custom)
            {
                ModelState.Remove($"{nameof(AccountPortUsageViewModel.PeakPorts)}.{nameof(AccountPeakPortsTabData.From)}");
                ModelState.Remove($"{nameof(AccountPortUsageViewModel.PeakPorts)}.{nameof(AccountPeakPortsTabData.To)}");
            }

            model.CurrentTab = "PeakPort";
            model = await model.Prime(RealTimePortUsageWebSettings, Mediator, id, ModelState.IsValid, session, AuthorisationManager, WebApi, false);

            return View(model);
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.PlatformUser, StaticRoles.Reporting })]
        public ActionResult GetDateRange(AccountUsageDateRange id)
        {
            var session = new SessionFacade(HttpContext);

            var calculatedRange = id != AccountUsageDateRange.Custom ?
                                      id.ConstructRange(session.UserTimezone)
                                      : AccountUsageDateRange.Last12Months.ConstructRange(session.UserTimezone);

            return new JsonCamelCaseResult
            {
                Data = new ScriptResult
                {
                    Data = new
                    {
                        from = calculatedRange.Item1.FormatToPickerDate(),
                        to = calculatedRange.Item2.FormatToPickerDate()
                    },
                    IsSuccess = true
                }
            };
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.PlatformUser, StaticRoles.Reporting })]
        public async Task<ActionResult> PortUsageExport(AccountPortUsageViewModel model, MediaType id = MediaType.Voice)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid || model.PeakPorts.DateRange != AccountUsageDateRange.Custom)
            {
                ModelState.Remove($"{nameof(AccountPortUsageViewModel.PeakPorts)}.{nameof(AccountPeakPortsTabData.From)}");
                ModelState.Remove($"{nameof(AccountPortUsageViewModel.PeakPorts)}.{nameof(AccountPeakPortsTabData.To)}");
            }

            model = await model.Prime(RealTimePortUsageWebSettings, Mediator, id, ModelState.IsValid, session, AuthorisationManager, WebApi, false);

            if (ModelState.IsValid)
            {
                return new CsvActionResult
                {
                    Name = Labels.PortUsageExport,
                    Content = model.PeakPorts.ExportPortUsageAsCsv()
                };
            }

            return View("PortUsage", model);
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.PlatformUser, StaticRoles.Reporting })]
        public async Task<ActionResult> RealTimePortUsage(RealTimePortUsageViewModel model)
        {
            var session = new SessionFacade(HttpContext);
            await model.Prime(Mediator, session, true, Logger);

            if (model.Message == null || model.Message.Severity == Severity.PageSuccess)
            {
                RealTimePortUsageResult result = Mapper.Map<RealTimePortUsageViewModel, RealTimePortUsageResult>(model);
                result.Success = true;
                return new JsonCamelCaseResult { Data = result };
            }

            return new JsonCamelCaseResult { Data = new RealTimePortUsageResult { Success = false, Error = model.Message?.Text?.ToString() } };
        }
        #endregion
    }
}