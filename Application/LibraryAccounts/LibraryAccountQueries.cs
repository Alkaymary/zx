using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure;
using MyApi.Models;

namespace MyApi.Application.LibraryAccounts;

internal static class LibraryAccountQueryBuilder
{
    public static IQueryable<LibraryAccount> Build(AppDbContext context, LibraryAccountQueryDto query, bool trackChanges = false)
    {
        IQueryable<LibraryAccount> accountQuery = context.LibraryAccounts;
        if (!trackChanges)
        {
            accountQuery = accountQuery.AsNoTracking();
        }

        if (query.Id.HasValue)
        {
            accountQuery = accountQuery.Where(x => x.Id == query.Id.Value);
        }

        if (query.LibraryId.HasValue)
        {
            accountQuery = accountQuery.Where(x => x.LibraryId == query.LibraryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.LibraryName))
        {
            var libraryName = SqlSearchPattern.Contains(query.LibraryName);
            accountQuery = accountQuery.Where(x => EF.Functions.ILike(x.Library.LibraryName, libraryName, "\\"));
        }

        if (query.RoleId.HasValue)
        {
            accountQuery = accountQuery.Where(x => x.RoleId == query.RoleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.RoleName))
        {
            var roleName = SqlSearchPattern.Contains(query.RoleName);
            accountQuery = accountQuery.Where(x => EF.Functions.ILike(x.Role.Name, roleName, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.FullName))
        {
            var fullName = SqlSearchPattern.Contains(query.FullName);
            accountQuery = accountQuery.Where(x => EF.Functions.ILike(x.FullName, fullName, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.Username))
        {
            var username = SqlSearchPattern.Contains(query.Username);
            accountQuery = accountQuery.Where(x => EF.Functions.ILike(x.Username, username, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.Phone))
        {
            var phone = query.Phone.Trim();
            accountQuery = accountQuery.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(phone));
        }

        if (query.Status.HasValue)
        {
            accountQuery = accountQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.ActivatedDevicesCount.HasValue)
        {
            accountQuery = accountQuery.Where(x => x.ActivatedDevices.Count == query.ActivatedDevicesCount.Value);
        }

        return accountQuery;
    }
}

public sealed class ListLibraryAccountsQuery
{
    private readonly AppDbContext _context;

    public ListLibraryAccountsQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LibraryAccountResponseDto>> ExecuteAsync(
        LibraryAccountQueryDto query,
        CancellationToken cancellationToken)
    {
        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200);

        return await LibraryAccountQueryBuilder.Build(_context, query)
            .OrderBy(x => x.Id)
            .Take(limit)
            .Select(LibraryAccountMappings.ToProjection())
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetLibraryAccountByIdQuery
{
    private readonly AppDbContext _context;

    public GetLibraryAccountByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryAccountResponseDto>> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var account = await _context.LibraryAccounts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(LibraryAccountMappings.ToProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return account is null
            ? AppResult<LibraryAccountResponseDto>.NotFound("Library account was not found.")
            : AppResult<LibraryAccountResponseDto>.Success(account);
    }
}

public sealed class GetAvailableLibraryAccountRolesQuery
{
    private readonly AppDbContext _context;

    public GetAvailableLibraryAccountRolesQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoleResponseDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(x => x.GuardName == GuardName.Office)
            .OrderBy(x => x.Id)
            .Select(x => new RoleResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                GuardName = x.GuardName,
                CreatedAt = x.CreatedAt,
                LibraryAccountsCount = x.LibraryAccounts.Count
            })
            .ToListAsync(cancellationToken);
    }
}
