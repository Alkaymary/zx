using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApi.Infrastructure.Security;
using MyApi.Models;
using MyApi.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyApi.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string CreateAdminToken(AdminUser adminUser)
    {
        var roleCode = RoleCatalog.NormalizeRoleCode(adminUser.Role.Code, GuardName.Admin);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, adminUser.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, adminUser.Username),
            new(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
            new(ClaimTypes.Name, adminUser.Username),
            new(ClaimTypes.Role, roleCode),
            new("user_type", "admin_user"),
            new("full_name", adminUser.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateLibraryAccountToken(LibraryAccount libraryAccount)
    {
        var roleCode = RoleCatalog.NormalizeRoleCode(libraryAccount.Role.Code, GuardName.Office);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, libraryAccount.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, libraryAccount.Username),
            new(ClaimTypes.NameIdentifier, libraryAccount.Id.ToString()),
            new(ClaimTypes.Name, libraryAccount.Username),
            new(ClaimTypes.Role, roleCode),
            new("user_type", "library_account"),
            new("full_name", libraryAccount.FullName),
            new("library_id", libraryAccount.LibraryId.ToString()),
            new("library_code", libraryAccount.Library.LibraryCode)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
