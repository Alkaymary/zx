using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure.Security;
using MyApi.Models;
using MyApi.Options;
using MyApi.Services;

namespace MyApi.Application.Auth;

public interface IAdminAuthAppService
{
    Task<AppResult<AuthResponseDto>> AdminLoginAsync(AdminLoginRequestDto request, CancellationToken cancellationToken);
    Task<AppResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken);
}

public class AdminAuthAppService : IAdminAuthAppService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtSettings _jwtSettings;

    public AdminAuthAppService(
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AppResult<AuthResponseDto>> AdminLoginAsync(AdminLoginRequestDto request, CancellationToken cancellationToken)
    {
        var admin = await _context.AdminUsers
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

        if (admin is null || admin.PasswordHash != request.Password)
        {
            return AppResult<AuthResponseDto>.Unauthorized("Invalid username or password.");
        }

        if (admin.Status != RecordStatus.Active)
        {
            return AppResult<AuthResponseDto>.Unauthorized("This admin user is not active.");
        }

        admin.LastLoginAt = DateTime.UtcNow;
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.CreateAdminToken(admin);
        var refreshToken = await _refreshTokenService.CreateForAdminAsync(admin, cancellationToken);
        var roleCode = RoleCatalog.NormalizeRoleCode(admin.Role.Code, GuardName.Admin);

        return AppResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            UserId = admin.Id,
            Username = admin.Username,
            RoleCode = roleCode
        });
    }

    public async Task<AppResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenService.GetActiveTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken?.AdminUser is null)
        {
            return AppResult<AuthResponseDto>.Unauthorized("Invalid or expired refresh token.");
        }

        if (refreshToken.AdminUser.Status != RecordStatus.Active)
        {
            return AppResult<AuthResponseDto>.Unauthorized("This admin user is not active.");
        }

        await _refreshTokenService.RevokeAsync(refreshToken, cancellationToken);

        var accessToken = _jwtTokenService.CreateAdminToken(refreshToken.AdminUser);
        var newRefreshToken = await _refreshTokenService.CreateForAdminAsync(refreshToken.AdminUser, cancellationToken);
        var roleCode = RoleCatalog.NormalizeRoleCode(refreshToken.AdminUser.Role.Code, GuardName.Admin);

        return AppResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            UserId = refreshToken.AdminUser.Id,
            Username = refreshToken.AdminUser.Username,
            RoleCode = roleCode
        });
    }
}
