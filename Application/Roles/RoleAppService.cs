using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure.Security;
using MyApi.Models;

namespace MyApi.Application.Roles;

public interface IRoleAppService
{
    Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AppResult<RoleResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<RoleResponseDto>> CreateAsync(CreateRoleDto request, CancellationToken cancellationToken);
    Task<AppResult<RoleResponseDto>> UpdateAsync(int id, UpdateRoleDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public class RoleAppService : IRoleAppService
{
    private readonly AppDbContext _context;

    public RoleAppService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RoleResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(ToProjection())
            .ToListAsync(cancellationToken);
    }

    public async Task<AppResult<RoleResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return role is null
            ? AppResult<RoleResponseDto>.NotFound("Role was not found.")
            : AppResult<RoleResponseDto>.Success(role);
    }

    public async Task<AppResult<RoleResponseDto>> CreateAsync(CreateRoleDto request, CancellationToken cancellationToken)
    {
        if (!TryNormalizeRoleRequest(request, out var normalizedCode, out var validationMessage))
        {
            return AppResult<RoleResponseDto>.BadRequest(validationMessage);
        }

        if (await _context.Roles.AnyAsync(x => x.Code == normalizedCode, cancellationToken))
        {
            return AppResult<RoleResponseDto>.Conflict("Role code already exists.");
        }

        var role = new Role
        {
            Name = request.Name,
            Code = normalizedCode,
            GuardName = request.GuardName
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult<RoleResponseDto>.Success(ToDto(role));
    }

    public async Task<AppResult<RoleResponseDto>> UpdateAsync(int id, UpdateRoleDto request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(x => x.LibraryAccounts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return AppResult<RoleResponseDto>.NotFound("Role was not found.");
        }

        if (!TryNormalizeRoleRequest(request, out var normalizedCode, out var validationMessage))
        {
            return AppResult<RoleResponseDto>.BadRequest(validationMessage);
        }

        if (await _context.Roles.AnyAsync(x => x.Id != id && x.Code == normalizedCode, cancellationToken))
        {
            return AppResult<RoleResponseDto>.Conflict("Role code already exists.");
        }

        role.Name = request.Name;
        role.Code = normalizedCode;
        role.GuardName = request.GuardName;

        await _context.SaveChangesAsync(cancellationToken);
        return AppResult<RoleResponseDto>.Success(ToDto(role));
    }

    public async Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(x => x.LibraryAccounts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return AppResult.NotFound("Role was not found.");
        }

        if (role.LibraryAccounts.Count > 0)
        {
            return AppResult.Conflict("Cannot delete a role that is assigned to library accounts.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }

    private static bool TryNormalizeRoleRequest(CreateRoleDto request, out string normalizedCode, out string validationMessage)
    {
        if (RoleCatalog.TryNormalizeRoleCode(request.Code, request.GuardName, out normalizedCode))
        {
            validationMessage = string.Empty;
            return true;
        }

        var allowedCodes = string.Join(", ", RoleCatalog.GetSupportedRoleCodes(request.GuardName));
        validationMessage = request.GuardName == GuardName.Admin
            ? $"Supported admin role codes: {allowedCodes}."
            : $"Supported office role codes: {allowedCodes}.";

        return false;
    }

    private static System.Linq.Expressions.Expression<Func<Role, RoleResponseDto>> ToProjection()
    {
        return x => new RoleResponseDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            GuardName = x.GuardName,
            CreatedAt = x.CreatedAt,
            LibraryAccountsCount = x.LibraryAccounts.Count
        };
    }

    private static RoleResponseDto ToDto(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Code = role.Code,
        GuardName = role.GuardName,
        CreatedAt = role.CreatedAt,
        LibraryAccountsCount = role.LibraryAccounts.Count
    };
}
