namespace Cyara.Web.Portal.Areas.Api.Extensions
{
    using System.Net.Http;

    public static class HttpResponseMessageExtensions
    {
        public static T ExtractContentFromHttpResponse<T>(this HttpResponseMessage value) where T : class
        {
            if (value.IsSuccessStatusCode)
            {
                var e = value.Content as ObjectContent<T>;
                return e?.Value as T;
            }

            return null;
        }
    }
}