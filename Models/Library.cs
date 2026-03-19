using System.ComponentModel.DataAnnotations;

namespace MyApi.Models;

public class Library
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string LibraryCode { get; set; } = string.Empty;

    [MaxLength(200)]
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LibraryAccount> Accounts { get; set; } = new List<LibraryAccount>();

    public ICollection<PosDevice> PosDevices { get; set; } = new List<PosDevice>();
}
