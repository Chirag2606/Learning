namespace Cyara.Web.Portal.Areas.Admin.Extensions
{
    using System.Web.Mvc;

    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Web.Identity;
    using Cyara.Shared.Web.Identity.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider;

    public static class IdentityProviderViewModelExtensions
    {
        public static IdentityProviderViewModel Prime(this IdentityProviderViewModel value, IdentitySettings settings)
        {
            value.LoginProviderList = new[] { new SelectListItem { Value = LoginProviders.Saml.ToString(), Text = LoginProviders.Saml.ToLabel() } };
            
            if (string.IsNullOrEmpty(value.ProviderId))
            {
                value.AssertionConsumerService = settings.GetSamlAssertionConsumerService("{provider}");
                value.ServiceProviderEntity = settings.GetSamlServiceProviderEntity("{provider}");
            }
            else
            {
                value.AssertionConsumerService = settings.GetSamlAssertionConsumerService(value.ProviderId);
                value.ServiceProviderEntity = settings.GetSamlServiceProviderEntity(value.ProviderId);
            }
            
            return value;
        }
    }
}