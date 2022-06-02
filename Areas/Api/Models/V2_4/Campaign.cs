namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
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
                       Status = AutoMapper.Mapper.Map<CampaignStatus>(V2_2.Campaign.StatusMapList.First(x => x.ModelStatus == campaign.Status).ApiStatus),
                       CCM = AutoMapper.Mapper.Map<CCMCampaign>(CCMCampaign.From(campaignObj, dataHelper)),
                       Cruncher = AutoMapper.Mapper.Map<CruncherCampaign>(V2_2.CruncherCampaign.From(campaignObj, false)),
                       CruncherLite = AutoMapper.Mapper.Map<CruncherCampaign>(V2_2.CruncherCampaign.From(campaignObj, true)),
                       Outbound = AutoMapper.Mapper.Map<OutboundCampaign>(V2_2.OutboundCampaign.From(campaignObj)),
                       PulseOutbound = AutoMapper.Mapper.Map<PulseOutboundCampaign>(PulseOutboundCampaign.From(campaignObj, dataHelper)),
                       Pulse = AutoMapper.Mapper.Map<PulseCampaign>(PulseCampaign.From(campaignObj, dataHelper)),
                       Replay = AutoMapper.Mapper.Map<ReplayCampaign>(V2_2.ReplayCampaign.From(campaignObj)),
                       Velocity = VelocityCampaign.From(campaignObj),
                       Plan = PlanSummary.From(campaign),
                       TestCaseList = isCcmCampaign || V2_2.TestCaseProbability.ArrayFrom(campaignObj.TestCases) == null ? null : V2_2.TestCaseProbability.ArrayFrom(campaignObj.TestCases).Select(AutoMapper.Mapper.Map<TestCaseProbability>).ToArray(),
                       AgentBehaviourList = V2_2.AgentBehaviour.ArrayFrom(campaignObj) == null ? null : V2_2.AgentBehaviour.ArrayFrom(campaignObj).Select(AutoMapper.Mapper.Map<AgentBehaviour>).ToArray(),
                       Active = campaign.Active
                   };
        }

        public static ApiResponse<CampaignList> List(IAgentService agentService, ICampaignService campaignService, IDataHelper dataHelper, User user, int account, string scope)
        {
            // Check that the URL parameters are correct
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
            return V2_2.Campaign.LoadAgents(campaign, agentService, accountId, campaignId);
        }

        public int SetOn(CampaignEditViewModel model)
        {
            // Set MediaType and PlanType specific values
            switch (model.PlanType)
            {
                case Cyara.Domain.Types.Plan.PlanType.Velocity:
                    ((VoiceCampaignVelocityEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignVelocityEditViewModel)model).Ports = Velocity.ConcurrentPorts;
                    ((VoiceCampaignVelocityEditViewModel)model).MaxCaps = Velocity.MaxCaps;
                    ((VoiceCampaignVelocityEditViewModel)model).Distribution = Velocity.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignVelocityEditViewModel)model).Active = Active;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.Replay:
                    ((VoiceCampaignReplayEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignReplayEditViewModel)model).Ports = Replay.ConcurrentPorts;
                    ((VoiceCampaignReplayEditViewModel)model).MaxCaps = Replay.MaxCaps;
                    ((VoiceCampaignReplayEditViewModel)model).Distribution = Replay.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignReplayEditViewModel)model).Active = Active;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.Outbound:
                    ((VoiceCampaignOutboundEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignOutboundEditViewModel)model).Ports = Outbound.ConcurrentPorts;
                    ((VoiceCampaignOutboundEditViewModel)model).Duration = Outbound.RunFor.Value;
                    ((VoiceCampaignOutboundEditViewModel)model).DurationLegend = Outbound.RunFor.Unit == RunForUnit.Minutes ? FormKeys.Mins : FormKeys.Calls;
                    ((VoiceCampaignOutboundEditViewModel)model).Distribution = Outbound.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignOutboundEditViewModel)model).Active = Active;
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
                    ((VoiceCampaignCruncherEditViewModel)model).Active = Active;
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
                    ((VoiceCampaignCruncherLiteEditViewModel)model).Active = Active;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.Pulse:
                    ((VoiceCampaignPulseEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignPulseEditViewModel)model).Ports = Pulse.ConcurrentPorts;
                    ((VoiceCampaignPulseEditViewModel)model).Frequency = Pulse.Frequency.Minutes;
                    ((VoiceCampaignPulseEditViewModel)model).MaxCaps = Pulse.MaxCaps;
                    ((VoiceCampaignPulseEditViewModel)model).Distribution = Pulse.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignPulseEditViewModel)model).Schedule = false;
                    if (Pulse.Schedule != null && Pulse.Schedule.Length > 0)
                    {
                        ((VoiceCampaignPulseEditViewModel)model).Schedule = true;
                        ((VoiceCampaignPulseEditViewModel)model).Schedules = Pulse.Schedule.Select(x => new CampaignScheduleViewData { Day = (int)x.Day, FromTime = DateTime.Parse(x.From).ToUniversalTime().ToString("HH:mm"), ToTime = DateTime.Parse(x.To).ToUniversalTime().ToString("HH:mm") });
                    }

                    if (Pulse.Pause != null && Pulse.Pause.Length > 0)
                    {
                        ((VoiceCampaignPulseEditViewModel)model).Schedule = true;
                        ((VoiceCampaignPulseEditViewModel)model).SchedulePauses = Pulse.Pause.Select(x => new SchedulePauseViewData { Note = x.Note, PauseStart = x.From.ToUniversalTime(), PauseEnd = x.To.ToUniversalTime() });
                    }

                    ((VoiceCampaignPulseEditViewModel)model).Active = Active;
                    break;
                case Cyara.Domain.Types.Plan.PlanType.PulseOutbound:
                    ((VoiceCampaignPulseOutboundEditViewModel)model).TestCases =
                        TestCaseList.Select(x => new TestCaseSelectionViewData { TestCaseId = x.TestCaseId, Probability = Convert.ToDecimal(x.Probability) })
                            .ToList();
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Ports = PulseOutbound.ConcurrentPorts;
                    ((VoiceCampaignPulseOutboundEditViewModel)model).DurationLegend = PulseOutbound.RunFor.Unit == RunForUnit.Minutes
                                                                                          ? FormKeys.Mins
                                                                                          : FormKeys.Calls;
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Duration = PulseOutbound.RunFor.Value;
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Frequency = PulseOutbound.Frequency.Minutes;
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Distribution = PulseOutbound.TestCaseDistributionProfile.ToModelTestCaseDistribution();
                    ((VoiceCampaignPulseOutboundEditViewModel)model).Schedule = false;
                    if (PulseOutbound.Schedule != null && PulseOutbound.Schedule.Length > 0)
                    {
                        ((VoiceCampaignPulseOutboundEditViewModel)model).Schedule = true;
                        ((VoiceCampaignPulseOutboundEditViewModel)model).Schedules = PulseOutbound.Schedule.Select(x => new CampaignScheduleViewData { Day = (int)x.Day, FromTime = DateTime.Parse(x.From).ToUniversalTime().ToString("HH:mm"), ToTime = DateTime.Parse(x.To).ToUniversalTime().ToString("HH:mm") });
                    }

                    if (PulseOutbound.Pause != null && PulseOutbound.Pause.Length > 0)
                    {
                        ((VoiceCampaignPulseOutboundEditViewModel)model).Schedule = true;
                        ((VoiceCampaignPulseOutboundEditViewModel)model).SchedulePauses = PulseOutbound.Pause.Select(x => new SchedulePauseViewData { Note = x.Note, PauseStart = x.From.ToUniversalTime(), PauseEnd = x.To.ToUniversalTime() });
                    }

                    ((VoiceCampaignPulseOutboundEditViewModel)model).Active = Active;
                    break;
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
                            MediaTypeLabel = Cyara.Web.Portal.Core.Extensions.MediaTypeExtensions.ToLabel,
                            PlanTypeLabel = PlanTypeExtensions.ToDisplay
                        })
                    {
                        AccountId = account,
                        User = user,
                        CurrentPage = 1,
                        PageSize = int.MaxValue,
                        SortField =
                            Columns
                                .ScheduledRunDate,
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
    }
}
