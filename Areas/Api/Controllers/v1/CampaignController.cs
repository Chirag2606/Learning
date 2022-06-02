 namespace Cyara.Web.Portal.Areas.Api.Controllers.V1
{
    using System;
    using System.Net.Http;
    using System.Web;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Portal.Models.Api.V1_0;

    public class CampaignController : BaseApiController
    {
        private static readonly Type Me = typeof(CampaignController);
        private ILogger _logger;
        private AccountResolver _accountResolver;
        private Cyara.Shared.Web.Types.Campaign.ICampaignService _campaignService;
        private Cyara.Shared.Web.Types.Storage.IStorageService _storageService;
        private IAccountService _accountService;
        private ITestCaseService _testCaseService;

        private IAuthorisationManager _authorisationManager;

        public CampaignController(
            ILogger logger,
            Cyara.Shared.Web.Types.Campaign.ICampaignService campaignService,
            Cyara.Shared.Web.Types.Storage.IStorageService storageService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            IAuthorisationManager authorisationManager)
        {
            _logger = logger;
            _accountService = accountService;
            _storageService = storageService;
            _campaignService = campaignService;
            _testCaseService = testCaseService;
            _authorisationManager = authorisationManager;
            _accountResolver = new AccountResolver(_logger, _campaignService, _accountService, _testCaseService);
        }

        [RestfulAuthorize(Roles = StaticRoles.Admin)]
        public HttpResponseMessage Get(string id)
        {
            var action = CampaignAction.ConstructAction(this.ModelState, "CampaignId", "GET", id);

            ModelState.ExceptionIfError();

            return ActionWrapper.Wrap(
                this,
                _authorisationManager,
                (session, sessionId) =>
                {
                    action.LogStart(Me, _logger, sessionId, "Campaign");
                    var result = action.ExecuteGet(session.User, session.SelectedAccount.Id, (string)sessionId, _logger, _campaignService);
                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    return _accountResolver.ByCampaign(action.IntegerValue, sessionId, user);
                },
                _logger);
        }

        [RestfulAuthorize(Roles = StaticRoles.Admin)]
        public HttpResponseMessage Delete(string id)
        {
            var action = CampaignAction.ConstructAction(this.ModelState, "CampaignId", "DELETE", id);

            ModelState.ExceptionIfError();

            return ActionWrapper.Wrap(
                this,
                _authorisationManager,
                (session, sessionId) =>
                {
                    action.LogStart(Me, _logger, sessionId, "Campaign");
                    var result = action.ExecuteDelete(session.User, session.SelectedAccount.Id, (string)sessionId, _logger, _campaignService);
                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    return _accountResolver.ByCampaign(action.IntegerValue, sessionId, user);
                },
                _logger);
        }

        [RestfulAuthorize(Roles = StaticRoles.Admin)]
        public HttpResponseMessage Put(Cyara.Web.Portal.Models.Api.V1_0.Campaign campaign)
        {
            ModelState.ExceptionIfError();

            HttpContextBase httpContext = (HttpContextBase)Request.Properties["MS_HttpContext"];

            var action = new CampaignAction
            {
                IntegerValue = campaign.CampaignId,
                Action = "PUT"
            };

            return ActionWrapper.Wrap(
                this,
                _authorisationManager,
                (session, sessionId) =>
                {
                    action.LogStart(Me, _logger, sessionId, "Campaign");
                    var result = action.ExecutePut(session.User, session.SelectedAccount.Id, (string)sessionId, _logger, _accountService, _authorisationManager, _campaignService, _storageService, _testCaseService, campaign, httpContext);
                    return result.Construct(this);
                },
                (sessionId, user) =>
                {
                    return _accountResolver.ByPlan(campaign.PlanId, sessionId, user);
                },
                _logger);
        }
    }
}
