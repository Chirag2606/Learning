namespace Cyara.Web.Portal.Areas.Api.Models.V2_2
{
    using System.Xml.Serialization;

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
    }
}