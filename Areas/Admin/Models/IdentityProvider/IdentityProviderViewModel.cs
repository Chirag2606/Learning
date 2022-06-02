namespace Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Cyara.Shared.Web.Models;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Resources;

    using LoginProviders = Cyara.Shared.Types.Authorisation.LoginProviders;

    public class IdentityProviderViewModel : BaseViewModel
    {
        public int IdentityProviderId { get; set; }

        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Type))]
        [Display(Name = "IdentityProviderType", ResourceType = typeof(Labels))]
        public LoginProviders LoginProvider { get; set; }

        public IEnumerable<SelectListItem> LoginProviderList { get; set; }

        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.ProviderId))]
        [Display(Name = "IdentityProviderId", ResourceType = typeof(Labels))]
        public string ProviderId { get; set; }

        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Name))]
        [Display(Name = "Name", ResourceType = typeof(Labels))]
        public string Name { get; set; }

        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Active))]
        public bool Active { get; set; }

        // SAML Options
        [Display(Name = "Certificate", ResourceType = typeof(Labels))]
        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Settings) + "." + nameof(CyaraWebApi.SamlProviderSettingsModel.Certificate))]
        public string Certificate { get; set; }

        [Display(Name = "AssertionConsumerService", ResourceType = typeof(Labels))]
        public string AssertionConsumerService { get; set; }

        [Display(Name = "ServiceProviderEntity", ResourceType = typeof(Labels))]
        public string ServiceProviderEntity { get; set; }

        [Display(Name = "IdentityProviderUrl", ResourceType = typeof(Labels))]
        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Settings) + "." + nameof(CyaraWebApi.SamlProviderSettingsModel.IdentityProviderUrl))]
        public string IdentityProviderUrl { get; set; }

        [Display(Name = "Metadata", ResourceType = typeof(Labels))]
        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Settings) + "." + nameof(CyaraWebApi.SamlProviderSettingsModel.Metadata))]
        public string Metadata { get; set; }

        [Display(Name = "MetadataUrl", ResourceType = typeof(Labels))]
        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Settings) + "." + nameof(CyaraWebApi.SamlProviderSettingsModel.MetadataUrl))]
        public string MetadataUrl { get; set; }

        [Display(Name = "SingleSignOnUrl", ResourceType = typeof(Labels))]
        [MapsToApiProperty(nameof(CyaraWebApi.IdentityProviderModel.Settings) + "." + nameof(CyaraWebApi.SamlProviderSettingsModel.SingleSignOnUrl))]
        public string SingleSignOnUrl { get; set; }
    }
}