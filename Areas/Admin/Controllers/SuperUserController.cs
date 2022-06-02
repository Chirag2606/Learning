namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    using Cyara.Domain.Types.Config;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Core.Extensions;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Reflection;
    using Cyara.Shared.Types.Account;
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
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Shared.Web.Types.Notification;
    using Cyara.Web.Common.Api;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Core.Account;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Core.Security;
    using Cyara.Web.Resources;
    using Microsoft.Owin.Security;

    [SecuredResource(StaticRoles.PlatformUserAdmin, false)]
    public class SuperUserController : BaseController
    {
        private readonly CyaraUserManager _userManager;

        public SuperUserController(
            IAccountService accountService,
            ILogger logger,
            INotificationService notificationService,
            IConfigurationService configurationService,
            PasswordResetter passwordResetter,
            IdentitySettings identitySettings,
            MembershipProviderSettings membershipProviderSettings,
            IAuthenticationManager authenticationManager,
            IRestApiFacade webApi,
            CyaraUserManager userManager)
        {
            AccountService = accountService;
            Logger = logger;
            NotificationService = notificationService;
            ConfigurationService = configurationService;
            PasswordResetter = passwordResetter;
            IdentitySettings = identitySettings;
            MembershipProviderSettings = membershipProviderSettings;
            AuthenticationManager = authenticationManager;
            _userManager = userManager;
        }

        public IAccountService AccountService { get; }

        public ILogger Logger { get; }

        public INotificationService NotificationService { get; }

        public IConfigurationService ConfigurationService { get; }

        public PasswordResetter PasswordResetter { get; }

        public MembershipProviderSettings MembershipProviderSettings { get; }

        public IdentitySettings IdentitySettings { get; }

        public IAuthenticationManager AuthenticationManager { get; }

        public async Task<ActionResult> Index(string messageId = null, Severity severity = Severity.PageSuccess)
        {
            var session = new SessionFacade(HttpContext);

            var response  = await AccountService.UsersGetByRoleAsync(new GenericRequest<string>(StaticRoles.PlatformUser) { User = session.User });
            response.ExceptionIfError();

            var model = new PaginatedView<UserViewData>().Prime<UserViewData, User, string>(
                response.Value.ToList(),
                data => data.Username,
                Mapper.Map<User, UserViewData>,
                Columns.Username,
                true,
                1,
                MvcApplication.Settings.DefaultPageSize);

            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId, severity);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> List(PaginatedView model)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UsersGetByRoleAsync(new GenericRequest<string>(StaticRoles.PlatformUser) { User = session.User });
            response.ExceptionIfError();

            var paginatedView = new PaginatedView<UserViewData>().Prime(
                response.Value.ToList(),
                data => data.Username,
                user => Mapper.Map<User, UserViewData>(user),
                model.SortColumn,
                model.SortAscending,
                model.PageNumber,
                model.PageSize);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<UserViewData>
                {
                    CollectionSize = paginatedView.CollectionSize.Value,
                    TotalPages = paginatedView.TotalPages,
                    List = paginatedView.Collection
                }
            };
        }

        public async Task<ActionResult> Create()
        {
            var session = new SessionFacade(HttpContext);

            var model = await new SuperUserEditViewModel().PrimePlatformUser(null, session.User, IdentitySettings, _userManager, true);

            return View(model as SuperUserEditViewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(SuperUserEditViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            // force the PlatformUser role to be selected
            var platformUser = model.PlatformLevelRoles.Single(x => x.Value == StaticRoles.PlatformUser);
            platformUser.Selected = true;

            string webPortalUrl = ConfigurationService.Get(ConfigurationKey.WebPortalSiteUrl.Key);

            var createResponse = await UserCommonActions.Create(
                true,
                model,
                ModelState,
                HttpContext,
                Logger,
                AccountService,
                NotificationService,
                IdentitySettings,
                (u, s) => AccountService.UserCreatePlatformUserAsync(new GenericRequest<User>(u) { User = s.User }),
                ConfigurationService,
                PasswordResetter,
                _userManager,
                (u, n, s) =>
                    {
                        n.Send(new NotificationSendRequest
                                   {
                                       Recipients = new[] { new EmailRecipient { Address = MvcApplication.Settings.InternalEmailAddress, RecipientType = RecipientType.To } },
                                       Template = "NewSuperUser",
                                       TemplateData = new
                                                          {
                                                              SiteUrl = webPortalUrl
                                                          }.ToJson(),
                                       TemplateDataSecure = new
                                                                {
                                                                    u.Username,
                                                                    CreatedBy = s.User.Username
                                                                }.ToJson(),
                                       User = s.User
                                   }).ExceptionIfError();
                    });

            if (createResponse.Item1)
            {
                return RedirectToAction("Index", "SuperUser", new { messageId = "User_Created" });
            }

            if (createResponse.Item2)
            {
                return RedirectToAction("Index", "SuperUser", new { messageId = createResponse.Item3, severity = Severity.PageFatal });
            }

            // the PlatformUser checkbox will be false as it was disabled, just remove all PlatformLevelRoles and build from the model
            ModelState.Keys.Where(x => x.StartsWith(ReflectOn<UserEditViewModel>.GetProperty(p => p.PlatformLevelRoles).Name))
                      .ToList()
                      .ForEach(x => ModelState.Remove(x));

            await model.PrimePlatformUser(null, session.User, IdentitySettings, _userManager);

            return View(model);
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var session = new SessionFacade(HttpContext);

            var model = new SuperUserEditViewModel { EditMode = true };

            var response = await AccountService.UserGetPlatformUserAsync(new GenericRequest<Guid>(id) { User = session.User });
            if (response.Value == null)
            {
                model.Message = new MessageViewData
                {
                    Severity = Severity.PageFatal,
                    Text = new HtmlString(Messages.User_Missing)
                };

                return View(model);
            }

            model = Mapper.Map<User, SuperUserEditViewModel>(response.Value);
            model.EditMode = true;

            await model.PrimePlatformUser(response.Value, session.User, IdentitySettings, _userManager);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(SuperUserEditViewModel model, Guid id)
        {
            var session = new SessionFacade(HttpContext);

            if (model.Action == "UNLOCK")
            {
                if (await UserCommonActions.UnlockUserAsync(
                    model,
                    id,
                    AccountService,
                    HttpContext,
                        (i, s) => AccountService.UserUnlockPlatformUserAsync(new GenericRequest<Guid>(i) { User = s.User })))
                {
                    return RedirectToAction("Index", "SuperUser", new { messageId = "User_Unlocked" });
                }

                // reset to prevent email confirmation on re-submission
                model.EmailConfirmed = null;

                return View(model);
            }

            // reload the email address if they do not have permission to change
            if (!UserEditViewModelExtensions.CanEditEmail(id, true, session.User))
            {
                var user = await AccountService.UserGetPlatformUserAsync(new GenericRequest<Guid>(id) { User = session.User });
                user.ExceptionIfError();
                model.Email = user?.Value?.Email;
            }

            // force the PlatformUser role to be selected
            var platformUser = model.PlatformLevelRoles.Single(x => x.Value == StaticRoles.PlatformUser);
            platformUser.Selected = true;

            if (await UserCommonActions.EditAsync(
                true,
                model,
                id,
                ModelState,
                HttpContext,
                Logger,
                AccountService,
                IdentitySettings,
                _userManager,
                (user, sessionparam) => AccountService.UserUpdatePlatformUserAsync(new GenericRequest<UserUpdate>(user) { User = sessionparam.User })))
            {
                // if current user had edited himself, refresh user instance stored in session
                if (id.Equals(session.User.UserId))
                {
                    var updatedUser = await AccountService.UserGetPlatformUserAsync(new GenericRequest<Guid>(session.User.UserId) { User = session.User });
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

                return RedirectToAction("Index", "SuperUser", new { messageId = "User_Updated" });
            }

            // the PlatformUser checkbox will be false as it was disabled, just remove all PlatformLevelRoles and build from the model
            ModelState.Keys.Where(x => x.StartsWith(ReflectOn<UserEditViewModel>.GetProperty(p => p.PlatformLevelRoles).Name))
                      .ToList()
                      .ForEach(x => ModelState.Remove(x));

            await model.PrimePlatformUser(null, session.User, IdentitySettings, _userManager);

            // reset to prevent email confirmation on re-submission
            model.EmailConfirmed = null;

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(Guid id)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UserDeletePlatformUserAsync(new GenericRequest<Guid>(id) { User = session.User });
            response.ExceptionIfError();

            return new JsonCamelCaseResult { Data = new { response.IsSuccess, ErrorMessage = response.ErrorMessage() } };
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
                Data = new UserRoleAccessData().Prime(model.LegacyAccountLevelRoles, model.AccountLevelRoles, model.PlatformLevelRoles, $"{model.FirstName} {model.LastName}")
            };
        }
    }
}
