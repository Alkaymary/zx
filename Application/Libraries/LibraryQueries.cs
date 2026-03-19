using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure;
using MyApi.Models;

namespace MyApi.Application.Libraries;

internal static class LibraryQueryBuilder
{
    public static IQueryable<Library> Build(AppDbContext context, LibraryQueryDto query, bool trackChanges = false)
    {
        IQueryable<Library> libraryQuery = context.Libraries;
        if (!trackChanges)
        {
            libraryQuery = libraryQuery.AsNoTracking();
        }

        if (query.Id.HasValue)
        {
            libraryQuery = libraryQuery.Where(x => x.Id == query.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.LibraryCode))
        {
            var libraryCode = SqlSearchPattern.Contains(query.LibraryCode);
            libraryQuery = libraryQuery.Where(x => EF.Functions.ILike(x.LibraryCode, libraryCode, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.LibraryName))
        {
            var libraryName = SqlSearchPattern.Contains(query.LibraryName);
            libraryQuery = libraryQuery.Where(x => EF.Functions.ILike(x.LibraryName, libraryName, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.OwnerName))
        {
            var ownerName = SqlSearchPattern.Contains(query.OwnerName);
            libraryQuery = libraryQuery.Where(x => x.OwnerName != null && EF.Functions.ILike(x.OwnerName, ownerName, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.Phone))
        {
            var phone = query.Phone.Trim();
            libraryQuery = libraryQuery.Where(x =>
                (x.OwnerPhone != null && x.OwnerPhone.Contains(phone))
                || (x.OwnerPhone2 != null && x.OwnerPhone2.Contains(phone)));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = SqlSearchPattern.Contains(query.City);
            libraryQuery = libraryQuery.Where(x => x.City != null && EF.Functions.ILike(x.City, city, "\\"));
        }

        if (query.Status.HasValue)
        {
            libraryQuery = libraryQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.AccountsCount.HasValue)
        {
            libraryQuery = libraryQuery.Where(x => x.Accounts.Count == query.AccountsCount.Value);
        }

        return libraryQuery;
    }

    public static IQueryable<Library> BuildSearch(AppDbContext context, string? q)
    {
        var trimmedQuery = q?.Trim();
        var libraryQuery = context.Libraries.AsNoTracking().AsQueryable();

        if (string.IsNullOrWhiteSpace(trimmedQuery))
        {
            return libraryQuery;
        }

        var pattern = SqlSearchPattern.Contains(trimmedQuery);
        return libraryQuery.Where(x =>
            EF.Functions.ILike(x.LibraryCode, pattern, "\\") ||
            EF.Functions.ILike(x.LibraryName, pattern, "\\") ||
            (x.OwnerName != null && EF.Functions.ILike(x.OwnerName, pattern, "\\")) ||
            (x.OwnerPhone != null && x.OwnerPhone.Contains(trimmedQuery)) ||
            (x.OwnerPhone2 != null && x.OwnerPhone2.Contains(trimmedQuery)) ||
            (x.City != null && EF.Functions.ILike(x.City, pattern, "\\")) ||
            x.PosDevices.Any(pos =>
                EF.Functions.ILike(pos.PosCode, pattern, "\\") ||
                (pos.SerialNumber != null && EF.Functions.ILike(pos.SerialNumber, pattern, "\\"))));
    }
}

public sealed class ListLibrariesQuery
{
    private readonly AppDbContext _context;

    public ListLibrariesQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LibraryResponseDto>> ExecuteAsync(
        LibraryQueryDto query,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200);

        var libraries = await LibraryQueryBuilder.Build(_context, query)
            .Select(LibraryMappings.ToProjection())
            .OrderBy(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        LibraryMappings.ApplyFinancialVisibility(libraries, actor);
        return libraries;
    }
}

public sealed class SearchLibrariesQuery
{
    private readonly AppDbContext _context;

    public SearchLibrariesQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LibraryResponseDto>> ExecuteAsync(
        string? q,
        int limit,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        var normalizedLimit = limit <= 0 ? 50 : Math.Min(limit, 200);

        var libraries = await LibraryQueryBuilder.BuildSearch(_context, q)
            .Select(LibraryMappings.ToProjection())
            .OrderBy(x => x.Id)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        LibraryMappings.ApplyFinancialVisibility(libraries, actor);
        return libraries;
    }
}

public sealed class GetLibraryStatsQuery
{
    private readonly AppDbContext _context;

    public GetLibraryStatsQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LibraryStatsDto> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _context.Libraries
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new LibraryStatsDto
            {
                Total = group.Count(),
                Active = group.Count(x => x.Status == RecordStatus.Active),
                Inactive = group.Count(x => x.Status == RecordStatus.Inactive),
                Suspended = group.Count(x => x.Status == RecordStatus.Suspended)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new LibraryStatsDto();
    }
}

public sealed class GetLibraryByIdQuery
{
    private readonly AppDbContext _context;

    public GetLibraryByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryResponseDto>> ExecuteAsync(
        int id,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        var library = await _context.Libraries
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(LibraryMappings.ToProjection())
            .FirstOrDefaultAsync(cancellationToken);

        if (library is null)
        {
            return AppResult<LibraryResponseDto>.NotFound("Library was not found.");
        }

        LibraryMappings.ApplyFinancialVisibility(library, actor);
        return AppResult<LibraryResponseDto>.Success(library);
    }
}
