namespace Cyara.Web.Portal.Areas.Api.Attributes
{
    using System.Net.Http;
    using System.Web.Http.Filters;
    using System.Web.Mvc;

    using Cyara.Foundation.Logging.Types;

    public class LogApiRequest : System.Web.Http.Filters.ActionFilterAttribute, IFilterPriority
    {
        public int Priority { get; set; } = 1;

        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var loggerService = DependencyResolver.Current.GetService<ILogger>();
            loggerService?.LogDebug(
                actionContext.ControllerContext.Controller.GetType(),
                $"Api Call Start [{actionContext.Request.GetCorrelationId()}]: {actionContext.Request.RequestUri}");
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var loggerService = DependencyResolver.Current.GetService<ILogger>();
            if (loggerService != null)
            {
                var statusCode = actionExecutedContext.Response?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError;
                var reasonPhrase = actionExecutedContext.Response != null ? actionExecutedContext.Response.ReasonPhrase : "Missing Response";

                loggerService.LogDebug(
                    actionExecutedContext.ActionContext.ControllerContext.Controller.GetType(),
                    $"Api Call Finish [{actionExecutedContext.Request.GetCorrelationId()}]: {actionExecutedContext.Request.RequestUri} - {statusCode}: {reasonPhrase}");
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}