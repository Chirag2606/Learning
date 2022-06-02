namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using System.Collections.Generic;

    using Cyara.Shared.Web.Models;
    using Cyara.Web.Portal.Models;

    public class AccountsListViewModel : PaginatedView
    {
        public IEnumerable<AccountViewData> Accounts { get; set; }

        public int? ChosenAccountId { get; set; }
    }
}