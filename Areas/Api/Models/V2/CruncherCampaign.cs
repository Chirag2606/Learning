namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Shared.Web.Mapping;

    public partial class CruncherCampaign
    {
        public static CruncherCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign, bool lite)
        {
            if (campaign == null || campaign.Plan == null
                || (lite == false && campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.Cruncher)
                || (lite == true && campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.CruncherLite))
            {
                return null;
            }

            return new CruncherCampaign
            {
                ConcurrentPorts = campaign.ConcurrentPorts ?? 1,
                MaxCaps = campaign.MaxCaps,
                RampUpTime = TimeUnitSeconds.From(campaign.RampUpTime),
                RunFor = RunFor.From(campaign),
                TestCaseDistributionProfile = Mapper.Map<Domain.Types.Campaign.TestCaseDistributionProfile, Models.V2.TestCaseDistributionProfile>(campaign.TestCaseDistributionProfile)
            };
        }
    }
}