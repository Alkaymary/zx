using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class PackageResponseDto
{
    /// <summary>The unique package identifier.</summary>
    public int Id { get; set; }

    /// <summary>The unique package code.</summary>
    public string PackageCode { get; set; } = string.Empty;

    /// <summary>The package name shown to admins and clients.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The package price in Iraqi dinars.</summary>
    public decimal PriceIqd { get; set; }

    /// <summary>The current package status.</summary>
    public RecordStatus Status { get; set; }

    /// <summary>The admin user id who created this package.</summary>
    public int AddedByAdminUserId { get; set; }

    /// <summary>The username of the admin who created this package.</summary>
    public string AddedByUsername { get; set; } = string.Empty;

    /// <summary>The package creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>The package last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

public class CreatePackageDto
{
    /// <summary>The unique package code.</summary>
    [Required, MaxLength(100)]
    public string PackageCode { get; set; } = string.Empty;

    /// <summary>The package name.</summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>The package price in Iraqi dinars.</summary>
    [Range(0, double.MaxValue)]
    public decimal PriceIqd { get; set; }

    /// <summary>The package status.</summary>
    public RecordStatus Status { get; set; } = RecordStatus.Active;
}

public class UpdatePackageDto : CreatePackageDto
{
}
