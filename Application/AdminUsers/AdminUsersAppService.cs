using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.AdminUsers;

public interface IAdminUsersAppService
{
    Task<IReadOnlyList<AdminUserResponseDto>> GetAllAsync(AdminUserQueryDto query, CancellationToken cancellationToken);
    Task<AppResult<AdminUserResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<AdminUserResponseDto>> CreateAsync(CreateAdminUserDto request, CancellationToken cancellationToken);
    Task<AppResult<AdminUserResponseDto>> UpdateAsync(int id, UpdateAdminUserDto request, CancellationToken cancellationToken);
    Task<AppResult<AdminUserResponseDto>> UpdateByQueryAsync(AdminUserQueryDto query, UpdateAdminUserDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<AppResult> DeleteByQueryAsync(AdminUserQueryDto query, CancellationToken cancellationToken);
}

public class AdminUsersAppService : IAdminUsersAppService
{
    private readonly ListAdminUsersQuery _listQuery;
    private readonly GetAdminUserByIdQuery _getByIdQuery;
    private readonly CreateAdminUserUseCase _createUseCase;
    private readonly UpdateAdminUserUseCase _updateUseCase;
    private readonly UpdateAdminUserByQueryUseCase _updateByQueryUseCase;
    private readonly DeleteAdminUserUseCase _deleteUseCase;
    private readonly DeleteAdminUserByQueryUseCase _deleteByQueryUseCase;

    public AdminUsersAppService(
        ListAdminUsersQuery listQuery,
        GetAdminUserByIdQuery getByIdQuery,
        CreateAdminUserUseCase createUseCase,
        UpdateAdminUserUseCase updateUseCase,
        UpdateAdminUserByQueryUseCase updateByQueryUseCase,
        DeleteAdminUserUseCase deleteUseCase,
        DeleteAdminUserByQueryUseCase deleteByQueryUseCase)
    {
        _listQuery = listQuery;
        _getByIdQuery = getByIdQuery;
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _updateByQueryUseCase = updateByQueryUseCase;
        _deleteUseCase = deleteUseCase;
        _deleteByQueryUseCase = deleteByQueryUseCase;
    }

    public Task<IReadOnlyList<AdminUserResponseDto>> GetAllAsync(
        AdminUserQueryDto query,
        CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(query, cancellationToken);

    public Task<AppResult<AdminUserResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, cancellationToken);

    public Task<AppResult<AdminUserResponseDto>> CreateAsync(
        CreateAdminUserDto request,
        CancellationToken cancellationToken)
        => _createUseCase.ExecuteAsync(request, cancellationToken);

    public Task<AppResult<AdminUserResponseDto>> UpdateAsync(
        int id,
        UpdateAdminUserDto request,
        CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, cancellationToken);

    public Task<AppResult<AdminUserResponseDto>> UpdateByQueryAsync(
        AdminUserQueryDto query,
        UpdateAdminUserDto request,
        CancellationToken cancellationToken)
        => _updateByQueryUseCase.ExecuteAsync(query, request, cancellationToken);

    public Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, cancellationToken);

    public Task<AppResult> DeleteByQueryAsync(AdminUserQueryDto query, CancellationToken cancellationToken)
        => _deleteByQueryUseCase.ExecuteAsync(query, cancellationToken);
}
