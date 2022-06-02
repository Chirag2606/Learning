namespace Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider
{
    public class IdentityProviderListViewData
    {
        public int IdentityProviderId { get; set; }

        public string ProviderId { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public string Type { get; set; }

        public int UsedIn { get; set; }

        public string StatusLabel { get; set; }

        public string StatusClass { get; set; }
    }
}