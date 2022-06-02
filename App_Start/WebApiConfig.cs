namespace Cyara.Web.Portal.App_Start
{
    using System.Web.Http;

    using Cyara.Web.Portal.Areas.Api;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            new ApiAreaRegistration().Register(config);
        }
    }
}
