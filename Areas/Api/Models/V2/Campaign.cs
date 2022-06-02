namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Responses;
    using Cyara.Shared.Data.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.Agent;
    using Cyara.Shared.Types.Campaign;
    using Cyara.Shared.Types.TestCase;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    public partial class Campaign
    {
        private static readonly StatusMap[] StatusMapList =
        {
            new StatusMap
            {
                ApiStatus = CampaignStatus.Aborted,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Aborted
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.ClashingCalledNumbers,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.ClashingCalledNumbers
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.Completed,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Completed
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.ExceededPorts,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.ExceededPorts
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.GenerationFailed,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.GenerationFailed
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.InternalError,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.InternalError
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.None,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.None
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.Pending,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Pending
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.Queued,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Queued
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.Running,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Running
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.Aborting,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Aborting
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.Finishing,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.Finishing
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.ExpiredPlan,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.ExpiredPlan
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.FuturePlan,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.FuturePlan
            }
        };

        public static Campaign From(ICampaign campaign, IDataHelper dataHelper)
        {
            Shared.Types.Campaign.Campaign campaignObj = campaign as Shared.Types.Campaign.Campaign;
            var isCcmCampaign = campaign.Plan.MediaType.IsVirtualAgent();

            return new Campaign
                   {
                       CampaignId = campaign.CampaignId,
                       Name = campaign.Name,
                       Description = campaign.Description ?? string.Empty,
                       RequestedRunDate = dataHelper.Output(campaign.NextRun),
                       ScheduledRunDate = dataHelper.Output(campaign.ScheduledRun),
                       LastRunDate = dataHelper.Output(campaign.LastRun),
                       Status = StatusMapList.First(x => x.ModelStatus == campaign.Status)
                           .ApiStatus,
                       CCM = CCMCampaign.From(campaignObj, dataHelper),
                       Cruncher = CruncherCampaign.From(campaignObj, false),
                       CruncherLite = CruncherCampaign.From(campaignObj, true),
                       Outbound = OutboundCampaign.From(campaignObj),
                       PulseOutbound = PulseOutboundCampaign.From(campaignObj, dataHelper),
                       Pulse = PulseCampaign.From(campaignObj, dataHelper),
                       Replay = ReplayCampaign.From(campaignObj),
                       Plan = PlanSummary.From(campaign),
                       TestCaseList = isCcmCampaign ? null : TestCaseProbability.ArrayFrom(campaignObj.TestCases),
                       AgentBehaviourList = AgentBehaviour.ArrayFrom(campaignObj)
                   };
        }

        public static ApiResponse<Campaign> Load(IAgentService agentService, ICampaignService campaignService, IDataHelper dataHelper, User user, int account, string scope, int id)
        {
            // Check that the url parameters are correct
            CampaignIdentifier campaignIdentifier = ApiModel.CampaignIdentifier(scope, id);
            if (campaignIdentifier == null)
            {
                // Couldn't build our CCM/Voice campaign id...
                return ApiResponse.Fails<Campaign>(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl);
            }

            return Load(agentService, campaignService, dataHelper, user, account, campaignIdentifier, campaignIdentifier.MediaType);
        }

        public static ICampaign LoadAgents(ICampaign campaign, IAgentService agentService, int accountId, int campaignId)
        {
            if (campaign.Plan.MediaType.IsVirtualAgent())
            {
                Shared.Types.Campaign.Campaign agentCampaign = campaign as Shared.Types.Campaign.Campaign;

                if (agentCampaign != null && agentCampaign.Agents == null)
                {
                    GenericResponse<IList<CampaignAgentBehaviour>> agents =
                        agentService.GetCampaignAgents(
                            new GenericRequest<AccountCampaign>(new AccountCampaign { AccountId = accountId, CampaignId = campaignId }));

                    agents.ExceptionIfError();

                    agentCampaign.Agents = agents.Value;
                }
            }

            return campaign;
        }

        public void SetOn(VoiceCampaignReplayEditViewModel model)
        {
            model.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
            model.Name = Name;
            model.Ports = Replay.ConcurrentPorts;
            model.MaxCaps = Replay.MaxCaps;
            model.Distribution = Replay.TestCaseDistributionProfile.ToModelTestCaseDistribution();
            if (RequestedRunDate.HasValue)
            {
                model.RequestedRunDate = RequestedRunDate.Value.ToUniversalTime();
            }
            else if (model.CampaignId <= 0)
            {
                model.RequestedRunDate = DateTime.UtcNow;
            }

            model.ScheduledRunDate = model.RequestedRunDate;
            model.TestCases =
                TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                    .ToList();
        }

        private static ApiResponse<Campaign> Load(
            IAgentService agentService,
            ICampaignService campaignService,
            IDataHelper dataHelper,
            User user,
            int account,
            CampaignIdentifier campaignIdentifier,
            MediaType mediaType)
        {
            // Get the campaign
            GenericResponse<ICampaign> campaignResponse = campaignService.CampaignGet(AccountRequest.Construct(campaignIdentifier, user, account));
            campaignResponse.ExceptionIfError();

            if (campaignResponse.Value == null)
            {
                return ApiResponse.NotFoundId<Campaign>(ApiMessages.Entity_Campaign, campaignIdentifier.CampaignId);
            }

            LoadAgents(campaignResponse.Value, agentService, account, campaignIdentifier.CampaignId);

            return ApiResponse.Succeeds(From(campaignResponse.Value, dataHelper));
        }

        private struct StatusMap
        {
            public CampaignStatus ApiStatus;

            public Domain.Types.Campaign.CampaignStatus ModelStatus;
        }
    }
}
