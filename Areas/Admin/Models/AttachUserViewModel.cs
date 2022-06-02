namespace Cyara.Web.Portal.Areas.Admin.Models
{
    using Cyara.Shared.Web.Models;

    public class AttachUserViewModel : PaginatedView<UserViewData>
    {
        public string Search { get; set; }        
    }
}