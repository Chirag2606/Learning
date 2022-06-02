namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Plan;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Core.Threading;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Agent;
    using Cyara.Shared.Types.Campaign;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Storage;
    using Cyara.Shared.Web.Types.TestCase;
    using Cyara.Web.Messaging.Types.Command.Campaign;
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Extensions;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2;
    using Cyara.Web.Portal.Core.Campaign;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Extensions.Api.V1_0;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    using MediatR;

    using AgentBehaviour = Cyara.Web.Portal.Areas.Api.Models.V2_2.AgentBehaviour;
    using Campaign = Cyara.Web.Portal.Areas.Api.Models.V2_2.Campaign;
    using CampaignRunSummary = Cyara.Web.Portal.Areas.Api.Models.V2.CampaignRunSummary;
    using CampaignTests = Cyara.Web.Portal.Areas.Api.Models.V2_2.CampaignTests;
    using PlanType = Cyara.Domain.Types.Plan.PlanType;
    using TestCaseDistributionProfile = Cyara.Web.Portal.Areas.Api.Models.V2_2.TestCaseDistributionProfile;
    using TestCaseProbability = Cyara.Web.Portal.Areas.Api.Models.V2_2.TestCaseProbability;

    [LogApiRequest]
    [V2ControllerConfiguration]
    [AuthorizeAccount(StaticRoles.Admin, StaticRoles.CCMAdmin)]
    [HandleApiException]
    public class CampaignController : BaseApiController
    {
        public CampaignController(
            ILogger logger,
            IAccountService accountService,
            IAuthorisationManager authorisationManager,
            ICampaignService campaignService,
            ITestCaseService testCaseService,
            IStorageService storageService,
            IAgentService agentService,
            IMediator mediator)
        {
            Logger = logger;
            AccountService = accountService;
            CampaignService = campaignService;
            TestCaseService = testCaseService;
            StorageService = storageService;
            AgentService = agentService;
            AuthorisationManager = authorisationManager;
            DataHelper = new DataHelper();
            Mediator = mediator;
        }

        protected ILogger Logger { get; }

        protected IAccountService AccountService { get; }

        protected ICampaignService CampaignService { get; }

        protected ITestCaseService TestCaseService { get; }

        protected IStorageService StorageService { get; }

        protected IAgentService AgentService { get; }

        protected IAuthorisationManager AuthorisationManager { get; }

        protected IMediator Mediator { get; }

        /// <summary>
        /// Returns a list of campaign details
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        [HttpGet, ActionName("Default")]
        public HttpResponseMessage CampaignList(int account, string scope)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var listResponse = Campaign.List(AgentService, CampaignService, DataHelper, session.User, account, scope);
            return listResponse.Construct(this, Logger);
        }

        [HttpPost, ActionName("Default")]
        public HttpResponseMessage CampaignAdd(int account, string scope, Campaign campaign)
        {
            // Check that we could parse the campaign ok
            if (campaign == null)
            {
                var res = ApiModel.CheckForErrors(ModelState);
                if (res != null)
                {
                    return res.Construct(this, Logger);
                }
            }

            // Check that we have a PlanId
            if (campaign?.Plan == null || campaign.Plan.PlanId <= 0)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Campaign/Plan/PlanId"), ApiMessages.Content).Construct(this, Logger);
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            // Get the plan
            var planResponse = AccountService.PlanGet(AccountRequest.Construct(campaign.Plan.PlanId, session.User, account));
            planResponse.ExceptionIfError();
            if (!planResponse.IsSuccess || planResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Plan, campaign.Plan.PlanId).Construct(this, Logger);
            }

            // Ensure that the plan has a media type of voice, as it is currently the only supported type
            if (planResponse.Value.MediaType != MediaType.Voice)
            {
                return ApiResponse.Fails(HttpStatusCode.NotAcceptable, ApiMessages.Content_WrongPlan, ApiMessages.Content_UnsupportedMediaType).Construct(this, Logger);
            }

            // Ensure that the specified plan is supported, and that the request body is not empty
            var result = CheckPlantypeSupported(campaign, planResponse);
            if (result != null)
            {
                return result;
            }

            HttpContextBase httpContext = (HttpContextBase)Request.Properties["MS_HttpContext"];

            var viewModel = ModelBuilder.BuildFromScratch(
                               campaign.Plan.PlanId,
                               planResponse.Value.MediaType,
                               session.User,
                               account,
                               AccountService,
                               AuthorisationManager,
                               StorageService,
                               CampaignService,
                               httpContext,
                               false) as CampaignEditViewModel;

            // map items from API parameters into the viewModel
            // TODO: overwrite me please
            if (campaign.SetOn(viewModel) > 0)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidArgument, ApiMessages.Content_UnsupportedPlanType).Construct(this, Logger);
            }

            // required before save
            viewModel.SetTestCaseProbability();

            // validate
            var validateResponse = CampaignActionExtensions.ValidateViewModel(viewModel, Logger, TestCaseService, session.User, Request.GetCorrelationId().ToString());
            if (validateResponse.StatusCode != HttpStatusCode.OK)
            {
                return ApiResponse.Fails(HttpStatusCode.NotAcceptable, validateResponse.Description, validateResponse.Reason).Construct(this, Logger);
            }

            // prepare for saving
            var campaignToSave = ModelBuilder.Convert(planResponse.Value, viewModel);

            // create campaign but do not execute it
            var createResponse = CampaignService.CampaignCreate(AccountRequest.Construct(new Tuple<ICampaign, bool>(campaignToSave, false), session.User, account));
            createResponse.ExceptionIfError();

            if (!createResponse.IsSuccess)
            {
                return ApiResponse.ValidationFails(ApiMessages.Error_SaveCampaign.FormatWith(createResponse.Code())).Construct(this, Logger);
            }

            return CampaignGet(account, scope, createResponse.Value);
        }

        [HttpPut, ActionName("Default")]
        public HttpResponseMessage CampaignUpdate(int account, string scope, int id, Campaign campaign)
        {
            // Check that we could parse the campaign ok
            if (campaign == null)
            {
                var res = ApiModel.CheckForErrors(ModelState);
                if (res != null)
                {
                    return res.Construct(this, Logger);
                }
            }

            // Check that we have a PlanId
            if (campaign?.Plan == null || campaign.Plan.PlanId <= 0)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Campaign/Plan/PlanId"), ApiMessages.Content).Construct(this, Logger);
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            // Get the plan
            var planResponse = AccountService.PlanGet(AccountRequest.Construct(campaign.Plan.PlanId, session.User, account));
            planResponse.ExceptionIfError();
            if (!planResponse.IsSuccess || planResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Plan, campaign.Plan.PlanId).Construct(this, Logger);
            }

            // Ensure that the plan has a media type of voice, as it is currently the only supported type
            if (planResponse.Value.MediaType != MediaType.Voice)
            {
                return ApiResponse.Fails(HttpStatusCode.NotAcceptable, ApiMessages.Content_WrongPlan, ApiMessages.Content_UnsupportedMediaType).Construct(this, Logger);
            }

            // Ensure that the specified plan is supported, and that the request body is not empty
            var result = CheckPlantypeSupported(campaign, planResponse);
            if (result != null)
            {
                return result;
            }

            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Get the campaign
            var campaignResponse = CampaignService.CampaignGet(AccountRequest.Construct(campaignIdentifier, session.User, account));
            campaignResponse.ExceptionIfError();

            if (campaignResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, campaignIdentifier.CampaignId).Construct(this, Logger);
            }

            // this is an update to existing campaign, convert campaign from db first
            var viewModel = ModelBuilder.Create(campaignResponse.Value) as CampaignEditViewModel;

            if (viewModel == null)
            {
                return ApiResponse.Fails(HttpStatusCode.NotAcceptable, ApiMessages.Content_OnlyReplayPlan, ApiMessages.Content_WrongPlan).Construct(this, Logger);
            }

            // map items from API parameters into the viewModel
            if (campaign.SetOn(viewModel) > 0)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidArgument, ApiMessages.Content_UnsupportedPlanType).Construct(this, Logger);
            }

            // required before save
            viewModel.SetTestCaseProbability();

            // validate
            var validateResponse = CampaignActionExtensions.ValidateViewModel(viewModel, Logger, TestCaseService, session.User, Request.GetCorrelationId().ToString());
            if (validateResponse.StatusCode != HttpStatusCode.OK)
            {
                return ApiResponse.Fails(HttpStatusCode.NotAcceptable, validateResponse.Description, validateResponse.Reason).Construct(this, Logger);
            }

            // prepare for saving
            var campaignToSave = ModelBuilder.Convert(planResponse.Value, viewModel);

            // update campaign but do not execute it
            var updateResponse = CampaignService.CampaignUpdate(AccountRequest.Construct(new Tuple<ICampaign, bool>(campaignToSave, false), session.User, account));
            updateResponse.ExceptionIfError();

            if (!updateResponse.IsSuccess)
            {
                return ApiResponse.ValidationFails(ApiMessages.Error_SaveCampaign.FormatWith(updateResponse.Code())).Construct(this, Logger);
            }

            return CampaignGet(account, scope, id);
        }

        #region DuplicateMethodsFromV2Controller

        #region campaign/[id]
        /// <summary>
        /// Return the campaign details
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        [HttpGet, ActionName("Default")]
        public HttpResponseMessage CampaignGet(int account, string scope, int id)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var loadResponse = Campaign.Load(AgentService, CampaignService, DataHelper, session.User, account, scope, id);
            /*
            var xs = new System.Xml.Serialization.XmlSerializer(loadResponse.Value.GetType());
            var sw = new System.IO.StringWriter();
            xs.Serialize(sw, loadResponse.Value);
             * Method to debug why it won't serialize...
             * TODO: I think we need to replace the serializer with our own class that will detect and return errors...
            */
            return loadResponse.Construct(this, Logger);
        }

        [HttpDelete, ActionName("Default")]
        public HttpResponseMessage CampaignDelete(int account, string scope, int id)
        {
            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            // Get the campaign
            var campaignRequest = AccountRequest.Construct(campaignIdentifier, session.User, account);
            var campaignResponse = CampaignService.CampaignGet(campaignRequest);
            campaignResponse.ExceptionIfError();

            if (campaignResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, campaignIdentifier.CampaignId).Construct(this, Logger);
            }

            // now we are sure campaign exists, so delete
            var response = CampaignService.CampaignDelete(campaignRequest);
            response.ExceptionIfError();

            if (!response.IsSuccess)
            {
                return ApiResponse.ValidationFails(response.ErrorMessage()).Construct(this, Logger);
            }

            return ApiResponse.Succeeds().Construct(this, Logger);
        }

        #endregion

        #region campaign/[id]/test
        /// <summary>
        /// Get all the tests associated with the campaign
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        [HttpGet, ActionName("Test")]
        public HttpResponseMessage GetTest(int account, string scope, int id)
        {
            // Get the tests associated with the campaign
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var loadResponse = Campaign.Load(AgentService, CampaignService, DataHelper, session.User, account, scope, id);
            if (loadResponse.IsSuccess && loadResponse.Value != null)
            {
                return ApiResponse.Succeeds(new CampaignTests
                {
                    AgentBehaviourList = loadResponse.Value.AgentBehaviourList?.OrderBy(x => x.Agent.AgentId).ToArray(),
                    TestCaseList = loadResponse.Value.TestCaseList // NOTE: These are already ordered by OrderNo
                }).Construct(this, Logger);
            }

            return loadResponse.Construct(this, Logger);
        }

        /// <summary>
        /// Remove Tests From a campaign
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        /// <param name="tests">The XML/JSON list of test cases to remove</param>
        [HttpDelete, ActionName("Test")]
        public HttpResponseMessage RemoveTests(int account, string scope, int id, CampaignTests tests)
        {
            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Construct the requests
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var accountRequestWithCampaignIdentifier = AccountRequest.Construct(campaignIdentifier, session.User, account);

            // Get the campaign (ensure access)
            var campaignResponse = CampaignService.CampaignGet(accountRequestWithCampaignIdentifier);
            campaignResponse.HttpExceptionIfRequired(this, Logger);

            var campaign = campaignResponse.Value;
            var ccm = campaign as Shared.Types.Campaign.Campaign;
            var voiceCampaign = campaign as Shared.Types.Campaign.Campaign;

            if (campaign == null || campaign.Plan.MediaType != campaignIdentifier.MediaType)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
            }

            if (tests == null)
            {
                var res = ApiModel.CheckForErrors(ModelState);
                if (res != null)
                {
                    return res.Construct(this, Logger);
                }
            }

            switch (campaign.Plan.MediaType)
            {
                case MediaType.AgentVoice:
                    if (tests?.AgentBehaviourList == null || !tests.AgentBehaviourList.Any())
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("CampaignTests/AgentBehaviourList"), ApiMessages.Content).Construct(this, Logger);
                    }

                    if (ccm == null || campaignIdentifier.MediaType != MediaType.AgentVoice)
                    {
                        return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
                    }

                    Campaign.LoadAgents(ccm, AgentService, account, campaignIdentifier.CampaignId);

                    // Remove the agent behaviours if they are there...
                    if (ccm.Agents != null)
                    {
                        // While DB has Behaviour as part of the PK, you can only add an agent once to a campaign.
                        // We only care about agents that are acutally included
                        ccm.Agents = ccm.Agents.Where(x => x.CampaignId > 0).ToList();
                        var agentsList = ccm.Agents.Join(tests.AgentBehaviourList, i => i.AgentId, o => o.Agent.AgentId, (i, o) => i);
                        ccm.Agents = ccm.Agents.Except(agentsList).ToList();
                    }

                    break;
                case MediaType.Voice:
                    if (tests?.TestCaseList == null || !tests.TestCaseList.Any())
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("CampaignTests/TestCaseList"), ApiMessages.Content).Construct(this, Logger);
                    }

                    if (voiceCampaign == null || campaignIdentifier.MediaType != MediaType.Voice)
                    {
                        return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
                    }

                    // Remove the specified test-cases if they are there...
                    if (voiceCampaign.TestCases != null)
                    {
                        var testcaseList = voiceCampaign.TestCases.Join(tests.TestCaseList, i => i.TestCase.TestCaseId, o => o.TestCaseId, (i, o) => i);
                        voiceCampaign.TestCases = voiceCampaign.TestCases.Except(testcaseList).ToList();
                        voiceCampaign.SetTestCaseProbabilities();
                    }

                    break;
            }

            // Save the changes
            var campaignUpdateRequest = AccountRequest.Construct(new Tuple<ICampaign, bool>(campaign, false), session.User, account);
            var response = ApiResponse.From(CampaignService.CampaignUpdate(campaignUpdateRequest));
            response.HttpExceptionIfRequired(this, Logger);

            // Return the result...
            if (response.IsSuccess)
            {
                return GetTest(account, scope, id);
            }

            // Return the result
            return response.Construct(this, Logger);
        }

        /// <summary>
        /// Specify the exact list of test cases to use in a campaign
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        /// <param name="tests">The test cases to set on the campaign</param>
        [HttpPut, ActionName("Test")]
        public HttpResponseMessage SetTests(int account, string scope, int id, CampaignTests tests)
        {
            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Construct the requests
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var accountRequestWithCampaignIdentifier = AccountRequest.Construct(campaignIdentifier, session.User, account);

            // Get the campaign (ensure access)
            var campaignResponse = CampaignService.CampaignGet(accountRequestWithCampaignIdentifier);
            campaignResponse.HttpExceptionIfRequired(this, Logger);

            var campaign = campaignResponse.Value;
            var ccm = campaign as Shared.Types.Campaign.Campaign;
            var voiceCampaign = campaign as Shared.Types.Campaign.Campaign;

            if (campaign == null || campaign.Plan.MediaType != campaignIdentifier.MediaType)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
            }

            if (tests == null)
            {
                var res = ApiModel.CheckForErrors(ModelState);
                if (res != null)
                {
                    return res.Construct(this, Logger);
                }
            }

            switch (campaign.Plan.MediaType)
            {
                case MediaType.AgentVoice:
                    if (tests?.AgentBehaviourList == null || !tests.AgentBehaviourList.Any())
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("CampaignTests/AgentBehaviourList"), ApiMessages.Content).Construct(this, Logger);
                    }

                    // Remove all the agents if they are there...
                    ccm.Agents = new List<CampaignAgentBehaviour>();

                    // Set to our new list...
                    var agentResponse = AgentBehaviour.SetOn(tests.AgentBehaviourList, ccm, AgentService, session, account);
                    if (!agentResponse.IsSuccess)
                    {
                        return agentResponse.Construct(this, Logger);
                    }

                    break;
                case MediaType.Voice:
                    if (tests?.TestCaseList == null || !tests.TestCaseList.Any())
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("CampaignTests/TestCaseList"), ApiMessages.Content).Construct(this, Logger);
                    }

                    if (voiceCampaign == null || campaignIdentifier.MediaType != MediaType.Voice)
                    {
                        return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
                    }

                    // Remove all the test-cases if they are there...
                    voiceCampaign.TestCases = new List<CampaignTestCase>();

                    // Set to our new list...
                    var testCaseResponse = TestCaseProbability.SetOn(tests.TestCaseList, voiceCampaign);
                    if (!testCaseResponse.IsSuccess)
                    {
                        return testCaseResponse.Construct(this, Logger);
                    }

                    break;
            }

            // Save the changes
            var campaignUpdateRequest = AccountRequest.Construct(new Tuple<ICampaign, bool>(campaign, false), session.User, account);
            var response = ApiResponse.From(CampaignService.CampaignUpdate(campaignUpdateRequest));
            response.HttpExceptionIfRequired(this, Logger);

            // Return the result...
            if (response.IsSuccess)
            {
                return GetTest(account, scope, id);
            }

            // Return the result
            return response.Construct(this, Logger);
        }

        /// <summary>
        /// Add test cases to a campaign
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        /// <param name="tests">The list of test cases to add (if not already in campaign)</param>
        [HttpPost, ActionName("Test")]
        public HttpResponseMessage AddTests(int account, string scope, int id, CampaignTests tests)
        {
            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Construct the requests
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var accountRequestWithCampaignIdentifier = AccountRequest.Construct(campaignIdentifier, session.User, account);

            // Get the campaign (ensure access)
            var campaignResponse = CampaignService.CampaignGet(accountRequestWithCampaignIdentifier);
            campaignResponse.HttpExceptionIfRequired(this, Logger);

            var campaign = campaignResponse.Value;
            var ccm = campaign as Shared.Types.Campaign.Campaign;
            var voiceCampaign = campaign as Shared.Types.Campaign.Campaign;

            if (campaign == null || campaign.Plan.MediaType != campaignIdentifier.MediaType)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
            }

            if (tests == null)
            {
                var res = ApiModel.CheckForErrors(ModelState);
                if (res != null)
                {
                    return res.Construct(this, Logger);
                }
            }

            switch (campaign.Plan.MediaType)
            {
                case MediaType.AgentVoice:

                    if (tests?.AgentBehaviourList == null || !tests.AgentBehaviourList.Any())
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("CampaignTests/AgentBehaviourList"), ApiMessages.Content).Construct(this, Logger);
                    }

                    Campaign.LoadAgents(ccm, AgentService, account, campaignIdentifier.CampaignId);
                    ccm.Agents = ccm.Agents.Where(x => x.CampaignId > 0).ToList();

                    // Set to our new list...
                    var agentResponse = AgentBehaviour.SetOn(tests.AgentBehaviourList, ccm, AgentService, session, account);
                    if (!agentResponse.IsSuccess)
                    {
                        return agentResponse.Construct(this, Logger);
                    }

                    break;
                case MediaType.Voice:
                    if (tests?.TestCaseList == null || !tests.TestCaseList.Any())
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("CampaignTests/TestCaseList"), ApiMessages.Content).Construct(this, Logger);
                    }

                    if (voiceCampaign == null || campaignIdentifier.MediaType != MediaType.Voice)
                    {
                        return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
                    }

                    // Set to our new list...
                    var testCaseResponse = TestCaseProbability.SetOn(tests.TestCaseList, voiceCampaign);
                    if (!testCaseResponse.IsSuccess)
                    {
                        return testCaseResponse.Construct(this, Logger);
                    }

                    break;
            }

            // Save the changes
            var campaignUpdateRequest = AccountRequest.Construct(new Tuple<ICampaign, bool>(campaign, false), session.User, account);
            var response = ApiResponse.From(CampaignService.CampaignUpdate(campaignUpdateRequest));
            response.HttpExceptionIfRequired(this, Logger);

            // Return the result...
            if (response.IsSuccess)
            {
                return GetTest(account, scope, id);
            }

            // Return the result
            return response.Construct(this, Logger);
        }

        #endregion

        #region campaign/[id]/run
        /// <summary>
        /// This allows you to query the scheduled status of the specified campaign, and obtain the current/previous run id
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        [HttpGet, ActionName("Run")]
        public HttpResponseMessage Run(int account, string scope, int id)
        {
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            var loadResponse = CampaignRunSummary.Load(CampaignService, DataHelper, session.User, account, scope, id);
            return loadResponse.Construct(this, Logger);
        }

        /// <summary>
        /// This allows you to abort a running campaign
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        [HttpDelete, ActionName("Run")]
        public HttpResponseMessage RunAbort(int account, string scope, int id)
        {
            // Check that the url parameters are correct
            var campaignAbortIdentifier = ApiModel.CampaignAbortIdentifier(scope, id, AbortType.HardAbort);
            if (campaignAbortIdentifier == null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Construct the requests
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            // Abort the campaign
            var command = new CampaignAbortCommand { AccountId = session.SelectedAccount.Id, CampaignId = campaignAbortIdentifier.CampaignId, CampaignRunId = Guid.NewGuid(), AbortType = campaignAbortIdentifier.AbortType };
            var response = AsyncHelpers.RunSync(() => Mediator.Send(command));
            if (!response.IsSuccess)
            {
                if (response.ErrorResult?.Code == nameof(Cyara.Web.Resources.Service.Messages.CAMPAIGN_CANNOT_FIND))
                {
                    return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
                }
            }

            var abortResponse = ApiResponse.From(response, ApiMessages.Error_CampaignAbort);
            abortResponse.HttpExceptionIfRequired(this, Logger);

            if (abortResponse.IsSuccess)
            {
                // Get the status and return it if we can
                var loadResponse = CampaignRunSummary.Load(CampaignService, DataHelper, session.User, account, scope, id);
                loadResponse.HttpExceptionIfRequired(this, Logger);

                if (loadResponse.IsSuccess)
                {
                    return loadResponse.Construct(this, Logger);
                }
            }

            // Return the result
            return abortResponse.Construct(this, Logger);
        }

        /// <summary>
        /// This allows you to start, or request a campaign to start in the future
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        /// <param name="campaignRunSummary">The time you wish to schedule the campaign to run, any past time will start the campaign.</param>
        [HttpPut, HttpPost, ActionName("Run")]
        public HttpResponseMessage RunSchedule(int account, string scope, int id, CampaignRunSummary campaignRunSummary)
        {
            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }

            // Validate request content
            if (campaignRunSummary == null)
            {
                var res = ApiModel.CheckForErrors(ModelState);
                if (res != null)
                {
                    return res.Construct(this, Logger);
                }
            }

            if (campaignRunSummary?.Request == null)
            {
                return ApiResponse.Fails(
                    HttpStatusCode.BadRequest,
                    campaignRunSummary == null ? ApiMessages.Content_Null.FormatWith("CampaignRunSummary") : ApiMessages.Content_MissingElement.FormatWith("Request/RunDate"),
                    ApiMessages.Content).Construct(this, Logger);
            }

            // Construct the requests
            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);
            var accountRequestWithCampaignIdentifier = new AccountRequest<CampaignIdentifier>(campaignIdentifier) { AccountId = account, User = session.User };

            // Get the campaign (ensure access)
            var campaignResponse = CampaignService.CampaignGet(accountRequestWithCampaignIdentifier);
            campaignResponse.HttpExceptionIfRequired(this, Logger);
            if (campaignResponse.Value == null)
            {
                return ApiResponse.NotFoundId(ApiMessages.Entity_Campaign, id).Construct(this, Logger);
            }

            // If the rundate date is sometime before 5 minutes from now, then run it straight away
            ApiResponse response = null;

            // Assume the date passed in is local, so we can simply get the UTC from this.
            campaignRunSummary.Request.RunDate = campaignRunSummary.Request.RunDate.ToUniversalTime();

            // Schedule it to go...
            var campaign = campaignResponse.Value;
            campaign.NextRun = campaignRunSummary.Request.RunDate;
            if (campaignRunSummary.Request.RunDate <= DateTime.UtcNow.AddMinutes(5))
            {
                // Allow 5 minutes off to mean now.
                campaign.NextRun = DateTime.UtcNow;
            }

            // if this is a load campaign, the campaign service will look after the scheduled run if the account is set up for load authorisation.
            campaign.ScheduledRun = campaign.NextRun;

            var campaignUpdateRequest = AccountRequest.Construct(new Tuple<ICampaign, bool>(campaign, true), session.User, account);
            response = ApiResponse.From(CampaignService.CampaignUpdate(campaignUpdateRequest));
            if (!response.IsSuccess)
            {
                if (response.Reason == "CAMPAIGN_RUNNING")
                {
                    return ApiResponse.Fails(HttpStatusCode.NotModified, ApiMessages.StateError_CampaignRunning, ApiMessages.StateError).Construct(this, Logger);
                }

                if (response.Reason == "SCHEDULER_IN_MAINTENANCE_MODE")
                {
                    return ApiResponse.Fails(HttpStatusCode.ServiceUnavailable, ApiMessages.InMaintenanceMode, ApiMessages.Unavailable).Construct(this, Logger);
                }

                response.Description = ApiMessages.Error_CampaignExecute;
            }

            response.HttpExceptionIfRequired(this, Logger);

            if (response.IsSuccess)
            {
                // Return the current status information...
                var loadResponse = CampaignRunSummary.Load(CampaignService, DataHelper, session.User, account, scope, id);
                if (loadResponse.IsSuccess)
                {
                    return loadResponse.Construct(this, Logger);
                }
            }

            return response.Construct(this, Logger);
        }

        #endregion

        /// <summary>
        /// Helper method to check if the supplied plan type is supported, and that the request body for the specified plan is not null
        /// </summary>
        /// <param name="campaign">The campaign we are validating</param>
        /// <param name="planResponse">The plan type we need to match with</param>
        /// <returns>A pre-formatted HTTPResponse message with the specific failure, or null if everything was OK</returns>
        private HttpResponseMessage CheckPlantypeSupported(Campaign campaign, GenericResponse<Plan> planResponse)
        {
            switch (planResponse.Value.PlanType)
            {
                case PlanType.Cruncher:
                    if (campaign?.Cruncher == null)
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Campaign/Cruncher"), ApiMessages.Content).Construct(this, Logger);
                    }

                    break;
                case PlanType.Replay:
                    if (campaign?.Replay == null)
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Campaign/Replay"), ApiMessages.Content).Construct(this, Logger);
                    }

                    if (campaign.Replay.TestCaseDistributionProfile != TestCaseDistributionProfile.RoundRobin)
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_InvalidProbability, ApiMessages.Content).Construct(this, Logger);
                    }

                    break;
                case PlanType.CruncherLite:
                    if (campaign?.CruncherLite == null)
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Campaign/CruncherLite"), ApiMessages.Content).Construct(this, Logger);
                    }

                    break;
                case PlanType.Outbound:
                    if (campaign?.Outbound == null)
                    {
                        return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.Content_MissingElement.FormatWith("Campaign/Outbound"), ApiMessages.Content).Construct(this, Logger);
                    }

                    break;
                /* Support for the following plan types to be added later
                case PlanType.VirtualAgent: break;
                case PlanType.Pulse: break;
                case PlanType.PulseOutbound: break;
                */
                default:
                    return ApiResponse.Fails(HttpStatusCode.NotAcceptable, ApiMessages.Content_WrongPlan, ApiMessages.Content_UnsupportedPlanType).Construct(this, Logger);
            }

            return null;
        }
        #endregion
    }
}
