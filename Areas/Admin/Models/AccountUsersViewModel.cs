namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Types.Constants;

    public class AccountUsersViewModel : PaginatedView<UserListViewData>
    {
        public AccountUsersViewModel()
        {
            PageNumber = 1;
            PageSize = MvcApplication.Settings.DefaultPageSize;
            SortColumn = Columns.Username;
            SortAscending = true;
        }
    }
}