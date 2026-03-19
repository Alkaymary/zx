using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.Packages;

public sealed class ValidatePackageDependencies
{
    private readonly AppDbContext _context;

    public ValidatePackageDependencies(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<PackageResponseDto>?> ExecuteAsync(
        string packageCode,
        string name,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (await _context.Packages.AnyAsync(
                x => x.PackageCode == packageCode && (!currentId.HasValue || x.Id != currentId.Value),
                cancellationToken))
        {
            return AppResult<PackageResponseDto>.Conflict("Package code already exists.");
        }

        if (await _context.Packages.AnyAsync(
                x => x.Name == name && (!currentId.HasValue || x.Id != currentId.Value),
                cancellationToken))
        {
            return AppResult<PackageResponseDto>.Conflict("Package name already exists.");
        }

        return null;
    }
}

public sealed class CreatePackageUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidatePackageDependencies _validator;

    public CreatePackageUseCase(AppDbContext context, ValidatePackageDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<PackageResponseDto>> ExecuteAsync(
        CreatePackageDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ExecuteAsync(
            request.PackageCode,
            request.Name,
            null,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        if (!actor.IsAdminUser || !actor.UserId.HasValue)
        {
            return AppResult<PackageResponseDto>.Unauthorized("Invalid admin token.");
        }

        var admin = await _context.AdminUsers
            .FirstOrDefaultAsync(x => x.Id == actor.UserId.Value, cancellationToken);
        if (admin is null)
        {
            return AppResult<PackageResponseDto>.BadRequest("Admin user was not found.");
        }

        var package = new Package
        {
            PackageCode = request.PackageCode,
            Name = request.Name,
            PriceIqd = request.PriceIqd,
            Status = request.Status,
            AddedByAdminUserId = actor.UserId.Value,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Packages.Add(package);
        await _context.SaveChangesAsync(cancellationToken);
        await PackageMappings.LoadAddedByAsync(_context, package, cancellationToken);
        return AppResult<PackageResponseDto>.Success(PackageMappings.ToDto(package));
    }
}

public sealed class UpdatePackageUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidatePackageDependencies _validator;

    public UpdatePackageUseCase(AppDbContext context, ValidatePackageDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<PackageResponseDto>> ExecuteAsync(
        int id,
        UpdatePackageDto request,
        CancellationToken cancellationToken)
    {
        var package = await _context.Packages
            .Include(x => x.AddedByAdminUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (package is null)
        {
            return AppResult<PackageResponseDto>.NotFound("Package was not found.");
        }

        var validation = await _validator.ExecuteAsync(
            request.PackageCode,
            request.Name,
            id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        PackageMappings.ApplyUpdate(package, request);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult<PackageResponseDto>.Success(PackageMappings.ToDto(package));
    }
}

public sealed class DeletePackageUseCase
{
    private readonly AppDbContext _context;

    public DeletePackageUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var package = await _context.Packages.FindAsync([id], cancellationToken);
        if (package is null)
        {
            return AppResult.NotFound("Package was not found.");
        }

        _context.Packages.Remove(package);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
