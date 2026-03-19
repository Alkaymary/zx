namespace MyApi.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string RequireRoleManagement = nameof(RequireRoleManagement);
    public const string RequireAdminRead = nameof(RequireAdminRead);
    public const string RequireAdminManagement = nameof(RequireAdminManagement);
    public const string RequireAuditAccess = nameof(RequireAuditAccess);
    public const string RequireFinancialRead = nameof(RequireFinancialRead);
    public const string RequireFinancialWrite = nameof(RequireFinancialWrite);
    public const string RequireFinancialDelete = nameof(RequireFinancialDelete);
    public const string RequireLibraryRead = nameof(RequireLibraryRead);
    public const string RequireLibraryWrite = nameof(RequireLibraryWrite);
    public const string RequireLibraryDelete = nameof(RequireLibraryDelete);
    public const string RequireLibraryAccountCreate = nameof(RequireLibraryAccountCreate);
    public const string RequireLibraryAccountManagement = nameof(RequireLibraryAccountManagement);
    public const string RequirePosRead = nameof(RequirePosRead);
    public const string RequirePosWrite = nameof(RequirePosWrite);
    public const string RequireQrRead = nameof(RequireQrRead);
    public const string RequireQrWrite = nameof(RequireQrWrite);
    public const string RequirePackageRead = nameof(RequirePackageRead);
    public const string RequirePackageWrite = nameof(RequirePackageWrite);
    public const string RequireLibraryAccount = nameof(RequireLibraryAccount);

    public const string UserTypeClaim = "user_type";
    public const string AdminUserType = "admin_user";
    public const string LibraryAccountUserType = "library_account";
}
