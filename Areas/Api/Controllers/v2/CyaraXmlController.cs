namespace Cyara.Web.Portal.Areas.Api.Controllers.V2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Autofac;

    using Cyara.Domain.Business.Data.DataDriven.Types;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Common.Api;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Serialisation;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    using CyaraWebApi;

    using HttpStatusCode = System.Net.HttpStatusCode;
    using MediaType = Cyara.Domain.Types.Common.MediaType;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class CyaraXmlController : BaseApiController
    {
        public const string OutboundStepzeroDesc = "Time to Answer";

        public CyaraXmlController(
            ILogger logger,
            ICampaignService campaignService,
            IStorageService storageService,
            IAccountService accountService,
            ITestCaseService testCaseService,
            IDataDrivenService dataDrivenService,
            IConfigurationService configService,
            IRestApiFacade restApi)
        {
            Logger = logger;
            AccountService = accountService;
            StorageService = storageService;
            CampaignService = campaignService;
            TestCaseService = testCaseService;
            DataDrivenService = dataDrivenService;
            ConfigService = configService;
            AccountResolver = new AccountResolver(Logger, CampaignService, AccountService, TestCaseService);
            DataHelper = new DataHelper();
            RestApi = restApi;
        }

        public virtual float TestSpecificationSupportedVersion { get; } = 0.1F;

        protected static BlockMap[] BlockMapList
        {
            get;
        }
            = {
                new BlockMap { ParamBlock = "include", ExportBlock = BlockExportParamsEnum.IncludeBlocks },
                new BlockMap { ParamBlock = "exclude", ExportBlock = BlockExportParamsEnum.ExcludeBlocks },
                new BlockMap { ParamBlock = "flatten", ExportBlock = BlockExportParamsEnum.FlattenBlocks }
            };

        protected ILogger Logger { get; set; }

        protected AccountResolver AccountResolver { get; set; }

        protected ICampaignService CampaignService { get; set; }

        protected IStorageService StorageService { get; set; }

        protected IAccountService AccountService { get; set; }

        protected ITestCaseService TestCaseService { get; set; }

        protected IDataDrivenService DataDrivenService { get; set; }

        protected IConfigurationService ConfigService { get; set; }

        protected IRestApiFacade RestApi { get; }

        [System.Web.Http.HttpGet, System.Web.Http.ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.Export })]
        public async Task<HttpResponseMessage> Get(int account, string scope, int id, string block, string data)
        {
            AccountRequest<ExportRequest> req = PrepareExportRequest(account, id, block, data, "no");

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

        [System.Web.Http.HttpPost, System.Web.Http.ActionName("Default")]
        [AuthorizeArea(testCase: new[] { Access.Write })]
        public virtual HttpResponseMessage Post(int account, string scope)
        {
            return PostInternal(account, scope, CyaraXmlImportResultList.From);
        }

        protected AccountRequest<ExportRequest> PrepareExportRequest(int account, int id, string block, string data, string service)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var bexport = BlockMapList.First(x => x.ParamBlock == block.ToLowerInvariant()).ExportBlock;

            var testSpecifcationVersion = TestSpecificationVersion.Get(TestSpecificationSupportedVersion);

            var req = AccountRequest.Construct(
                new ExportRequest
                    {
                        EntityIDs = new[] { id },
                        BlockExportParam = bexport,
                        ExcelXslt = null,
                        ExportFormat = ExportFormatEnum.XML,
                        WhatToExport = ObjectsToExportEnum.TestCases,
                        IncludeDataDriven = data.ToLowerInvariant() == "yes",
                        IncludeServices = service.ToLowerInvariant() == "yes",
                        Specification = new SpecificationVersion
                                            {
                                                BlockStepType = testSpecifcationVersion.BlockStepTypeInstance,
                                                BlockType = testSpecifcationVersion.BlockTypeInstance,
                                                TestCaseStepType = testSpecifcationVersion.TestCaseStepTypeInstance,
                                                TestCasesType = testSpecifcationVersion.TestCasesTypeInstance
                                            }
                    },
                session.User,
                account);
            return req;
        }

        protected virtual HttpResponseMessage PostInternal<T>(int account, string scope, Func<IEnumerable<TestSpecificationConversionResult.ReportItem>, T> report)
            where T : class
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            HttpContextBase httpContext = (HttpContextBase)Request.Properties["MS_HttpContext"];

            if (ApiModel.TargetForScope(scope) != MediaType.Voice)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_VoiceScopeExpected, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            var accountResponse = AccountService.AccountGet(new GenericRequest<int>(account)
            {
                User = session.User
            });
            accountResponse.ExceptionIfError();

            if (accountResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Account, account).Construct(this, Logger);
            }

            var conversionResult = new TestSpecificationConversionResult
            {
                AccountId = account,
                User = session.User
            };

            var testSpecifcationVersion = TestSpecificationVersion.Get(TestSpecificationSupportedVersion);

            T results;

            using (
                var specificationSource =
                    TestSpecificationSource.Load(
                        httpContext.Server.MapPath(testSpecifcationVersion.Path),
                        httpContext.Server.MapPath("~/Content/Serialisation/TestCaseFromExcel.xsl")))
            {
                var testSpecification = ImportViewModelExtensions.ParseTransformXml(
                    conversionResult,
                    Request.Content?.ReadAsStringAsync().Result,
                    Logger,
                    specificationSource.TestSpecificationStream,
                    specificationSource.TestCaseFromExcelStream);

                if (conversionResult.IsErrored)
                {
                    results = report(conversionResult.Report);
                }
                else
                {
                    var result = conversionResult.Import(
                        testSpecification,
                        StorageService,
                        TestCaseService,
                        DependencyResolver.Current.GetService<IComponentContext>(),
                        Logger,
                        accountResponse.Value.AccountId,
                        session.User,
                        ConfigService);

                    results = report(result.Report);
                }
            }

            return ApiResponse.Succeeds(results).Construct(this, Logger);
        }

        protected struct BlockMap
        {
            public string ParamBlock;
            public BlockExportParamsEnum ExportBlock;
        }
    }
}
