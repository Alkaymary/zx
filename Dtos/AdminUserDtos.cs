using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class AdminUserResponseDto
{
    /// <summary>The unique admin user identifier.</summary>
    public int Id { get; set; }

    /// <summary>The full display name of the admin user.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>The unique username used for login.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The admin email address if available.</summary>
    public string? Email { get; set; }

    /// <summary>The admin phone number if available.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>The assigned role identifier.</summary>
    public int RoleId { get; set; }

    /// <summary>The assigned role name.</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>The assigned role code.</summary>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>The account status.</summary>
    public RecordStatus Status { get; set; }

    /// <summary>The last successful login time if available.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>The creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>The last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

public class CreateAdminUserDto
{
    /// <summary>The admin user's full name.</summary>
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>The unique username for login.</summary>
    [Required, MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    /// <summary>The unique email for the admin user.</summary>
    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>The admin phone number.</summary>
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    /// <summary>The stored password hash or password value used by the current auth flow.</summary>
    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>The role identifier. It must reference a role with guard name Admin.</summary>
    [Required]
    public int RoleId { get; set; }

    /// <summary>The account status.</summary>
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    /// <summary>The last login time if you want to seed it.</summary>
    public DateTime? LastLoginAt { get; set; }
}

public class UpdateAdminUserDto : CreateAdminUserDto
{
}

public class AdminUserQueryDto
{
    /// <summary>Filter by exact admin user id.</summary>
    public int? Id { get; set; }

    /// <summary>Filter by phone number using partial match.</summary>
    public string? Phone { get; set; }

    /// <summary>Filter by username using partial match.</summary>
    public string? Username { get; set; }

    /// <summary>Filter by email using partial match.</summary>
    public string? Email { get; set; }

    /// <summary>Filter by exact account status.</summary>
    public RecordStatus? Status { get; set; }

    /// <summary>Filter by exact role id.</summary>
    public int? RoleId { get; set; }

    /// <summary>Filter by role code using partial match.</summary>
    public string? RoleCode { get; set; }

    /// <summary>Filter by created date from this UTC datetime.</summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>Filter by created date until this UTC datetime.</summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>Filter by updated date from this UTC datetime.</summary>
    public DateTime? UpdatedFrom { get; set; }

    /// <summary>Filter by updated date until this UTC datetime.</summary>
    public DateTime? UpdatedTo { get; set; }

    /// <summary>Maximum number of records to return. Default is 50 and maximum is 200.</summary>
    public int Limit { get; set; } = 50;
}
