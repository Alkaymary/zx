using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class QrCode
{
    public int Id { get; set; }

    public int LibraryId { get; set; }

    public int PackageId { get; set; }

    public int PosDeviceId { get; set; }

    public int CreatedByLibraryAccountId { get; set; }

    public int FinancialTransactionId { get; set; }

    [MaxLength(100)]
    public string QrReference { get; set; } = string.Empty;

    [MaxLength(200)]
    public string StudentName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string StudentPhoneNumber { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string QrPayload { get; set; } = string.Empty;

    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(LibraryId))]
    public Library Library { get; set; } = null!;

    [ForeignKey(nameof(PackageId))]
    public Package Package { get; set; } = null!;

    [ForeignKey(nameof(PosDeviceId))]
    public PosDevice PosDevice { get; set; } = null!;

    [ForeignKey(nameof(CreatedByLibraryAccountId))]
    public LibraryAccount CreatedByLibraryAccount { get; set; } = null!;

    [ForeignKey(nameof(FinancialTransactionId))]
    public FinancialTransaction FinancialTransaction { get; set; } = null!;
}
