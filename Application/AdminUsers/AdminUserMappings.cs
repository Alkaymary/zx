using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.AdminUsers;

internal static class AdminUserMappings
{
    public static Expression<Func<AdminUser, AdminUserResponseDto>> ToProjection()
    {
        return x => new AdminUserResponseDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Username = x.Username,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            RoleId = x.RoleId,
            RoleName = x.Role.Name,
            RoleCode = x.Role.Code,
            Status = x.Status,
            LastLoginAt = x.LastLoginAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static void ApplyUpdate(AdminUser admin, UpdateAdminUserDto request)
    {
        admin.FullName = request.FullName;
        admin.Username = request.Username;
        admin.Email = request.Email;
        admin.PhoneNumber = request.PhoneNumber;
        admin.PasswordHash = request.PasswordHash;
        admin.RoleId = request.RoleId;
        admin.Status = request.Status;
        admin.LastLoginAt = request.LastLoginAt;
        admin.UpdatedAt = DateTime.UtcNow;
    }

    public static Task LoadRoleAsync(AppDbContext context, AdminUser admin, CancellationToken cancellationToken)
    {
        return context.Entry(admin).Reference(x => x.Role).LoadAsync(cancellationToken);
    }

    public static AdminUserResponseDto ToDto(AdminUser admin) => new()
    {
        Id = admin.Id,
        FullName = admin.FullName,
        Username = admin.Username,
        Email = admin.Email,
        PhoneNumber = admin.PhoneNumber,
        RoleId = admin.RoleId,
        RoleName = admin.Role.Name,
        RoleCode = admin.Role.Code,
        Status = admin.Status,
        LastLoginAt = admin.LastLoginAt,
        CreatedAt = admin.CreatedAt,
        UpdatedAt = admin.UpdatedAt
    };
}
