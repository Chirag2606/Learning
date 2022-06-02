// <copyright file="CyaraXmlController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace
namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_3
{
    using Cyara.Domain.Business.Data.DataDriven.Types;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Common.Api;

    public class CyaraXmlController : V2_2.CyaraXmlController
    {
        public CyaraXmlController(ILogger logger, ICampaignService campaignService, IStorageService storageService, IAccountService accountService, ITestCaseService testCaseService, IDataDrivenService dataDrivenService, IConfigurationService configService, IRestApiFacade apiFacade)
            : base(logger, campaignService, storageService, accountService, testCaseService, dataDrivenService, configService, apiFacade)
        {
        }
    }
}