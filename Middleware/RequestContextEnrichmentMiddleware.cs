using Serilog.Context;
using System.Diagnostics;

namespace MyApi.Middlewarex;

public sealed class RequestContextEnrichmentMiddleware
{
    public const string TraceHeaderName = "X-Trace-Id";
    public const string CorrelationHeaderName = "X-Correlation-ID";
    public const string CorrelationIdItemName = "__CorrelationId";

    private readonly RequestDelegate _next;

    public RequestContextEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var activityTraceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrWhiteSpace(activityTraceId))
        {
            context.TraceIdentifier = activityTraceId;
        }

        var correlationId = ResolveCorrelationId(context) ?? context.TraceIdentifier;
        context.Items[CorrelationIdItemName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[TraceHeaderName] = context.TraceIdentifier;
            context.Response.Headers[CorrelationHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString() ?? string.Empty))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string? ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[CorrelationHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(incoming))
        {
            incoming = context.Request.Headers["X-Request-ID"].ToString();
        }

        if (string.IsNullOrWhiteSpace(incoming))
        {
            return null;
        }

        incoming = incoming.Trim();
        return incoming.Length <= 128 ? incoming : incoming[..128];
    }
}
