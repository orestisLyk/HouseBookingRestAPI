using HouseBookingRestApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HouseBookingRestApi.Helpers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        private static (int StatusCode, string Title, bool isExpected) MapException(Exception ex) => ex switch
        {
            EntityAlreadyExistsException => (StatusCodes.Status409Conflict, "Resource already exists", true),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Resource not found", true),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation", true),
            EntityForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", true),
            EntityNotAuthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", true),
            BookingsOverlapException => (StatusCodes.Status409Conflict, "dates overlap with existing booking", true),
            InvalidBookingDatesException => (StatusCodes.Status400BadRequest, "Invalid booking dates", true),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials", true),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error", false)

        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, isExpected) = MapException(exception);

            if (isExpected)
            {
                _logger.LogWarning(exception, "Handled exception: {Title}", title);

            }
            else
            {
                _logger.LogError(exception, "Unhandled exception: {Title}", title);

            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = isExpected ? exception.Message : "An unexpected error occurred.",
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;

        }
    }
}
