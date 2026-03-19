using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.AdminUsers;

public sealed class ValidateAdminUserDependencies
{
    private readonly AppDbContext _context;

    public ValidateAdminUserDependencies(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<AdminUserResponseDto>?> ExecuteAsync(
        int roleId,
        string username,
        string? email,
        int? currentId,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        if (role is null)
        {
            return AppResult<AdminUserResponseDto>.BadRequest("RoleId does not exist.");
        }

        if (role.GuardName != GuardName.Admin)
        {
            return AppResult<AdminUserResponseDto>.BadRequest("RoleId must belong to an admin role.");
        }

        var usernameExists = await _context.AdminUsers
            .AnyAsync(x => x.Username == username && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);
        if (usernameExists)
        {
            return AppResult<AdminUserResponseDto>.Conflict("Username already exists.");
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailExists = await _context.AdminUsers
                .AnyAsync(x => x.Email == email && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);
            if (emailExists)
            {
                return AppResult<AdminUserResponseDto>.Conflict("Email already exists.");
            }
        }

        return null;
    }
}

public sealed class CreateAdminUserUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidateAdminUserDependencies _validator;

    public CreateAdminUserUseCase(AppDbContext context, ValidateAdminUserDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<AdminUserResponseDto>> ExecuteAsync(
        CreateAdminUserDto request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ExecuteAsync(
            request.RoleId,
            request.Username,
            request.Email,
            null,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        var admin = new AdminUser
        {
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = request.PasswordHash,
            RoleId = request.RoleId,
            Status = request.Status,
            LastLoginAt = request.LastLoginAt,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AdminUsers.Add(admin);
        await _context.SaveChangesAsync(cancellationToken);
        await AdminUserMappings.LoadRoleAsync(_context, admin, cancellationToken);

        return AppResult<AdminUserResponseDto>.Success(AdminUserMappings.ToDto(admin));
    }
}

public sealed class UpdateAdminUserUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidateAdminUserDependencies _validator;

    public UpdateAdminUserUseCase(AppDbContext context, ValidateAdminUserDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<AdminUserResponseDto>> ExecuteAsync(
        int id,
        UpdateAdminUserDto request,
        CancellationToken cancellationToken)
    {
        var admin = await _context.AdminUsers
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (admin is null)
        {
            return AppResult<AdminUserResponseDto>.NotFound("Admin user was not found.");
        }

        var validation = await _validator.ExecuteAsync(
            request.RoleId,
            request.Username,
            request.Email,
            id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        AdminUserMappings.ApplyUpdate(admin, request);
        await _context.SaveChangesAsync(cancellationToken);
        await AdminUserMappings.LoadRoleAsync(_context, admin, cancellationToken);
        return AppResult<AdminUserResponseDto>.Success(AdminUserMappings.ToDto(admin));
    }
}

public sealed class UpdateAdminUserByQueryUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidateAdminUserDependencies _validator;

    public UpdateAdminUserByQueryUseCase(AppDbContext context, ValidateAdminUserDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<AdminUserResponseDto>> ExecuteAsync(
        AdminUserQueryDto query,
        UpdateAdminUserDto request,
        CancellationToken cancellationToken)
    {
        var matches = await AdminUserQueryBuilder.Build(_context, query, trackChanges: true)
            .Include(x => x.Role)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult<AdminUserResponseDto>.NotFound("No admin user matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult<AdminUserResponseDto>.Conflict("Query matched more than one admin user. Narrow the filters before updating.");
        }

        var admin = matches[0];
        var validation = await _validator.ExecuteAsync(
            request.RoleId,
            request.Username,
            request.Email,
            admin.Id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        AdminUserMappings.ApplyUpdate(admin, request);
        await _context.SaveChangesAsync(cancellationToken);
        await AdminUserMappings.LoadRoleAsync(_context, admin, cancellationToken);
        return AppResult<AdminUserResponseDto>.Success(AdminUserMappings.ToDto(admin));
    }
}

public sealed class DeleteAdminUserUseCase
{
    private readonly AppDbContext _context;

    public DeleteAdminUserUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var admin = await _context.AdminUsers.FindAsync([id], cancellationToken);
        if (admin is null)
        {
            return AppResult.NotFound("Admin user was not found.");
        }

        _context.AdminUsers.Remove(admin);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}

public sealed class DeleteAdminUserByQueryUseCase
{
    private readonly AppDbContext _context;

    public DeleteAdminUserByQueryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(AdminUserQueryDto query, CancellationToken cancellationToken)
    {
        var matches = await AdminUserQueryBuilder.Build(_context, query, trackChanges: true)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult.NotFound("No admin user matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult.Conflict("Query matched more than one admin user. Narrow the filters before deleting.");
        }

        _context.AdminUsers.Remove(matches[0]);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
