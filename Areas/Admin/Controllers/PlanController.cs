namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Plan;
    using Cyara.Web.Resources;

    [SecuredResource(new[] { StaticRoles.PlatformAdmin, StaticRoles.Admin, StaticRoles.CCMAdmin })]
    public class PlanController : BaseController
    {
        public PlanController(IAccountService accountService, ILogger logger)
        {
            AccountService = accountService;
            Logger = logger;
        }

        public IAccountService AccountService { get; private set; }

        public ILogger Logger { get; private set; }

        public ActionResult Create(MediaType media, PlanType plan)
        {
            // ensure the correct media/plan combination
            var session = new SessionFacade(HttpContext);
            if (media.GetPlanTypes(session.SelectedAccount.Id).All(x => x != plan))
            {
                return new HttpNotFoundResult();
            }

            var response = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();

            var model = PlanModelBuilder.Build(media, plan, response.Value, Logger);

            return View(model);
        }

        [HttpPost]
        [SecuredResource(StaticRoles.PlatformAdmin)]
        [ValidateInput(false)]
        public ActionResult Create(MediaType media, PlanType plan, PlanEditViewModel model)
        {
            return SavePlan(
                media,
                plan,
                model,
                (p, s) => AccountService.PlanCreate(new GenericRequest<Plan>(p) { User = s.User }),
                "Plan_Created",
                Logger);
        }

        [SecuredResource(new[] { StaticRoles.PlatformAdmin, StaticRoles.Admin, StaticRoles.CCMAdmin })]
        public ActionResult View(int id)
        {
            var session = new SessionFacade(HttpContext);

            var planResponse = AccountService.PlanGet(new AccountRequest<int>(id) { AccountId = session.SelectedAccount.Id,  User = session.User });
            planResponse.ExceptionIfError();

            var plan = planResponse.Value;

            var accountResponse = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            accountResponse.ExceptionIfError();

            var account = accountResponse.Value;

            PlanEditViewModel model;

            if (plan == null)
            {
                model = PlanModelBuilder.Default(account);
                model.ReadOnly = true;
                model.Message = new MessageViewData
                {
                    Severity = Severity.PageFatal,
                    Text = new HtmlString(Messages.Plan_NotFound)
                };
                return View(model);
            }

            model = PlanModelBuilder.MapToModelAndPrime(plan.MediaType, plan.PlanType, plan, account, Logger);

            if (model != null)
            {
                model.ReadOnly = true;
                return View(model);
            }

            model = new PlanEditViewModel().Prime(account, default(PlanType));
            model.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(Messages.Plan_NotFound) };
            return View(model);
        }

        [SecuredResource(StaticRoles.PlatformAdmin)]
        public ActionResult Edit(int id)
        {
            var session = new SessionFacade(HttpContext);

            var planResponse = AccountService.PlanGet(new AccountRequest<int>(id) { AccountId = session.SelectedAccount.Id, User = session.User });
            planResponse.ExceptionIfError();

            var plan = planResponse.Value;

            var accountResponse = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            accountResponse.ExceptionIfError();

            var account = accountResponse.Value;

            PlanEditViewModel model;

            if (plan == null)
            {
                model = PlanModelBuilder.Default(account);
                model.Message = new MessageViewData
                {
                    Severity = Severity.PageFatal,
                    Text = new HtmlString(Messages.Plan_NotFound)
                };
                return View(model);
            }

            model = PlanModelBuilder.MapToModelAndPrime(plan.MediaType, plan.PlanType, plan, account, Logger);

            if (model != null)
            {
                return View(model);
            }

            model = new PlanEditViewModel().Prime(account, default(PlanType));
            model.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(Messages.Plan_NotFound) };
            return View(model);
        }

        [HttpPost]
        [SecuredResource(StaticRoles.PlatformAdmin)]
        [ValidateInput(false)]
        public ActionResult Edit(int id, PlanEditViewModel model)
        {
            return SavePlan(
                model.MediaType,
                model.PlanType,
                model,
                (p, s) =>
                {
                    p.PlanId = id;
                    p.AccountId = s.SelectedAccount.Id;
                    return AccountService.PlanUpdate(new GenericRequest<Plan>(p) { User = s.User });
                },
                "Plan_Updated",
                Logger);
        }

        #region Privates

        private ActionResult SavePlan(
            MediaType mediaType,
            PlanType planType,
            PlanEditViewModel model,
            Func<Plan, SessionFacade, BaseResponse> saveAction,
            string message,
            ILogger logger)
        {
            var session = new SessionFacade(HttpContext);

            bool errorSaving = false;

            if (ModelState.IsValid)
            {
                var plan = PlanModelBuilder.MapFromModel(mediaType, planType, model, logger);

                if (plan != null)
                {
                    plan.AccountId = session.SelectedAccount.Id;
                    var response = saveAction(plan, session);
                    response.ExceptionIfError();

                    var configService = DependencyResolver.Current.GetService<IConfigurationService>();

                    if (!response.IsSuccess)
                    {
                        model.Message = new MessageViewData
                        {
                            Severity = Severity.ValidationError,
                            Text = new HtmlString(HttpUtility.HtmlEncode(response.ErrorMessage()))
                        };

                        errorSaving = true;
                    }

                    if (!errorSaving)
                    {
                        return RedirectToAction("edit", "Account", new { messageId = message });
                    }
                }
                else
                {
                    model.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(Messages.Plan_NotFound) };
                }
            }

            var getResponse = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            getResponse.ExceptionIfError();

            var primed = planType.Primer(mediaType, model, getResponse.Value, false);
            if (primed == null)
            {
                Logger.LogErrorWithFormat(GetType(), "Unable to resolve model primer.  PlanType:{0} MediaType:{1}", planType, mediaType);

                    model.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(Messages.Plan_NotFound) };

                return View(model);
            }

            if (primed.Message == null)
            {
                primed.Message = new MessageViewData
                {
                    Severity = Severity.ValidationError,
                    Text = new HtmlString(LocalisationHelpers.GetCommonResource("PageError"))
                };
            }

            return View(primed);
        }

        #endregion
    }
}
