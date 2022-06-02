namespace Cyara.Web.Portal.Areas.Report.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Portal.Models.SchedulerStatistics;
    using Cyara.Web.Resources;

    using MediatR;

    [SecuredResource(StaticRoles.PlatformAdmin, false)]
    public class PlatformController : BaseController
    {
        public PlatformController(ILogger logger, IMediator mediator, IAuthorisationManager authorisationManager, RealTimePortUsageWebSettings realTimePortUsageWebSettings)
        {
            Logger = logger;
            Mediator = mediator;
            AuthorisationManager = authorisationManager;
            RealTimePortUsageWebSettings = realTimePortUsageWebSettings;
        }

        public ILogger Logger { get; }

        public IMediator Mediator { get; }

        public IAuthorisationManager AuthorisationManager { get; }

        public RealTimePortUsageWebSettings RealTimePortUsageWebSettings { get; }

        #region Platform Port Usage

        public async Task<ActionResult> PortUsage(MediaType id = MediaType.Voice)
        {
            var session = new SessionFacade(HttpContext);

            var model = await new PlatformPortUsageViewModel().Prime(RealTimePortUsageWebSettings, Mediator, id, true, session, AuthorisationManager, true);
            if (model.SelectedTab != "cruncherLite")
            {
                model.SelectedTab = "platform";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PortUsage(PlatformPortUsageViewModel model, MediaType id = MediaType.Voice)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid || model.DateRange != DateRange.Custom)
            {
                ModelState.Remove(nameof(PlatformPortUsageViewModel.From));
                ModelState.Remove(nameof(PlatformPortUsageViewModel.To));
            }

            model = await model.Prime(RealTimePortUsageWebSettings, Mediator, id, ModelState.IsValid, session, AuthorisationManager, false);
            if (model.SelectedTab != "cruncherLite")
            {
                model.SelectedTab = "platform";
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult GetDateRange(DateRange id)
        {
            var calculatedRange = id != DateRange.Custom ?
                                      id.ToDateTimeInterval().ConstructRange("UTC")
                                      : DateRange.Last24Hours.ToDateTimeInterval().ConstructRange("UTC");

            return new JsonCamelCaseResult
            {
                Data = new ScriptResult
                {
                    Data = new
                    {
                        from = calculatedRange.Item1.FormatToPickerDateTime(),
                        to = calculatedRange.Item2.FormatToPickerDateTime()
                    },
                    IsSuccess = true
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> PortUsageExport(PlatformPortUsageViewModel model, MediaType id = MediaType.Voice)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid || model.DateRange != DateRange.Custom)
            {
                ModelState.Remove(nameof(PlatformPortUsageViewModel.From));
                ModelState.Remove(nameof(PlatformPortUsageViewModel.To));
            }

            model = await model.Prime(RealTimePortUsageWebSettings, Mediator, id, ModelState.IsValid, session, AuthorisationManager, false);
            if (model.SelectedTab != "cruncherLite")
            {
                model.SelectedTab = "platform";
            }

            if (ModelState.IsValid)
            {
                return new CsvActionResult
                {
                    Name = model.MediaType == MediaType.Chat ? Labels.SessionUsageExport : Labels.PortUsageExport,
                    Content = model.ExportPlatformPortUsageAsCsv()
                };
            }

            return View("PortUsage", model);
        }

        public async Task<ActionResult> GetPortUsage(PlatformPortUsageViewModel model, string tabName, DateRange submittedDateRange, MediaType id = MediaType.Voice)
        {
            ModelState.Remove(nameof(PlatformPortUsageViewModel.From));
            ModelState.Remove(nameof(PlatformPortUsageViewModel.To));

            if (ModelState.IsValid)
            {
                model.DateRange = submittedDateRange;

                var tab = await new PlatformPortUsageDetailViewData().Prime(
                              Mediator,
                              id,
                              model.From,
                              model.To,
                              model.DateRange,
                              tabName);

                if (tab != null)
                {
                    return new JsonCamelCaseResult { Data = GenericResponse<PlatformPortUsageDetailViewData>.Succeeds(tab) };
                }
            }

            return new JsonCamelCaseResult { Data = GenericResponse<PlatformPortUsageDetailViewData>.Fails(Resources.Service.Messages.ERR_UNKNOWN) };
        }

        #endregion
    }
}
