namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Collections.Generic;

    using Cyara.Domain.Types.Shared;
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class CCMCampaignSchedule
    {
        public static CCMCampaignSchedule From(Cyara.Shared.Types.Campaign.Campaign campaign, IDataHelper dataHelper)
        {
            if (campaign == null || campaign.Schedules == null || !campaign.Frequency.HasValue)
            {
                return null;
            }

            var periods = new List<SchedulePeriod>();
            foreach (Schedule s in campaign.Schedules)
            {
                periods.Add(SchedulePeriod.FromVal(s, dataHelper));
            }

            return new CCMCampaignSchedule
            {
                ResetInterval = new FrequencyUnit { Minutes = (int)campaign.Frequency.Value.TotalMinutes },
                Period = periods.ToArray()
            };
        }
    }
}