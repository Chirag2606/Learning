namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_5
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Messaging.Types.Command;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    
    using MediatR;

    [LogApiRequest]
    [V2ControllerConfiguration]
    [Authorize]
    [HandleApiException]
    public class UserController : BaseApiController
    {
        private readonly IMediator _mediator;

        private readonly ILogger _logger;

        public UserController(ILogger logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost, ActionName("apitoken")]
        public async Task<HttpResponseMessage> ApiToken()
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            var command = ApiTokenCreateCommand.Construct(session, Request.GetCorrelationId().ToString());

            var response = await _mediator.Send(command);
            response.ExceptionIfError();

            return ApiResponse.Succeeds(AutoMapper.Mapper.Map<Models.V2_5.ApiCredentials>(response.Value)).Construct(this, _logger);
        }
    }
}
