namespace Cyara.Web.Portal.Areas.Api.Models.V2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Web.Portal.Areas.Api.Extensions;

    public partial class CyaraXmlItemResultList
    {
        public static CyaraXmlItemResultList From(IEnumerable<Cyara.Web.Portal.Models.TestSpecificationConversionResult.ReportItem> reportItems)
        {
            if (reportItems == null || !reportItems.Any())
            {
                return null;
            }

            var resList = new List<CyaraXmlItemResult>();
            CyaraXmlItemResult curRes = null;
            
            var infoList = new List<CyaraXmlResult>();
            var errList = new List<CyaraXmlResult>();

            foreach (var r in reportItems.OrderBy(x => x.XPath))
            {
                if (curRes == null 
                    || !r.XPath.StartsWith(curRes.xPath) 
                    || (curRes.xPath == "/" && r.XPath != "/"))
                {
                    if (curRes != null)
                    {
                        curRes.Information = infoList.ToArray();
                        curRes.Error = errList.ToArray();
                        if (curRes.HasData())
                        {
                            resList.Add(curRes);
                        }
                    }

                    curRes = new CyaraXmlItemResult { xPath = r.XPath };
                    infoList = new List<CyaraXmlResult>();
                    errList = new List<CyaraXmlResult>();
                }

                switch (r.ItemType)
                {
                    case Portal.Models.TestSpecificationConversionResult.ReportItemType.Id:
                        curRes.id = Convert.ToInt32(r.Item);
                        break;
                    case Portal.Models.TestSpecificationConversionResult.ReportItemType.Folder:
                        curRes.Folder = r.Item;
                        break;
                    case Portal.Models.TestSpecificationConversionResult.ReportItemType.Title:
                        curRes.Name = r.Item;
                        break;
                    case Portal.Models.TestSpecificationConversionResult.ReportItemType.Progress:
                        // infoList.Add(new CyaraXmlResult { xPath = r.xPath, Result = r.Item });
                        break;
                    case Portal.Models.TestSpecificationConversionResult.ReportItemType.Information:
                        infoList.Add(new CyaraXmlResult { xPath = r.XPath, Result = r.Item });
                        break;
                    case Portal.Models.TestSpecificationConversionResult.ReportItemType.Error:
                        errList.Add(new CyaraXmlResult { xPath = r.XPath, Result = r.Item });
                        break;
                }
            }

            if (curRes != null)
            {
                curRes.Information = infoList.ToArray();
                curRes.Error = errList.ToArray();
                resList.Add(curRes);
            }

            return new CyaraXmlItemResultList { CyaraXmlItemResult = resList.ToArray() };
        }
    }
}