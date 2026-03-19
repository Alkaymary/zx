using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.FinancialTransactions;

public sealed class CreateFinancialTransactionUseCase
{
    private readonly AppDbContext _context;
    private readonly GetFinancialTransactionByIdQuery _getByIdQuery;

    public CreateFinancialTransactionUseCase(AppDbContext context, GetFinancialTransactionByIdQuery getByIdQuery)
    {
        _context = context;
        _getByIdQuery = getByIdQuery;
    }

    public async Task<AppResult<FinancialTransactionResponseDto>> ExecuteAsync(
        CreateFinancialTransactionDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        if (!await _context.Libraries.AnyAsync(x => x.Id == request.LibraryId, cancellationToken))
        {
            return AppResult<FinancialTransactionResponseDto>.BadRequest("LibraryId does not exist.");
        }

        if (!actor.IsAdminUser || !actor.UserId.HasValue)
        {
            return AppResult<FinancialTransactionResponseDto>.Unauthorized("Invalid admin token.");
        }

        var transaction = new FinancialTransaction
        {
            LibraryId = request.LibraryId,
            TransactionType = FinancialTransactionType.OpenInvoice,
            Amount = request.Amount,
            PaidAmount = 0m,
            RemainingAmount = request.Amount,
            Description = request.Description,
            TransactionDate = request.TransactionDate,
            DueDate = request.DueDate,
            Status = FinancialTransactionStatus.Open,
            CreatedByAdminUserId = actor.UserId.Value,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return AppResult<FinancialTransactionResponseDto>.Success(
            await _getByIdQuery.LoadRequiredAsync(transaction.Id, cancellationToken));
    }
}

public sealed class UpdateFinancialTransactionUseCase
{
    private readonly AppDbContext _context;
    private readonly GetFinancialTransactionByIdQuery _getByIdQuery;

    public UpdateFinancialTransactionUseCase(AppDbContext context, GetFinancialTransactionByIdQuery getByIdQuery)
    {
        _context = context;
        _getByIdQuery = getByIdQuery;
    }

    public async Task<AppResult<FinancialTransactionResponseDto>> ExecuteAsync(
        int id,
        UpdateFinancialTransactionDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await _context.FinancialTransactions
            .Include(x => x.Settlements)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (transaction is null)
        {
            return AppResult<FinancialTransactionResponseDto>.NotFound("Financial transaction was not found.");
        }

        if (transaction.Settlements.Count > 0 || transaction.PaidAmount > 0)
        {
            return AppResult<FinancialTransactionResponseDto>.Conflict("Cannot update a transaction that already has settlements.");
        }

        transaction.Amount = request.Amount;
        transaction.PaidAmount = 0m;
        transaction.RemainingAmount = request.Amount;
        transaction.Description = request.Description;
        transaction.TransactionDate = request.TransactionDate;
        transaction.DueDate = request.DueDate;
        transaction.Status = request.Status == FinancialTransactionStatus.Cancelled
            ? FinancialTransactionStatus.Cancelled
            : FinancialTransactionStatus.Open;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return AppResult<FinancialTransactionResponseDto>.Success(
            await _getByIdQuery.LoadRequiredAsync(transaction.Id, cancellationToken));
    }
}

public sealed class DeleteFinancialTransactionUseCase
{
    private readonly AppDbContext _context;

    public DeleteFinancialTransactionUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var transaction = await _context.FinancialTransactions
            .Include(x => x.Settlements)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (transaction is null)
        {
            return AppResult.NotFound("Financial transaction was not found.");
        }

        if (transaction.Settlements.Count > 0)
        {
            return AppResult.Conflict("Cannot delete a transaction that has settlements.");
        }

        _context.FinancialTransactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}

public sealed class CreateTransactionSettlementUseCase
{
    private readonly AppDbContext _context;
    private readonly GetTransactionSettlementByIdQuery _getSettlementByIdQuery;

    public CreateTransactionSettlementUseCase(
        AppDbContext context,
        GetTransactionSettlementByIdQuery getSettlementByIdQuery)
    {
        _context = context;
        _getSettlementByIdQuery = getSettlementByIdQuery;
    }

    public async Task<AppResult<TransactionSettlementResponseDto>> ExecuteAsync(
        int transactionId,
        CreateTransactionSettlementDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        var transaction = await _context.FinancialTransactions
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken);

        if (transaction is null)
        {
            return AppResult<TransactionSettlementResponseDto>.NotFound("Financial transaction was not found.");
        }

        if (transaction.Status == FinancialTransactionStatus.Cancelled)
        {
            return AppResult<TransactionSettlementResponseDto>.Conflict("Cancelled transactions cannot be settled.");
        }

        if (transaction.RemainingAmount <= 0)
        {
            return AppResult<TransactionSettlementResponseDto>.Conflict("This transaction is already fully settled.");
        }

