using MyApi.Models;

namespace MyApi.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateForAdminAsync(AdminUser adminUser, CancellationToken cancellationToken = default);

    Task<RefreshToken> CreateForLibraryAccountAsync(LibraryAccount libraryAccount, CancellationToken cancellationToken = default);

    Task RevokeAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);
}
