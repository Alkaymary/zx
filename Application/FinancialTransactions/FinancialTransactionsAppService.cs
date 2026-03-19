using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.FinancialTransactions;

public interface IFinancialTransactionsAppService
{
    Task<IReadOnlyList<FinancialTransactionResponseDto>> GetAllAsync(int? libraryId, CancellationToken cancellationToken);
    Task<AppResult<FinancialTransactionResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<FinancialTransactionResponseDto>> CreateAsync(CreateFinancialTransactionDto request, AdminActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<FinancialTransactionResponseDto>> UpdateAsync(int id, UpdateFinancialTransactionDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<TransactionSettlementResponseDto>> CreateSettlementAsync(int transactionId, CreateTransactionSettlementDto request, AdminActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<LibrarySettlementResultDto>> CreateLibrarySettlementAsync(int libraryId, CreateLibrarySettlementDto request, AdminActorContext actor, CancellationToken cancellationToken);
    Task<AppResult> DeleteSettlementAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<LibraryFinancialStatementDto>> GetStatementByLibraryAsync(int libraryId, CancellationToken cancellationToken);
}

public class FinancialTransactionsAppService : IFinancialTransactionsAppService
{
    private readonly ListFinancialTransactionsQuery _listQuery;
    private readonly GetFinancialTransactionByIdQuery _getByIdQuery;
    private readonly GetLibraryFinancialStatementQuery _statementQuery;
    private readonly CreateFinancialTransactionUseCase _createUseCase;
    private readonly UpdateFinancialTransactionUseCase _updateUseCase;
    private readonly DeleteFinancialTransactionUseCase _deleteUseCase;
    private readonly CreateTransactionSettlementUseCase _createSettlementUseCase;
    private readonly CreateLibrarySettlementUseCase _createLibrarySettlementUseCase;
    private readonly DeleteTransactionSettlementUseCase _deleteSettlementUseCase;

    public FinancialTransactionsAppService(
        ListFinancialTransactionsQuery listQuery,
        GetFinancialTransactionByIdQuery getByIdQuery,
        GetLibraryFinancialStatementQuery statementQuery,
        CreateFinancialTransactionUseCase createUseCase,
        UpdateFinancialTransactionUseCase updateUseCase,
        DeleteFinancialTransactionUseCase deleteUseCase,
        CreateTransactionSettlementUseCase createSettlementUseCase,
        CreateLibrarySettlementUseCase createLibrarySettlementUseCase,
        DeleteTransactionSettlementUseCase deleteSettlementUseCase)
    {
        _listQuery = listQuery;
        _getByIdQuery = getByIdQuery;
        _statementQuery = statementQuery;
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
        _createSettlementUseCase = createSettlementUseCase;
        _createLibrarySettlementUseCase = createLibrarySettlementUseCase;
        _deleteSettlementUseCase = deleteSettlementUseCase;
    }

    public Task<IReadOnlyList<FinancialTransactionResponseDto>> GetAllAsync(int? libraryId, CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(libraryId, cancellationToken);

    public Task<AppResult<FinancialTransactionResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, cancellationToken);

    public Task<AppResult<FinancialTransactionResponseDto>> CreateAsync(
        CreateFinancialTransactionDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _createUseCase.ExecuteAsync(request, actor, cancellationToken);

    public Task<AppResult<FinancialTransactionResponseDto>> UpdateAsync(
        int id,
        UpdateFinancialTransactionDto request,
        CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, cancellationToken);

    public Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, cancellationToken);

    public Task<AppResult<TransactionSettlementResponseDto>> CreateSettlementAsync(
        int transactionId,
        CreateTransactionSettlementDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _createSettlementUseCase.ExecuteAsync(transactionId, request, actor, cancellationToken);

    public Task<AppResult<LibrarySettlementResultDto>> CreateLibrarySettlementAsync(
        int libraryId,
        CreateLibrarySettlementDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _createLibrarySettlementUseCase.ExecuteAsync(libraryId, request, actor, cancellationToken);

    public Task<AppResult> DeleteSettlementAsync(int id, CancellationToken cancellationToken)
        => _deleteSettlementUseCase.ExecuteAsync(id, cancellationToken);

    public Task<AppResult<LibraryFinancialStatementDto>> GetStatementByLibraryAsync(int libraryId, CancellationToken cancellationToken)
        => _statementQuery.ExecuteAsync(libraryId, cancellationToken);
}
