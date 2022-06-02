// <copyright file="CyaraXmlController.cs" company="Cyara">
// </copyright>
// ReSharper disable once CheckNamespace
namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_5
{
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Cyara.Domain.Business.Data.DataDriven.Types;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Common.Api;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models.V2_5;
    using Cyara.Web.Portal.Core.Extensions;

    using CyaraWebApi;

    using HttpStatusCode = System.Net.HttpStatusCode;
    
    public class CyaraXmlController : V2_4.CyaraXmlController
    {
        public CyaraXmlController(ILogger logger, ICampaignService campaignService, IStorageService storageService, IAccountService accountService, ITestCaseService testCaseService, IDataDrivenService dataDrivenService, IConfigurationService configService, IRestApiFacade apiFacade)
            : base(logger, campaignService, storageService, accountService, testCaseService, dataDrivenService, configService, apiFacade)
        {
            DataHelper = new DataHelper();
        }

        public override float TestSpecificationSupportedVersion { get; } = 0.5F;

        [System.Web.Http.HttpGet, System.Web.Http.ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.Export })]
        public async Task<HttpResponseMessage> Get(int account, string scope, int id, string block, string data, string service)
        {
            AccountRequest<ExportRequest> req = PrepareExportRequest(account, id, block, data, service);

            var testCaseRequest = new CyaraXmlTestCaseRequest
                                      {
                                          TestCaseIds = req.Value.EntityIDs.Select(x => x).ToList(),
                                          IncludeServices = req.Value.IncludeServices,
                                          IncludeDataDriven = req.Value.IncludeDataDriven,
                                          BlockOptions = req.Value.BlockExportParam.ToApi(),
                                          ExportFormat = req.Value.ExportFormat.ToApi(),
                                          FolderId = null,
                                          Recursive = false,
                                          TestCaseFolderPathOverride = null,
                                          BlockFolderPathOverride = null,
                                          ServicesFolderPathOverride = null
                                      };

            var response = await RestApi.TestCasesApi.PostCyaraXmlAsync(account, testCaseRequest);

            var res = new HttpResponseMessage(HttpStatusCode.OK);
            res.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(response)));
            res.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");

            return res;
        }
    }
}