namespace Cyara.Web.Portal.Areas.Api.Extensions
{
    using System.Web.Http;

    using Cyara.Domain.Types.Responses;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Web.Portal.Areas.Api.Models;

    public static class GenericResponseExtensions
    {
        public static void HttpExceptionIfRequired<T>(this GenericResponse<T> response, ApiController controller, ILogger logger)
        {
            var ex = response?.Exception;
            if (ex != null)
            {
                ApiResponse.Error(ex).HttpExceptionIfRequired(controller, logger);
            }
        }
    }
}
