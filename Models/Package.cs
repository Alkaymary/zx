using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class Package
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string PackageCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal PriceIqd { get; set; }

    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public int AddedByAdminUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AddedByAdminUserId))]
    public AdminUser AddedByAdminUser { get; set; } = null!;
}
