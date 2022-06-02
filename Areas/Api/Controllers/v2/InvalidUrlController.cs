namespace Cyara.Web.Portal.Areas.Api.Controllers.V2
{
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Cyara.Web.Portal.Areas.Api.Attributes;
    using Cyara.Web.Resources;

    /// <summary>
    /// This controller should only be invoked if the url does not match any of our handling patterns, so we will attempt to determine the reason it didn't match here.
    /// </summary>
    [LogApiRequest]
    [HandleApiException]
    [V2ControllerConfiguration]
    public class InvalidUrlController : BaseApiController
    {
        /// <summary>
        /// This is meant to be a method that can be used to help diagnose why the url pattern was not acceptable and provide a meaningful response to the user
        /// </summary>
        /// <param name="urlpattern">A parameter inserted by the route definition to provide meaning as to what sort of pattern it matched. (1=2.0; 2=2.0 Report)</param>
        /// <param name="version_str">Text that was where the version string should be</param>
        /// <param name="account_spec">Text that was where the account literal should be</param>
        /// <param name="account_str">Text where the account ID should be</param>
        /// <param name="scope_str">Text where the scope should be</param>
        /// <param name="controller_str">Text where the controller name should be</param>
        /// <param name="id_str">Text where the id should be</param>
        /// <param name="action_str">Text where the action should be</param>
        /// <param name="entity_str">Text where the action id should be</param>
        /// <param name="extra">Any extra part of the path that is beyond our routing rules</param>
        [ActionName("ReasonV2")]
        [HttpGet, HttpPost, HttpPut, HttpDelete]
        public HttpResponseMessage GetReasonV2(int? urlpattern, string version_str = null, string account_spec = null, string account_str = null, string scope_str = null, string controller_str = null, string id_str = null, string action_str = null, string entity_str = null, string extra = null)
        {
            var sb = new StringBuilder();

            // the version must be 1.0 or 2.0
            decimal version;
            if (!decimal.TryParse(version_str, out version) || (version != 1 && version != 2 && version != 2.1m && version != 2.2m && version != 2.3m && version != 2.4m && version != 2.5m))
            {
                sb.AppendLine(ApiMessages.InvalidUrl_Version);
            }

            if (version >= 2)
            {
                // the account string must be specified
                if (!"account".Equals(account_spec, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.AppendLine(ApiMessages.InvalidUrl_AccountSpec);
                }

                // the account must be an int
                int account;
                if (string.IsNullOrWhiteSpace(account_str) || !int.TryParse(account_str, out account))
                {
                    sb.AppendLine(ApiMessages.InvalidUrl_Account);
                }

                // the scope(area) must be agent or voice
                switch ((scope_str ?? string.Empty).ToLower())
                {
                    case "agent":
                    case "voice":
                        break;
                    default:
                        sb.AppendLine(ApiMessages.InvalidUrl_Scope);
                        break;
                }

                // the controller must be specified
                if (string.IsNullOrWhiteSpace(controller_str))
                {
                    sb.AppendLine(ApiMessages.InvalidUrl_Controller);
                }

                // the entity (if specified) must be an int
                int id;
                if (id_str != null && (string.IsNullOrWhiteSpace(id_str) || !int.TryParse(id_str, out id)))
                {
                    sb.AppendLine(ApiMessages.InvalidUrl_Id);
                }

                if (!string.IsNullOrWhiteSpace(extra))
                {
                    sb.AppendLine(ApiMessages.InvlidUrl_TooLong);
                }

                // if no other reason is known, guess that it could be a missing controller
                if (sb.Length <= 0)
                {
                    sb.AppendLine(ApiMessages.InvalidUrl_ControllerOrAction);
                }

                // Provide the correct pattern for the request (as close as we can determine)
                switch (urlpattern)
                {
                    case 1:
                        sb.AppendLine(ApiMessages.ValidUrl_Pattern_V2);
                        break;
                    case 2:
                        sb.AppendLine(ApiMessages.ValidUrl_Pattern_V2Report);
                        break;
                }
            }
            else
            {
                sb.AppendLine(ApiMessages.ValidUrl_Patterns);
            }

            var res = Request.CreateErrorResponse(HttpStatusCode.BadRequest, sb.ToString());
            res.ReasonPhrase = ApiMessages.InvalidUrl;
            return res;
        }
    }
}
