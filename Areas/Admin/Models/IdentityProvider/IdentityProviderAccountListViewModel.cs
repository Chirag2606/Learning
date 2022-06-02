namespace Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider
{
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Types.Constants;

    public class IdentityProviderAccountListViewModel : PaginatedView<IdentityProviderAccountViewData>
    {
        public IdentityProviderAccountListViewModel()
        {
            PageNumber = 1;
            PageSize = MvcApplication.Settings.DefaultPageSize;
            SortColumn = Columns.Name;
            SortAscending = true;
        }
    }
}