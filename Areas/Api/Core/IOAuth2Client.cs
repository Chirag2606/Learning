namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Thinktecture.IdentityModel.Client;

    public interface IOAuth2Client
    {
        Task<TokenResponse> RequestResourceOwnerPasswordAsync(
            string userName,
            string password,
            string scope = null,
            Dictionary<string, string> additionalValues = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}