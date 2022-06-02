namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System;

    using Cyara.Shared.Web.Models;

    [Serializable]
    public class CustomReportViewModel : BaseViewModel
    {
        /// <summary>
        /// Page worth of Custom Report items
        /// </summary>
        public PaginatedView<CustomReportViewData> Reports { get; set; }
    }
}