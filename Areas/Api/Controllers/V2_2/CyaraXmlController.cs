namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_2
{
    using Cyara.Domain.Business.Data.DataDriven.Types;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Common.Api;
    using Cyara.Web.Portal.Areas.Api.Attributes;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class CyaraXmlController : V2_1.CyaraXmlController
    {
        public CyaraXmlController(
            ILogger logger,
            ICampaignService campaignService,
            IStorageService storageService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            IDataDrivenService dataDrivenService,
            IConfigurationService configService,
            IRestApiFacade apiFacade)
            : base(logger, campaignService, storageService, accountService, testCaseService, dataDrivenService, configService, apiFacade)
        {
        }

        public override float TestSpecificationSupportedVersion { get; } = 0.4F;
    }
}
