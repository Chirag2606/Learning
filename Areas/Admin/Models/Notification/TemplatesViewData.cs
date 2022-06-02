namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using System.Linq;
    using System.Web.Mvc;

    using Cyara.Shared.Web.Models;

    public class TemplatesViewData : PaginatedView<TemplateViewModel>
    {       
        public TemplatesViewData()
        {
            Collection = Enumerable.Empty<TemplateViewModel>();
        }

        [AllowHtml]
        public string SearchTerm { get; set; }
    }
}