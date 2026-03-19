using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MyApi.Infrastructure.AdminAccess;

namespace MyApi.Middlewarex;

public sealed class AdminApiAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdminApiAuthorizationMiddleware> _logger;

    public AdminApiAuthorizationMiddleware(
        RequestDelegate next,
        ILogger<AdminApiAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await _next(context);
            return;
        }

        if (!AdminApiPermissionResolver.TryResolve(context.Request.Path, context.Request.Method, out var requirement))
        {
            await _next(context);
            return;
        }

        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            await _next(context);
            return;
        }

        var userType = context.User.FindFirstValue("user_type");
        if (!string.Equals(userType, "admin_user", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Blocked non-admin token from {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteForbiddenAsync(
                context,
                requirement,
                "This endpoint is available to admin accounts only.");
            return;
        }

        var roleCode = context.User.FindFirstValue(ClaimTypes.Role);
        if (AdminPermissionProfiles.HasAccess(roleCode, requirement.Area, requirement.Access))
        {
            await _next(context);
            return;
        }

        _logger.LogInformation(
            "Blocked role {RoleCode} from {Access} access to {Area} via {Method} {Path}.",
            roleCode,
            requirement.Access,
            requirement.Area,
            context.Request.Method,
            context.Request.Path);

        await WriteForbiddenAsync(
            context,
            requirement,
            $"Your admin account does not have {requirement.Access.ToString().ToLowerInvariant()} access to {requirement.Area}.");
    }

    private static Task WriteForbiddenAsync(
        HttpContext context,
        AdminApiRequirement requirement,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/403",
            title = "Forbidden",
            status = StatusCodes.Status403Forbidden,
            detail,
            area = requirement.Area.ToString(),
            access = requirement.Access.ToString(),
            traceId = context.TraceIdentifier
        });
    }
}
