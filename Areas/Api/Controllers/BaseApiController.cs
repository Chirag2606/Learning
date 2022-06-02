namespace Cyara.Web.Portal.Areas.Api.Controllers
{
    using System.Web.Http;

    using Cyara.Shared.Web.Api;
    using Cyara.Web.Portal.Areas.Api.Core;

    [ClientCacheControl]
    public class BaseApiController : ApiController
    {
        protected IDataHelper DataHelper { get; set; }
    }
}