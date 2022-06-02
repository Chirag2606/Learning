namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Autofac;

    using Cyara.Domain.Types.Licensing;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Contracts;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Voice.Scheduler.Client.Contracts;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Areas.Admin.Models.CallRouting;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Extensions.Platform;
    using Cyara.Web.Resources;

    using MediatR;

    [SecuredResource(new[] { StaticRoles.PlatformAdmin }, false)]
    public class PlatformController : BaseController
    {
        private readonly IComponentContext _componentContext;

        public PlatformController(
            IConfigurationService configurationService,
            ILogger logger,
            IMediator mediator,
            IAccountService accountService,
            CyaraWebApi.ComponentsClient componentsClient,
            IVoiceSchedulerClient voiceSchedulerClient,
            IComponentContext componentContext)
        {
            ConfigurationService = configurationService;
            Logger = logger;
            Mediator = mediator;
            AccountService = accountService;
            ComponentsClient = componentsClient;
            _componentContext = componentContext;
            VoiceSchedulerClient = voiceSchedulerClient;
        }

        protected IAccountService AccountService { get; }

        protected IConfigurationService ConfigurationService { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IMediator Mediator { get; }

        protected CyaraWebApi.ComponentsClient ComponentsClient { get; private set; }

        protected IVoiceSchedulerClient VoiceSchedulerClient { get; private set; }

        [HttpGet]
        public async Task<ActionResult> CallRouting(string messageId = null)
        {
            var session = new SessionFacade(HttpContext);

            var model = await new CallRoutingViewModel().Prime(Mediator, AccountService, session, true);
            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId);
            }

            if (!string.IsNullOrEmpty(model.LastStatus?.Validation))
            {
                foreach (string err in model.LastStatus.Validation.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ModelState.AddModelError("model", err);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> CallRouting(CallRoutingViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                bool stored = await model.Store(Mediator, session, Logger);

                if (stored)
                {
                    if (await model.Validate(Mediator, session, Logger))
                    {
                        return RedirectToAction(nameof(CallRouting), new { messageId = "CallRoutingRulesetSuccess" });
                    }

                    foreach (var valMsg in model.ValidationIssues ?? Enumerable.Empty<string>())
                    {
                        ModelState.AddModelError("model", valMsg);
                    }
                }
            }

            model.SystemApproved = false;

            ModelState.Remove(
                new KeyValuePair<string, ModelState>(nameof(CallRoutingViewModel.SystemApproved), ModelState[nameof(CallRoutingViewModel.SystemApproved)]));

            await model.Prime(Mediator, AccountService, session, false);

            return View(model);
        }

        public ActionResult Home()
        {
            // fake action for navigation
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Index(string messageId = null, string tab = null)
        {
            if (!ConfigurationService.IsLicensed(LicensedFeature.PlatformAdminPage))
            {
                Logger.LogDebug(typeof(PlatformController), "Attempt to access platform administration page when feature is disabled");
                return HttpNotFound();
            }

            var session = new SessionFacade(HttpContext);

            var model = await new ConfigurationViewModel().PrimeAsync(ConfigurationService, ComponentsClient, session, true).ConfigureAwait(true);
            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId);
            }

            model.SelectedTab = tab ?? model.SelectedTab;

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Index(ConfigurationViewModel model)
        {
            if (!ConfigurationService.IsLicensed(LicensedFeature.PlatformAdminPage))
            {
                Logger.LogWarn(
                    typeof(PlatformController),
                    "Attempt to access platform administration page when feature is disabled. Returning 404 NotFound result.");
                return HttpNotFound();
            }

            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                model.Update(ConfigurationService, session);
                if (model.Message != null)
                {
                    await model.PrimeAsync(ConfigurationService, ComponentsClient, session, false).ConfigureAwait(true);
                    return View(model);
                }

                return RedirectToAction("Index", "Platform", new { messageId = "PlatformConfiguration_Updated" });
            }

            await model.PrimeAsync(ConfigurationService, ComponentsClient, session, false).ConfigureAwait(true);
            model.Message = new MessageViewData().Prime("PlatformConfigurationFailed", Severity.ValidationError);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> IntHub(IntHubConfigurationViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                await model.SaveSecrets(ConfigurationService, session);

                return RedirectToAction("Index", "Platform", new { messageId = "PlatformSecrets_Updated" });
            }

            return RedirectToAction("Index", "Platform", new { messageId = "PlatformSecrets_Failed", tab = "inthub" });
        }

        [HttpGet]
        public async Task<ActionResult> Maintenance(string messageId, Severity severity = Severity.PageSuccess)
        {
            var model = new MaintenanceViewModel();
            await model.PrimeAsync(ConfigurationService, ComponentsClient, Mediator, true).ConfigureAwait(true);
            if (!string.IsNullOrEmpty(messageId))
            {
                model.Message = new MessageViewData().Prime(messageId, severity);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Maintenance(MaintenanceViewModel model)
        {
            await model.PrimeAsync(ConfigurationService, ComponentsClient, Mediator, false).ConfigureAwait(true);

            if (!ModelState.IsValid)
            {
                if (model.Message == null)
                {
                    model.Message = new MessageViewData().Prime("PlatformConfigurationFailed", Severity.ValidationError);
                }

                return View(model);
            }

            var session = new SessionFacade(HttpContext);
            if (!await model.UpdateAsync(Logger, ConfigurationService, ComponentsClient, Mediator, VoiceSchedulerClient, session).ConfigureAwait(true))
            {
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.PageWarning))
            {
                return RedirectToAction("Maintenance", "Platform", new { messageId = model.PageWarning, severity = Severity.PageWarning });
            }

            return RedirectToAction("Maintenance", "Platform", new { messageId = "PlatformMaintenance_Updated", severity = Severity.PageSuccess });
        }

        [HttpPost]
        public ActionResult Reload()
        {
            if (_componentContext.IsRegistered<IDashboardRemote>())
            {
                var dashboardRemote = _componentContext.Resolve<IDashboardRemote>();
                dashboardRemote.ReloadDashboards();
            }

            return RedirectToAction("Index", "Platform", new { messageId = "Dashboard_Reloaded", tab = "dashboard" });
        }

        [HttpPost]
        public ActionResult GenerateSecret(int routeAccountId)
        {
            return new JsonCamelCaseResult
                   {
                       Data = Guid.NewGuid().ToString("N").ToUpper()
                   };
        }

        [HttpPost]
        public async Task<ActionResult> AbortAll()
        {
            var voiceSchedulerAbortTask = ComponentsClient.AbortComponentWorkAsync(CyaraWebApi.Component.VoiceScheduler);

            var schedulerAbortTask = ComponentsClient.AbortComponentWorkAsync(CyaraWebApi.Component.Scheduler);

            await Task.WhenAll(voiceSchedulerAbortTask, schedulerAbortTask);

            var combined = voiceSchedulerAbortTask.Result.Union(schedulerAbortTask.Result).ToList();

            return new JsonCamelCaseResult
                       {
                           Data = new
                                      {
                                          Results = combined,
                                          Success = combined.Count(x => x.Success == false) == 0,
                                          SuccessMessage = Messages.AbortAll_Success,
                                          FailedMessage = Messages.AbortAll_Failed,
                                          NoResponseMessage = Messages.AbortAll_NoResponse
                                      }
                       };
        }
    }
}