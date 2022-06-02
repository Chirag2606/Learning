namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System.Collections.Generic;

    public partial class CampaignRunsForCampaignResponse
    {
        public int CampaignId { get; internal set; }

        public string CampaignName { get; internal set; }

        public List<CampaignRun> RunHistory { get; internal set; }

        public string SelfLink { get; internal set; }

        public string NextLink { get; internal set; }

        public string PreviousLink { get; internal set; }

        public string FirstLink { get; internal set; }

        public string LastLink { get; internal set; }
    }
}