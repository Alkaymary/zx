using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class QrCodeResponseDto
{
    /// <summary>The unique QR record identifier.</summary>
    public int Id { get; set; }

    /// <summary>The unique QR reference code.</summary>
    public string QrReference { get; set; } = string.Empty;

    /// <summary>The library identifier.</summary>
    public int LibraryId { get; set; }

    /// <summary>The library code used to export the QR.</summary>
    public string LibraryCode { get; set; } = string.Empty;

    /// <summary>The library name.</summary>
    public string LibraryName { get; set; } = string.Empty;

    /// <summary>The package identifier.</summary>
    public int PackageId { get; set; }

    /// <summary>The selected package code.</summary>
    public string PackageCode { get; set; } = string.Empty;

    /// <summary>The selected package name.</summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>The POS device identifier.</summary>
    public int PosDeviceId { get; set; }

    /// <summary>The POS code used to export the QR.</summary>
    public string PosCode { get; set; } = string.Empty;

    /// <summary>The student name included in the QR.</summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>The student phone number included in the QR.</summary>
    public string StudentPhoneNumber { get; set; } = string.Empty;

    /// <summary>The generated QR payload text.</summary>
    public string QrPayload { get; set; } = string.Empty;

    /// <summary>The linked financial transaction identifier created for this export.</summary>
    public int FinancialTransactionId { get; set; }

    /// <summary>The package charge amount in Iraqi dinars linked to this export.</summary>
    public decimal ChargeAmountIqd { get; set; }

    /// <summary>The current financial status for the linked package charge.</summary>
    public FinancialTransactionStatus FinancialStatus { get; set; }

    /// <summary>The remaining amount on the linked financial charge.</summary>
    public decimal RemainingAmountIqd { get; set; }

    /// <summary>The QR status.</summary>
    public RecordStatus Status { get; set; }

    /// <summary>The library account id that created the QR.</summary>
    public int CreatedByLibraryAccountId { get; set; }

    /// <summary>The username of the creator library account.</summary>
    public string CreatedByUsername { get; set; } = string.Empty;

    /// <summary>The creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>The last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

public class AdminQrCodeDetailsDto
{
    /// <summary>The QR record identifier.</summary>
    public int Id { get; set; }

    /// <summary>The exported QR reference code.</summary>
    public string QrReference { get; set; } = string.Empty;

    /// <summary>The library identifier.</summary>
    public int LibraryId { get; set; }

    /// <summary>The library code.</summary>
    public string LibraryCode { get; set; } = string.Empty;

    /// <summary>The library name.</summary>
    public string LibraryName { get; set; } = string.Empty;

    /// <summary>The POS device identifier.</summary>
    public int PosDeviceId { get; set; }

    /// <summary>The POS code used in the export.</summary>
    public string PosCode { get; set; } = string.Empty;

    /// <summary>The package identifier.</summary>
    public int PackageId { get; set; }

    /// <summary>The package code.</summary>
    public string PackageCode { get; set; } = string.Empty;

    /// <summary>The package name.</summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>The package price in Iraqi dinars.</summary>
    public decimal PackagePriceIqd { get; set; }

    /// <summary>The student name.</summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>The student phone number.</summary>
    public string StudentPhoneNumber { get; set; } = string.Empty;

    /// <summary>The QR payload text.</summary>
    public string QrPayload { get; set; } = string.Empty;

    /// <summary>The QR status.</summary>
    public RecordStatus Status { get; set; }

    /// <summary>The creator library account id.</summary>
    public int CreatedByLibraryAccountId { get; set; }

    /// <summary>The creator library account username.</summary>
    public string CreatedByUsername { get; set; } = string.Empty;

    /// <summary>The creator library account full name.</summary>
    public string CreatedByFullName { get; set; } = string.Empty;

    /// <summary>The linked financial transaction id.</summary>
    public int FinancialTransactionId { get; set; }

    /// <summary>The linked financial transaction type.</summary>
    public FinancialTransactionType FinancialTransactionType { get; set; }

    /// <summary>The linked financial amount in Iraqi dinars.</summary>
    public decimal ChargeAmountIqd { get; set; }

    /// <summary>The linked financial paid amount in Iraqi dinars.</summary>
    public decimal PaidAmountIqd { get; set; }

    /// <summary>The linked financial remaining amount in Iraqi dinars.</summary>
    public decimal RemainingAmountIqd { get; set; }

    /// <summary>The linked financial status.</summary>
    public FinancialTransactionStatus FinancialStatus { get; set; }

    /// <summary>The linked financial description.</summary>
    public string? FinancialDescription { get; set; }

    /// <summary>The linked financial transaction date.</summary>
    public DateTime FinancialTransactionDate { get; set; }

    /// <summary>The linked financial due date.</summary>
    public DateTime? FinancialDueDate { get; set; }

    /// <summary>The QR creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>The QR last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

public class CreateQrCodeDto
{
    /// <summary>The library code. It must match the current logged-in library.</summary>
    [Required, MaxLength(50)]
    public string LibraryCode { get; set; } = string.Empty;

    /// <summary>The POS code belonging to the current library.</summary>
    [Required, MaxLength(50)]
    public string PosCode { get; set; } = string.Empty;

    /// <summary>The selected package code.</summary>
    [Required, MaxLength(100)]
    public string PackageCode { get; set; } = string.Empty;

    /// <summary>The student name that will appear in the QR payload.</summary>
    [Required, MaxLength(200)]
    public string StudentName { get; set; } = string.Empty;

    /// <summary>The student phone number that will appear in the QR payload.</summary>
    [Required, MaxLength(50)]
    public string StudentPhoneNumber { get; set; } = string.Empty;
}

public class UpdateQrCodeDto
{
    /// <summary>The student name that will appear in the QR payload.</summary>
    [Required, MaxLength(200)]
    public string StudentName { get; set; } = string.Empty;

    /// <summary>The student phone number that will appear in the QR payload.</summary>
    [Required, MaxLength(50)]
    public string StudentPhoneNumber { get; set; } = string.Empty;

    /// <summary>The QR status.</summary>
    public RecordStatus Status { get; set; } = RecordStatus.Active;
}

public class LibraryQrMetricsDto
{
    /// <summary>The library identifier.</summary>
    public int LibraryId { get; set; }

    /// <summary>Total generated QR codes for the library.</summary>
    public int TotalGenerated { get; set; }

    /// <summary>Total QR codes considered used/activated for the library.</summary>
    public int TotalUsed { get; set; }
}

public class AdminQrCodeListItemDto
{
    public int Id { get; set; }
    public string QrReference { get; set; } = string.Empty;
    public int LibraryId { get; set; }
    public string LibraryCode { get; set; } = string.Empty;
    public string LibraryName { get; set; } = string.Empty;
    public string PosCode { get; set; } = string.Empty;
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentPhoneNumber { get; set; } = string.Empty;
    public RecordStatus Status { get; set; }
    public decimal ChargeAmountIqd { get; set; }
    public decimal RemainingAmountIqd { get; set; }
    public DateTime CreatedAt { get; set; }
}
