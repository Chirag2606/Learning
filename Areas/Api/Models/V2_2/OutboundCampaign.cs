namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Shared.Web.Mapping;

    public partial class OutboundCampaign
    {
        public static OutboundCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign)
        {
            if (campaign == null || campaign.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.Outbound)
            {
                return null;
            }

            return new OutboundCampaign
            {
                ConcurrentPorts = campaign.ConcurrentPorts ?? 1,
                MaxCaps = campaign.MaxCaps,
                RunFor = RunFor.From(campaign),
                TestCaseDistributionProfile = Mapper.Map<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2_2.TestCaseDistributionProfile>(campaign.TestCaseDistributionProfile)
            };
        }
    }
}