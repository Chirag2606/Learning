namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using Cyara.Shared.Web.Mapping;

    public partial class VelocityCampaign
    {
        public static VelocityCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign)
        {
            if (campaign?.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.Velocity)
            {
                return null;
            }

            return new VelocityCampaign
                   {
                       ConcurrentPorts = campaign.ConcurrentPorts ?? 1,
                       MaxCaps = campaign.MaxCaps,
                       TestCaseDistributionProfile = Mapper.Map<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2_4.TestCaseDistributionProfile>(campaign.TestCaseDistributionProfile),
                   };
        }
    }
}