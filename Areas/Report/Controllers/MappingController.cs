namespace Cyara.Web.Portal.Areas.Report.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Messaging.Types.Command;
    using Cyara.Web.Portal.Areas.Report.Extensions;
    using Cyara.Web.Portal.Areas.Report.Models;
    using Cyara.Web.Portal.Core.Extensions;

    using MediatR;

    [SecuredResource(new[] { StaticRoles.Reporting })]
    public class MappingController : BaseController
    {
        private readonly IMediator _mediator;

        public MappingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index(int routeAccountId)
        {
            return View(await new SeverityMappingViewModel().Prime(routeAccountId, _mediator, applyDefaults: true));
        }

        [HttpPost]
        public async Task<ActionResult> Index(int routeAccountId, SeverityMappingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var session = new SessionFacade(HttpContext);

                var command = CustomSeveritySaveCommand.Construct(session);
                command.CustomSeverities = model.DetailedResults.Select(AutoMapper.Mapper.Map<CustomSeverityEntity>);

                var response = await _mediator.Send(command);
                response.ExceptionIfError();

                if (response.IsSuccess)
                {
                    model.Message = new MessageViewData().Prime("CustomSeverity_Saved");
                }
                else
                {
                    model.Message = new MessageViewData { Severity = Severity.PageFatal, Text = new HtmlString(response.ErrorResult.GetDisplayMessage()) };
                }
            }

            return View(await model.Prime(routeAccountId, _mediator));
        }
    }
}