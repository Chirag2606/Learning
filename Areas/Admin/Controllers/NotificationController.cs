namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Core.Threading;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Script;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Notification;
    using Cyara.Web.Messaging.Types.Command;
    using Cyara.Web.Messaging.Types.Query;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models.Notification;

    using MediatR;

    [SecuredResource(new[] { StaticRoles.PlatformAdmin }, RequiresAccount = false)]
    public class NotificationController : BaseController
    {
        private readonly IMediator _mediator;

        private readonly ILogger _logger;

        private readonly NotificationSettings _notificationSettings;

        public NotificationController(IMediator mediator, ILogger logger, NotificationSettings notificationSettings)
        {
            _mediator = mediator;
            _logger = logger;
            _notificationSettings = notificationSettings;
        }

        public async Task<ActionResult> Index()
        {
            var model = new NotificationViewModel();
            await model.Prime(_mediator, _notificationSettings);

            model.EmailLogs.PopulateEmailLogs(_mediator, Url, applyDefaults: true);
            
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(NotificationViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                await model.SendMessage(_mediator, _logger, session, _notificationSettings);

                // if everything went fine, we want to clear up submitted values in POST collection so we render stuff from model
                if (model.Message != null && model.Message.Severity == Severity.PageSuccess)
                {
                    ModelState.Clear();
                }

                model.EmailLogs.PopulateEmailLogs(_mediator, Url, applyDefaults: true); // re-read emails applying defaults to reset back to first page

                return View("Index", model);
            }

            // not valid, prime without applying defaults to render validation errors
            await model.Prime(_mediator, _notificationSettings, applyDefaults: false);

            model.EmailLogs.PopulateEmailLogs(_mediator, Url, applyDefaults: false);

            return View("Index", model);
        }

        [HttpPost]
        public JsonResult DeleteAttachment(string attachment)
        {
            var session = new SessionFacade(HttpContext);

            var command = CustomerNotificationsDeleteAttachmentCommand.Construct(session);
            command.Attachment = attachment;
            var response = AsyncHelpers.RunSync(()=> _mediator.Send(command));
            
            return new JsonResult() { Data = response.IsSuccess };
        }

        [HttpPost]
        public ActionResult ListEmailLogs(EmailLogsViewData model)
        {
            var emailLogs = model.PopulateEmailLogs(_mediator, Url, applyDefaults: false);
            var v = new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<EmailLogViewData>
                {
                    CollectionSize = emailLogs.CollectionSize ?? 0,
                    TotalPages = emailLogs.TotalPages,
                    List = emailLogs.Logs.AsEnumerable()
                }
            };
            return v;
        }

        [HttpPost]
        public ActionResult DeleteEmailLog(int id)
        {
            var session = new SessionFacade(HttpContext);

            var requestToDeleteEmail = CustomerNotificationsDeleteEmailLogCommand.Construct(session);
            requestToDeleteEmail.LogId = id;

            var emailDeletionResult = AsyncHelpers.RunSync(()=> _mediator.Send(requestToDeleteEmail));
            emailDeletionResult.ExceptionIfError();
            
            return new JsonCamelCaseResult { Data = new { emailDeletionResult.IsSuccess } };
        }

        [HttpPost]
        public ActionResult CreateTemplate(TemplateViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                var command = CustomerNotificationsCreateTemplateCommand.Construct(session);
                command.Template = Mapper.Map<TemplateViewModel, NotificationTemplateEntity>(model);

                var response = AsyncHelpers.RunSync(()=> _mediator.Send(command));
                response.ExceptionIfError();

                return new JsonCamelCaseResult { Data = new AjaxResponse { IsSuccess = response.IsSuccess, Error = response.ErrorMessage() } };
            }

            return new JsonCamelCaseResult { Data = new AjaxResponse { IsSuccess = false, Error = ModelState.FirstErrorMessage() } };
        }

        [HttpPost]
        public async Task<ActionResult> ListTemplates(TemplatesViewData model)
        {
            var templatesQuery = new CustomerNotificationsGetTemplatesQuery().FromPaginatedView(model);
            templatesQuery.SearchTerm = model.SearchTerm;

            var templatesResponse = await _mediator.Send(templatesQuery);
            templatesResponse.ExceptionIfError();

            var viewData = (TemplatesViewData)new TemplatesViewData().FromPaginatedResponseAndQuery(templatesResponse, templatesQuery);

            var v = new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<TemplateViewModel>
                {
                    CollectionSize = viewData.CollectionSize ?? 0,
                    TotalPages = viewData.TotalPages,
                    List = viewData.Collection
                }
            };
            return v;
        }

        [HttpPost]
        public async Task<ActionResult> DeleteTemplate(int id)
        {
            var session = new SessionFacade(HttpContext);

            var requestToDeleteTemplate = CustomerNotificationsDeleteTemplateCommand.Construct(session);
            requestToDeleteTemplate.TemplateId = id;

            var templateDeletionResult = await _mediator.Send(requestToDeleteTemplate);
            templateDeletionResult.ExceptionIfError();

            return new JsonCamelCaseResult { Data = new { templateDeletionResult.IsSuccess } };
        }

        public async Task<ActionResult> Template(int id)
        {
            var form = new AjaxFormResult();

            var model = new TemplateViewModel();
            model = await model.Prime(_mediator, id, applyDefaults: true);

            if (model.TemplateId == 0)
            {
                ModelState.AddModelError(nameof(model.TemplateId), Resources.ValidationMessages.NotificationTemplate_NotFound);
            }
            
            form.AddPartial(string.Empty, ControllerContext, null, model);
            return new JsonCamelCaseResult { Data = form, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public async Task<ActionResult> Template(TemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var session = new SessionFacade(HttpContext);

                var command = CustomerNotificationsUpdateTemplateCommand.Construct(session);
                command.NotificationTemplate = Mapper.Map<TemplateViewModel, NotificationTemplateEntity>(model);
                var response = await _mediator.Send(command);
                response.ExceptionIfError();

                if (response.IsSuccess)
                {
                    return new JsonCamelCaseResult
                    {
                        Data = AjaxFormSubmission.Success(
                                       ControllerContext,
                                       await model.Prime(_mediator, model.TemplateId)),
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                ModelState.AddModelError(nameof(model.TemplateId), response.ErrorMessage());
            }

            var form = AjaxFormSubmission.Failed(
                ControllerContext, 
                await model.Prime(_mediator,  model.TemplateId));
            return new JsonCamelCaseResult { Data = form, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
