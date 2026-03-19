using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class AdminUser
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;
}
