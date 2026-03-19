using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.Infrastructure.ExceptionHandling;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (ApiExceptionClassifier.IsRequestAborted(exception, httpContext))
        {
            _logger.LogDebug(
                exception,
                "Request aborted before the API response completed. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
            return true;
        }

        var isTransient = ApiExceptionClassifier.IsTransient(exception, httpContext);
        var statusCode = isTransient
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status500InternalServerError;
        var title = isTransient
            ? "A transient database or timeout error occurred. Please retry."
            : "An unexpected server error occurred.";

        _logger.Log(
            isTransient ? LogLevel.Warning : LogLevel.Error,
            exception,
            "API request failed with status {StatusCode}. TraceId: {TraceId}",
            statusCode,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = $"https://httpstatuses.com/{statusCode}"
            },
            Exception = exception
        });
    }
}
