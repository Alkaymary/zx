using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class PosDevice
{
    public int Id { get; set; }

    public int? LibraryId { get; set; }

    [MaxLength(50)]
    public string PosCode { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? SerialNumber { get; set; }

    [MaxLength(100)]
    public string? DeviceModel { get; set; }

    [MaxLength(100)]
    public string? DeviceVendor { get; set; }

    public PosDeviceStatus Status { get; set; } = PosDeviceStatus.Inactive;

    public bool IsActivated { get; set; }

    public int? ActivatedByAccountId { get; set; }

    [MaxLength(200)]
    public string? ActivationToken { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime? LastAuthenticatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(LibraryId))]
    public Library? Library { get; set; }

    [ForeignKey(nameof(ActivatedByAccountId))]
    public LibraryAccount? ActivatedByAccount { get; set; }
}
