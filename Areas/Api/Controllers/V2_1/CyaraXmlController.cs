namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_1
{
    using System.Net.Http;
    using System.Web.Mvc;

    using Cyara.Domain.Business.Data.DataDriven.Types;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Common.Api;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models.V2;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class CyaraXmlController : V2.CyaraXmlController
    {
        public CyaraXmlController(
            ILogger logger,
            ICampaignService campaignService,
            IStorageService storageService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            IDataDrivenService dataDrivenService,
            IConfigurationService configService,
            IRestApiFacade apiFacade) : 
            base(logger, campaignService, storageService, accountService, testCaseService, dataDrivenService, configService, apiFacade)
        {
        }

        [HttpPost, ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.Write })]
        public override HttpResponseMessage Post(int account, string scope)
        {
            return PostInternal(account, scope, (r) => { return CyaraXmlItemResultList.From(r); });
        }
    }
}
