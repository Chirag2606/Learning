namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class CCMCampaign
    {
        public static CCMCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign, IDataHelper dataHelper)
        {
            if (campaign == null || campaign.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.VirtualAgent)
            {
                return null;
            }

            return new CCMCampaign
            {
                RunFor = RunFor.From(campaign),
                Schedule = CCMCampaignSchedule.From(campaign, dataHelper),
            };
        }
    }
}