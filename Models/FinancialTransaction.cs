using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class FinancialTransaction
{
    public int Id { get; set; }

    public int LibraryId { get; set; }

    public FinancialTransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public FinancialTransactionStatus Status { get; set; } = FinancialTransactionStatus.Open;

    public int? CreatedByAdminUserId { get; set; }

    public int? CreatedByLibraryAccountId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(LibraryId))]
    public Library Library { get; set; } = null!;

    [ForeignKey(nameof(CreatedByAdminUserId))]
    public AdminUser? CreatedByAdminUser { get; set; }

    [ForeignKey(nameof(CreatedByLibraryAccountId))]
    public LibraryAccount? CreatedByLibraryAccount { get; set; }

    public ICollection<TransactionSettlement> Settlements { get; set; } = new List<TransactionSettlement>();

    public ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();
}
