using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace MyApi.Dtos;

public class FinancialTransactionResponseDto
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public string LibraryName { get; set; } = string.Empty;
    public FinancialTransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? DueDate { get; set; }
    public FinancialTransactionStatus Status { get; set; }
    public int? CreatedByAdminUserId { get; set; }
    public string? CreatedByAdminUsername { get; set; }
    public int? CreatedByLibraryAccountId { get; set; }
    public string? CreatedByLibraryUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateFinancialTransactionDto
{
    [Required]
    public int LibraryId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
}

public class UpdateFinancialTransactionDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public FinancialTransactionStatus Status { get; set; }
}

public class CreateTransactionSettlementDto
{
    [Required]
    public FinancialSettlementMode SettlementMode { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? UnitAmount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime? SettlementDate { get; set; }
}

public class CreateLibrarySettlementDto : CreateTransactionSettlementDto
{
}

public class TransactionSettlementResponseDto
{
    public int Id { get; set; }
    public int FinancialTransactionId { get; set; }
    public FinancialSettlementMode SettlementMode { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitAmount { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime SettlementDate { get; set; }
    public int? CreatedByAdminUserId { get; set; }
    public string? CreatedByAdminUsername { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LibrarySettlementResultDto
{
    public int LibraryId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal AppliedAmount { get; set; }
    public int SettlementsCreated { get; set; }
    public decimal RemainingLibraryDue { get; set; }
}

public class LibraryFinancialSummaryDto
{
    public int LibraryId { get; set; }
    public string LibraryCode { get; set; } = string.Empty;
    public string LibraryName { get; set; } = string.Empty;
    public decimal TotalCharges { get; set; }
    public decimal TotalSettled { get; set; }
    public decimal TotalDue { get; set; }
    public int TransactionsCount { get; set; }
    public int SettlementsCount { get; set; }

    // Compatibility aliases for existing pages.
    public decimal TotalBalance { get; set; }
    public decimal TotalOpenInvoice { get; set; }
    public decimal TotalPaid { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}

public class LibraryFinancialStatementDto
{
    public LibraryFinancialSummaryDto Summary { get; set; } = new();
    public List<FinancialTransactionResponseDto> Transactions { get; set; } = new();
    public List<TransactionSettlementResponseDto> Settlements { get; set; } = new();
}
