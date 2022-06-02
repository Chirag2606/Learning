namespace Cyara.Web.Portal.Areas.Api.Models.V2_5
{
    using System.Xml.Serialization;

    using Cyara.Web.Portal.Areas.Api.Core;

    using Newtonsoft.Json;

    public partial class CampaignRun
    {
        /// <summary>
        /// Using field "[field name]Specified" we control visibility of related fields "[field name]", 
        /// <see cref="http://stackoverflow.com/questions/5818513/xml-serialization-hide-null-values"/>
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public bool EndDateSpecified { get; set; }

        public static CampaignRun From(Shared.Types.Campaign.CampaignRun run, IDataHelper dataHelper, string liveStatus = null)
        {
            return AutoMapper.Mapper.Map<V2.CampaignRun, CampaignRun>(V2.CampaignRun.From(run, dataHelper, liveStatus));
        }
    }
}