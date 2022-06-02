namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    public partial class CyaraXmlImportResult
    {
        public static CyaraXmlImportResult From(Cyara.Web.Portal.Models.TestSpecificationConversionResult.ReportItem reportItem)
        {
            if (reportItem == null)
            {
                return null;
            }

            var type = reportItem.ItemType.ToCyaraXmlImportResultType();

            if (type == null)
            {
                return null;
            }

            return new CyaraXmlImportResult
            {
                Result = reportItem.Item,
                Type = type.Value
            };
        }
    }
}