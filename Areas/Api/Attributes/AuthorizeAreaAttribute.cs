namespace Cyara.Web.Portal.Areas.Api.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Config;
    using Cyara.Shared.Types.Authorisation;
    using Cyara.Shared.Types.Shared;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Web.Portal.Areas.Api.Models;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Resources;

    public class AuthorizeAreaAttribute : System.Web.Http.AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAreaAttribute"/> class.  This allows you to specify access for a secured area based on the role capacities given.
        /// Each parameter is an <see cref="Access"/> list.  If the user has any of the access levels listed for the area, then they are allowed to use this controller or method.
        /// <para>
        /// If a user must have View and Execute permissions then these can be joined with an OR; new[] { Access.View | Access.Execute }.  This will ensure that the user has BOTH permissions.
        /// </para>
        /// <para>
        /// If a user can have either View or Execute permissions, then add these separately to the list; new[] { Access.View, Access.Execute }.  This will verify the user has EITHER permission for the area listed
        /// </para>
        /// <para>
        /// If more than one area is specified, then if the user has any required access to any of the areas they will be allowed to use the method or class.
        /// </para>
        /// </summary>
        /// <param name="account">
        /// If specified, the User must have access to <see cref="SecuredArea.Account"/>
        /// </param>
        /// <param name="accountPlan">
        /// If specified, the User must have access to <see cref="SecuredArea.AccountPlan"/>
        /// </param>
        /// <param name="accountUser">
        /// If specified, the User must have access to <see cref="SecuredArea.AccountUser"/>
        /// </param>
        /// <param name="accountReport">
        /// If specified, the User must have access to <see cref="SecuredArea.AccountReport"/>
        /// </param>
        /// <param name="testCase">
        /// If specified, the User must have access to <see cref="SecuredArea.TestCase"/>
        /// </param>
        /// <param name="block">
        /// If specified, the User must have access to <see cref="SecuredArea.Block"/>
        /// </param>
        /// <param name="campaign">
        /// If specified, the User must have access to <see cref="SecuredArea.Campaign"/>
        /// </param>
        /// <param name="report">
        /// If specified, the User must have access to <see cref="SecuredArea.Report"/>
        /// </param>
        /// <param name="testTools">
        /// If specified, the User must have access to <see cref="SecuredArea.TestTools"/>
        /// </param>
        /// <param name="integration">
        /// If specified, the User must have access to <see cref="SecuredArea.Integration"/>
        /// </param>
        /// <param name="dashboard">
        /// If specified, the User must have access to <see cref="SecuredArea.Dashboard"/>
        /// </param>
        /// <param name="agent">
        /// If specified, the User must have access to <see cref="SecuredArea.Agent"/>
        /// </param>
        /// <param name="agentCampaign">
        /// If specified, the User must have access to <see cref="SecuredArea.AgentCampaign"/>
        /// </param>
        /// <param name="agentReport">
        /// If specified, the User must have access to <see cref="SecuredArea.AgentReport"/>
        /// </param>
        /// <param name="model">
        /// If specified, the User must have access to <see cref="SecuredArea.CxModel"/>
        /// </param>
        // ReSharper disable SuggestBaseTypeForParameter - Invalid for Attributes
        public AuthorizeAreaAttribute(
            Access[] account = null,
            Access[] accountPlan = null,
            Access[] accountUser = null,
            Access[] accountReport = null,
            Access[] testCase = null,
            Access[] block = null,
            Access[] campaign = null,
            Access[] report = null,
            Access[] testTools = null,
            Access[] integration = null, 
            Access[] dashboard = null, 
            Access[] agent = null, 
            Access[] agentCampaign = null, 
            Access[] agentReport = null, 
            Access[] model = null)
        {
            Roles = string.Empty;
            Allowed = new Dictionary<SecuredArea, IList<Access>>();

            void AddIfNotNull(SecuredArea area, IList<Access> access)
            {
                if (access?.Count > 0)
                {
                    this.Allowed.Add(area, access);
                }
            }

            AddIfNotNull(SecuredArea.Account, account);
            AddIfNotNull(SecuredArea.AccountPlan, accountPlan);
            AddIfNotNull(SecuredArea.AccountUser, accountUser);
            AddIfNotNull(SecuredArea.AccountReport, accountReport);
            AddIfNotNull(SecuredArea.TestCase, testCase);
            AddIfNotNull(SecuredArea.Block, block);
            AddIfNotNull(SecuredArea.Campaign, campaign);
            AddIfNotNull(SecuredArea.Report, report);
            AddIfNotNull(SecuredArea.TestTools, testTools);
            AddIfNotNull(SecuredArea.Integration, integration);
            AddIfNotNull(SecuredArea.Dashboard, dashboard);
            AddIfNotNull(SecuredArea.Agent, agent);
            AddIfNotNull(SecuredArea.AgentCampaign, agentCampaign);
            AddIfNotNull(SecuredArea.AgentReport, agentReport);
            AddIfNotNull(SecuredArea.CxModel, model);
        }

        public Dictionary<SecuredArea, IList<Access>> Allowed { get; }

        // Based on the base.IsAuthorized from the AuthorizeAttribute
        protected virtual bool IsAuthorizedBase(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            IPrincipal user = actionContext.ControllerContext.RequestContext.Principal;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Users) && !Users.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            var accessSet = user.AreaAccess();
            return Allowed.Any(
                    a =>
                        {
                            Access access = accessSet.AccessFor(a.Key);
                            return access != Access.None && a.Value.Any(x => access.Has(x));
                        });
        }
 
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            ApiSessionFacade session = new ApiSessionFacade(actionContext.ControllerContext);

            // Must have logged in as a valid user.
            // NOTE session.User can be null if using test page and username/password are unknown, but you are actually logged in, so base.IsAuthorized returns true
            if (!IsAuthorizedBase(actionContext))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, 
                    ApiMessages.Unauthorised);
                return false;
            }

            // Must have account in route
            IHttpRouteData data = actionContext.Request.GetRouteData();
            object account = data?.Values["account"];
            if (account == null || !int.TryParse(account.ToString(), out var accountId))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    ApiMessages.Unauthorised_ForAccount);
                return false;
            }

            // Must have access to account.
            IdNamePair auth = UserAccountAuthoriser.AuthoriseAccount(session.User, accountId);
            if (auth == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    ApiMessages.Unauthorised_ForAccount);
                return false;
            }

            // Only CCM can access CCM data and vice-versa
            object scope = data.Values["scope"];
            MediaType? target = ApiModel.TargetForScope((string)scope);
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
                if (bool.TryParse(DependencyResolver.Current.GetService<IConfigurationService>().Get(ConfigurationKey.MaintenanceModePortal.Key), out var isInMaintenanceMode) && isInMaintenanceMode)
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