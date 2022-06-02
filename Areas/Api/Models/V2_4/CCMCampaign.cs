namespace Cyara.Web.Portal.Areas.Api.Models.V2_4
{
    using System.Linq;

    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class CCMCampaign
    {
        public static CCMCampaign From(Cyara.Shared.Types.Campaign.Campaign campaign, IDataHelper dataHelper)
        {
            if (campaign?.Plan == null || campaign.Plan.PlanType != Cyara.Domain.Types.Plan.PlanType.VirtualAgent)
            {
                return null;
            }

            return new CCMCampaign
                   {
                       RunFor = RunFor.From(campaign),
                       Schedule = CCMCampaignSchedule.From(campaign, dataHelper),
                       Pause = campaign.SchedulePauses == null || campaign.SchedulePauses.Count == 0
                                   ? null
                                   : campaign.SchedulePauses.Select(x => SchedulePause.FromVal(x, dataHelper)).ToArray()
            };
        }
    }
}