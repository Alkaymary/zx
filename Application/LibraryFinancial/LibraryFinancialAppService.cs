using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.LibraryFinancial;

public interface ILibraryFinancialAppService
{
    Task<AppResult<LibraryFinancialSummaryDto>> GetMySummaryAsync(LibraryActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<LibraryFinancialStatementDto>> GetMyStatementAsync(LibraryActorContext actor, CancellationToken cancellationToken);
}

public class LibraryFinancialAppService : ILibraryFinancialAppService
{
    private readonly AppDbContext _context;

    public LibraryFinancialAppService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryFinancialSummaryDto>> GetMySummaryAsync(LibraryActorContext actor, CancellationToken cancellationToken)
    {
        var library = await GetCurrentLibraryAsync(actor, cancellationToken);
        if (library is null)
        {
            return AppResult<LibraryFinancialSummaryDto>.Unauthorized("Invalid library token.");
        }

        var statement = await BuildStatementAsync(library.Id, library.LibraryCode, library.LibraryName, cancellationToken);
        return AppResult<LibraryFinancialSummaryDto>.Success(statement.Summary);
    }

    public async Task<AppResult<LibraryFinancialStatementDto>> GetMyStatementAsync(LibraryActorContext actor, CancellationToken cancellationToken)
    {
        var library = await GetCurrentLibraryAsync(actor, cancellationToken);
        if (library is null)
        {
            return AppResult<LibraryFinancialStatementDto>.Unauthorized("Invalid library token.");
        }

        return AppResult<LibraryFinancialStatementDto>.Success(
            await BuildStatementAsync(library.Id, library.LibraryCode, library.LibraryName, cancellationToken));
    }

    private async Task<Library?> GetCurrentLibraryAsync(LibraryActorContext actor, CancellationToken cancellationToken)
    {
        if (!actor.IsLibraryAccount || !actor.AccountId.HasValue)
        {
            return null;
        }

        var account = await _context.LibraryAccounts
            .AsNoTracking()
            .Where(x => x.Id == actor.AccountId.Value)
            .Select(x => new { x.LibraryId })
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            return null;
        }

        return await _context.Libraries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == account.LibraryId, cancellationToken);
    }

    private async Task<LibraryFinancialStatementDto> BuildStatementAsync(
        int libraryId,
        string libraryCode,
        string libraryName,
        CancellationToken cancellationToken)
    {
        var transactions = await _context.FinancialTransactions
            .AsNoTracking()
            .Where(x => x.LibraryId == libraryId)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new FinancialTransactionResponseDto
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
            })
            .ToListAsync(cancellationToken);

        var settlements = await _context.TransactionSettlements
            .AsNoTracking()
            .Where(x => x.FinancialTransaction.LibraryId == libraryId)
            .OrderByDescending(x => x.SettlementDate)
            .Select(x => new TransactionSettlementResponseDto
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
            })
            .ToListAsync(cancellationToken);

        var totalCharges = transactions.Where(x => x.Status != FinancialTransactionStatus.Cancelled).Sum(x => x.Amount);
        var totalSettled = settlements.Sum(x => x.Amount);
        var totalDue = transactions.Where(x => x.Status != FinancialTransactionStatus.Cancelled).Sum(x => x.RemainingAmount);

        var summary = new LibraryFinancialSummaryDto
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
        };

        return new LibraryFinancialStatementDto
        {
            Summary = summary,
            Transactions = transactions,
            Settlements = settlements
        };
    }
}