        if (!actor.IsAdminUser || !actor.UserId.HasValue)
        {
            return AppResult<TransactionSettlementResponseDto>.Unauthorized("Invalid admin token.");
        }

        var settlementAmount = FinancialTransactionMappings.ResolveSettlementAmount(
            request,
            transaction.RemainingAmount,
            out var quantity,
            out var unitAmount);

        if (!settlementAmount.HasValue)
        {
            return AppResult<TransactionSettlementResponseDto>.BadRequest("Settlement data is invalid.");
        }

        if (settlementAmount.Value > transaction.RemainingAmount)
        {
            return AppResult<TransactionSettlementResponseDto>.Conflict("Settlement amount exceeds the remaining due amount.");
        }

        var settlement = new TransactionSettlement
        {
            FinancialTransactionId = transactionId,
            SettlementMode = request.SettlementMode,
            Quantity = quantity,
            UnitAmount = unitAmount,
            Amount = settlementAmount.Value,
            Notes = request.Notes,
            SettlementDate = request.SettlementDate ?? DateTime.UtcNow,
            CreatedByAdminUserId = actor.UserId.Value
        };

        FinancialTransactionMappings.ApplySettlementToTransaction(transaction, settlement.Amount);
        _context.TransactionSettlements.Add(settlement);
        await _context.SaveChangesAsync(cancellationToken);

        return AppResult<TransactionSettlementResponseDto>.Success(
            await _getSettlementByIdQuery.LoadRequiredAsync(settlement.Id, cancellationToken));
    }
}

public sealed class CreateLibrarySettlementUseCase
{
    private readonly AppDbContext _context;

    public CreateLibrarySettlementUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibrarySettlementResultDto>> ExecuteAsync(
        int libraryId,
        CreateLibrarySettlementDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
    {
        var library = await _context.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == libraryId, cancellationToken);

        if (library is null)
        {
            return AppResult<LibrarySettlementResultDto>.NotFound("Library was not found.");
        }

        if (!actor.IsAdminUser || !actor.UserId.HasValue)
        {
            return AppResult<LibrarySettlementResultDto>.Unauthorized("Invalid admin token.");
        }

        var openTransactions = await _context.FinancialTransactions
            .Where(x => x.LibraryId == libraryId
                && x.Status != FinancialTransactionStatus.Cancelled
                && x.RemainingAmount > 0)
            .OrderBy(x => x.DueDate ?? x.TransactionDate)
            .ThenBy(x => x.TransactionDate)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (openTransactions.Count == 0)
        {
            return AppResult<LibrarySettlementResultDto>.Conflict("This library has no open dues to settle.");
        }

        var totalDue = openTransactions.Sum(x => x.RemainingAmount);
        var requestedAmount = FinancialTransactionMappings.ResolveSettlementAmount(request, totalDue, out _, out _);
        if (!requestedAmount.HasValue)
        {
            return AppResult<LibrarySettlementResultDto>.BadRequest("Settlement data is invalid.");
        }

        if (requestedAmount.Value > totalDue)
        {
            return AppResult<LibrarySettlementResultDto>.Conflict("Settlement amount exceeds the library due amount.");
        }

        var remainingToApply = requestedAmount.Value;
        var createdCount = 0;
        var settlementDate = request.SettlementDate ?? DateTime.UtcNow;

        foreach (var transaction in openTransactions)
        {
            if (remainingToApply <= 0)
            {
                break;
            }

            var applyAmount = Math.Min(transaction.RemainingAmount, remainingToApply);
            if (applyAmount <= 0)
            {
                continue;
            }

            _context.TransactionSettlements.Add(new TransactionSettlement
            {
                FinancialTransactionId = transaction.Id,
                SettlementMode = request.SettlementMode,
                Amount = applyAmount,
                Notes = request.Notes,
                SettlementDate = settlementDate,
                CreatedByAdminUserId = actor.UserId.Value
            });

            FinancialTransactionMappings.ApplySettlementToTransaction(transaction, applyAmount);
            remainingToApply -= applyAmount;
            createdCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return AppResult<LibrarySettlementResultDto>.Success(new LibrarySettlementResultDto
        {
            LibraryId = libraryId,
            RequestedAmount = requestedAmount.Value,
            AppliedAmount = requestedAmount.Value - remainingToApply,
            SettlementsCreated = createdCount,
            RemainingLibraryDue = openTransactions.Sum(x => x.RemainingAmount)
        });
    }
}

public sealed class DeleteTransactionSettlementUseCase
{
    private readonly AppDbContext _context;

    public DeleteTransactionSettlementUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var settlement = await _context.TransactionSettlements
            .Include(x => x.FinancialTransaction)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (settlement is null)
        {
            return AppResult.NotFound("Settlement was not found.");
        }

        FinancialTransactionMappings.ReverseSettlementOnTransaction(settlement.FinancialTransaction, settlement.Amount);
        _context.TransactionSettlements.Remove(settlement);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
