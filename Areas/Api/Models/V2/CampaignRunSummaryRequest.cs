namespace Cyara.Web.Portal.Areas.Api.Models.V2
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
            switch (status)
            {
                case Domain.Types.Campaign.CampaignStatus.Aborted:
                    return CampaignStatus.Aborted;
                case Domain.Types.Campaign.CampaignStatus.ClashingCalledNumbers:
                    return CampaignStatus.ClashingCalledNumbers;
                case Domain.Types.Campaign.CampaignStatus.Completed:
                    return CampaignStatus.Completed;
                case Domain.Types.Campaign.CampaignStatus.ExceededPorts:
                    return CampaignStatus.ExceededPorts;
                case Domain.Types.Campaign.CampaignStatus.GenerationFailed:
                    return CampaignStatus.GenerationFailed;
                case Domain.Types.Campaign.CampaignStatus.InternalError:
                    return CampaignStatus.InternalError;
                case Domain.Types.Campaign.CampaignStatus.None:
                    return CampaignStatus.None;
                case Domain.Types.Campaign.CampaignStatus.Pending:
                    return CampaignStatus.Pending;
                case Domain.Types.Campaign.CampaignStatus.Queued:
                    return CampaignStatus.Queued;
                case Domain.Types.Campaign.CampaignStatus.Running:
                    return CampaignStatus.Running;
            }

            throw new Exception("Error converting CampaignStatus");
        }
    }
}