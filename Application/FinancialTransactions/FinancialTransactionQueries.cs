using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;

namespace MyApi.Application.FinancialTransactions;

public sealed class ListFinancialTransactionsQuery
{
    private readonly AppDbContext _context;

    public ListFinancialTransactionsQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FinancialTransactionResponseDto>> ExecuteAsync(int? libraryId, CancellationToken cancellationToken)
    {
        var query = _context.FinancialTransactions.AsNoTracking().AsQueryable();
        if (libraryId.HasValue)
        {
            query = query.Where(x => x.LibraryId == libraryId.Value);
        }

        return await query
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Select(FinancialTransactionMappings.ToTransactionProjection())
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetFinancialTransactionByIdQuery
{
    private readonly AppDbContext _context;

    public GetFinancialTransactionByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<FinancialTransactionResponseDto>> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var transaction = await TryLoadAsync(id, cancellationToken);
        return transaction is null
            ? AppResult<FinancialTransactionResponseDto>.NotFound("Financial transaction was not found.")
            : AppResult<FinancialTransactionResponseDto>.Success(transaction);
    }

    public Task<FinancialTransactionResponseDto> LoadRequiredAsync(int id, CancellationToken cancellationToken)
    {
        return _context.FinancialTransactions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(FinancialTransactionMappings.ToTransactionProjection())
            .FirstAsync(cancellationToken);
    }

    private Task<FinancialTransactionResponseDto?> TryLoadAsync(int id, CancellationToken cancellationToken)
    {
        return _context.FinancialTransactions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(FinancialTransactionMappings.ToTransactionProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class GetTransactionSettlementByIdQuery
{
    private readonly AppDbContext _context;

    public GetTransactionSettlementByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public Task<TransactionSettlementResponseDto> LoadRequiredAsync(int id, CancellationToken cancellationToken)
    {
        return _context.TransactionSettlements
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(FinancialTransactionMappings.ToSettlementProjection())
            .FirstAsync(cancellationToken);
    }
}

public sealed class GetLibraryFinancialStatementQuery
{
    private readonly AppDbContext _context;

    public GetLibraryFinancialStatementQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryFinancialStatementDto>> ExecuteAsync(int libraryId, CancellationToken cancellationToken)
    {
        var library = await _context.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == libraryId, cancellationToken);

        if (library is null)
        {
            return AppResult<LibraryFinancialStatementDto>.NotFound("Library was not found.");
        }

        var transactions = await _context.FinancialTransactions
            .AsNoTracking()
            .Where(x => x.LibraryId == libraryId)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Select(FinancialTransactionMappings.ToTransactionProjection())
            .ToListAsync(cancellationToken);

        var settlements = await _context.TransactionSettlements
            .AsNoTracking()
            .Where(x => x.FinancialTransaction.LibraryId == libraryId)
            .OrderByDescending(x => x.SettlementDate)
            .ThenByDescending(x => x.Id)
            .Select(FinancialTransactionMappings.ToSettlementProjection())
            .ToListAsync(cancellationToken);

        return AppResult<LibraryFinancialStatementDto>.Success(
            FinancialTransactionMappings.BuildStatement(
                library.Id,
                library.LibraryCode,
                library.LibraryName,
                transactions,
                settlements));
    }
}
