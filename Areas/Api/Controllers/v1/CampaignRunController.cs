namespace Cyara.Web.Portal.Areas.Api.Controllers.V1
{
    using System;
    using System.Net.Http;
    using System.Web.Http;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Scheduler;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Portal.Models.Api.V1_0;

    using MediatR;

    public class CampaignRunController : BaseApiController
    {
        private static readonly Type Me = typeof(CampaignController);
        private ILogger _logger;
        private AccountResolver _accountResolver;
        private Cyara.Shared.Web.Types.Campaign.ICampaignService _campaignService;
        private IAccountService _accountService;
        private ITestCaseService _testCaseService;
        private ISchedulerService _schedulerService;
        private IMediator _mediator;
        private IAuthorisationManager _authorisationManager;

        public CampaignRunController(
            ILogger logger,
            Cyara.Shared.Web.Types.Campaign.ICampaignService campaignService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            ISchedulerService schedulerService,
            IAuthorisationManager authorisationManager,
            IMediator mediator)
        {
            _logger = logger;
            _accountService = accountService;
            _campaignService = campaignService;
            _testCaseService = testCaseService;
            _schedulerService = schedulerService;
            _authorisationManager = authorisationManager;
            _mediator = mediator;
            _accountResolver = new AccountResolver(_logger, _campaignService, _accountService, _testCaseService);
        }

        [HttpPost]
        [RestfulAuthorize(Roles = StaticRoles.Admin)]
        public HttpResponseMessage Runner(string id, string command)
        {
            var action = CampaignAction.ConstructAction(ModelState, "CampaignId", command, id);

            ModelState.ExceptionIfError();

            return ActionWrapper.Wrap(
               this,
               _authorisationManager,
               (session, sessionId) =>
               {
                   action.LogStart(Me, _logger, sessionId, "Campaign");
                   var result = action.ExecuteRunner(session.User, session.SelectedAccount.Id, sessionId, _logger, _campaignService, _schedulerService, _mediator, command);
                   return result.Construct(this);
               },
               (sessionId, user) => _accountResolver.ByCampaign(action.IntegerValue, sessionId, user),
               _logger);
        }

        [RestfulAuthorize(Roles = StaticRoles.Admin)]
        public HttpResponseMessage Get(string id)
        {
            var action = CampaignAction.ConstructAction(ModelState, "CampaignRunId", "GetCampaignRunStatus", id);

            ModelState.ExceptionIfError();

            return ActionWrapper.Wrap(
               this,
               _authorisationManager,
               (session, sessionId) =>
               {
                   action.LogStart(Me, _logger, sessionId, "CampaignRun");
                   var result = action.ExecuteGetRunStatus(session.User, session.SelectedAccount.Id, sessionId, _logger, _campaignService, _schedulerService);
                   return result.Construct(this);
               },
               (sessionId, user) => _accountResolver.ByCampaignRun(action.IntegerValue, sessionId, user),
               _logger);
        }
    }
}
