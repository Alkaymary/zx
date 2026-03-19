using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class PosDeviceResponseDto
{
    public int Id { get; set; }
    public int? LibraryId { get; set; }
    public string? LibraryName { get; set; }
    public string PosCode { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceVendor { get; set; }
    public PosDeviceStatus Status { get; set; }
    public bool IsActivated { get; set; }
    public int? ActivatedByAccountId { get; set; }
    public string? ActivatedByUsername { get; set; }
    public string? ActivationToken { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? LastAuthenticatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePosDeviceDto
{
    public int? LibraryId { get; set; }

    [Required, MaxLength(50)]
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
    public string? ActivationToken { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? LastAuthenticatedAt { get; set; }
}

public class UpdatePosDeviceDto : CreatePosDeviceDto
{
}

public class PosDeviceQueryDto
{
    public int? Id { get; set; }
    public int? LibraryId { get; set; }
    public string? LibraryName { get; set; }
    public string? PosCode { get; set; }
    public string? SerialNumber { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceVendor { get; set; }
    public PosDeviceStatus? Status { get; set; }
    public bool? IsActivated { get; set; }
    public int? ActivatedByAccountId { get; set; }
    public string? ActivatedByUsername { get; set; }
    public int Limit { get; set; } = 50;
}
