using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using System.Security.Cryptography;

namespace MyApi.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;

    public RefreshTokenService(AppDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken> CreateForAdminAsync(AdminUser adminUser, CancellationToken cancellationToken = default)
    {
        return CreateAsync("admin_user", cancellationToken, adminUserId: adminUser.Id);
    }

    public Task<RefreshToken> CreateForLibraryAccountAsync(LibraryAccount libraryAccount, CancellationToken cancellationToken = default)
    {
        return CreateAsync("library_account", cancellationToken, libraryAccountId: libraryAccount.Id);
    }

    public async Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .Include(x => x.AdminUser!)
                .ThenInclude(x => x.Role)
            .Include(x => x.LibraryAccount!)
                .ThenInclude(x => x.Role)
            .Include(x => x.LibraryAccount!)
                .ThenInclude(x => x.Library)
            .FirstOrDefaultAsync(x =>
                x.Token == token &&
                x.RevokedAt == null &&
                x.ExpiresAt > DateTime.UtcNow,
                cancellationToken);
    }

    private async Task<RefreshToken> CreateAsync(
        string userType,
        CancellationToken cancellationToken,
        int? adminUserId = null,
        int? libraryAccountId = null)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateToken(),
            UserType = userType,
            AdminUserId = adminUserId,
            LibraryAccountId = libraryAccountId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return refreshToken;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
