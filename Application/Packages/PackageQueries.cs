using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;

namespace MyApi.Application.Packages;

public sealed class ListPackagesQuery
{
    private readonly AppDbContext _context;

    public ListPackagesQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PackageResponseDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _context.Packages
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(PackageMappings.ToProjection())
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetPackageByIdQuery
{
    private readonly AppDbContext _context;

    public GetPackageByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<PackageResponseDto>> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var package = await _context.Packages
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(PackageMappings.ToProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return package is null
            ? AppResult<PackageResponseDto>.NotFound("Package was not found.")
            : AppResult<PackageResponseDto>.Success(package);
    }
}
