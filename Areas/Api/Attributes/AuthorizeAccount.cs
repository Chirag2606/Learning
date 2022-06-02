namespace Cyara.Web.Portal.Areas.Api.Attributes
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http.Controllers;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Resources;

    public class AuthorizeAccount : System.Web.Http.AuthorizeAttribute
    {
        public AuthorizeAccount(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }

        // Based on the base.IsAuthorized from the AuthorizeAttribute
        protected virtual bool IsAuthorizedBase(HttpActionContext actionContext)
        {
            return actionContext.ValidateUserRoles(Users, Roles);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            ApiSessionFacade session = new ApiSessionFacade(actionContext.ControllerContext);

            // Must have logged in as a valid user.
            // NOTE session.User can be null if using test page and username/password are unknown, but you are actually logged in, so base.IsAuthorized returns true
            if (!IsAuthorizedBase(actionContext) || session.User == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, 
                    ApiMessages.Unauthorised);
                return false;
            }

            // Must have account in route
            var data = actionContext.Request.GetRouteData();
            var account = data?.Values["account"];
            int accountId;
            if (account == null || !int.TryParse(account.ToString(), out accountId))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    ApiMessages.Unauthorised_ForAccount);
                return false;
            }

            // Must have access to account.  TODO - save auth for later use?
            var auth = UserAccountAuthoriser.AuthoriseAccount(session.User, accountId);
            if (auth == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    ApiMessages.Unauthorised_ForAccount);
                return false;
            }

            // Only CCM can access CCM data and vice-versa
            var scope = data != null ? data.Values["scope"] : null;
            var target = ApiModel.TargetForScope((string)scope);
            if (target.HasValue && !target.Value.HasAccess(Thread.CurrentPrincipal))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized,
                    ApiMessages.Unauthorised);
                return false;
            }

            if (session.User == null 
                || !DependencyResolver.Current.GetService<IAuthorisationManager>().HasAccess(session.User.Roles, session.User.AreaAccess, ResourceType.SiteInMaintenance, AccessType.Read))
            {
                bool isInMainenanceMode;
                if (bool.TryParse(DependencyResolver.Current.GetService<IConfigurationService>().Get(ConfigurationKey.MaintenanceModePortal.Key), out isInMainenanceMode) && isInMainenanceMode)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.ServiceUnavailable,
                        Cyara.Web.Resources.ApiMessages.InMaintenanceMode);
                    return false;
                }
            }

            session.SelectedAccount = auth;
            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext != null)
            {
                return;
            }

            throw new ArgumentNullException(nameof(actionContext));
        }
    }
}
