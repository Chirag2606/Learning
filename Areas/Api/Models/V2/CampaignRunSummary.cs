namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Net;

    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.Campaign;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Resources;

    public partial class CampaignRunSummary
    {
        public static CampaignRunSummary From(ICampaign campaign, Shared.Types.Campaign.CampaignRun run, IDataHelper dataHelper, string liveStatus = null)
        {
            return new CampaignRunSummary
                { 
                    Request = CampaignRunSummaryRequest.From(campaign, run, dataHelper),
                    Previous = CampaignRunSummaryPrevious.From(run, dataHelper, liveStatus) 
                };
        }

        public static ApiResponse<CampaignRunSummary> Load(ICampaignService campaignService, IDataHelper dataHelper, User user, int account, string scope, int id)
        {
            // Check that the url parameters are correct
            var campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                // Couldn't build our CCM/Voice campaign id... 
                return ApiResponse.Fails<CampaignRunSummary>(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl);
            }

            return Load(campaignService, dataHelper, user, account, campaignIdentifier);
        }

        public static ApiResponse<CampaignRunSummary> Load(ICampaignService campaignService, IDataHelper dataHelper, User user, int account, CampaignIdentifier campaignIdentifier)
        {
            // Get the campaign
            var campaignResponse = campaignService.CampaignGet(AccountRequest.Construct(campaignIdentifier, user, account));
            campaignResponse.ExceptionIfError();

            if (campaignResponse.Value == null)
            {
                return ApiResponse.NotFoundId<CampaignRunSummary>(ApiMessages.Entity_Campaign, campaignIdentifier.CampaignId);
            }
            
            // Search for a last runid (can be null)
            var runIdResponse = campaignService.CampaignRunsGetLatestRunId(AccountRequest.Construct(campaignIdentifier.CampaignId, user, account));
            runIdResponse.ExceptionIfError();

            // Get the details of the specified campaign run if found
            Tuple<Shared.Types.Campaign.CampaignRun, string> campaignRun = null;
            if (runIdResponse.Value.HasValue)
            {
                var accountRequestWithRunId = AccountRequest.Construct(runIdResponse.Value.Value, user, account);
                var campaignRunResponseWithStatus = campaignService.CampaignRunGetIncludeRealTimeStatus(accountRequestWithRunId);
                campaignRunResponseWithStatus.ExceptionIfError();

                if (campaignRunResponseWithStatus != null && campaignRunResponseWithStatus.IsSuccess)
                {
                    campaignRun = campaignRunResponseWithStatus.Value;
                }
                else
                {
                    // This is here just in case we can't contact the CallEngine - it doesn't return the campaign run it fetched - go figure!
                    var campaignRunResponse = campaignService.CampaignRunGet(accountRequestWithRunId);
                    campaignRunResponse.ExceptionIfError();

                    if (campaignRunResponse.IsSuccess && campaignRunResponse.Value != null)
                    {
                        campaignRun = new Tuple<Shared.Types.Campaign.CampaignRun, string>(campaignRunResponse.Value, null);
                    }
                }
            }

            // Return the data we found
            return ApiResponse.Succeeds(From(campaignResponse.Value, (campaignRun != null) ? campaignRun.Item1 : null, dataHelper, (campaignRun != null) ? campaignRun.Item2 : null));
        }
    }
}