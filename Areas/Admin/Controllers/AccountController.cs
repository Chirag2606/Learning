namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Cyara.Domain.Types.Common;
    using Cyara.Domain.Types.Licensing;
    using Cyara.Domain.Types.Plan;
    using Cyara.Foundation.Core.Messages;
    using Cyara.Domain.Types.Roles;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Types.Account;
    using Cyara.Shared.Types.Constants;
    using Cyara.Shared.Web.Authorisation;
    using Cyara.Shared.Web.Configuration;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Shared.Web.Identity;
    using Cyara.Shared.Web.Licensing;
    using Cyara.Shared.Web.Mapping;
    using Cyara.Shared.Web.Messaging;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Shared.Web.Session;
    using Cyara.Shared.Web.Types.Account;
    using Cyara.Shared.Web.Types.Constants;
    using Cyara.Web.Messaging.Types.Command;
    using Cyara.Web.Portal.Areas.Admin.Models;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions;
    using Cyara.Web.Portal.Models;

    using MediatR;

    using PlanViewData = Cyara.Web.Portal.Areas.Admin.Models.PlanViewData;

    public class AccountController : BaseController
    {
        private readonly RestApiFacade _webApi;

        private readonly IdentitySettings _identitySettings;

        private readonly IConfigurationService _configurationService;

        public AccountController(IAccountService accountService,
                                    ILogger logger,
                                    IAuthorisationManager authorisationManager,
                                    IMediator mediator,
                                    RestApiFacade webApi,
                                    IdentitySettings identitySettings,
                                    IConfigurationService configurationService)
        {
            AccountService = accountService;
            AuthorisationManager = authorisationManager;
            Logger = logger;
            Mediator = mediator;
            _webApi = webApi;
            _identitySettings = identitySettings;
            _configurationService = configurationService;
        }

        public IAccountService AccountService { get; }

        public ILogger Logger { get; }

        public IAuthorisationManager AuthorisationManager { get; }

        public IMediator Mediator { get; }

        [SecuredResource(new[] { StaticRoles.PlatformUser, StaticRoles.UserAdmin, StaticRoles.Admin, StaticRoles.CCMAdmin }, false)]
        public ActionResult Home()
        {
            // fake action for navigation
            return View();
        }

        [SecuredResource(StaticRoles.PlatformUser, false)]
        public ActionResult Index(string warnMessageId = null, string sortColumn = null, bool? sortAscending = null)
        {
            var session = new SessionFacade(HttpContext);

            // defaults
            var sort = sortColumn ?? Columns.AccountName;
            var ascending = sortAscending ?? true;

            var accountsResponse = session.SelectedAccount == null ?
                AccountService.AccountsGet(new PaginatedRequest<IEnumerable<int>>(null)
                        {
                            CurrentPage = 1,
                            PageSize = MvcApplication.Settings.DefaultPageSize,
                            User = session.User,
                            SortField = sort,
                            SortAscending = ascending
                        })
                        : AccountService.AccountsGetPageForAccount(new AccountRequest<PaginatedRequest>(new PaginatedRequest
                        {
                            PageSize = MvcApplication.Settings.DefaultPageSize,
                            User = session.User,
                            SortField = sort,
                            SortAscending = ascending
                        }) { AccountId = session.SelectedAccount.Id });
            accountsResponse.ExceptionIfError();

            var model = new AccountsListViewModel
            {
                Accounts = accountsResponse.Collection.Select(Mapper.Map<Account, AccountViewData>),
                PageNumber = accountsResponse.CurrentPage,
                CollectionSize = accountsResponse.CollectionSize,
                SortColumn = sort,
                SortAscending = ascending,
                PageSize = accountsResponse.PageSize,
                ChosenAccountId = session.SelectedAccount == null ? null : (int?)session.SelectedAccount.Id
            };

            if (warnMessageId != null)
            {
                model.Message = new MessageViewData().Prime(warnMessageId, Severity.PageWarning);
            }

            return View(model);
        }

        [HttpPost]
        [SecuredResource(StaticRoles.PlatformUser, false)]
        public ActionResult List(PaginatedView model)
        {
            var session = new SessionFacade(HttpContext);

            var accountsResponse = AccountService.AccountsGet(new PaginatedRequest<IEnumerable<int>>(null)
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
                            List =
                                accountsResponse.Collection.Select(
                                    Mapper.Map<Account, AccountViewData>)
                        }
                };
        }

        /// <summary>
        /// default account is set for current user and redirection is sent back (302)
        /// </summary>
        [SecuredResource(new string[0], false)]
        public async Task<ActionResult> Choose(int? id, string sortColumn = null, bool? sortAscending = null)
        {
            var session = new SessionFacade(HttpContext);

            if (!AuthorisationManager.HasAccess(session.User.Roles, session.User.AreaAccess, ResourceType.AllAccounts, AccessType.Read))
            {
                if (id == null)
                {
                    // if this user cannot be set to no-account mode, we redirect to home page with no account selected
                    // this would enforce the SecuredResource attribute to issue another redirect to the default account
                    return RedirectToAction("index", "home", new { routeAccountId = 0, area = string.Empty });
                }

                // ensure this account is available for the user in question
                if (!session.User.Accounts.Any(a => a.IsDeleted == false && a.IsActive && a.Id == id.Value))
                {
                    return this.RedirectToAction("index", "home", new { routeAccountId = 0, area = string.Empty });
                }
            }

            var response = await AccountService.AccountSetDefaultAsync(new GenericRequest<int>(id.Value) { User = session.User });
            response.ExceptionIfError();

            if (!session.User.Roles.Contains(StaticRoles.PlatformUser))
            {
                return RedirectToAction("index", "home", new { routeAccountId = id.Value, area = string.Empty });
            }

            return this.RedirectToAction("Index", new { routeAccountId = id.Value, sortColumn, sortAscending });
        }

        [SecuredResource(StaticRoles.PlatformAdmin, false)]
        public async Task<ActionResult> Create()
        {
            var session = new SessionFacade(HttpContext);

            var model = await new AccountEditViewModel().Prime(AccountService, Mediator, session, _webApi, _identitySettings, false, true, modelState: ModelState);

            return View(model);
        }

        [SecuredResource(StaticRoles.PlatformAdmin, false)]
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(AccountEditViewModel model)
        {
            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                var account = Mapper.Map<AccountEditViewModel, Account>(model);

                var createResponse = AccountService.AccountCreate(new GenericRequest<Account>(account)
                    {
                        User = session.User
                    });
                createResponse.ExceptionIfError();

                if (createResponse.IsSuccess)
                {
                    if (model.IdentityProviderId.HasValue)
                    {
                        await _webApi.IdentityProvidersApi.UpdateAccountWithIdentityProviderAsync(model.IdentityProviderId.Value, createResponse.Value.AccountId);
                    }

                    var setResponse = await AccountService.AccountSetDefaultAsync(new GenericRequest<int>(createResponse.Value.AccountId) { User = session.User });
                    setResponse.ExceptionIfError();

                    session.SelectedAccount = new Shared.Types.Shared.IdNamePair(createResponse.Value.AccountId, createResponse.Value.Name, false, true);
                    model.EmailMailboxesSave(session, Mediator);
                    model.SmsNumbersSave(session, Mediator);
                    model.DefaultSite(session, createResponse.Value.AccountId);
                    await model.SaveFeatures(createResponse.Value.AccountId);

                    return RedirectToAction("edit", "Account", new { messageId = "Account_Created", routeAccountId = setResponse.Value.AccountId });
                }

                await model.Prime(AccountService, Mediator, session, _webApi, _identitySettings, modelState: ModelState);

                model.Message = new MessageViewData
                    {
                        Severity = Severity.ValidationError,
                        Text = new HtmlString(HttpUtility.HtmlEncode(createResponse.ErrorMessage()))
                    };

                return View(model);
            }

            await model.Prime(AccountService, Mediator, session, _webApi, _identitySettings, modelState: ModelState);

            model.Message = new MessageViewData().Failed();

            return View(model);
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.UserAdmin })]
        public async Task<ActionResult> UpgradeToVelocity(bool continueIfWarning)
        {
            var session = new SessionFacade(HttpContext);
            string error = null;
            string message = null;

            var command = UpdateReplayCampaignsToUseVelocityPlanCommand.Construct(session);
            command.ContinueIfWarning = continueIfWarning;

            var updateResponse = await Mediator.Send(command);
            updateResponse.ExceptionIfError();

            var licenseManageer = DependencyResolver.Current.GetService<ILicenseManager>();

            if (updateResponse.IsSuccess)
            {
                if (updateResponse.Value > 0 || await licenseManageer.HasAccess(command.AccountId, LicensedFeature.Velocity) == false)
                {
                    await licenseManageer.GrantAccess(command.AccountId, LicensedFeature.Velocity);
                    message = Resources.Messages.VelocityUpgradeSuccess;
                }
                else
                {
                    message = Resources.Messages.VelocityUpgradeNoAction;
                }
            }
            else
            {
                // Convert the MediaType args into string labels for display purposes.
                if (updateResponse.ErrorResult.Code == "VELOCITY_UPGRADE_MISSING_ACTIVE_PLANS")
                {
                    MediaType[] args = updateResponse.ErrorResult.Args[0] as MediaType[];

                    if (args != null)
                    {
                        updateResponse = Domain.Types.Responses.GenericResponse<int>.Fails(new SystemMessage(
                            updateResponse.ErrorResult.Code,
                            updateResponse.ErrorResult.SubSystem,
                            new object[] { string.Join(", ", args.Select(x => x.ToLabel())) }));
                    }

                    return new JsonCamelCaseResult
                    {
                        Data = new
                        {
                            IsWarning = true,
                            Warning = updateResponse.ErrorMessage(),
                            IsLicensed = await licenseManageer.HasAccess(command.AccountId, LicensedFeature.Velocity)
                        }
                    };
                }

                error = updateResponse.ErrorMessage();
                message = Resources.Messages.VelocityUpgradeError;
            }

            return new JsonCamelCaseResult
            {
                Data = new
                {
                    IsSuccess = error == null,
                    Error = error,
                    Message = message,
                    IsLicensed = await licenseManageer.HasAccess(command.AccountId, LicensedFeature.Velocity)
                }
            };
        }

        [SecuredResource(new[] { StaticRoles.Admin, StaticRoles.CCMAdmin })]
        public async Task<ActionResult> Show(string messageId = null)
        {
            var session = new SessionFacade(HttpContext);

            // setup breadcrumbs for edit user, create user and view plan back to the show page
            session.CreateBreadcrumb(
                new[] { Url.Action("Edit", "User", new { area = "admin" }), Url.Action("Create", "User", new { area = "admin" }), Url.Action("View", "Plan", new { area = "admin" }) },
                new RouteValueDictionary { { "controller", "Account" }, { "action", "Show" }, { "area", "Admin" } });

            var response = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();

            var model = await new AccountEditViewModel().Prime(
                AccountService,
                Mediator,
                session,
                _webApi,
                _identitySettings,
                loadAdditional: true,
                account: response.Value,
                modelState: ModelState,
                showIdentityProviders: false);

            model.AccountId = session.SelectedAccount.Id;
            model.ReadOnly = true;

            model = Mapper.Map(response.Value, model);

            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId);
            }

            return View(Mapper.Map(response.Value, model));
        }

        [SecuredResource(new[] { StaticRoles.Admin, StaticRoles.CCMAdmin })]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Show(AccountEditViewModel model)
        {
            throw new NotSupportedException("This is only a placeholder to allow the sitemap to render the view account link when the current page has been POSTed");
        }

        [SecuredResource(new[] { StaticRoles.PlatformAdmin, StaticRoles.Admin, StaticRoles.CCMAdmin })]
        public ActionResult ShowOrEdit(string messageId = null)
        {
            if (User.IsInRole(StaticRoles.PlatformAdmin))
            {
                return RedirectToAction("Edit", new { messageId });
            }

            return RedirectToAction("Show", new { messageId });
        }

        [SecuredResource(StaticRoles.PlatformAdmin)]
        public async Task<ActionResult> Edit(string messageId = null)
        {
            var session = new SessionFacade(HttpContext);
            session.RemoveBreadcrumb(Url.Action("Edit", "Plan"));

            var ensure = AccountService.AccountEnsureDefaultSpeaker(AccountRequest.Construct(session.User, session.SelectedAccount.Id));
            ensure.ExceptionIfError();

            var response = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();

            var model = await new AccountEditViewModel().Prime(
                AccountService,
                Mediator,
                session,
                _webApi,
                _identitySettings,
                loadAdditional: true,
                account: response.Value,
                modelState: ModelState);

            model.AccountId = session.SelectedAccount.Id;

            model = Mapper.Map(response.Value, model);

            // this has to be after Mapper.Map()
            PrepareValidationForDefaultLanguages(response.Value, model);

            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId);
            }

            return View(model);
        }

        [SecuredResource(StaticRoles.PlatformAdmin, true)]
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(AccountEditViewModel model)
        {
            Domain.Types.Responses.GenericResponse<Account> getResponse;

            var session = new SessionFacade(HttpContext);

            if (ModelState.IsValid)
            {
                var account = Mapper.Map<AccountEditViewModel, Account>(model);
                account.AccountId = session.SelectedAccount.Id;

                // capture the original value before we commit so we can detect changes in the identity provider
                getResponse = AccountService.AccountGet(new GenericRequest<int>(account.AccountId) { User = session.User });
                getResponse.ExceptionIfError();
                var originalAccount = getResponse.Value;

                var response = AccountService.AccountUpdate(new GenericRequest<Account>(account)
                {
                    User = session.User
                });
                response.ExceptionIfError();

                if (response.IsSuccess)
                {
                    if (originalAccount.IdentityProviderId.HasValue && model.IdentityProviderId == null)
                    {
                        // Identity Provider has been removed
                        await _webApi.IdentityProvidersApi.DeleteAccountFromIdentityProviderAsync(originalAccount.IdentityProviderId.Value, model.AccountId.Value);
                    }
                    else if (originalAccount.IdentityProviderId == null && model.IdentityProviderId.HasValue)
                    {
                        // Identity Provider has been added
                        await _webApi.IdentityProvidersApi.UpdateAccountWithIdentityProviderAsync(model.IdentityProviderId.Value, model.AccountId.Value);
                    }

                    session.SelectedAccount = response.Value;

                    model.EmailMailboxesSave(session, Mediator);

                    model.SmsNumbersSave(session, Mediator);
                    await model.SaveFeatures(model.AccountId.Value);

                    return RedirectToAction("edit", "Account", new { messageId = "Account_Updated" });
                }

                await model.Prime(AccountService, Mediator, session, _webApi, _identitySettings, modelState: ModelState);

                model.Message = new MessageViewData
                {
                    Severity = Severity.ValidationError,
                    Text = new HtmlString(HttpUtility.HtmlEncode(response.ErrorMessage()))
                };

                return View(model);
            }

            getResponse = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            getResponse.ExceptionIfError();

            await model.Prime(AccountService, Mediator, session, _webApi, _identitySettings, account: getResponse.Value, modelState: ModelState);
            model.AccountId = session.SelectedAccount.Id;

            model.Message = new MessageViewData().Failed();

            return View(model);
        }

        [SecuredResource(new[] { StaticRoles.UserAdmin })]
        public async Task<ActionResult> Users(int routeAccountId, string messageId = null, Severity severity = Severity.PageSuccess)
        {
            var isLoadingAngular = _configurationService.IsLicensed(LicensedFeature.LoadUsersPageInAngular);

            // navigate to Angular view if feature flag is ON
            if (isLoadingAngular)
            {
                return View("~/Views/Angular.cshtml");
            }

            var model = new AccountUsersViewModel();

            var response = await _webApi.AccountUsersApi.GetUsersForAccountAsync(
                routeAccountId,
                searchTerm: null,
                attachedUsers: true,
                sortField: model.SortColumn,
                sortAsc: model.SortAscending,
                pageSize: model.PageSize,
                pageNo: model.PageNumber);

            if (response != null)
            {
                model = AutoMapper.Mapper.Map(response, model);
            }

            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId, severity);
            }

            return View(model);
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.UserAdmin })]
        public async Task<ActionResult> ListUsers(int routeAccountId, AccountUsersViewModel model)
        {
            var response = await _webApi.AccountUsersApi.GetUsersForAccountAsync(
                               routeAccountId,
                               searchTerm: null,
                               attachedUsers: true,
                               sortField: model.SortColumn,
                               sortAsc: model.SortAscending,
                               pageSize: model.PageSize,
                               pageNo: model.PageNumber);

            var paginatedView = AutoMapper.Mapper.Map(response, model);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<UserListViewData>
                {
                    CollectionSize = paginatedView.CollectionSize.Value,
                    TotalPages = paginatedView.TotalPages,
                    List = paginatedView.Collection
                }
            };
        }

        [SecuredResource(new[] { StaticRoles.UserAdmin })]
        [HttpPost]
        public async Task<ActionResult> ListUsersForAttach(AttachUserViewModel view)
        {
            var session = new SessionFacade(HttpContext);

            if (!ModelState.IsValid)
            {
                return new JsonCamelCaseResult
                {
                    Data = new AjaxPaginatedResponse<UserViewData> { Error = ModelState.FirstErrorMessage() }
                };
            }

            var response = await AccountService.UsersGetForAttachAsync(
                new PaginatedRequest<string>((view.Search ?? string.Empty).Trim())
                {
                    AccountId = session.SelectedAccount.Id,
                    User = session.User,
                    CurrentPage = view.PageNumber,
                    SortField = view.SortColumn,
                    SortAscending = view.SortAscending,
                    PageSize = view.PageSize
                });
            response.ExceptionIfError();

            var paginatedView = new PaginatedView<UserViewData>().FromPaginatedResponse(response);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<UserViewData>
                {
                    CollectionSize = paginatedView.CollectionSize ?? 0,
                    TotalPages = paginatedView.TotalPages,
                    List = paginatedView.Collection
                }
            };
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.UserAdmin })]
        public async Task<ActionResult> AttachUser(Guid userId)
        {
            var session = new SessionFacade(HttpContext);

            var response = await AccountService.UserAttachAsync(
                new AccountRequest<Guid>(userId)
                {
                    AccountId = session.SelectedAccount.Id,
                    User = session.User
                });
            response.ExceptionIfError();

            return new JsonCamelCaseResult
            {
                Data = new { IsSuccess = response.IsSuccess, Error = response.ErrorMessage() }
            };
        }

        [HttpPost]
        [SecuredResource(new[] { StaticRoles.PlatformAdmin, StaticRoles.Admin, StaticRoles.CCMAdmin })]
        public ActionResult Plans(PaginatedView model)
        {
            var session = new SessionFacade(HttpContext);

            var response = AccountService.AccountGet(new GenericRequest<int>(session.SelectedAccount.Id) { User = session.User });
            response.ExceptionIfError();

            var paginatedView = PlanViewPagination(response.Value.Plans.ToList(), model);

            return new JsonCamelCaseResult
            {
                Data = new AjaxPaginatedResponse<PlanViewData>
                {
                    CollectionSize = paginatedView.CollectionSize.Value,
                    TotalPages = paginatedView.TotalPages,
                    List = paginatedView.Collection
                }
            };
        }

        #region Privates

        private PaginatedView<PlanViewData> PlanViewPagination(IList<Plan> users, PaginatedView model)
        {
            return new PaginatedView<PlanViewData>().Prime<PlanViewData, Plan, DateTime?>(
                users,
                data => data.EndDate,
                Mapper.Map<Plan, PlanViewData>,
                model.SortColumn,
                model.SortAscending,
                model.PageNumber,
                model.PageSize);
        }

        /// <summary>
        /// Checks if default Language and Voice for this account are present on the lists of enabled languages and voices,
        /// adds them if not (which means they are disabled) and empties id's for them in that case (which will trigger JavaScript validation)
        /// </summary>
        private void PrepareValidationForDefaultLanguages(Account account, AccountEditViewModel model)
        {
            if (account != null && !model.Languages.Any(a => a.Value.Equals(account.LanguageId.ToString())))
            {
                model.Languages.Add(new SelectListItem()
                {
                    Value = string.Empty,
                    Text =
                        AccountService.DefaultSpeechRecognitionLanguageGetForAccount(new GenericRequest<Account>(account))
                            .Value.Description
                });
                model.RecognitionLanguage = string.Empty;
            }

            if (account != null && !model.Voices.Any(a => a.Value.Equals(account.VoiceId.ToString())))
            {
                model.Voices.Add(new SelectListItem()
                {
                    Value = string.Empty,
                    Text =
                        AccountService.DefaultTextToSpeechVoiceGetForAccount(new GenericRequest<Account>(account))
                            .Value.Description
                });
                model.ReplyVoice = string.Empty;
            }
        }

        #endregion
    }
}
