namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Identity;
    using Cyara.Shared.Web.Identity.ActiveDirectory;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Notification;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Core.Account;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Security;
    using Cyara.Web.Portal.Models;
    using Cyara.Web.Portal.Models.Client;
    using Cyara.Web.Resources;

    using MediatR;

    using Microsoft.Owin.Security;

    [SecuredResource(new[] { StaticRoles.UserAdmin })]
    public class UserController : BaseController
    {
        private readonly CyaraUserManager _userManager;

        public UserController(
            IAccountService accountService,
            ILogger logger,
            INotificationService notificationService,
            IConfigurationService configurationService,
            PasswordResetter passwordResetter,
            IdentitySettings identitySettings,
            MembershipProviderSettings membershipProviderSettings,
            IAuthenticationManager authenticationManager,
            IAuthorisationManager authorisationManager,
            IMediator mediator,
            CyaraUserManager userManager)
        {
            AccountService = accountService;
            Logger = logger;
            NotificationService = notificationService;
            ConfigurationService = configurationService;
            AuthenticationManager = authenticationManager;
            PasswordResetter = passwordResetter;
            MembershipProviderSettings = membershipProviderSettings;
            IdentitySettings = identitySettings;
            AuthorisationManager = authorisationManager;
            Mediator = mediator;
            _userManager = userManager;
        }

        public IAccountService AccountService { get; }

        public ILogger Logger { get; }

        public INotificationService NotificationService { get; }

        public IConfigurationService ConfigurationService { get; }

        public IAuthenticationManager AuthenticationManager { get; }

        public IAuthorisationManager AuthorisationManager { get; }

        public IMediator Mediator { get; }

        public PasswordResetter PasswordResetter { get; }

        public MembershipProviderSettings MembershipProviderSettings { get; }

        public IdentitySettings IdentitySettings { get; }

        public async Task<ActionResult> Create()
        {
            var session = new SessionFacade(HttpContext);

            var model = await new UserEditViewModel().PrimeAccountUser(null, session.User, IdentitySettings, session, AccountService, Mediator, _userManager, true);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(UserEditViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (!model.CheckAccountLevelRoles(Logger, GetType(), session.User.Username))
            {
                throw new Exception(Messages.General_Error);
            }

            var createResponse = await UserCommonActions.Create(
                false,
                model,
                ModelState,
                HttpContext,
                Logger,
                AccountService,
                NotificationService,
                IdentitySettings,
                (u, s) => AccountService.UserCreateAsync(new AccountRequest<User>(u) { User = s.User, AccountId = s.SelectedAccount.Id }),
                ConfigurationService,
                PasswordResetter,
                _userManager);

            if (createResponse.Item1)
            {
                return RedirectToAction("users", "account", new { messageId = "User_Created" });
            }

            if (createResponse.Item2)
            {
                return RedirectToAction("users", "account", new { messageId = createResponse.Item3, severity = Severity.PageFatal });
            }

            return View(model);
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var session = new SessionFacade(HttpContext);

            var model = new UserEditViewModel { EditMode = true };

            var response = await AccountService.UserGetAsync(new AccountRequest<Guid>(id) { AccountId = session.SelectedAccount.Id, User = session.User });
            if (response.Value == null)
            {
                model.Message = new MessageViewData
                    {
                        Severity = Severity.PageFatal,
                        Text = new HtmlString(Messages.User_Missing)
                    };

                return View(model);
            }

            model = Mapper.Map<User, UserEditViewModel>(response.Value);
            model.EditMode = true;

            await model.PrimeAccountUser(response.Value, session.User, IdentitySettings, session, AccountService, Mediator, _userManager);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(UserEditViewModel model, Guid id)
        {
            var session = new SessionFacade(HttpContext);

            if (!model.CheckAccountLevelRoles(Logger, GetType(), session.User.Username))
            {
                throw new Exception(Messages.General_Error);
            }

            if (model.Action == "UNLOCK")
            {
                if (await UserCommonActions.UnlockUserAsync(
                    model,
                    id,
                    AccountService,
                    HttpContext,
                    (i, s) => AccountService.UserUnlockAsync(new AccountRequest<Guid>(i) { User = s.User, AccountId = s.SelectedAccount.Id })))
                {
                    return RedirectToAction("users", "account", new { messageId = "User_Unlocked" });
                }

                // reset to prevent email confirmation on re-submission
                model.EmailConfirmed = null;

                return View(model);
            }

            try
            {
                // reload the email address if they do not have permission to change
                if (!UserEditViewModelExtensions.CanEditEmail(id, true, session.User))
                {
                    var user = await AccountService.UserGetAsync(AccountRequest.Construct(id, session.User, session.SelectedAccount.Id));
                    user.ExceptionIfError();
                    model.Email = user?.Value?.Email;
                }

                if (await UserCommonActions.EditAsync(
                    false,
                    model,
                    id,
                    ModelState,
                    HttpContext,
                    Logger,
                    AccountService,
                    IdentitySettings,
                    _userManager,
                    (u, s) => AccountService.UserUpdateAsync(new AccountRequest<UserUpdate>(u) { User = session.User, AccountId = s.SelectedAccount.Id }),
                    model.AccountsChanged))
                {
                    // if current user had edited himself, refresh user instance stored in session
                    if (id.Equals(session.User.UserId))
                    {
                        var updatedUser = await AccountService.UserGetAsync(new AccountRequest<Guid>(session.User.UserId) { AccountId = session.SelectedAccount.Id, User = session.User });
                        updatedUser.ExceptionIfError();
                        if (updatedUser.Value == null)
                        {
                            throw new Exception("Unable to retrieve user after being updated via EditUser page");
                        }

                        // if user has disabled himself, logout now
                        if (!updatedUser.Value.Active)
                        {
                            updatedUser.Value.Logout(HttpContext.GetOwinContext(), AccountService, HttpContext);
                            return Redirect(Url.RouteUrl("Root"));
                        }

                        // update session
                        updatedUser.Value.IpAddress = session.User.IpAddress;
                        session.User = updatedUser.Value;
                        session.User.Properties.SessionId = session.SessionId;

                        // notification preferences could be changed
                        session.User.Properties.PlatformNotifications = updatedUser.Value.Properties.PlatformNotifications;

                        await this.RefreshClaimsIdentity(_userManager);
                    }

                    return RedirectToAction("users", "account", new { messageId = "User_Updated" });
                }
            }
            catch (Exception exp)
            {
                var smtp = exp.ResolveSmtpRelated();
                if (smtp == null)
                {
                    throw;
                }

                MvcApplication.Logger.LogErrorWithFormat(GetType(), "Unable to send password email: {0} Details: {1}", smtp.Message, smtp.InnerException?.Message);
                model.Message = new MessageViewData { Severity = Severity.PageWarning, Text = new HtmlString(Messages.CustomNotifications_EmailSentFailure) };
            }

            // reset to prevent email confirmation on re-submission
            model.EmailConfirmed = null;

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(Guid id)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UserDeleteAsync(new AccountRequest<Guid>(id) { User = session.User, AccountId = session.SelectedAccount.Id });
            response.ExceptionIfError();

            return new JsonCamelCaseResult { Data = new { response.IsSuccess, ErrorMessage = response.ErrorMessage() } };
        }

        [HttpPost]
        public async Task<ActionResult> Detach(Guid id)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UserDetachAsync(new AccountRequest<Guid>(id) { User = session.User, AccountId = session.SelectedAccount.Id });
            response.ExceptionIfError();

            return new JsonCamelCaseResult { Data = new { response.IsSuccess, ErrorMessage = response.ErrorMessage() } };
        }

        [HttpPost]
        public async Task<ActionResult> DetachOrDelete(Guid id)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UserGetAsync(new AccountRequest<Guid>(id) { User = session.User, AccountId = session.SelectedAccount.Id });
            response.ExceptionIfError();

            return new JsonCamelCaseResult
            {
                Data = new DetachOrDeleteUser
                            {
                                IsSuccess = response.IsSuccess,
                                Error = response.ErrorMessage(),
                                IsDelete = response.Value.Accounts.Count(a => !a.IsDeleted) < 2,
                                IsChoice = response.Value.Accounts.Count(a => !a.IsDeleted) > 1 && AuthorisationManager.HasAccess(session.User.Roles, session.User.AreaAccess, ResourceType.AllAccounts, AccessType.Read)
                            }
            };
        }

        [HttpPost]
        public ActionResult CheckUserInAd(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new JsonResult() { Data = false };
            }

            var user = new ADUser(Logger, username, MembershipProviderSettings);
            var result = user.Search();
            return new JsonResult() { Data = result != null && result.Exists };
        }

        [HttpPost]
        public ActionResult ListAccountsForAttach(string exclude, PaginatedView<AccountViewData> model)
        {
            var session = new SessionFacade(HttpContext);

            List<int> excludeAccounts = new List<int>();
            if (!string.IsNullOrEmpty(exclude))
            {
                foreach (var s in exclude.Split(','))
                {
                    excludeAccounts.Add(int.Parse(s));
                }
            }

            var accountsResponse = AccountService.AccountsGetForUserAttach(new PaginatedRequest<IEnumerable<int>>(excludeAccounts)
            {
                CurrentPage = model.PageNumber,
                PageSize = model.PageSize,
                User = session.User,
                SortField = model.SortColumn,
                SortAscending = model.SortAscending
            });
            accountsResponse.ExceptionIfError();

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<AccountViewData>
                {
                    CollectionSize = accountsResponse.CollectionSize,
                    TotalPages = accountsResponse.TotalPages,
                    List = Mapper.MapList<Account, AccountViewData>(accountsResponse.Collection)
                }
            };
        }

        [HttpPost]
        public async Task<ActionResult> SendInvite(Guid id, int routeAccountId)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UserSendMobileAppInviteAsync(new AccountRequest<Guid>(id) { AccountId = routeAccountId, User = session.User });
            if (response.IsSuccess)
            {
                return new JsonCamelCaseResult { Data = new { response.IsSuccess, ErrorMessage = response.ErrorMessage() } };
            }

            if (response.Exception?.GetBaseException() is InvalidOperationException)
            {
                return new JsonCamelCaseResult { Data = new { response.IsSuccess, ErrorMessage = Messages.User_CannotSendEmail } };
            }

            return new JsonCamelCaseResult { Data = new { response.IsSuccess, ErrorMessage = response.ErrorMessage() } };
        }

        [HttpPost]
        public ActionResult UserRolesAccess(string category)
        {
            return new JsonCamelCaseResult
            {
                Data = new UserRoleAccessData().Prime(category)
            };
        }

        [HttpPost]
        public ActionResult ViewAssignedPrivileges(SuperUserEditViewModel model)
        {
            return new JsonCamelCaseResult
            {
                Data = new UserRoleAccessData().Prime(model.LegacyAccountLevelRoles, model.AccountLevelRoles, new List<UserRoleViewData>(), $"{model.FirstName} {model.LastName}")
            };
        }
    }
}