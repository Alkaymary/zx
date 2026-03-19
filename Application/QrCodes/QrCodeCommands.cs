using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Services;

namespace MyApi.Application.QrCodes;

public sealed class ExportLibraryQrCodeUseCase
{
    private readonly GetLibraryQrActorDataQuery _actorDataQuery;
    private readonly IQrExportService _qrExportService;

    public ExportLibraryQrCodeUseCase(
        GetLibraryQrActorDataQuery actorDataQuery,
        IQrExportService qrExportService)
    {
        _actorDataQuery = actorDataQuery;
        _qrExportService = qrExportService;
    }

    public async Task<AppResult<string>> ExecuteAsync(
        CreateQrCodeDto request,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
    {
        var contextData = await _actorDataQuery.ExecuteAsync(actor, cancellationToken);
        if (contextData is null)
        {
            return AppResult<string>.Unauthorized("Invalid library token.");
        }

        var result = await _qrExportService.ExportAsync(
            contextData.Account,
            contextData.Library,
            request,
            cancellationToken);

        return result.IsSuccess && !string.IsNullOrWhiteSpace(result.QrReference)
            ? AppResult<string>.Success(result.QrReference)
            : AppResult<string>.BadRequest(result.ErrorMessage ?? "QR export failed.");
    }
}

public sealed class UpdateLibraryQrCodeUseCase
{
    private readonly AppDbContext _context;
    private readonly GetLibraryQrActorDataQuery _actorDataQuery;
    private readonly GetLibraryQrCodeByIdQuery _getByIdQuery;

    public UpdateLibraryQrCodeUseCase(
        AppDbContext context,
        GetLibraryQrActorDataQuery actorDataQuery,
        GetLibraryQrCodeByIdQuery getByIdQuery)
    {
        _context = context;
        _actorDataQuery = actorDataQuery;
        _getByIdQuery = getByIdQuery;
    }

    public async Task<AppResult<QrCodeResponseDto>> ExecuteAsync(
        int id,
        UpdateQrCodeDto request,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
    {
        var contextData = await _actorDataQuery.ExecuteAsync(actor, cancellationToken);
        if (contextData is null)
        {
            return AppResult<QrCodeResponseDto>.Unauthorized("Invalid library token.");
        }

        var item = await _context.QrCodes
            .Include(x => x.Library)
            .Include(x => x.Package)
            .Include(x => x.PosDevice)
            .Include(x => x.CreatedByLibraryAccount)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.LibraryId == contextData.Library.Id,
                cancellationToken);

        if (item is null)
        {
            return AppResult<QrCodeResponseDto>.NotFound("QR export was not found.");
        }

        QrCodeMappings.ApplyUpdate(item, request);
        await _context.SaveChangesAsync(cancellationToken);

        return AppResult<QrCodeResponseDto>.Success(
            await _getByIdQuery.LoadRequiredAsync(item.Id, cancellationToken));
    }
}

public sealed class DeleteLibraryQrCodeUseCase
{
    private readonly AppDbContext _context;
    private readonly GetLibraryQrActorDataQuery _actorDataQuery;

    public DeleteLibraryQrCodeUseCase(
        AppDbContext context,
        GetLibraryQrActorDataQuery actorDataQuery)
    {
        _context = context;
        _actorDataQuery = actorDataQuery;
    }

    public async Task<AppResult> ExecuteAsync(
        int id,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
    {
        var contextData = await _actorDataQuery.ExecuteAsync(actor, cancellationToken);
        if (contextData is null)
        {
            return AppResult.Unauthorized("Invalid library token.");
        }

        var item = await _context.QrCodes
            .Include(x => x.FinancialTransaction)
            .ThenInclude(x => x.Settlements)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.LibraryId == contextData.Library.Id,
                cancellationToken);

        if (item is null)
        {
            return AppResult.NotFound("QR export was not found.");
        }

        if (item.FinancialTransaction.PaidAmount > 0 || item.FinancialTransaction.Settlements.Count > 0)
        {
            return AppResult.Conflict("Cannot delete a QR export that already has linked payments or settlements.");
        }

        _context.FinancialTransactions.Remove(item.FinancialTransaction);
        _context.QrCodes.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
