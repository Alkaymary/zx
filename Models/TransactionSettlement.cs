using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class TransactionSettlement
{
    public int Id { get; set; }

    public int FinancialTransactionId { get; set; }

    public FinancialSettlementMode SettlementMode { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? UnitAmount { get; set; }

    public decimal Amount { get; set; }

    public DateTime SettlementDate { get; set; } = DateTime.UtcNow;

    public int? CreatedByAdminUserId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(FinancialTransactionId))]
    public FinancialTransaction FinancialTransaction { get; set; } = null!;

    [ForeignKey(nameof(CreatedByAdminUserId))]
    public AdminUser? CreatedByAdminUser { get; set; }
}
