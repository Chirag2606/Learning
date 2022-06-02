namespace Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider
{
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Types.Constants;

    public class IdentityProviderListViewModel : PaginatedView<IdentityProviderListViewData>
    {
        public IdentityProviderListViewModel()
        {
            PageNumber = 1;
            PageSize = MvcApplication.Settings.DefaultPageSize;
            SortColumn = Columns.Name;
            SortAscending = true;
        }
    }
}