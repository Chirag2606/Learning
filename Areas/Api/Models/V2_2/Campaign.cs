namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
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
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Types.Agent;
    using Cyara.Shared.Web.Types.Campaign;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Web.Portal.Areas.Api.Core;
    using Cyara.Web.Portal.Core.Constants;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Resources;

    public partial class Campaign
    {
        public static readonly StatusMap[] StatusMapList =
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
                ApiStatus = CampaignStatus.FailedValidation,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.FailedValidation
            },
            new StatusMap
            {
                ApiStatus = CampaignStatus.FailedLoading,
                ModelStatus = Domain.Types.Campaign.CampaignStatus.FailedLoading
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
                       AgentBehaviourList = AgentBehaviour.ArrayFrom(campaignObj),
                       Active = campaign.Active
                   };
        }

        public static ApiResponse<CampaignList> List(IAgentService agentService, ICampaignService campaignService, IDataHelper dataHelper, User user, int account, string scope)
        {
            // Check that the url parameters are correct
            MediaType? mediaType = ApiModel.TargetForScope(scope);
            if (mediaType == null)
            {
                return ApiResponse.Fails<CampaignList>(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_Scope, ApiMessages.InvalidUrl);
            }

            return List(agentService, campaignService, dataHelper, user, account, mediaType.Value);
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

        public int SetOn(CampaignEditViewModel model)
        {
            // Set MediaType and PlanType specific values
            switch (model.PlanType)
            {
                case Cyara.Domain.Types.Plan.PlanType.Replay:
                    ((VoiceCampaignReplayEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignReplayEditViewModel)model).Ports = Replay.ConcurrentPorts;
                    ((VoiceCampaignReplayEditViewModel)model).MaxCaps = Replay.MaxCaps;
                    ((VoiceCampaignReplayEditViewModel)model).Distribution = Replay.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignReplayEditViewModel)model).Active = true;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.Outbound:
                    ((VoiceCampaignOutboundEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignOutboundEditViewModel)model).Ports = Outbound.ConcurrentPorts;
                    ((VoiceCampaignOutboundEditViewModel)model).Duration = Outbound.RunFor.Value;
                    ((VoiceCampaignOutboundEditViewModel)model).DurationLegend = Outbound.RunFor.Unit == RunForUnit.Minutes ? FormKeys.Mins : FormKeys.Calls;
                    ((VoiceCampaignOutboundEditViewModel)model).Distribution = Outbound.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignOutboundEditViewModel)model).Active = true;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.Cruncher:
                    ((VoiceCampaignCruncherEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignCruncherEditViewModel)model).Ports = Cruncher.ConcurrentPorts;
                    ((VoiceCampaignCruncherEditViewModel)model).MaxCaps = Cruncher.MaxCaps;
                    ((VoiceCampaignCruncherEditViewModel)model).Distribution = Cruncher.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignCruncherEditViewModel)model).Duration = Cruncher.RunFor.Value;
                    ((VoiceCampaignCruncherEditViewModel)model).DurationLegend = Cruncher.RunFor.Unit == RunForUnit.Minutes ? FormKeys.Mins : FormKeys.Calls;
                    ((VoiceCampaignCruncherEditViewModel)model).RampUpTime = Cruncher.RampUpTime.Seconds;
                    ((VoiceCampaignCruncherEditViewModel)model).Active = true;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.CruncherLite:
                    ((VoiceCampaignCruncherLiteEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignCruncherLiteEditViewModel)model).Ports = CruncherLite.ConcurrentPorts;
                    ((VoiceCampaignCruncherLiteEditViewModel)model).MaxCaps = CruncherLite.MaxCaps;
                    ((VoiceCampaignCruncherLiteEditViewModel)model).Distribution = CruncherLite.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignCruncherLiteEditViewModel)model).Duration = CruncherLite.RunFor.Value;
                    ((VoiceCampaignCruncherLiteEditViewModel)model).DurationLegend = CruncherLite.RunFor.Unit == RunForUnit.Minutes
                                                                                         ? FormKeys.Mins
                                                                                         : FormKeys.Calls;
                    ((VoiceCampaignCruncherLiteEditViewModel)model).RampUpTime = CruncherLite.RampUpTime.Seconds;
                    ((VoiceCampaignCruncherLiteEditViewModel)model).Active = true;
                    break;
                /* These plan types should be handled later
                case Cyara.Shared.Types.Account.PlanType.PulseOutbound:
                    // TODO: Is this mapping correct?
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Schedule = this.PulseOutbound.Schedule.Length > 0 ? true : false;
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Schedules = this.PulseOutbound.Schedule.Select(x => new CampaignScheduleViewData { DayName = x.Day.ToString(), FromTime = x.From, ToTime = x.To });
                    break;
                case Cyara.Shared.Types.Account.PlanType.Pulse: break;
                */
                default:
                    // Unsupported Plan Type for Media Type
                    return 1;
            }

            // Set common values
            model.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
            model.Name = Name;

            if (RequestedRunDate.HasValue)
            {
                model.RequestedRunDate = RequestedRunDate.Value.ToUniversalTime();
            }
            else if (model.CampaignId <= 0)
            {
                model.RequestedRunDate = DateTime.UtcNow;
            }

            model.ScheduledRunDate = model.RequestedRunDate;

            // Success
            return 0;
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

        private static ApiResponse<CampaignList> List(IAgentService agentService, ICampaignService campaignService, IDataHelper dataHelper, User user, int account, MediaType mediaType)
        {
            PaginatedResponse<Shared.Types.Campaign.Campaign> campaignResponse =
                campaignService.CampaignsGet(
                    new PaginatedRequest<CampaignSearchRequest>(
                        new CampaignSearchRequest
                        {
                            ShowInactive = true,
                            SearchTerm = null,
                            MediaTypeLabel = Portal.Core.Extensions.MediaTypeExtensions.ToLabel,
                            PlanTypeLabel = PlanTypeExtensions.ToDisplay
                        })
                    {
                        AccountId = account,
                        User = user,
                        CurrentPage = 1,
                        PageSize = int.MaxValue,
                        SortField = Columns.ScheduledRunDate,
                        SortAscending = false
                    });

            campaignResponse.ExceptionIfError();

            List<Shared.Types.Campaign.Campaign> filteredCampaigns = campaignResponse.Collection.Where(x => x.Plan.MediaType == mediaType).ToList();

            switch (mediaType)
            {
                case MediaType.Voice:
                {
                    GenericResponse<IEnumerable<Tuple<int, CampaignTestCase>>> result = campaignService.CampaignTestCases(new AccountRequest<int[]>(filteredCampaigns.Select(x => x.CampaignId).ToArray()) { AccountId = account });
                    result.ExceptionIfError();

                    List<Tuple<int, CampaignTestCase>> testcases = result.Value.ToList();
                    filteredCampaigns.ForEach(x => x.TestCases = testcases.Where(y => y.Item1 == x.CampaignId).Select(y => y.Item2).ToList());

                    break;
                }

                case MediaType.AgentVoice:
                {
                    GenericResponse<IEnumerable<CampaignAgentBehaviour>> result = campaignService.CampaignAgentBehaviors(new AccountRequest<int[]>(filteredCampaigns.Select(x => x.CampaignId).ToArray()) { AccountId = account });
                    result.ExceptionIfError();

                    List<CampaignAgentBehaviour> agentbehaviours = result.Value.ToList();
                    filteredCampaigns.ForEach(x => x.Agents = agentbehaviours.Where(y => y.CampaignId == x.CampaignId).Select(y => y).ToList());

                    break;
                }
            }

            Campaign[] campaignsByMedia = filteredCampaigns
                .Select(x => From(x, dataHelper))
                .ToArray();

            return ApiResponse.Succeeds(new CampaignList { Data = campaignsByMedia });
        }

        public struct StatusMap
        {
            public CampaignStatus ApiStatus;

            public Domain.Types.Campaign.CampaignStatus ModelStatus;
        }
    }
}
