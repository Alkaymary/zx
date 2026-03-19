using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using MyApi.Models;
using MyApi.Services;

namespace MyApi.Middlewarex;

public sealed class AuditLoggingMiddleware
{
    private const int MaxPayloadLength = 12000;
    private static readonly HashSet<string> SensitiveKeys =
    [
        "password",
        "passwordhash",
        "password_hash",
        "access_token",
        "accesstoken",
        "refresh_token",
        "refreshtoken",
        "authorization",
        "token",
        "secretkey",
        "secret_key"
    ];

    private readonly RequestDelegate _next;
    private readonly IAuditLogSink _auditLogSink;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        IAuditLogSink auditLogSink,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _auditLogSink = auditLogSink;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await CapturePrincipalAsync(context);
        var requestPayload = await ReadRequestPayloadAsync(context);
        var originalBody = context.Response.Body;

        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        Exception? exception = null;
        var requestAborted = false;

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            requestAborted = true;
            throw;
        }
        catch (IOException) when (context.RequestAborted.IsCancellationRequested)
        {
            requestAborted = true;
            throw;
        }
        catch (Exception ex)
        {
            exception = ex;
            if (context.Response.StatusCode < 400)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }

        if (requestAborted || context.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        string? responsePayload;
        if (exception is null)
        {
            responsePayload = await ReadResponsePayloadAsync(responseBuffer, context.Response.ContentType);
            responseBuffer.Position = 0;

            try
            {
                await responseBuffer.CopyToAsync(originalBody, context.RequestAborted);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                return;
            }
            catch (IOException) when (context.RequestAborted.IsCancellationRequested)
            {
                return;
            }
        }
        else
        {
            responsePayload = TrimPayload(RedactText(exception.ToString()));
        }

        stopwatch.Stop();

        var endpoint = context.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var (accountType, accountId, accountUsername, roleCode) = ResolveActor(context.User);

        var log = new AuditLog
        {
            TraceIdentifier = context.TraceIdentifier,
            OperationDate = DateTime.UtcNow,
            AccountType = accountType,
            AccountId = accountId,
            AccountUsername = accountUsername,
            RoleCode = roleCode,
            Endpoint = context.Request.Path.Value ?? "/",
            QueryString = string.IsNullOrWhiteSpace(context.Request.QueryString.Value) ? null : context.Request.QueryString.Value,
            ActionName = ResolveActionName(context, endpoint, actionDescriptor),
            HttpMethod = context.Request.Method,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload,
            StatusCode = context.Response.StatusCode,
            Status = ResolveStatus(context.Response.StatusCode),
            SecurityLevel = ResolveSecurityLevel(context, endpoint),
            IpAddress = ResolveIpAddress(context),
            DurationMs = stopwatch.ElapsedMilliseconds
        };

        try
        {
            _auditLogSink.TryWrite(log);
        }
        catch (Exception sinkException)
        {
            _logger.LogWarning(
                sinkException,
                "Audit log enqueue failed for trace {TraceIdentifier}.",
                log.TraceIdentifier);
        }

        if (exception is not null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private static async Task CapturePrincipalAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            return;
        }

        var authenticateResult = await context.AuthenticateAsync();
        if (authenticateResult.Succeeded && authenticateResult.Principal is not null)
        {
            context.User = authenticateResult.Principal;
        }
    }

    private static async Task<string?> ReadRequestPayloadAsync(HttpContext context)
    {
        if (!HasReadableBody(context.Request.ContentLength, context.Request.Method))
        {
            return null;
        }

        context.Request.EnableBuffering();
        context.Request.Body.Position = 0;

        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var raw = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (!IsTextContent(context.Request.ContentType))
        {
            return $"[{context.Request.ContentType ?? "binary"} request body omitted]";
        }

        return TrimPayload(SanitizePayload(raw));
    }

    private static async Task<string?> ReadResponsePayloadAsync(Stream responseBuffer, string? contentType)
    {
        if (responseBuffer.Length == 0)
        {
            return null;
        }

        if (!IsTextContent(contentType))
        {
            return $"[{contentType ?? "binary"} response body omitted - {responseBuffer.Length} bytes]";
        }

        responseBuffer.Position = 0;
        using var reader = new StreamReader(responseBuffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var raw = await reader.ReadToEndAsync();
        responseBuffer.Position = 0;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return TrimPayload(SanitizePayload(raw));
    }

    private static bool HasReadableBody(long? contentLength, string method)
    {
        if (contentLength.GetValueOrDefault() <= 0)
        {
            return false;
        }

        return !HttpMethods.IsGet(method)
            && !HttpMethods.IsHead(method)
            && !HttpMethods.IsOptions(method)
            && !HttpMethods.IsTrace(method);
    }

    private static bool IsTextContent(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return true;
        }

        var normalized = contentType.ToLowerInvariant();
        return normalized.Contains("json")
            || normalized.Contains("text")
            || normalized.Contains("xml")
            || normalized.Contains("javascript")
            || normalized.Contains("html")
            || normalized.Contains("x-www-form-urlencoded");
    }

    private static string SanitizePayload(string raw)
    {
        var trimmed = raw.Trim();

        if (LooksLikeJson(trimmed))
        {
            try
            {
                var node = JsonNode.Parse(trimmed);
                RedactJsonNode(node);
                return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? RedactText(trimmed);
            }
            catch
            {
                return RedactText(trimmed);
            }
        }

        return RedactText(trimmed);
    }

    private static bool LooksLikeJson(string value)
    {
        return value.StartsWith("{", StringComparison.Ordinal)
            || value.StartsWith("[", StringComparison.Ordinal);
    }

    private static void RedactJsonNode(JsonNode? node)
    {
        if (node is null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            foreach (var property in obj.ToList())
            {
                if (SensitiveKeys.Contains(property.Key))
                {
                    obj[property.Key] = "***";
                    continue;
                }

                RedactJsonNode(property.Value);
            }

            return;
        }

        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                RedactJsonNode(item);
            }
        }
    }

    private static string RedactText(string value)
    {
        var sanitized = value;

        foreach (var key in SensitiveKeys)
        {
            sanitized = Regex.Replace(
                sanitized,
                $@"(""{Regex.Escape(key)}""\s*:\s*"")[^""]*(""?)",
                "$1***$2",
                RegexOptions.IgnoreCase);

            sanitized = Regex.Replace(
                sanitized,
                $@"({Regex.Escape(key)}=)[^&\s]+",
                "$1***",
                RegexOptions.IgnoreCase);
        }

        return sanitized;
    }

    private static string TrimPayload(string value)
    {
        if (value.Length <= MaxPayloadLength)
        {
            return value;
        }

        return $"{value[..MaxPayloadLength]} ...[truncated]";
    }

    private static (string accountType, int? accountId, string? accountUsername, string? roleCode) ResolveActor(ClaimsPrincipal user)
    {
        if (!(user.Identity?.IsAuthenticated ?? false))
        {
            return ("Anonymous", null, "Anonymous", null);
        }

        var identifierText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        int? accountId = int.TryParse(identifierText, out var parsedId) ? parsedId : null;
        var accountUsername = user.FindFirstValue(ClaimTypes.Name) ?? user.FindFirstValue("unique_name");
        var roleCode = user.FindFirstValue(ClaimTypes.Role);
        var accountType = user.HasClaim(claim => claim.Type == "library_id") ? "LibraryAccount" : "AdminUser";

        return (accountType, accountId, accountUsername, roleCode);
    }

    private static string ResolveActionName(HttpContext context, Endpoint? endpoint, ControllerActionDescriptor? actionDescriptor)
    {
        if (actionDescriptor is not null)
        {
            return $"{actionDescriptor.ControllerName}.{actionDescriptor.ActionName}";
        }

        return endpoint?.DisplayName ?? context.Request.Method;
    }

    private static AuditLogStatus ResolveStatus(int statusCode)
    {
        if (statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
        {
            return AuditLogStatus.Blocked;
        }

        if (statusCode >= 500)
        {
            return AuditLogStatus.ServerError;
        }

        if (statusCode >= 400)
        {
            return AuditLogStatus.ClientError;
        }

        if (statusCode >= 300)
        {
            return AuditLogStatus.Redirected;
        }

        return AuditLogStatus.Succeeded;
    }

    private static AuditSecurityLevel ResolveSecurityLevel(HttpContext context, Endpoint? endpoint)
    {
        var path = context.Request.Path;
        var allowsAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (allowsAnonymous || !path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return AuditSecurityLevel.Public;
        }

        if (context.User.HasClaim(claim => claim.Type == "library_id")
            || path.StartsWithSegments("/api/LibraryAuth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/LibraryFinancial", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/QrCodes", StringComparison.OrdinalIgnoreCase))
        {
            return AuditSecurityLevel.Protected;
        }

        return AuditSecurityLevel.Privileged;
    }

    private static string? ResolveIpAddress(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var forwardedValue = forwarded.ToString();
            if (!string.IsNullOrWhiteSpace(forwardedValue))
            {
                return forwardedValue.Split(',')[0].Trim();
            }
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var realIpValue = realIp.ToString();
            if (!string.IsNullOrWhiteSpace(realIpValue))
            {
                return realIpValue.Trim();
            }
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}


