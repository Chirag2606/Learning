namespace Cyara.Web.Portal.Areas.Admin.Models.Notification
{
    using System.Collections.Generic;
    using System.Linq;

    using Cyara.Shared.Web.Models;

    public class EmailLogsViewData : PaginatedView
    {
        public EmailLogsViewData()
        {
            Logs = Enumerable.Empty<EmailLogViewData>();
        }

        public string SearchTerm { get; set; }

        public IEnumerable<EmailLogViewData> Logs { get; set; }
    }
}