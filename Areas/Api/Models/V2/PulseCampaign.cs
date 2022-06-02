namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Linq;

    using Cyara.Shared.Web.Mapping;
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class PulseCampaign
    {
        public static PulseCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign, IDataHelper dataHelper)
        {
            if (campaign == null || campaign.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.Pulse)
            {
                return null;
            }

            return new PulseCampaign
            {
                ConcurrentPorts = campaign.ConcurrentPorts ?? 1,
                Frequency = new FrequencyUnit { Minutes = (int)campaign.Frequency.Value.TotalMinutes },
                MaxCaps = campaign.MaxCaps, 
                TestCaseDistributionProfile = Mapper.Map<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2.TestCaseDistributionProfile>(campaign.TestCaseDistributionProfile),
                Schedule = (campaign.Schedules == null || campaign.Schedules.Count <= 0) 
                    ? null 
                    : campaign.Schedules.Select(x => SchedulePeriod.FromVal(x, dataHelper)).ToArray()
            };
        }
    }
}