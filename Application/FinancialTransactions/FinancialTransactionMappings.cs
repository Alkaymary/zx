using System.Linq.Expressions;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.FinancialTransactions;

internal static class FinancialTransactionMappings
{
    public static Expression<Func<FinancialTransaction, FinancialTransactionResponseDto>> ToTransactionProjection()
    {
        return x => new FinancialTransactionResponseDto
        {
            Id = x.Id,
            LibraryId = x.LibraryId,
            LibraryName = x.Library.LibraryName,
            TransactionType = x.TransactionType,
            Amount = x.Amount,
            PaidAmount = x.PaidAmount,
            RemainingAmount = x.RemainingAmount,
            Description = x.Description,
            TransactionDate = x.TransactionDate,
            DueDate = x.DueDate,
            Status = x.Status,
            CreatedByAdminUserId = x.CreatedByAdminUserId,
            CreatedByAdminUsername = x.CreatedByAdminUser != null ? x.CreatedByAdminUser.Username : null,
            CreatedByLibraryAccountId = x.CreatedByLibraryAccountId,
            CreatedByLibraryUsername = x.CreatedByLibraryAccount != null ? x.CreatedByLibraryAccount.Username : null,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static Expression<Func<TransactionSettlement, TransactionSettlementResponseDto>> ToSettlementProjection()
    {
        return x => new TransactionSettlementResponseDto
        {
            Id = x.Id,
            FinancialTransactionId = x.FinancialTransactionId,
            SettlementMode = x.SettlementMode,
            Quantity = x.Quantity,
            UnitAmount = x.UnitAmount,
            Amount = x.Amount,
            Notes = x.Notes,
            SettlementDate = x.SettlementDate,
            CreatedByAdminUserId = x.CreatedByAdminUserId,
            CreatedByAdminUsername = x.CreatedByAdminUser != null ? x.CreatedByAdminUser.Username : null,
            CreatedAt = x.CreatedAt
        };
    }

    public static decimal? ResolveSettlementAmount(
        CreateTransactionSettlementDto request,
        decimal fullAmount,
        out decimal? quantity,
        out decimal? unitAmount)
    {
        quantity = null;
        unitAmount = null;

        return request.SettlementMode switch
        {
            FinancialSettlementMode.Full => fullAmount,
            FinancialSettlementMode.PartialAmount when request.Amount.HasValue && request.Amount.Value > 0
                => request.Amount.Value,
            FinancialSettlementMode.ByQuantity when request.Quantity.HasValue
                && request.Quantity.Value > 0
                && request.UnitAmount.HasValue
                && request.UnitAmount.Value > 0
                => ResolveByQuantity(request, out quantity, out unitAmount),
            _ => null
        };
    }

    public static void ApplySettlementToTransaction(FinancialTransaction transaction, decimal settlementAmount)
    {
        transaction.PaidAmount += settlementAmount;
        transaction.RemainingAmount -= settlementAmount;
        transaction.Status = transaction.RemainingAmount == 0
            ? FinancialTransactionStatus.Paid
            : FinancialTransactionStatus.PartiallyPaid;
        transaction.UpdatedAt = DateTime.UtcNow;
    }

    public static void ReverseSettlementOnTransaction(FinancialTransaction transaction, decimal settlementAmount)
    {
        transaction.PaidAmount -= settlementAmount;
        transaction.RemainingAmount += settlementAmount;
        transaction.Status = transaction.PaidAmount <= 0
            ? FinancialTransactionStatus.Open
            : FinancialTransactionStatus.PartiallyPaid;
        transaction.UpdatedAt = DateTime.UtcNow;
    }

    public static LibraryFinancialStatementDto BuildStatement(
        int libraryId,
        string libraryCode,
        string libraryName,
        List<FinancialTransactionResponseDto> transactions,
        List<TransactionSettlementResponseDto> settlements)
    {
        var totalCharges = transactions.Where(x => x.Status != FinancialTransactionStatus.Cancelled).Sum(x => x.Amount);
        var totalSettled = settlements.Sum(x => x.Amount);
        var totalDue = transactions.Where(x => x.Status != FinancialTransactionStatus.Cancelled).Sum(x => x.RemainingAmount);

        return new LibraryFinancialStatementDto
        {
            Summary = new LibraryFinancialSummaryDto
            {
                LibraryId = libraryId,
                LibraryCode = libraryCode,
                LibraryName = libraryName,
                TotalCharges = totalCharges,
                TotalSettled = totalSettled,
                TotalDue = totalDue,
                TransactionsCount = transactions.Count,
                SettlementsCount = settlements.Count,
                TotalBalance = 0m,
                TotalOpenInvoice = totalCharges,
                TotalPaid = totalSettled,
                LastPaymentDate = settlements.Select(x => (DateTime?)x.SettlementDate)
                    .OrderByDescending(x => x)
                    .FirstOrDefault()
            },
            Transactions = transactions,
            Settlements = settlements
        };
    }

    private static decimal ResolveByQuantity(
        CreateTransactionSettlementDto request,
        out decimal? quantity,
        out decimal? unitAmount)
    {
        quantity = request.Quantity!.Value;
        unitAmount = request.UnitAmount!.Value;
        return quantity.Value * unitAmount.Value;
    }
}
