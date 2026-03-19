using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class LibraryAccountResponseDto
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public string LibraryName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public RecordStatus Status { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ActivatedDevicesCount { get; set; }
}

public class CreateLibraryAccountDto
{
    [Required]
    public int LibraryId { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime? LastLoginAt { get; set; }
}

public class UpdateLibraryAccountDto
{
    [Required]
    public int LibraryId { get; set; }

    [Required]
    public int RoleId { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? PasswordHash { get; set; }

    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTime? LastLoginAt { get; set; }
}

public class LibraryAccountQueryDto
{
    public int? Id { get; set; }
    public int? LibraryId { get; set; }
    public string? LibraryName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public string? Phone { get; set; }
    public RecordStatus? Status { get; set; }
    public int? ActivatedDevicesCount { get; set; }
    public int Limit { get; set; } = 50;
}
