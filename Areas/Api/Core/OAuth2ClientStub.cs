namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Cyara.Shared.Web.Identity;
    using Cyara.Web.Common.Extensions;

    using Thinktecture.IdentityModel.Client;
    
    public class OAuth2ClientStub : IOAuth2Client
    {
        private readonly string _url;

        public OAuth2ClientStub(IdentitySettings settings)
        {
            _url = settings.OidcAuthority.EnsureTrailingSlash() + "connect/token";
        }
        
        public Task<TokenResponse> RequestResourceOwnerPasswordAsync(
            string userName,
            string password,
            string scope = null,
            Dictionary<string, string> additionalValues = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = new OAuth2Client(new Uri(_url));

            // let's use the mobile apps client details for now.  this is a temporary fix until we can retire the 2.x routes
            return client.RequestResourceOwnerPasswordAsync(
                            userName, 
                            password, 
                            Startup.Auth.Scope,
                            additionalValues: new Dictionary<string, string> { { "client_id", "cyara.mobile" }, { "client_secret", "DFCD52F214AD4E14A9055B7149127B39" } },
                            cancellationToken: cancellationToken);
        }
    }
}