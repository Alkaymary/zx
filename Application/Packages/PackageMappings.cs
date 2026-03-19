using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.Packages;

internal static class PackageMappings
{
    public static Expression<Func<Package, PackageResponseDto>> ToProjection()
    {
        return x => new PackageResponseDto
        {
            Id = x.Id,
            PackageCode = x.PackageCode,
            Name = x.Name,
            PriceIqd = x.PriceIqd,
            Status = x.Status,
            AddedByAdminUserId = x.AddedByAdminUserId,
            AddedByUsername = x.AddedByAdminUser.Username,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static void ApplyUpdate(Package package, UpdatePackageDto request)
    {
        package.PackageCode = request.PackageCode;
        package.Name = request.Name;
        package.PriceIqd = request.PriceIqd;
        package.Status = request.Status;
        package.UpdatedAt = DateTime.UtcNow;
    }

    public static Task LoadAddedByAsync(AppDbContext context, Package package, CancellationToken cancellationToken)
    {
        return context.Entry(package).Reference(x => x.AddedByAdminUser).LoadAsync(cancellationToken);
    }

    public static PackageResponseDto ToDto(Package package) => new()
    {
        Id = package.Id,
        PackageCode = package.PackageCode,
        Name = package.Name,
        PriceIqd = package.PriceIqd,
        Status = package.Status,
        AddedByAdminUserId = package.AddedByAdminUserId,
        AddedByUsername = package.AddedByAdminUser.Username,
        CreatedAt = package.CreatedAt,
        UpdatedAt = package.UpdatedAt
    };
}
