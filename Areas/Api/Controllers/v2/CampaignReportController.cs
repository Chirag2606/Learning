namespace Cyara.Web.Portal.Areas.Api.Controllers.V2
{
    using System.Net.Http;

    using Cyara.Domain.Types.Roles;
    using Cyara.Web.Portal.Areas.Api.Attributes;

    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    [AuthorizeAccount(StaticRoles.Admin, StaticRoles.CCMAdmin)]
    public class CampaignReportController : BaseApiController
    {
        public HttpResponseMessage Get(int account, string scope, int id)
        {
            return null;
        }
    }
}
