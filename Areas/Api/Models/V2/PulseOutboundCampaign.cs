namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Linq;

    using AutoMapper;

    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class PulseOutboundCampaign
    {
        public static PulseOutboundCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign, IDataHelper dataHelper)
        {
            if (campaign == null || campaign.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.PulseOutbound)
            {
                return null;
            }

            return new PulseOutboundCampaign
            {
                ConcurrentPorts = campaign.ConcurrentPorts ?? 1,
                Frequency = new FrequencyUnit { Minutes = (int)campaign.Frequency.Value.TotalMinutes },
                MaxCaps = campaign.MaxCaps,
                RunFor = RunFor.From(campaign),
                TestCaseDistributionProfile = Mapper.Map<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2.TestCaseDistributionProfile>(campaign.TestCaseDistributionProfile),
                Schedule = (campaign.Schedules == null || campaign.Schedules.Count <= 0)
                    ? null
                    : campaign.Schedules.Select(x => SchedulePeriod.FromVal(x, dataHelper)).ToArray()
            };
        }
    }
}