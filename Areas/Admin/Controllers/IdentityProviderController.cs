namespace Cyara.Web.Portal.Areas.Admin.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using AutoMapper;

    using Cyara.Domain.Types.Roles;
    using Cyara.Shared.Web.Identity;
    using Cyara.Shared.Web.Models;
    using Cyara.Shared.Web.Mvc;
    using Cyara.Shared.Web.Security;
    using Cyara.Web.Portal.Areas.Admin.Extensions;
    using Cyara.Web.Portal.Areas.Admin.Models.IdentityProvider;
    using Cyara.Web.Portal.Core.Api;
    using Cyara.Web.Portal.Core.Extensions;

    [SecuredResource(new[] { StaticRoles.PlatformAdmin }, requiresAccount: false)]
    public class IdentityProviderController : BaseController
    {
        private readonly RestApiFacade _webApi;

        private readonly IdentitySettings _identitySettings;

        public IdentityProviderController(RestApiFacade webApi, IdentitySettings identitySettings)
        {
            _webApi = webApi;
            _identitySettings = identitySettings;
        }

        public async Task<ActionResult> Index(string messageId = null)
        {
            var model = new IdentityProviderListViewModel();

            var response = await _webApi.IdentityProvidersApi.GetIdentityProvidersAsync(pageSize: model.PageSize, pageNo: 1, sortField: "Name", sortAsc: true);
            
            model.CollectionSize = response.TotalResults;
            model.PageNumber = 1;
            model.PageSize = model.PageSize;

            model.Collection = response.Results.Select(Mapper.Map<IdentityProviderListViewData>);

            if (messageId != null)
            {
                model.Message = new MessageViewData().Prime(messageId, Severity.PageSuccess);
            }

            return View(model);
        }

        public async Task<ActionResult> List(IdentityProviderListViewModel model)
        {
            var response = await _webApi.IdentityProvidersApi.GetIdentityProvidersAsync(pageSize: model.PageSize, pageNo: model.PageNumber, sortField: model.SortColumn, sortAsc: model.SortAscending);

            model.CollectionSize = response.TotalResults;
            model.PageNumber = model.PageNumber;
            model.PageSize = model.PageSize;

            var paginatedView = Mapper.Map(response, model);

            return new JsonCamelCaseResult
                       {
                           Data = new AjaxPaginatedResponse<IdentityProviderListViewData>
                                      {
                                          CollectionSize = paginatedView.CollectionSize.Value,
                                          TotalPages = paginatedView.TotalPages,
                                          List = paginatedView.Collection
                                       }
                       };
        }

        [HttpPost]
        public async Task<ActionResult> Used(int id, IdentityProviderAccountListViewModel model)
        {
            var response = await _webApi.IdentityProvidersApi.GetAccountsForIdentityProviderAsync(id, pageSize: model.PageSize, pageNo: model.PageNumber, sortField: model.SortColumn, sortAsc: model.SortAscending);

            model.CollectionSize = response.TotalResults;
            model.PageNumber = model.PageNumber;
            model.PageSize = model.PageSize;

            var paginatedView = Mapper.Map(response, model);

            return new JsonCamelCaseResult
                       {
                           Data = new AjaxPaginatedResponse<IdentityProviderAccountViewData>
                                      {
                                          CollectionSize = paginatedView.CollectionSize.Value,
                                          TotalPages = paginatedView.TotalPages,
                                          List = paginatedView.Collection
                                      }
                       };
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = new IdentityProviderViewModel();
            
            return View(model.Prime(_identitySettings));
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(IdentityProviderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var providerModel = Mapper.Map<CyaraWebApi.IdentityProviderModel>(model);

                TryValidateModel(providerModel);
                TryValidateModel(providerModel?.Settings);
                if (!ModelState.IsValid)
                {
                    return View(model.Prime(_identitySettings));
                }

                await BindApiModelErrors.ExecuteApiAndBind(
                    () => _webApi.IdentityProvidersApi.CreateIdentityProviderModelAsync(providerModel),
                    ModelState,
                    model);

                if (ModelState.IsValid && model.Message == null)
                {
                    return RedirectToAction("Index", new { messageId = "IdentityProviderCreate_Success" });
                }
            }
            
            return View(model.Prime(_identitySettings));
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var response = await _webApi.IdentityProvidersApi.GetIdentityProviderAsync(id);

            var model = Mapper.Map<IdentityProviderViewModel>(response);

            return View(model.Prime(_identitySettings));
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(int id, IdentityProviderViewModel model)
        {
            model.IdentityProviderId = id;

            if (ModelState.IsValid)
            {
                var providerModel = Mapper.Map<CyaraWebApi.IdentityProviderModel>(model);

                TryValidateModel(providerModel);
                TryValidateModel(providerModel?.Settings);
                if (!ModelState.IsValid)
                {
                    return View(model.Prime(_identitySettings));
                }

                await BindApiModelErrors.ExecuteApiAndBind(
                    () => _webApi.IdentityProvidersApi.UpdateIdentityProviderModelAsync(id, providerModel),
                    ModelState,
                    model);

                if (ModelState.IsValid && model.Message == null)
                {
                    return RedirectToAction("Index", new { messageId = "IdentityProviderUpdate_Success" });
                }
            }

            return View(model.Prime(_identitySettings));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            await _webApi.IdentityProvidersApi.DeleteIdentityProviderModelAsync(id);

            return new JsonCamelCaseResult();
        }
    }
}