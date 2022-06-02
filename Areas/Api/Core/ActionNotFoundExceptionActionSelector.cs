namespace Cyara.Web.Portal.Areas.Api.Core
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Web.Resources;

    public class ActionNotFoundExceptionActionSelector : ApiControllerActionSelector
    {
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            try
            {
                var action = base.SelectAction(controllerContext);
                return action;
            }
            catch (Exception ex)
            {
                if (ex is HttpResponseException)
                {
                    var resEx = (HttpResponseException)ex;

                    if (resEx.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        var loggerService = DependencyResolver.Current.GetService<ILogger>();
                        loggerService?.LogError(
                            GetType(),
                            $"Api Call Error [{controllerContext.Request.GetCorrelationId()}]: Action not found : {controllerContext.Request.RequestUri}");

                        var res = controllerContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ApiMessages.InvalidUrl_ControllerOrAction + "\r\n" + ApiMessages.ValidUrl_Patterns);
                        res.ReasonPhrase = ApiMessages.InvalidUrl;
                        throw new HttpResponseException(res);
                    }
                }

                throw;
            }
        }
    }
}