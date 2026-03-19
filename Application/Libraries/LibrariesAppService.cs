using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.Libraries;

public interface ILibrariesAppService
{
    Task<IReadOnlyList<LibraryResponseDto>> GetAllAsync(LibraryQueryDto query, AdminActorContext actor, CancellationToken cancellationToken);
    Task<IReadOnlyList<LibraryResponseDto>> SearchAsync(string? q, int limit, AdminActorContext actor, CancellationToken cancellationToken);
    Task<LibraryStatsDto> GetStatsAsync(CancellationToken cancellationToken);
    Task<AppResult<LibraryResponseDto>> GetByIdAsync(int id, AdminActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<LibraryResponseDto>> CreateAsync(CreateLibraryDto request, CancellationToken cancellationToken);
    Task<AppResult<LibraryResponseDto>> UpdateAsync(int id, UpdateLibraryDto request, CancellationToken cancellationToken);
    Task<AppResult<LibraryResponseDto>> UpdateByQueryAsync(LibraryQueryDto query, UpdateLibraryDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<AppResult> DeleteByQueryAsync(LibraryQueryDto query, CancellationToken cancellationToken);
}

public class LibrariesAppService : ILibrariesAppService
{
    private readonly ListLibrariesQuery _listQuery;
    private readonly SearchLibrariesQuery _searchQuery;
    private readonly GetLibraryStatsQuery _statsQuery;
    private readonly GetLibraryByIdQuery _getByIdQuery;
    private readonly CreateLibraryUseCase _createUseCase;
    private readonly UpdateLibraryUseCase _updateUseCase;
    private readonly UpdateLibraryByQueryUseCase _updateByQueryUseCase;
    private readonly DeleteLibraryUseCase _deleteUseCase;
    private readonly DeleteLibraryByQueryUseCase _deleteByQueryUseCase;

    public LibrariesAppService(
        ListLibrariesQuery listQuery,
        SearchLibrariesQuery searchQuery,
        GetLibraryStatsQuery statsQuery,
        GetLibraryByIdQuery getByIdQuery,
        CreateLibraryUseCase createUseCase,
        UpdateLibraryUseCase updateUseCase,
        UpdateLibraryByQueryUseCase updateByQueryUseCase,
        DeleteLibraryUseCase deleteUseCase,
        DeleteLibraryByQueryUseCase deleteByQueryUseCase)
    {
        _listQuery = listQuery;
        _searchQuery = searchQuery;
        _statsQuery = statsQuery;
        _getByIdQuery = getByIdQuery;
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _updateByQueryUseCase = updateByQueryUseCase;
        _deleteUseCase = deleteUseCase;
        _deleteByQueryUseCase = deleteByQueryUseCase;
    }

    public Task<IReadOnlyList<LibraryResponseDto>> GetAllAsync(
        LibraryQueryDto query,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(query, actor, cancellationToken);

    public Task<IReadOnlyList<LibraryResponseDto>> SearchAsync(
        string? q,
        int limit,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _searchQuery.ExecuteAsync(q, limit, actor, cancellationToken);

    public Task<LibraryStatsDto> GetStatsAsync(CancellationToken cancellationToken)
        => _statsQuery.ExecuteAsync(cancellationToken);

    public Task<AppResult<LibraryResponseDto>> GetByIdAsync(
        int id,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, actor, cancellationToken);

    public Task<AppResult<LibraryResponseDto>> CreateAsync(CreateLibraryDto request, CancellationToken cancellationToken)
        => _createUseCase.ExecuteAsync(request, cancellationToken);

    public Task<AppResult<LibraryResponseDto>> UpdateAsync(int id, UpdateLibraryDto request, CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, cancellationToken);

    public Task<AppResult<LibraryResponseDto>> UpdateByQueryAsync(
        LibraryQueryDto query,
        UpdateLibraryDto request,
        CancellationToken cancellationToken)
        => _updateByQueryUseCase.ExecuteAsync(query, request, cancellationToken);

    public Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, cancellationToken);

    public Task<AppResult> DeleteByQueryAsync(LibraryQueryDto query, CancellationToken cancellationToken)
        => _deleteByQueryUseCase.ExecuteAsync(query, cancellationToken);
}
