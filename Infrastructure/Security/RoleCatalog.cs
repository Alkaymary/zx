using System.Collections.ObjectModel;
using MyApi.Models;

namespace MyApi.Infrastructure.Security;

public static class RoleCodes
{
    public const string SuperAdmin = "super_admin";
    public const string Admin = "admin";
    public const string Finance = "finance";
    public const string Support = "support";
    public const string Viewer = "viewer";
    public const string OfficeManager = "office_manager";
    public const string OfficeFinance = "office_finance";
    public const string OfficeViewer = "office_viewer";
}

public readonly record struct RequiredRole(string Code, string Name, GuardName GuardName);

public static class RoleCatalog
{
    private static readonly IReadOnlyDictionary<string, string> AdminAliases =
        new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [RoleCodes.SuperAdmin] = RoleCodes.SuperAdmin,
                ["general_manager"] = RoleCodes.SuperAdmin,
                ["admin-general-manager"] = RoleCodes.SuperAdmin,
                [RoleCodes.Admin] = RoleCodes.Admin,
                [RoleCodes.Finance] = RoleCodes.Finance,
                ["accountant"] = RoleCodes.Finance,
                ["admin-sales-manager"] = RoleCodes.Finance,
                [RoleCodes.Support] = RoleCodes.Support,
                ["technician"] = RoleCodes.Support,
                ["technical"] = RoleCodes.Support,
                ["tech"] = RoleCodes.Support,
                ["admin-technical-manager"] = RoleCodes.Support,
                [RoleCodes.Viewer] = RoleCodes.Viewer,
                ["admin-viewer"] = RoleCodes.Viewer,
                ["display"] = RoleCodes.Viewer,
                ["read_only"] = RoleCodes.Viewer,
                ["readonly"] = RoleCodes.Viewer
            });

    private static readonly IReadOnlyDictionary<string, string> OfficeAliases =
        new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [RoleCodes.OfficeManager] = RoleCodes.OfficeManager,
                [RoleCodes.OfficeFinance] = RoleCodes.OfficeFinance,
                [RoleCodes.OfficeViewer] = RoleCodes.OfficeViewer,
                ["office_user"] = RoleCodes.OfficeViewer,
                ["office-accountant"] = RoleCodes.OfficeFinance,
                ["test_role"] = RoleCodes.OfficeViewer
            });

    public static IReadOnlyList<RequiredRole> RequiredRoles { get; } =
    [
        new(RoleCodes.SuperAdmin, "Super Admin", GuardName.Admin),
        new(RoleCodes.Admin, "Admin", GuardName.Admin),
        new(RoleCodes.Finance, "Finance", GuardName.Admin),
        new(RoleCodes.Support, "Support", GuardName.Admin),
        new(RoleCodes.Viewer, "Viewer", GuardName.Admin),
        new(RoleCodes.OfficeManager, "Office Manager", GuardName.Office),
        new(RoleCodes.OfficeFinance, "Office Finance", GuardName.Office),
        new(RoleCodes.OfficeViewer, "Office Viewer", GuardName.Office)
    ];

    public static IReadOnlyList<RequiredRole> RequiredAdminRoles { get; } =
        RequiredRoles.Where(x => x.GuardName == GuardName.Admin).ToArray();

    public static IReadOnlyList<RequiredRole> RequiredOfficeRoles { get; } =
        RequiredRoles.Where(x => x.GuardName == GuardName.Office).ToArray();

    public static IReadOnlyList<string> SupportedAdminRoleCodes { get; } =
    [
        RoleCodes.SuperAdmin,
        RoleCodes.Admin,
        RoleCodes.Finance,
        RoleCodes.Support,
        RoleCodes.Viewer
    ];

    public static IReadOnlyList<string> SupportedOfficeRoleCodes { get; } =
    [
        RoleCodes.OfficeManager,
        RoleCodes.OfficeFinance,
        RoleCodes.OfficeViewer
    ];

    public static string NormalizeRoleCode(string? roleCode, GuardName guardName)
    {
        TryNormalizeRoleCode(roleCode, guardName, out var normalizedCode);
        return normalizedCode;
    }

    public static bool TryNormalizeRoleCode(string? roleCode, GuardName guardName, out string normalizedCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
        {
            normalizedCode = GetFallbackRoleCode(guardName);
            return false;
        }

        var aliases = guardName == GuardName.Admin ? AdminAliases : OfficeAliases;
        if (aliases.TryGetValue(roleCode.Trim(), out normalizedCode!))
        {
            return true;
        }

        normalizedCode = GetFallbackRoleCode(guardName);
        return false;
    }

    public static bool IsCanonicalRoleCode(string? roleCode, GuardName guardName)
    {
        var supportedRoleCodes = guardName == GuardName.Admin ? SupportedAdminRoleCodes : SupportedOfficeRoleCodes;
        return supportedRoleCodes.Contains(roleCode ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> GetSupportedRoleCodes(GuardName guardName)
    {
        return guardName == GuardName.Admin ? SupportedAdminRoleCodes : SupportedOfficeRoleCodes;
    }

    public static string GetFallbackRoleCode(GuardName guardName)
    {
        return guardName == GuardName.Admin ? RoleCodes.Viewer : RoleCodes.OfficeViewer;
    }
}
