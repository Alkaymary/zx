using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure;
using MyApi.Models;

namespace MyApi.Application.AdminUsers;

internal static class AdminUserQueryBuilder
{
    public static IQueryable<AdminUser> Build(AppDbContext context, AdminUserQueryDto query, bool trackChanges = false)
    {
        IQueryable<AdminUser> adminQuery = context.AdminUsers;
        if (!trackChanges)
        {
            adminQuery = adminQuery.AsNoTracking();
        }

        if (query.Id.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.Id == query.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Phone))
        {
            var phone = query.Phone.Trim();
            adminQuery = adminQuery.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(phone));
        }

        if (!string.IsNullOrWhiteSpace(query.Username))
        {
            var username = SqlSearchPattern.Contains(query.Username);
            adminQuery = adminQuery.Where(x => EF.Functions.ILike(x.Username, username, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = SqlSearchPattern.Contains(query.Email);
            adminQuery = adminQuery.Where(x => x.Email != null && EF.Functions.ILike(x.Email, email, "\\"));
        }

        if (query.Status.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.RoleId.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.RoleId == query.RoleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.RoleCode))
        {
            var roleCode = SqlSearchPattern.Contains(query.RoleCode);
            adminQuery = adminQuery.Where(x => EF.Functions.ILike(x.Role.Code, roleCode, "\\"));
        }

        if (query.CreatedFrom.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        if (query.UpdatedFrom.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.UpdatedAt >= query.UpdatedFrom.Value);
        }

        if (query.UpdatedTo.HasValue)
        {
            adminQuery = adminQuery.Where(x => x.UpdatedAt <= query.UpdatedTo.Value);
        }

        return adminQuery;
    }
}

public sealed class ListAdminUsersQuery
{
    private readonly AppDbContext _context;

    public ListAdminUsersQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminUserResponseDto>> ExecuteAsync(
        AdminUserQueryDto query,
        CancellationToken cancellationToken)
    {
        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200);

        return await AdminUserQueryBuilder.Build(_context, query)
            .OrderBy(x => x.Id)
            .Take(limit)
            .Select(AdminUserMappings.ToProjection())
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetAdminUserByIdQuery
{
    private readonly AppDbContext _context;

    public GetAdminUserByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<AdminUserResponseDto>> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var admin = await _context.AdminUsers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(AdminUserMappings.ToProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return admin is null
            ? AppResult<AdminUserResponseDto>.NotFound("Admin user was not found.")
            : AppResult<AdminUserResponseDto>.Success(admin);
    }
}
