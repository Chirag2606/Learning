namespace Cyara.Web.Portal.Areas.Api.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Filters;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;
    using Cyara.Web.Resources;

    public class HandleApiException : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var loggerService = DependencyResolver.Current.GetService<ILogger>();
            var logExceptionString = $"[{actionExecutedContext.Request.GetCorrelationId()}]: Message:{actionExecutedContext.Exception.Message} Stack:{actionExecutedContext.Exception.StackTrace}";

            if (IsSuppressibleException(actionExecutedContext.Exception))
            {
                loggerService?.LogDebug(
                    actionExecutedContext.ActionContext.ControllerContext.Controller.GetType(),
                    $"Suppressed Api Call Exception {logExceptionString}");
            }
            else
            {
                loggerService?.LogError(
                    actionExecutedContext.ActionContext.ControllerContext.Controller.GetType(),
                    $"Api Call Exception {logExceptionString}");
            }

            actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ApiMessages.UnhandledException);
            actionExecutedContext.Response.ReasonPhrase = ApiMessages.UnhandledExceptionReason;
            actionExecutedContext.Exception = new HttpResponseException(actionExecutedContext.Response);

            base.OnException(actionExecutedContext);
        }

        private static bool IsSuppressibleException(Exception exception)
        {
            var ignoredExceptionTypes = new List<Type>
                                      {
                                          typeof(OperationCanceledException),
                                          typeof(TaskCanceledException)
                                      };

            return ignoredExceptionTypes.Contains(exception.GetType());
        }
    }
}