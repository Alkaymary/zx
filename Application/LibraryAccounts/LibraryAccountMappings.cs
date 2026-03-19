using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.LibraryAccounts;

internal static class LibraryAccountMappings
{
    public static Expression<Func<LibraryAccount, LibraryAccountResponseDto>> ToProjection()
    {
        return x => new LibraryAccountResponseDto
        {
            Id = x.Id,
            LibraryId = x.LibraryId,
            LibraryName = x.Library.LibraryName,
            RoleId = x.RoleId,
            RoleName = x.Role.Name,
            FullName = x.FullName,
            Username = x.Username,
            PhoneNumber = x.PhoneNumber,
            Status = x.Status,
            LastLoginAt = x.LastLoginAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ActivatedDevicesCount = x.ActivatedDevices.Count
        };
    }

    public static void ApplyUpdate(LibraryAccount account, UpdateLibraryAccountDto request)
    {
        account.LibraryId = request.LibraryId;
        account.RoleId = request.RoleId;
        account.FullName = request.FullName;
        account.Username = request.Username;
        account.PhoneNumber = request.PhoneNumber;
        if (!string.IsNullOrWhiteSpace(request.PasswordHash))
        {
            account.PasswordHash = request.PasswordHash;
        }

        account.Status = request.Status;
        account.LastLoginAt = request.LastLoginAt;
        account.UpdatedAt = DateTime.UtcNow;
    }

    public static async Task LoadReferencesAsync(AppDbContext context, LibraryAccount account, CancellationToken cancellationToken)
    {
        await context.Entry(account).Reference(x => x.Library).LoadAsync(cancellationToken);
        await context.Entry(account).Reference(x => x.Role).LoadAsync(cancellationToken);
        await context.Entry(account).Collection(x => x.ActivatedDevices).LoadAsync(cancellationToken);
    }

    public static LibraryAccountResponseDto ToDto(LibraryAccount account) => new()
    {
        Id = account.Id,
        LibraryId = account.LibraryId,
        LibraryName = account.Library.LibraryName,
        RoleId = account.RoleId,
        RoleName = account.Role.Name,
        FullName = account.FullName,
        Username = account.Username,
        PhoneNumber = account.PhoneNumber,
        Status = account.Status,
        LastLoginAt = account.LastLoginAt,
        CreatedAt = account.CreatedAt,
        UpdatedAt = account.UpdatedAt,
        ActivatedDevicesCount = account.ActivatedDevices.Count
    };
}
