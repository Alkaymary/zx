using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Services;

public interface IQrExportService
{
    Task<QrExportResult> ExportAsync(
        LibraryAccount account,
        Library library,
        CreateQrCodeDto request,
        CancellationToken cancellationToken);
}

public sealed record QrExportResult
{
    public bool IsSuccess { get; init; }

    public string? QrReference { get; init; }

    public string? ErrorMessage { get; init; }

    public static QrExportResult Success(string qrReference) => new()
    {
        IsSuccess = true,
        QrReference = qrReference
    };

    public static QrExportResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
