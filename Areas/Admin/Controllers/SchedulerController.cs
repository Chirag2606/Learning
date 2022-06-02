namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models;

    [SecuredResource(StaticRoles.PlatformAdmin, false)]
    public class SchedulerController : BaseController
    {
        public SchedulerController(ISchedulerService schedulerService, CyaraWebApi.ComponentsClient componentsClient, IConfigurationService configurationService, ILogger logger)
        {
            SchedulerService = schedulerService;
            Logger = logger;
            ComponentsClient = componentsClient;
            ConfigurationService = configurationService;
        }

        public ISchedulerService SchedulerService { get; private set; }

        public ILogger Logger { get; private set; }

        public CyaraWebApi.ComponentsClient ComponentsClient { get; set; }

        public IConfigurationService ConfigurationService { get; set; }

        public async Task<ActionResult> Index()
        {
            var session = new SessionFacade(HttpContext);

            var model = await new SchedulerStatusViewModel().PrimeAsync(SchedulerService, ComponentsClient, ConfigurationService, Logger, session, Url, true).ConfigureAwait(true);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SchedulerStatus()
        {
            var session = new SessionFacade(HttpContext);

            var model = await new SchedulerStatusViewModel().PrimeAsync(SchedulerService, ComponentsClient, ConfigurationService, Logger, session, Url, true).ConfigureAwait(true);

            return new JsonCamelCaseResult
                {
                    Data = model
                };
        }
    }
}
