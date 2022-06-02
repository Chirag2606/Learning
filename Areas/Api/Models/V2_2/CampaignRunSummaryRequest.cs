namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System;

    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class CampaignRunSummaryRequest
    {
        public static CampaignRunSummaryRequest From(Cyara.Shared.Types.Campaign.ICampaign campaign, Cyara.Shared.Types.Campaign.CampaignRun run, IDataHelper dataHelper)
        {
            // Check that we have a requested next run date
            if (campaign != null && campaign.NextRun.HasValue)
            {
                // Only display it, if it hasn't been run yet!
                if (run == null || run.StartDate < campaign.NextRun.Value)
                {
                    return new CampaignRunSummaryRequest { RunDate = dataHelper.Output(campaign.NextRun.Value), Status = StatusFrom(campaign.Status) };
                }
            }

            return null;
        }

        public static CampaignStatus StatusFrom(Cyara.Domain.Types.Campaign.CampaignStatus status)
        {
            CampaignStatus retValue;
            
            if (!Enum.TryParse(status.ToString(), true, out retValue))
            {
                throw new Exception("Error converting CampaignStatus");
            }

            return retValue;
        }
    }
}