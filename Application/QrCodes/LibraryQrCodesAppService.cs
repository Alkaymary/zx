using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Dtos;

namespace MyApi.Application.QrCodes;

public interface ILibraryQrCodesAppService
{
    Task<AppResult<IReadOnlyList<QrCodeResponseDto>>> GetAllAsync(LibraryActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<QrCodeResponseDto>> GetByIdAsync(int id, LibraryActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<string>> ExportAsync(CreateQrCodeDto request, LibraryActorContext actor, CancellationToken cancellationToken);
    Task<AppResult<QrCodeResponseDto>> UpdateAsync(int id, UpdateQrCodeDto request, LibraryActorContext actor, CancellationToken cancellationToken);
    Task<AppResult> DeleteAsync(int id, LibraryActorContext actor, CancellationToken cancellationToken);
}

public class LibraryQrCodesAppService : ILibraryQrCodesAppService
{
    private readonly ListLibraryQrCodesQuery _listQuery;
    private readonly GetLibraryQrCodeByIdQuery _getByIdQuery;
    private readonly ExportLibraryQrCodeUseCase _exportUseCase;
    private readonly UpdateLibraryQrCodeUseCase _updateUseCase;
    private readonly DeleteLibraryQrCodeUseCase _deleteUseCase;

    public LibraryQrCodesAppService(
        ListLibraryQrCodesQuery listQuery,
        GetLibraryQrCodeByIdQuery getByIdQuery,
        ExportLibraryQrCodeUseCase exportUseCase,
        UpdateLibraryQrCodeUseCase updateUseCase,
        DeleteLibraryQrCodeUseCase deleteUseCase)
    {
        _listQuery = listQuery;
        _getByIdQuery = getByIdQuery;
        _exportUseCase = exportUseCase;
        _updateUseCase = updateUseCase;
        _deleteUseCase = deleteUseCase;
    }

    public Task<AppResult<IReadOnlyList<QrCodeResponseDto>>> GetAllAsync(
        LibraryActorContext actor,
        CancellationToken cancellationToken)
        => _listQuery.ExecuteAsync(actor, cancellationToken);

    public Task<AppResult<QrCodeResponseDto>> GetByIdAsync(
        int id,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
        => _getByIdQuery.ExecuteAsync(id, actor, cancellationToken);

    public Task<AppResult<string>> ExportAsync(
        CreateQrCodeDto request,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
        => _exportUseCase.ExecuteAsync(request, actor, cancellationToken);

    public Task<AppResult<QrCodeResponseDto>> UpdateAsync(
        int id,
        UpdateQrCodeDto request,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
        => _updateUseCase.ExecuteAsync(id, request, actor, cancellationToken);

    public Task<AppResult> DeleteAsync(
        int id,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
        => _deleteUseCase.ExecuteAsync(id, actor, cancellationToken);
}
