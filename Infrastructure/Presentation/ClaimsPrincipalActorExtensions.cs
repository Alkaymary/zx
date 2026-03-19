using System.Security.Claims;
using MyApi.Application.Common.Contexts;
using MyApi.Infrastructure.Security;

namespace MyApi.Infrastructure.Presentation;

public static class ClaimsPrincipalActorExtensions
{
    public static AdminActorContext ToAdminActorContext(this ClaimsPrincipal principal)
    {
        var isAdminUser = principal.IsUserType(AuthorizationPolicies.AdminUserType);
        var userId = isAdminUser ? principal.TryGetNameIdentifier() : null;
        var canViewLibraryFinancialData = isAdminUser && !principal.IsInRole(RoleCodes.Support);

        return new AdminActorContext(userId, isAdminUser, canViewLibraryFinancialData);
    }

    public static LibraryActorContext ToLibraryActorContext(this ClaimsPrincipal principal)
    {
        var isLibraryAccount = principal.IsUserType(AuthorizationPolicies.LibraryAccountUserType);
        var accountId = isLibraryAccount ? principal.TryGetNameIdentifier() : null;

        return new LibraryActorContext(accountId, isLibraryAccount);
    }

    private static bool IsUserType(this ClaimsPrincipal principal, string expectedUserType)
    {
        var userType = principal.FindFirstValue(AuthorizationPolicies.UserTypeClaim);
        return string.Equals(userType, expectedUserType, StringComparison.OrdinalIgnoreCase);
    }

    private static int? TryGetNameIdentifier(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var value) ? value : null;
    }
}
