using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class RefreshToken
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;

    public int? AdminUserId { get; set; }

    public int? LibraryAccountId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    [MaxLength(100)]
    public string UserType { get; set; } = string.Empty;

    [ForeignKey(nameof(AdminUserId))]
    public AdminUser? AdminUser { get; set; }

    [ForeignKey(nameof(LibraryAccountId))]
    public LibraryAccount? LibraryAccount { get; set; }
}
