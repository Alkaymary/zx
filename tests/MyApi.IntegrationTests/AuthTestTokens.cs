using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyApi.Infrastructure.Security;
using MyApi.Models;

namespace MyApi.IntegrationTests;

public static class AuthTestTokens
{
    public const string Issuer = "MyApi";
    public const string Audience = "MyApiClient";
    public const string SecretKey = "MyApi_Super_Secret_Key_2026_Change_This";
    public const int DefaultAdminUserId = 2001;
    public const int DefaultLibraryAccountId = 3001;
    public const int DefaultLibraryId = 1001;

    public static string CreateAdminToken(
        string roleCode,
        int userId = DefaultAdminUserId,
        string username = "admin.test",
        DateTime? expiresAt = null)
    {
        var normalizedRoleCode = RoleCatalog.NormalizeRoleCode(roleCode, GuardName.Admin);

        return CreateToken(
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, normalizedRoleCode),
            new Claim(AuthorizationPolicies.UserTypeClaim, AuthorizationPolicies.AdminUserType),
            new Claim("full_name", "Integration Admin")
        ],
        expiresAt);
    }

    public static string CreateLibraryAccountToken(
        string roleCode = RoleCodes.OfficeViewer,
        int accountId = DefaultLibraryAccountId,
        int libraryId = DefaultLibraryId,
        string username = "office.test",
        string libraryCode = "LIB-1001",
        DateTime? expiresAt = null)
    {
        var normalizedRoleCode = RoleCatalog.NormalizeRoleCode(roleCode, GuardName.Office);

        return CreateToken(
        [
            new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, normalizedRoleCode),
            new Claim(AuthorizationPolicies.UserTypeClaim, AuthorizationPolicies.LibraryAccountUserType),
            new Claim("full_name", "Integration Office User"),
            new Claim("library_id", libraryId.ToString()),
            new Claim("library_code", libraryCode)
        ],
        expiresAt);
    }

    private static string CreateToken(IEnumerable<Claim> claims, DateTime? expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = expiresAt ?? DateTime.UtcNow.AddMinutes(30);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
