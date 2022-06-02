namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class CyaraXmlImportResultList
    {
        public static CyaraXmlImportResultList From(IEnumerable<Cyara.Web.Portal.Models.TestSpecificationConversionResult.ReportItem> reportItems)
        {
            if (reportItems == null || !reportItems.Any())
            {
                return null;
            }

            return new CyaraXmlImportResultList { CyaraXmlImportResult = reportItems.Select(V2.CyaraXmlImportResult.From).Where(x => x != null).ToArray() };
        }
    }
}