namespace Cyara.Web.Portal.Areas.Api.Controllers.V2
{
    using System.Net.Http;
    using System.Web.Http;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models.V2;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class RunController : BaseApiController
    {
        public RunController(
            ILogger logger,
            IAccountService accountService,
            ICampaignService campaignService)
        {
            Logger = logger;
            AccountService = accountService;
            CampaignService = campaignService;
            DataHelper = new DataHelper();
        }

        protected ILogger Logger { get; }

        protected IAccountService AccountService { get; }

        protected ICampaignService CampaignService { get; }

        /// <summary>
        /// Get the campaign run status for a specific RunID
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Run ID</param>
        [HttpGet, ActionName("Default")]
        [AuthorizeAccount(StaticRoles.Admin, StaticRoles.CCMAdmin)]
        public HttpResponseMessage Get(int account, string scope, int id)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var loadResponse = CampaignRun.Load(CampaignService, AccountService, DataHelper, session.User, account, scope, id);
            /*
            var xs = new System.Xml.Serialization.XmlSerializer(loadResponse.Value.GetType());
            var sw = new System.IO.StringWriter();
            xs.Serialize(sw, loadResponse.Value);
             * Method to debug why it won't serialize... 
             * TODO: I think we need to replace the serializer with our own class that will detect and return errors...
            */
            return loadResponse.Construct(this, Logger);
        }
    }
}
