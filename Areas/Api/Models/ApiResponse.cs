namespace Cyara.Web.Portal.Areas.Api.Models
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Cyara.Domain.Types.Responses;
    using Cyara.Foundation.Logging.Types;
    using Cyara.Shared.Web.Extensions;
    using Cyara.Web.Resources;

    public class ApiResponse
    {
        protected ApiResponse()
        {
        }

        public HttpStatusCode StatusCode { get; set; }

        public string Description { get; set; }

        public string Reason { get; set; }

        public Exception Exception { get; set; }

        public bool IsSuccess => StatusCode == HttpStatusCode.OK;

        public static ApiResponse Succeeds()
        {
            return new ApiResponse { StatusCode = HttpStatusCode.OK };
        }

        // Failure construction methods
        public static ApiResponse Fails(HttpStatusCode statusCode, string description, string reason)
        {
            return new ApiResponse { StatusCode = statusCode, Description = description, Reason = reason };
        }

        public static ApiResponse Fails(BaseResponse response)
        {
            return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Description = response.ErrorMessage(), Reason = response.Code(), Exception = response.Exception };
        }

        public static ApiResponse<T> Fails<T>(GenericResponse<T> response)
        {
            return new ApiResponse<T> { StatusCode = HttpStatusCode.InternalServerError, Description = response.ErrorMessage(), Reason = response.Code(), Value = response.Value, Exception = response.Exception };
        }

        public static ApiResponse<T> Fails<T>(HttpStatusCode statusCode, string description, string reason)
        {
            return new ApiResponse<T> { StatusCode = statusCode, Description = description, Reason = reason };
        }

        public static ApiResponse NotFoundId(string entity, int id)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Reason = string.Format(ApiMessages.NotFound, entity),
                Description = string.Format(ApiMessages.NotFound_Id, entity, id)
            };
        }

        public static ApiResponse<T> NotFoundId<T>(string entity, int id)
        {
            return new ApiResponse<T>
            {
                StatusCode = HttpStatusCode.NotFound,
                Reason = string.Format(ApiMessages.NotFound, entity),
                Description = string.Format(ApiMessages.NotFound_Id, entity, id)
            };
        }

        public static ApiResponse ValidationFails(string description)
        {
            return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, Description = description, Reason = ApiMessages.InvalidArgument };
        }

        public static ApiResponse From(BaseResponse response, string failureMessage = null)
        {
            if (response != null)
            {
                var ex = response.Exception;
                if (ex != null)
                {
                    return Error(ex);
                }

                if (response.IsSuccess)
                {
                    return Succeeds();
                }
            }
            
            return Fails(HttpStatusCode.BadRequest, failureMessage ?? response.ErrorMessage(), response.Code());
        }

        public static ApiResponse<T> From<T>(GenericResponse<T> response)
        {
            if (response != null)
            {
                var ex = response.Exception;
                if (ex != null)
                {
                    return Error<T>(ex);
                }

                if (response.IsSuccess)
                {
                    return Succeeds(response.Value);
                }
            }

            return Fails(response);
        }

        public static ApiResponse<T> Succeeds<T>(T value)
        {
            return new ApiResponse<T> { StatusCode = HttpStatusCode.OK, Value = value };
        }

        public static ApiResponse Error(Exception ex)
        {
            return new ApiResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Exception = ex,
                Description = ApiMessages.UnhandledException,
                Reason = ApiMessages.UnhandledExceptionReason
            };
        }

        public static ApiResponse<T> Error<T>(Exception ex)
        {
            return new ApiResponse<T>
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Exception = ex,
                Description = ApiMessages.UnhandledException,
                Reason = ApiMessages.UnhandledExceptionReason
            };
        }

        public virtual HttpResponseMessage Construct(ApiController controller, ILogger logger)
        {
            HttpExceptionIfRequired(controller, logger);
            return controller.ControllerContext.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        public void HttpExceptionIfRequired(ApiController controller, ILogger logger)
        {
            if (StatusCode != HttpStatusCode.OK)
            {
                if (logger != null && Exception != null)
                {
                    // new System.Diagnostics.StackTrace().GetFrame(1).
                    logger.LogError(
                        controller.GetType(), 
                        $"Api Call Exception [{controller.Request.GetCorrelationId()}]: Message:{Exception.Message} Stack:{Exception.StackTrace}");
                }

                var res = controller.Request.CreateErrorResponse(StatusCode, Description);
                if (!string.IsNullOrWhiteSpace(Reason))
                {
                    res.ReasonPhrase = Reason;
                }

                throw new HttpResponseException(res);
            }
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Value { get; set; }

        public static new ApiResponse<T> Fails(HttpStatusCode statusCode, string description, string reason)
        {
            return new ApiResponse<T> { StatusCode = statusCode, Description = description, Reason = reason };
        }

        public static ApiResponse<T> Fails(ApiResponse from)
        {
            return new ApiResponse<T> { StatusCode = from.StatusCode, Description = from.Description, Reason = from.Reason };
        }

        public static new ApiResponse<T> From(BaseResponse response, string failureMessage = null)
        {
            if (response != null)
            {
                var ex = response.Exception;
                if (ex != null)
                {
                    return Error<T>(ex);
                }

                if (response.IsSuccess)
                {
                    return Succeeds(default(T));
                }
            }

            return Fails<T>(HttpStatusCode.BadRequest, failureMessage ?? response.ErrorMessage(), response.Code());
        }

        public static new ApiResponse<T> ValidationFails(string description)
        {
            return new ApiResponse<T> { StatusCode = HttpStatusCode.BadRequest, Description = description, Reason = ApiMessages.InvalidArgument };
        }

        public override HttpResponseMessage Construct(ApiController controller, ILogger logger)
        {
            HttpExceptionIfRequired(controller, logger);

            if (Value == null && StatusCode == HttpStatusCode.OK)
            {
                return controller.ControllerContext.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            HttpResponseMessage res = IsSuccess 
                                            ? controller.ControllerContext.Request.CreateResponse(StatusCode, Value) 
                                            : controller.ControllerContext.Request.CreateErrorResponse(StatusCode, Description);

            if (!string.IsNullOrWhiteSpace(Reason))
            {
                res.ReasonPhrase = Reason;
            }

            return res;
        }
    }
}
