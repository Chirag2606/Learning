namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using Cyara.Web.Portal.Areas.Api.Core;

    public partial class CampaignRunSummaryPrevious
    {
        public static CampaignRunSummaryPrevious From(Cyara.Shared.Types.Campaign.CampaignRun run, IDataHelper dataHelper, string liveStatus = null)
        {
            if (run != null)
            {
                return new CampaignRunSummaryPrevious { Run = CampaignRun.From(run, dataHelper, liveStatus) };
            }

            return null;
        }
    }
}