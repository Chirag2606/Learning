namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System;

    public partial class CampaignRunsForCampaignRequest
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int? Page { get; set; }

        public int? PerPage { get; set; }
    }
}