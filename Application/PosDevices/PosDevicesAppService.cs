using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.PosDevices;

public interface IPosDevicesAppService
{
    Task<IReadOnlyList<PosDeviceResponseDto>> GetAllAsync(PosDeviceQueryDto query, CancellationToken cancellationToken);
    Task<AppResult<PosDeviceResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AppResult<PosDeviceResponseDto>> CreateAsync(CreatePosDeviceDto request, CancellationToken cancellationToken);
    Task<AppResult<PosDeviceResponseDto>> UpdateAsync(int id, UpdatePosDeviceDto request, CancellationToken cancellationToken);
    Task<AppResult<PosDeviceResponseDto>> UpdateByQueryAsync(PosDeviceQueryDto query, UpdatePosDeviceDto request, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<AppResult> DeleteByQueryAsync(PosDeviceQueryDto query, CancellationToken cancellationToken);
}

public class PosDevicesAppService : IPosDevicesAppService
{
    private readonly ListPosDevicesQuery _listQuery;
    private readonly GetPosDeviceByIdQuery _getByIdQuery;
    private readonly CreatePosDeviceUseCase _createUseCase;
    private readonly UpdatePosDeviceUseCase _updateUseCase;
    private readonly UpdatePosDeviceByQueryUseCase _updateByQueryUseCase;
    private readonly DeletePosDeviceUseCase _deleteUseCase;
    private readonly DeletePosDeviceByQueryUseCase _deleteByQueryUseCase;

    public PosDevicesAppService(
        ListPosDevicesQuery listQuery,
        GetPosDeviceByIdQuery getByIdQuery,
        CreatePosDeviceUseCase createUseCase,
        UpdatePosDeviceUseCase updateUseCase,
        UpdatePosDeviceByQueryUseCase updateByQueryUseCase,
        DeletePosDeviceUseCase deleteUseCase,
        DeletePosDeviceByQueryUseCase deleteByQueryUseCase)
    {
        _listQuery = listQuery;
        _getByIdQuery = getByIdQuery;
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _updateByQueryUseCase = updateByQueryUseCase;
        _deleteUseCase = deleteUseCase;
        _deleteByQueryUseCase = deleteByQueryUseCase;
    }

    public Task<IReadOnlyList<PosDeviceResponseDto>> GetAllAsync(
        PosDeviceQueryDto query,
        CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(query, cancellationToken);

    public Task<AppResult<PosDeviceResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, cancellationToken);

    public Task<AppResult<PosDeviceResponseDto>> CreateAsync(
        CreatePosDeviceDto request,
        CancellationToken cancellationToken)
        => _createUseCase.ExecuteAsync(request, cancellationToken);

    public Task<AppResult<PosDeviceResponseDto>> UpdateAsync(
        int id,
        UpdatePosDeviceDto request,
        CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, cancellationToken);

    public Task<AppResult<PosDeviceResponseDto>> UpdateByQueryAsync(
        PosDeviceQueryDto query,
        UpdatePosDeviceDto request,
        CancellationToken cancellationToken)
        => _updateByQueryUseCase.ExecuteAsync(query, request, cancellationToken);

    public Task<AppResult> DeleteAsync(int id, CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, cancellationToken);

    public Task<AppResult> DeleteByQueryAsync(PosDeviceQueryDto query, CancellationToken cancellationToken)
        => _deleteByQueryUseCase.ExecuteAsync(query, cancellationToken);
}
