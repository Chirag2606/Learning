namespace Cyara.Web.Portal.Areas.Api.Controllers.V2_5
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Responses;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
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
    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Areas.Api.Models.V2_5;
    using Cyara.Web.Resources;

    using MediatR;

    using CampaignRun = Cyara.Shared.Types.Campaign.CampaignRun;
    using CampaignRunsForCampaignRequest = Cyara.Web.Portal.Areas.Api.Models.V2_5.CampaignRunsForCampaignRequest;

    [LogApiRequest]
    [V2ControllerConfiguration]
    [AuthorizeAccount(StaticRoles.Admin, StaticRoles.CCMAdmin)]
    [HandleApiException]
    public class CampaignController : V2_4.CampaignController
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
            : base(logger, accountService, authorisationManager, campaignService, testCaseService, storageService, agentService, mediator)
        {
            DataHelper = new DataHelper();
        }

        /// <summary>
        ///     Get the campaign run summary report for a specific Campaign run
        /// </summary>
        [HttpGet]
        [ActionName("Runs")]
        [AuthorizeAccount(StaticRoles.Admin, StaticRoles.CCMAdmin)]
        public virtual HttpResponseMessage Runs(int account, string scope, int id, [FromUri] CampaignRunsForCampaignRequest request)
        {
            // make sure we don't pickup the default route.  
            IHttpRouteData routeData = Request.GetRouteData();

            if (routeData != null)
            {
                var action = (routeData.Values["action"] ?? string.Empty).ToString();

                if (!action.Equals("Runs", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.ValidUrl_Pattern_V2Report, ApiMessages.InvalidUrl).Construct(this, Logger);
                }
            }

            // Check that the params are ok... and perform request
            MediaType? mediatype = ApiModel.TargetForScope(scope);

            switch (mediatype ?? MediaType.None)
            {
                case MediaType.Voice:
                case MediaType.AgentVoice:

                    CampaignIdentifier campaignIdentifier = new CampaignIdentifier { MediaType = mediatype.Value, CampaignId = id };

                    ApiResponse<CampaignRunsForCampaignResponse> result = CampaignRunsForCampaign(request, account, scope, campaignIdentifier);

                    return result.Construct(this, Logger);

                default:

                    // invalid media type being used
                    return ApiResponse.Fails(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl).Construct(this, Logger);
            }
        }

        /// <summary>
        /// This allows you to abort a running campaign
        /// </summary>
        /// <param name="account">Account ID</param>
        /// <param name="scope">Which area you are using, voice or agent(CCM)</param>
        /// <param name="id">Campaign ID</param>
        /// <param name="soft">Whether a soft abort should be executed</param>
        [HttpDelete, ActionName("Run")]
        public HttpResponseMessage RunAbort(int account, string scope, int id, [FromUri] bool soft)
        {
            return RunAbortImpl(account, scope, id, soft ? AbortType.SoftAbort : AbortType.HardAbort);
        }

        private ApiResponse<CampaignRunsForCampaignResponse> CampaignRunsForCampaign(CampaignRunsForCampaignRequest request, int accountId, string scope, CampaignIdentifier campaignIdentifier)
        {
            NameValueCollection query = Request.RequestUri.ParseQueryString();

            ApiSessionFacade session = new ApiSessionFacade(ControllerContext);

            // get campaign information
            GenericResponse<ICampaign> campaignResponse = CampaignService.CampaignGet(
                new AccountRequest<CampaignIdentifier>(campaignIdentifier)
                {
                    AccountId = accountId,
                    User = session.User
                });
            campaignResponse.ExceptionIfError();

            if (!campaignResponse.IsSuccess || campaignResponse.Value == null)
            {
                return ApiResponse.NotFoundId<CampaignRunsForCampaignResponse>(ApiMessages.Entity_CampaignRun, campaignIdentifier.CampaignId);
            }

            Func<DateTime?, DateTime, DateTime> fixDate = (i, d) => !i.HasValue ? d : (i.Value.Kind == DateTimeKind.Local ? i.Value.ToUniversalTime() : i.Value.SetUtcKind());

            // get campaign run list
            PaginatedResponse<CampaignRun> runsResponse = CampaignService.CampaignRunsForCampaign(
                new PaginatedRequest<Shared.Web.Types.Campaign.CampaignRunsForCampaignRequest>(
                        new Shared.Web.Types.Campaign.CampaignRunsForCampaignRequest
                        {
                            From = fixDate(request.FromDate,  DateTime.MinValue),
                            To = fixDate(request.ToDate, DateTime.MaxValue),
                            CampaignId = campaignIdentifier.CampaignId,
                            MediaType = campaignIdentifier.MediaType
                        })
                    {
                        CurrentPage = request.Page ?? 1,
                        PageSize = request.PerPage ?? 100,
                        User = session.User,
                        AccountId = accountId
                    });
            runsResponse.ExceptionIfError();

            // build response
            var response = new CampaignRunsForCampaignResponse();

            response.CampaignName = campaignResponse.Value.Name;
            response.CampaignId = campaignResponse.Value.CampaignId;

            response.RunHistory = runsResponse.Collection.Select(
                x =>
                {
                    Models.V2_5.CampaignRun result = Models.V2_5.CampaignRun.From(x, DataHelper);

                    result.EndDateSpecified = x.EndDateSpecified;

                    return result;
                }).ToList();

            query.Remove("page");

            var route =
                $"{Request.RequestUri.Scheme}://{Request.RequestUri.Host}{string.Join(string.Empty, Request.RequestUri.Segments.TakeWhile(x => string.Compare(x, "account/", StringComparison.InvariantCultureIgnoreCase) != 0))}account/{accountId}/{scope}/campaign/{campaignIdentifier.CampaignId}/runs?page={{0}}{string.Join(string.Empty, query.AllKeys.Select(x => $"&{x}={query[x]}"))}";

            response.SelfLink = route.FormatWith(runsResponse.CurrentPage);
            response.NextLink = runsResponse.CurrentPage >= runsResponse.TotalPages ? null : route.FormatWith(runsResponse.CurrentPage + 1);
            response.PreviousLink = runsResponse.CurrentPage <= 1 ? null : route.FormatWith(runsResponse.CurrentPage - 1);
            response.FirstLink = runsResponse.CollectionSize == 0 ? null : route.FormatWith(1);
            response.LastLink = runsResponse.CollectionSize == 0 ? null : route.FormatWith(runsResponse.TotalPages);

            return ApiResponse.Succeeds(response);
        }
    }
}
