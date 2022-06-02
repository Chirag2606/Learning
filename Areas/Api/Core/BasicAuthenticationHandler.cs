namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Extensions;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models.Api.V1_0;

    public class BasicAuthenticationHandler : DelegatingHandler
    {
        private static readonly Type Me = typeof(BasicAuthenticationHandler);

        private readonly ILogger _logger;

        public BasicAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "ip is a valid prefix here")]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IAccountService accountService = DependencyResolver.Current.GetService<IAccountService>();
            IAuthorisationManager authorisationManager = DependencyResolver.Current.GetService<IAuthorisationManager>();

            var sessionId = request.GetCorrelationId().ToString();
            HttpContextBase httpContext = (HttpContextBase)request.Properties["MS_HttpContext"];

            // indicate this was served via API
            var apiSession = new ApiSessionFacade(httpContext);
            apiSession.IsApiRequest = true;

            // Must have Authorization header
            if (request.Headers.Authorization != null)
            {
                string ipAddress = httpContext.Request.UserHostAddressReal();
                var credentials = ExtractCredentials(request.Headers.Authorization, sessionId);
                Tuple<User, string> loginResponse = await LoginUserAsync(credentials, sessionId, ipAddress, accountService);

                var user = loginResponse.Item1;

                // Must be able to login
                if (user != null)
                {
                    // pull back an Api Access Token for use with any Api 3.0 calls
                    var client = DependencyResolver.Current.GetService<IOAuth2Client>();

                    // let's use the mobile apps client details for now.  this is a temporary fix until we can retire the 2.x routes
                    var token = await client.RequestResourceOwnerPasswordAsync(
                                    credentials.Item1,
                                    credentials.Item2,
                                    Startup.Auth.Scope,
                                    additionalValues: new Dictionary<string, string> { { "client_id", "cyara.mobile" }, { "client_secret", "DFCD52F214AD4E14A9055B7149127B39" } },
                                    cancellationToken: cancellationToken);

                    if (token.IsError)
                    {
                        _logger.LogError(Me, $"Failed to get access token. Username:{credentials.Item1} Error:{token.Error}");
                    }

                    apiSession.User = user;
                    apiSession.SelectedAccount = user.FirstValidAccount(accountService, authorisationManager);

                    // this is required so that security attributes work fine
                    var identity = new GenericIdentity(credentials.Item1, "Basic");
                    identity.AddClaim(new Claim("access_token", token.AccessToken));

                    var principal = new ApiPrincipal(identity, user.Roles, httpContext);

                    request.Properties["MS_UserPrincipal"] = principal;
                    request.GetRequestContext().Principal = principal;
                    httpContext.User = principal;
                    Thread.CurrentPrincipal = principal;

                    // for API we want to hardcode the culture to English
                    var englishCulture = new CultureInfo("en-AU", false);
                    Thread.CurrentThread.CurrentCulture = englishCulture;
                    Thread.CurrentThread.CurrentUICulture = englishCulture;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }

                    return await base.SendAsync(request, cancellationToken);
                }

                return GenerateError(request, HttpStatusCode.Unauthorized, ApiMessageConstants.UsernamePasswordInvalid);
            }

            return GenerateError(request, HttpStatusCode.Unauthorized, ApiMessageConstants.UsernamePasswordRequired);
        }

        private HttpResponseMessage GenerateError(HttpRequestMessage request, HttpStatusCode code, string message)
        {
            var host = request.RequestUri.DnsSafeHost;

            var response = request.CreateErrorResponse(code, message);
            response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");

            return response;
        }

        /// <summary>
        /// Performs login operation
        /// </summary>
        /// <returns>Tuple(User, string) - User is user instance if successfully logged in, string is error message otherwise</returns>
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "ip is a valid prefix here")]
        private async Task<Tuple<User, string>> LoginUserAsync(Tuple<string, string> credentials, string sessionId, string ipAddress, IAccountService accountService)
        {
            try
            {
                if (string.IsNullOrEmpty(credentials?.Item1) || string.IsNullOrEmpty(credentials.Item2))
                {
                    return new Tuple<User, string>(null, ApiMessageConstants.UsernamePasswordRequired);
                }

                var response = await accountService.UserLoginAsync(new LoginRequest
                {
                    Username = credentials.Item1,
                    Password = credentials.Item2,
                    SessionId = sessionId,
                    IpAddress = ipAddress,
                    Host = HostType.Api
                });
                response.ExceptionIfError();

                if (response.IsSuccess)
                {
                    var user = response.Value;
                    return new Tuple<User, string>(user, null);
                }
                else
                {
                    _logger.LogInfoWithFormat(
                        Me,
                        "Authentication failed for Username:{0} SessionId:{1} ErrorCode:{2} Message:{3}".FormatWith(credentials.Item1, sessionId, response.Code(), response.ErrorMessage()));

                    return new Tuple<User, string>(null, ApiMessageConstants.Unauthorised.AddErrorCodeToMessage(response.ErrorResult, false));
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithFormat(Me, "Exception in LoginUserAsync. SessionId:{0} ipAddress:{1} Exception:{2}".FormatWith(sessionId, ipAddress, ex.ToString()));
                return new Tuple<User, string>(null, ApiMessageConstants.UnhandledException);
            }
        }

        /// <summary>
        /// Extract credentials from authentication header
        /// </summary>
        /// <returns>Tuple(string,string) where Item1 is username and Item2 is password</returns>
        private Tuple<string, string> ExtractCredentials(AuthenticationHeaderValue authHeader, string sessionId)
        {
            try
            {
                if (authHeader == null)
                {
                    _logger.LogInfoWithFormat(Me, "Authorization header is null, returning null. SessionId:{0}".FormatWith(sessionId));
                    return null;
                }

                if (authHeader.Scheme != "Basic")
                {
                    _logger.LogInfoWithFormat(Me, "Unsupported scheme {0}, returning null. SessionId:{1}".FormatWith(authHeader.Scheme, sessionId));
                    return null;
                }

                var encodedUserPass = authHeader.Parameter.Trim();
                var userPass = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUserPass));

                if (userPass.Count(x => x.Equals(':')) > 1)
                {
                    _logger.LogInfoWithFormat(Me, "User/password string contains more than one separator : This *may* result in a failed login if its in their username. SessionId:{0}".FormatWith(sessionId));
                }

                var parts = userPass.Split(":".ToCharArray(), 2);
                return new Tuple<string, string>(parts[0], parts[1]);
            }
            catch (Exception ex)
            {
                _logger.LogInfoWithFormat(Me, "Cannot extract credentials. SessionId:{0} Exception:{1}".FormatWith(sessionId, ex.ToString()));
                return null;
            }
        }
    }
}
