using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ERP.Application.Common.Exceptions;
using System.Net;

namespace ERP.Web.API.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;

        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            _logger = logger;

            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(NotFoundException), HandleNotFoundException },
                { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
                { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
            };
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);

            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();

            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            if (!context.ModelState.IsValid)
            {
                HandleInvalidModelStateException(context);
                return;
            }

            HandleUnknownException(context);
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = (ValidationException)context.Exception;

            var details = new ValidationProblemDetails(exception.Errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation error occurred.",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            _logger.LogWarning(exception, "Validation exception occurred: {Errors}",
                string.Join(", ", exception.Errors.SelectMany(e => e.Value)));

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleInvalidModelStateException(ExceptionContext context)
        {
            var details = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Invalid model state.",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "One or more model validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            };

            // null 조건부 연산자를 사용한 안전한 로깅
            var errorMessages = context.ModelState
                .Where(e => e.Value?.Errors?.Count > 0)
                .SelectMany(e => e.Value?.Errors?.Select(er => $"{e.Key}: {er.ErrorMessage}") ?? Enumerable.Empty<string>())
                .Where(msg => !string.IsNullOrEmpty(msg));

            _logger.LogWarning("Invalid model state: {ModelState}", string.Join(", ", errorMessages));

            context.Result = new BadRequestObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = (NotFoundException)context.Exception;

            var details = new ProblemDetails()
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "The specified resource was not found.",
                Status = (int)HttpStatusCode.NotFound,
                Detail = exception.Message,
                Instance = context.HttpContext.Request.Path
            };

            _logger.LogWarning(exception, "Not found exception occurred: {Message}", exception.Message);

            context.Result = new NotFoundObjectResult(details);
            context.ExceptionHandled = true;
        }

        private void HandleUnauthorizedAccessException(ExceptionContext context)
        {
            var details = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "You are not authorized to access this resource.",
                Instance = context.HttpContext.Request.Path
            };

            _logger.LogWarning(context.Exception, "Unauthorized access attempt");

            context.Result = new ObjectResult(details)
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
            context.ExceptionHandled = true;
        }

        private void HandleForbiddenAccessException(ExceptionContext context)
        {
            var details = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = (int)HttpStatusCode.Forbidden,
                Detail = "You do not have permission to access this resource.",
                Instance = context.HttpContext.Request.Path
            };

            _logger.LogWarning(context.Exception, "Forbidden access attempt");

            context.Result = new ObjectResult(details)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
            context.ExceptionHandled = true;
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            var details = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "An error occurred while processing your request.",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = context.HttpContext.Request.Path
            };

            _logger.LogError(context.Exception, "Unhandled exception occurred: {Message}",
                context.Exception.Message);

            context.Result = new ObjectResult(details)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
            context.ExceptionHandled = true;
        }
    }
}
