namespace Cyara.Web.Portal
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Helpers;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Data.Entity;
    using Cyara.Shared.Identity;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Identity;
    using Cyara.Shared.Web.Identity.Extensions;
    using Cyara.Shared.Web.Types.Journal;
    using Cyara.Web.Common.Extensions;
    using Cyara.Web.Journal.Repository;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Security;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Host.SystemWeb;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OpenIdConnect;

    using Owin;

    public partial class Startup
    {
        private static readonly Type Me = MethodBase.GetCurrentMethod().DeclaringType;

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            var settings = DependencyResolver.Current.GetService<IdentitySettings>();
            var configService = DependencyResolver.Current.GetService<IConfigurationService>();
            var siteUrl = configService.Get(ConfigurationKey.WebPortalSiteUrl.Key);

            // Configure the db context, user manager and sign in manager to use a single instance per request
            app.CreatePerOwinContext(
                () =>
                {
                    var identityContext = DependencyResolver.Current.GetService<CyaraIdentityContext>();
                    return identityContext;
                });

            app.CreatePerOwinContext(
                () =>
                {
                    var userManager = DependencyResolver.Current.GetService<CyaraUserManager>();
                    return userManager;
                });

            // Load our roles from the database
            ClaimsIdentityExtensions.SetRoleAccess(DependencyResolver.Current.GetService<CyaraIdentityContext>().GetRoleAccessSet());

            // rest api uses its own BasicAuthenticationHandler to authenticate and should not use cookies
            app.Map("/api", ctx => { });

            app.Map("/Content", ctx => { });

            app.Map("/Scripts", ctx => { });

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Provider =
                            new CookieAuthenticationProvider
                            {
                                OnValidateIdentity = OnValidateIdentity(TimeSpan.FromSeconds(0)),
                                OnApplyRedirect = ctx =>
                                {
                                    // in case of Unauthorized response when user is logged in do not redirect to login page
                                    if (ctx.Request.User.Identity.IsAuthenticated && ctx.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                                    {
                                        return;
                                    }

                                    ctx.Response.Redirect(ctx.RedirectUri);
                                }
                            },
                SlidingExpiration = settings.CookieSlidingExpiration,
                ExpireTimeSpan = settings.CookieExpireTimeSpan,
                CookieManager = new SameSiteCookieLaxMinimumManager(new SystemWebCookieManager()),
                CookieSameSite = SameSiteMode.Lax,
                SessionStore = DependencyResolver.Current.GetService<IAuthenticationSessionStore>()
            });

            app.UseOpenIdConnectAuthentication(openIdConnectOptions: new OpenIdConnectAuthenticationOptions
            {
                Authority = settings.OidcAuthority,
                ClientId = Auth.ClientId,
                ResponseType = Auth.ResponseType,
                RequireHttpsMetadata = false,
                UseTokenLifetime = false,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Scope = Auth.Scope,
                CookieManager = new SameSiteCookieLaxMinimumManager(new SystemWebCookieManager()),
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                    {
                        var id = n.AuthenticationTicket.Identity;

                        id.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));
                        id.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        n.AuthenticationTicket = new AuthenticationTicket(id, n.AuthenticationTicket.Properties);

                        // we cannot refresh the token (claims) without re-logging the user on.
                        // as the security stamp can change (update password) persist this outside of the token.
                        var securityStamp = n.AuthenticationTicket.Identity.Claims.FirstOrDefault(x => x.Type == "security_stamp")?.Value;
                        var handler = DependencyResolver.Current.GetService<ISecurityStampHandler>();
                        handler.WriteStamp(securityStamp, n.OwinContext);

                        // This shouldn't exist here, but there's no other way without refactoring journaling for the whole platform.
                        // Currently, Identity adds this record to the DB, but it can't add it to the real time log, so we have to do it here.
                        var journal = DependencyResolver.Current.GetService<IJournalRepository>();
                        journal?.AppendToRealTimeLog(new JournalEntity
                        {
                            IpAddress = n.Request.RemoteIpAddress,
                            Category = Category.Security.Name,
                            DateCreated = DateTime.UtcNow,
                            SubCategory = Category.Security.SubCategory.Login,
                            UserId = Guid.Parse(n.AuthenticationTicket.Identity.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value),
                            Result = Result.Success
                        });

                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Logout)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token")?.Value;
                            n.ProtocolMessage.IdTokenHint = idTokenHint;
                        }

                        // This assumes the identity server config has an allowed URL that matches *exact* including the trailing slash.
                        siteUrl = siteUrl.EnsureTrailingSlash();

                        n.ProtocolMessage.RedirectUri = siteUrl;
                        n.ProtocolMessage.PostLogoutRedirectUri = siteUrl;

                        // if this is ajax request do not redirect to identity site, just return whatever status code is there already
                        if (StringComparer.OrdinalIgnoreCase.Equals(n.OwinContext.Request.Headers["X-Requested-With"], "XMLHttpRequest") || StringComparer.OrdinalIgnoreCase.Equals(n.OwinContext.Request.Headers["x-requested-with"], "XMLHttpRequest"))
                        {
                            // stop the cookie size blowing up with the middleware adding new nonce cookies when it redirects to the identity provider. 
                            n.Response.Headers.RemoveCookie("OpenIdConnect.nonce.");
                            n.HandleResponse();
                        }

                        return Task.FromResult(0);
                    }
                }
            });
        }

        private static Func<CookieValidateIdentityContext, Task> OnValidateIdentity(TimeSpan validateInterval)
        {
            return async context =>
                       {
                           // setting the expiry datetime for this cookie
                           var claimType = nameof(AuthenticationProperties.ExpiresUtc);

                           DateTimeOffset? expireUtc = context.Properties.ExpiresUtc;

                           if (expireUtc.HasValue)
                           {
                               if (context.Identity.HasClaim(c => c.Type == claimType))
                               {
                                   Claim existingClaim = context.Identity.FindFirst(claimType);
                                   context.Identity.RemoveClaim(existingClaim);
                               }

                               Claim newClaim = new Claim(claimType, expireUtc.Value.UtcTicks.ToString());
                               context.Identity.AddClaim(newClaim);
                           }

                           // --
                           DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
                           if (context.Options?.SystemClock != null)
                           {
                               currentUtc = context.Options.SystemClock.UtcNow;
                           }

                           DateTimeOffset? issuedUtc = context.Properties.IssuedUtc;
                           var validate = !issuedUtc.HasValue;
                           if (issuedUtc.HasValue)
                           {
                               validate = currentUtc.Subtract(issuedUtc.Value) > validateInterval;
                           }

                           if (!validate)
                           {
                               return;
                           }

                           var manager = context.OwinContext.GetUserManager<CyaraUserManager>();
                           var id = context.Identity.GetUserId();
                           if (manager == null || id == null)
                           {
                               return;
                           }

                           var userId = Guid.Parse(id);

                           var user = await manager.FindByIdAsync(userId);
                           var reject = true;
                           if (user != null && manager.SupportsUserSecurityStamp)
                           {
                               var handler = DependencyResolver.Current.GetService<ISecurityStampHandler>();

                               var securityStamp = handler.GetStamp(context.OwinContext);
                               if (securityStamp == await manager.GetSecurityStampAsync(userId))
                               {
                                   reject = false;
                               }
                           }

                           if (!reject)
                           {
                               return;
                           }

                           var logger = DependencyResolver.Current.GetService<ILogger>();
                           logger.LogInfo(Me, $"Security stamp mismatch, logging user off. UserId:{userId}");

                           context.OwinContext.Authentication.SignOut();
                       };
        }

        internal class Auth
        {
            public const string ClientId = "cyara.web.portal";

            public const string ResponseType = "id_token token";

            public const string Scope = "openid profile roles identity web_api zendesk_sso inthub introspect";
        }
    }
}