using System.Collections.ObjectModel;
using MyApi.Infrastructure.Security;
using MyApi.Models;

namespace MyApi.Infrastructure.AdminAccess;

public readonly record struct AdminApiRequirement(AdminArea Area, AdminAccessLevel Access);

public static class AdminPermissionProfiles
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<AdminArea, AdminAccessLevel>> Profiles =
        new ReadOnlyDictionary<string, IReadOnlyDictionary<AdminArea, AdminAccessLevel>>(
            new Dictionary<string, IReadOnlyDictionary<AdminArea, AdminAccessLevel>>(StringComparer.OrdinalIgnoreCase)
            {
                [RoleCodes.SuperAdmin] = Permissions(
                    (AdminArea.Dashboard, AdminAccessLevel.Manage),
                    (AdminArea.Search, AdminAccessLevel.Manage),
                    (AdminArea.Offices, AdminAccessLevel.Manage),
                    (AdminArea.Devices, AdminAccessLevel.Manage),
                    (AdminArea.Finance, AdminAccessLevel.Manage),
                    (AdminArea.Users, AdminAccessLevel.Manage),
                    (AdminArea.Roles, AdminAccessLevel.Manage),
                    (AdminArea.Reports, AdminAccessLevel.Manage),
                    (AdminArea.Performance, AdminAccessLevel.Manage)),
                [RoleCodes.Admin] = Permissions(
                    (AdminArea.Dashboard, AdminAccessLevel.Read),
                    (AdminArea.Search, AdminAccessLevel.Read),
                    (AdminArea.Offices, AdminAccessLevel.Manage),
                    (AdminArea.Devices, AdminAccessLevel.Manage),
                    (AdminArea.Finance, AdminAccessLevel.Read),
                    (AdminArea.Users, AdminAccessLevel.Read),
                    (AdminArea.Reports, AdminAccessLevel.Read),
                    (AdminArea.Performance, AdminAccessLevel.Read)),
                [RoleCodes.Finance] = Permissions(
                    (AdminArea.Dashboard, AdminAccessLevel.Read),
                    (AdminArea.Finance, AdminAccessLevel.Manage),
                    (AdminArea.Reports, AdminAccessLevel.Read),
                    (AdminArea.Performance, AdminAccessLevel.Read)),
                [RoleCodes.Support] = Permissions(
                    (AdminArea.Dashboard, AdminAccessLevel.Read),
                    (AdminArea.Search, AdminAccessLevel.Read),
                    (AdminArea.Offices, AdminAccessLevel.Read),
                    (AdminArea.Devices, AdminAccessLevel.Manage),
                    (AdminArea.Reports, AdminAccessLevel.Read)),
                [RoleCodes.Viewer] = Permissions(
                    (AdminArea.Dashboard, AdminAccessLevel.Read),
                    (AdminArea.Search, AdminAccessLevel.Read),
                    (AdminArea.Offices, AdminAccessLevel.Read),
                    (AdminArea.Reports, AdminAccessLevel.Read),
                    (AdminArea.Performance, AdminAccessLevel.Read))
            });

    public static IReadOnlyList<RequiredRole> RequiredAdminRoles { get; } = RoleCatalog.RequiredAdminRoles;

    public static string NormalizeRoleCode(string? roleCode)
    {
        return RoleCatalog.NormalizeRoleCode(roleCode, GuardName.Admin);
    }

    public static bool HasAccess(string? roleCode, AdminArea area, AdminAccessLevel requiredAccess)
    {
        var normalizedRoleCode = NormalizeRoleCode(roleCode);
        if (!Profiles.TryGetValue(normalizedRoleCode, out var permissions))
        {
            return false;
        }

        return permissions.TryGetValue(area, out var grantedAccess)
            && grantedAccess >= requiredAccess;
    }

    private static IReadOnlyDictionary<AdminArea, AdminAccessLevel> Permissions(
        params (AdminArea Area, AdminAccessLevel Access)[] entries)
    {
        var map = new Dictionary<AdminArea, AdminAccessLevel>();
        foreach (var (area, access) in entries)
        {
            map[area] = access;
        }

        return new ReadOnlyDictionary<AdminArea, AdminAccessLevel>(map);
    }
}
