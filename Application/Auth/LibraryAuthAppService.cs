using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure.Security;
using MyApi.Models;
using MyApi.Options;
using MyApi.Services;

namespace MyApi.Application.Auth;

public interface ILibraryAuthAppService
{
    Task<AppResult<LibraryAuthResponseDto>> LoginAsync(LibraryLoginRequestDto request, CancellationToken cancellationToken);
    Task<AppResult<LibraryMeResponseDto>> GetMeAsync(LibraryActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<LibraryAuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken);
}

public class LibraryAuthAppService : ILibraryAuthAppService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtSettings _jwtSettings;

    public LibraryAuthAppService(
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

    public async Task<AppResult<LibraryAuthResponseDto>> LoginAsync(LibraryLoginRequestDto request, CancellationToken cancellationToken)
    {
        var account = await _context.LibraryAccounts
            .Include(x => x.Role)
            .Include(x => x.Library)
            .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

        if (account is null || account.PasswordHash != request.Password)
        {
            return AppResult<LibraryAuthResponseDto>.Unauthorized("Invalid username or password.");
        }

        var validation = ValidateLibraryAccount(account);
        if (validation is not null)
        {
            return validation;
        }

        account.LastLoginAt = DateTime.UtcNow;
        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.CreateLibraryAccountToken(account);
        var refreshToken = await _refreshTokenService.CreateForLibraryAccountAsync(account, cancellationToken);

        return AppResult<LibraryAuthResponseDto>.Success(
            await BuildLibraryAuthResponseAsync(account, cancellationToken, token, refreshToken.Token));
    }

    public async Task<AppResult<LibraryMeResponseDto>> GetMeAsync(LibraryActorContext actor, CancellationToken cancellationToken)
    {
        if (!actor.IsLibraryAccount)
        {
            return AppResult<LibraryMeResponseDto>.Forbid();
        }

        if (!actor.AccountId.HasValue)
        {
            return AppResult<LibraryMeResponseDto>.Unauthorized("Invalid token.");
        }

        var account = await _context.LibraryAccounts
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Library)
            .FirstOrDefaultAsync(x => x.Id == actor.AccountId.Value, cancellationToken);

        if (account is null)
        {
            return AppResult<LibraryMeResponseDto>.NotFound("Library account was not found.");
        }

        return AppResult<LibraryMeResponseDto>.Success(
            await BuildLibraryMeResponseAsync(account, cancellationToken));
    }

    public async Task<AppResult<LibraryAuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenService.GetActiveTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken?.LibraryAccount is null)
        {
            return AppResult<LibraryAuthResponseDto>.Unauthorized("Invalid or expired refresh token.");
        }

        var account = refreshToken.LibraryAccount;
        var validation = ValidateLibraryAccount(account);
        if (validation is not null)
        {
            return validation;
        }

        await _refreshTokenService.RevokeAsync(refreshToken, cancellationToken);

        var accessToken = _jwtTokenService.CreateLibraryAccountToken(account);
        var newRefreshToken = await _refreshTokenService.CreateForLibraryAccountAsync(account, cancellationToken);

        return AppResult<LibraryAuthResponseDto>.Success(
            await BuildLibraryAuthResponseAsync(account, cancellationToken, accessToken, newRefreshToken.Token));
    }

    private AppResult<LibraryAuthResponseDto>? ValidateLibraryAccount(LibraryAccount account)
    {
        if (account.Status != RecordStatus.Active)
        {
            return AppResult<LibraryAuthResponseDto>.Unauthorized("This library account is not active.");
        }

        if (account.Library.Status != RecordStatus.Active)
        {
            return AppResult<LibraryAuthResponseDto>.Unauthorized("This library is not active.");
        }

        if (account.Role.GuardName != GuardName.Office)
        {
            return AppResult<LibraryAuthResponseDto>.Unauthorized("This account is not allowed to use LibraryAPI login.");
        }

        return null;
    }

    private async Task<LibraryAuthResponseDto> BuildLibraryAuthResponseAsync(
        LibraryAccount account,
        CancellationToken cancellationToken,
        string? token = null,
        string? refreshToken = null)
    {
        var posDevices = await GetLibraryPosDevicesAsync(account.LibraryId, cancellationToken);

        return new LibraryAuthResponseDto
        {
            AccessToken = token ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty,
            ExpiresAt = token is null ? DateTime.MinValue : DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            UserId = account.Id,
            Username = account.Username,
            RoleCode = RoleCatalog.NormalizeRoleCode(account.Role.Code, GuardName.Office),
            Account = BuildAccountProfile(account),
            Library = BuildLibraryProfile(account),
            PosDevices = posDevices
        };
    }

    private async Task<LibraryMeResponseDto> BuildLibraryMeResponseAsync(LibraryAccount account, CancellationToken cancellationToken)
    {
        var posDevices = await GetLibraryPosDevicesAsync(account.LibraryId, cancellationToken);

        return new LibraryMeResponseDto
        {
            Account = BuildAccountProfile(account),
            Library = BuildLibraryProfile(account),
            PosDevices = posDevices
        };
    }

    private static LibraryAccountProfileDto BuildAccountProfile(LibraryAccount account) => new()
    {
        UserId = account.Id,
        Username = account.Username,
        FullName = account.FullName,
        RoleCode = RoleCatalog.NormalizeRoleCode(account.Role.Code, GuardName.Office),
        PhoneNumber = account.PhoneNumber,
        Status = account.Status.ToString(),
        LastLoginAt = account.LastLoginAt
    };

    private static LibraryProfileDto BuildLibraryProfile(LibraryAccount account) => new()
    {
        LibraryId = account.LibraryId,
        LibraryCode = account.Library.LibraryCode,
        LibraryName = account.Library.LibraryName,
        OwnerName = account.Library.OwnerName,
        OwnerPhone = account.Library.OwnerPhone,
        City = account.Library.City,
        Status = account.Library.Status.ToString()
    };

    private Task<List<LibraryPosDeviceDto>> GetLibraryPosDevicesAsync(int libraryId, CancellationToken cancellationToken)
    {
        return _context.PosDevices
            .AsNoTracking()
            .Where(x => x.LibraryId == libraryId)
            .OrderBy(x => x.Id)
            .Select(x => new LibraryPosDeviceDto
            {
                Id = x.Id,
                PosCode = x.PosCode,
                SerialNumber = x.SerialNumber,
                DeviceModel = x.DeviceModel,
                DeviceVendor = x.DeviceVendor,
                Status = x.Status.ToString(),
                IsActivated = x.IsActivated,
                ActivatedAt = x.ActivatedAt,
                LastAuthenticatedAt = x.LastAuthenticatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
