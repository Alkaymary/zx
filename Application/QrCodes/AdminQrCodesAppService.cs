using MyApi.Application.Common.Results;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.QrCodes;

public interface IAdminQrCodesAppService
{
    Task<LibraryQrMetricsDto> GetLibraryMetricsAsync(int libraryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminQrCodeListItemDto>> GetLibraryItemsAsync(int libraryId, RecordStatus? status, CancellationToken cancellationToken);
    Task<AppResult<AdminQrCodeDetailsDto>> GetByReferenceAsync(string reference, CancellationToken cancellationToken);
}

public class AdminQrCodesAppService : IAdminQrCodesAppService
{
    private readonly GetAdminQrLibraryMetricsQuery _metricsQuery;
    private readonly ListAdminLibraryQrItemsQuery _itemsQuery;
    private readonly GetAdminQrByReferenceQuery _byReferenceQuery;

    public AdminQrCodesAppService(
        GetAdminQrLibraryMetricsQuery metricsQuery,
        ListAdminLibraryQrItemsQuery itemsQuery,
        GetAdminQrByReferenceQuery byReferenceQuery)
    {
        _metricsQuery = metricsQuery;
        _itemsQuery = itemsQuery;
        _byReferenceQuery = byReferenceQuery;
    }

    public Task<LibraryQrMetricsDto> GetLibraryMetricsAsync(int libraryId, CancellationToken cancellationToken)
        => _metricsQuery.ExecuteAsync(libraryId, cancellationToken);

    public Task<IReadOnlyList<AdminQrCodeListItemDto>> GetLibraryItemsAsync(
        int libraryId,
        RecordStatus? status,
        CancellationToken cancellationToken)
        => _itemsQuery.ExecuteAsync(libraryId, status, cancellationToken);

    public Task<AppResult<AdminQrCodeDetailsDto>> GetByReferenceAsync(string reference, CancellationToken cancellationToken)
        => _byReferenceQuery.ExecuteAsync(reference, cancellationToken);
}
