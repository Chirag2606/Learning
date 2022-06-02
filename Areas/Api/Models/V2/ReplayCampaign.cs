namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Shared.Web.Mapping;

    public partial class ReplayCampaign
    {
        public static ReplayCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign)
        {
            if (campaign == null || campaign.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.Replay)
            {
                return null;
            }

            return new ReplayCampaign
            {
                ConcurrentPorts = campaign.ConcurrentPorts ?? 1,
                MaxCaps = campaign.MaxCaps,
                TestCaseDistributionProfile = Mapper.Map<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2.TestCaseDistributionProfile>(campaign.TestCaseDistributionProfile)
            };
        }
    }
}