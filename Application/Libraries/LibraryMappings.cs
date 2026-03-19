using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.Libraries;

internal static class LibraryMappings
{
    public static Expression<Func<Library, LibraryResponseDto>> ToProjection()
    {
        return x => new LibraryResponseDto
        {
            Id = x.Id,
            LibraryCode = x.LibraryCode,
            LibraryName = x.LibraryName,
            OwnerName = x.OwnerName,
            OwnerPhone = x.OwnerPhone,
            OwnerPhone2 = x.OwnerPhone2,
            Address = x.Address,
            Province = x.Province,
            City = x.City,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            Status = x.Status,
            CreditLimit = x.CreditLimit,
            CurrentBalance = x.CurrentBalance,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            AccountsCount = x.Accounts.Count,
            PosDevicesCount = x.PosDevices.Count
        };
    }

    public static void ApplyUpdate(Library library, UpdateLibraryDto request)
    {
        library.LibraryCode = request.LibraryCode;
        library.LibraryName = request.LibraryName;
        library.OwnerName = request.OwnerName;
        library.OwnerPhone = request.OwnerPhone;
        library.OwnerPhone2 = request.OwnerPhone2;
        library.Address = request.Address;
        library.Province = request.Province;
        library.City = request.City;
        library.Latitude = request.Latitude;
        library.Longitude = request.Longitude;
        library.Status = request.Status;
        library.CreditLimit = request.CreditLimit;
        library.CurrentBalance = request.CurrentBalance;
        library.Notes = request.Notes;
        library.UpdatedAt = DateTime.UtcNow;
    }

    public static void ApplyFinancialVisibility(List<LibraryResponseDto> libraries, AdminActorContext actor)
    {
        if (actor.CanViewLibraryFinancialData)
        {
            return;
        }

        foreach (var library in libraries)
        {
            ApplyFinancialVisibility(library, actor);
        }
    }

    public static void ApplyFinancialVisibility(LibraryResponseDto library, AdminActorContext actor)
    {
        if (actor.CanViewLibraryFinancialData)
        {
            return;
        }

        library.CreditLimit = 0m;
        library.CurrentBalance = 0m;
    }

    public static async Task LoadReferencesAsync(AppDbContext context, Library library, CancellationToken cancellationToken)
    {
        await context.Entry(library).Collection(x => x.Accounts).LoadAsync(cancellationToken);
        await context.Entry(library).Collection(x => x.PosDevices).LoadAsync(cancellationToken);
    }

    public static LibraryResponseDto ToDto(Library library) => new()
    {
        Id = library.Id,
        LibraryCode = library.LibraryCode,
        LibraryName = library.LibraryName,
        OwnerName = library.OwnerName,
        OwnerPhone = library.OwnerPhone,
        OwnerPhone2 = library.OwnerPhone2,
        Address = library.Address,
        Province = library.Province,
        City = library.City,
        Latitude = library.Latitude,
        Longitude = library.Longitude,
        Status = library.Status,
        CreditLimit = library.CreditLimit,
        CurrentBalance = library.CurrentBalance,
        Notes = library.Notes,
        CreatedAt = library.CreatedAt,
        UpdatedAt = library.UpdatedAt,
        AccountsCount = library.Accounts.Count,
        PosDevicesCount = library.PosDevices.Count
    };
}
