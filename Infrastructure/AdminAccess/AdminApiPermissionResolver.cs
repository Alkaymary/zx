namespace MyApi.Infrastructure.AdminAccess;

public static class AdminApiPermissionResolver
{
    public static bool TryResolve(PathString path, string? method, out AdminApiRequirement requirement)
    {
        requirement = default;
        var value = path.Value;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsBypassedApi(value))
        {
            return false;
        }

        var access = IsReadMethod(method)
            ? AdminAccessLevel.Read
            : AdminAccessLevel.Manage;

        if (value.StartsWith("/api/libraries", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/libraryaccounts", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Offices, access);
            return true;
        }

        if (value.StartsWith("/api/posdevices", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Devices, access);
            return true;
        }

        if (value.StartsWith("/api/financialtransactions", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/packages", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/adminqrcodes", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Finance, access);
            return true;
        }

        if (value.StartsWith("/api/adminusers", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Users, access);
            return true;
        }

        if (value.StartsWith("/api/roles", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Roles, access);
            return true;
        }

        if (value.StartsWith("/api/auditlogs", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Reports, AdminAccessLevel.Read);
            return true;
        }

        if (value.StartsWith("/api/weatherforecast", StringComparison.OrdinalIgnoreCase))
        {
            requirement = new AdminApiRequirement(AdminArea.Dashboard, AdminAccessLevel.Read);
            return true;
        }

        return false;
    }

    private static bool IsBypassedApi(string path)
    {
        return path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/libraryauth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/libraryfinancial", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/qrcodes", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsReadMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return false;
        }

        return HttpMethods.IsGet(method)
            || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method);
    }
}
