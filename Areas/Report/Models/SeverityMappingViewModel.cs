namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Cyara.Shared.Web.Models;

    public class SeverityMappingViewModel : BaseViewModel
    {
        public List<DetailedResultMappingViewData> DetailedResults { get; set; }

        public IEnumerable<SelectListItem> SeverityChoices { get; set; }
    }
}