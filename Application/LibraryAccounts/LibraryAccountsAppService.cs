using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.LibraryAccounts;

public interface ILibraryAccountsAppService
{
    Task<IReadOnlyList<LibraryAccountResponseDto>> GetAllAsync(LibraryAccountQueryDto query, CancellationToken cancellationToken);
    Task<AppResult<LibraryAccountResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleResponseDto>> GetAvailableRolesAsync(CancellationToken cancellationToken);
    Task<AppResult<LibraryAccountResponseDto>> CreateAsync(CreateLibraryAccountDto request, CancellationToken cancellationToken);
    Task<AppResult<LibraryAccountResponseDto>> UpdateAsync(int id, UpdateLibraryAccountDto request, CancellationToken cancellationToken);
    Task<AppResult<LibraryAccountResponseDto>> UpdateByQueryAsync(LibraryAccountQueryDto query, UpdateLibraryAccountDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<AppResult> DeleteByQueryAsync(LibraryAccountQueryDto query, CancellationToken cancellationToken);
}

public class LibraryAccountsAppService : ILibraryAccountsAppService
{
    private readonly ListLibraryAccountsQuery _listQuery;
    private readonly GetLibraryAccountByIdQuery _getByIdQuery;
    private readonly GetAvailableLibraryAccountRolesQuery _getAvailableRolesQuery;
    private readonly CreateLibraryAccountUseCase _createUseCase;
    private readonly UpdateLibraryAccountUseCase _updateUseCase;
    private readonly UpdateLibraryAccountByQueryUseCase _updateByQueryUseCase;
    private readonly DeleteLibraryAccountUseCase _deleteUseCase;
    private readonly DeleteLibraryAccountByQueryUseCase _deleteByQueryUseCase;

    public LibraryAccountsAppService(
        ListLibraryAccountsQuery listQuery,
        GetLibraryAccountByIdQuery getByIdQuery,
        GetAvailableLibraryAccountRolesQuery getAvailableRolesQuery,
        CreateLibraryAccountUseCase createUseCase,
        UpdateLibraryAccountUseCase updateUseCase,
        UpdateLibraryAccountByQueryUseCase updateByQueryUseCase,
        DeleteLibraryAccountUseCase deleteUseCase,
        DeleteLibraryAccountByQueryUseCase deleteByQueryUseCase)
    {
        _listQuery = listQuery;
        _getByIdQuery = getByIdQuery;
        _getAvailableRolesQuery = getAvailableRolesQuery;
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _updateByQueryUseCase = updateByQueryUseCase;
        _deleteUseCase = deleteUseCase;
        _deleteByQueryUseCase = deleteByQueryUseCase;
    }

    public Task<IReadOnlyList<LibraryAccountResponseDto>> GetAllAsync(
        LibraryAccountQueryDto query,
        CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(query, cancellationToken);

    public Task<AppResult<LibraryAccountResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, cancellationToken);

    public Task<IReadOnlyList<RoleResponseDto>> GetAvailableRolesAsync(CancellationToken cancellationToken)
        => _getAvailableRolesQuery.ExecuteAsync(cancellationToken);

    public Task<AppResult<LibraryAccountResponseDto>> CreateAsync(
        CreateLibraryAccountDto request,
        CancellationToken cancellationToken)
        => _createUseCase.ExecuteAsync(request, cancellationToken);

    public Task<AppResult<LibraryAccountResponseDto>> UpdateAsync(
        int id,
        UpdateLibraryAccountDto request,
        CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, cancellationToken);

    public Task<AppResult<LibraryAccountResponseDto>> UpdateByQueryAsync(
        LibraryAccountQueryDto query,
        UpdateLibraryAccountDto request,
        CancellationToken cancellationToken)
        => _updateByQueryUseCase.ExecuteAsync(query, request, cancellationToken);

    public Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, cancellationToken);

    public Task<AppResult> DeleteByQueryAsync(LibraryAccountQueryDto query, CancellationToken cancellationToken)
        => _deleteByQueryUseCase.ExecuteAsync(query, cancellationToken);
}
