namespace Cyara.Web.Portal.Areas.Api.Extensions
{
    using System.Linq;

    using Cyara.Web.Portal.Areas.Api.Models.V2;

    public static class CyaraXmlItemResultExtensions
    {
        /// <summary>
        /// Returns true if there is any value in this result item
        /// </summary>
        public static bool HasData(this CyaraXmlItemResult value)
        {
            return value != null && (value.Error.Any()
                                        || value.Information.Any()
                                        || !string.IsNullOrEmpty(value.Folder)
                                        || !string.IsNullOrEmpty(value.Name)
                                        || value.id.HasValue);
        }

        /// <summary>
        /// Returns true if there is any value in this result item
        /// </summary>
        public static bool HasData(this Cyara.Web.Portal.Areas.Api.Models.V2_2.CyaraXmlItemResult value)
        {
            return value != null && (value.Error.Any()
                                        || value.Information.Any()
                                        || !string.IsNullOrEmpty(value.Folder)
                                        || !string.IsNullOrEmpty(value.Name)
                                        || value.id.HasValue);
        }
    }
}