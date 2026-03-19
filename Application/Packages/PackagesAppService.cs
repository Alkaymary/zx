using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.Packages;

public interface IPackagesAppService
{
    Task<IReadOnlyList<PackageResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AppResult<PackageResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<PackageResponseDto>> CreateAsync(CreatePackageDto request, AdminActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<PackageResponseDto>> UpdateAsync(int id, UpdatePackageDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public class PackagesAppService : IPackagesAppService
{
    private readonly ListPackagesQuery _listQuery;
    private readonly GetPackageByIdQuery _getByIdQuery;
    private readonly CreatePackageUseCase _createUseCase;
    private readonly UpdatePackageUseCase _updateUseCase;
    private readonly DeletePackageUseCase _deleteUseCase;

    public PackagesAppService(
        ListPackagesQuery listQuery,
        GetPackageByIdQuery getByIdQuery,
        CreatePackageUseCase createUseCase,
        UpdatePackageUseCase updateUseCase,
        DeletePackageUseCase deleteUseCase)
    {
        _listQuery = listQuery;
        _getByIdQuery = getByIdQuery;
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
    }

    public Task<IReadOnlyList<PackageResponseDto>> GetAllAsync(CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(cancellationToken);

    public Task<AppResult<PackageResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, cancellationToken);

    public Task<AppResult<PackageResponseDto>> CreateAsync(
        CreatePackageDto request,
        AdminActorContext actor,
        CancellationToken cancellationToken)
        => _createUseCase.ExecuteAsync(request, actor, cancellationToken);

    public Task<AppResult<PackageResponseDto>> UpdateAsync(
        int id,
        UpdatePackageDto request,
        CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, cancellationToken);

    public Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, cancellationToken);
}
