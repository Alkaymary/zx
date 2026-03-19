using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class LibraryResponseDto
{
    public int Id { get; set; }
    public string LibraryCode { get; set; } = string.Empty;
    public string LibraryName { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? OwnerPhone2 { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public RecordStatus Status { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int AccountsCount { get; set; }
    public int PosDevicesCount { get; set; }
}

public class LibraryStatsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Suspended { get; set; }
}

public class CreateLibraryDto
{
    [Required, MaxLength(50)]
    public string LibraryCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string LibraryName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? OwnerName { get; set; }

    [MaxLength(50)]
    public string? OwnerPhone { get; set; }

    [MaxLength(50)]
    public string? OwnerPhone2 { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLibraryDto : CreateLibraryDto
{
}

public class LibraryQueryDto
{
    public int? Id { get; set; }
    public string? LibraryCode { get; set; }
    public string? LibraryName { get; set; }
    public string? OwnerName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public RecordStatus? Status { get; set; }
    public int? AccountsCount { get; set; }
    public int Limit { get; set; } = 50;
}
