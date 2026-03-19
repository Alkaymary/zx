using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Services;

public sealed class QrExportService : IQrExportService
{
    private readonly AppDbContext _context;

    public QrExportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QrExportResult> ExportAsync(
        LibraryAccount account,
        Library library,
        CreateQrCodeDto request,
        CancellationToken cancellationToken)
    {
        if (account.Status != RecordStatus.Active)
        {
            return QrExportResult.Failure("This library account is not active, so QR export is not allowed.");
        }

        if (library.Status != RecordStatus.Active)
        {
            return QrExportResult.Failure("This library is not active, so QR export is not allowed.");
        }

        if (!string.Equals(library.LibraryCode, request.LibraryCode, StringComparison.OrdinalIgnoreCase))
        {
            return QrExportResult.Failure("LibraryCode does not match the current library.");
        }

        var posDevice = await _context.PosDevices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.LibraryId == library.Id && x.PosCode == request.PosCode,
                cancellationToken);

        if (posDevice is null)
        {
            return QrExportResult.Failure("PosCode does not belong to the current library.");
        }

        if (posDevice.Status != PosDeviceStatus.Active)
        {
            return QrExportResult.Failure("This POS device is not active, so QR export is not allowed.");
        }

        if (!posDevice.IsActivated)
        {
            return QrExportResult.Failure("This POS device is not activated, so QR export is not allowed.");
        }

        if (posDevice.ActivatedByAccountId.HasValue && posDevice.ActivatedByAccountId.Value != account.Id)
        {
            return QrExportResult.Failure("This POS device is linked to another library account, so QR export is not allowed.");
        }

        var package = await _context.Packages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PackageCode == request.PackageCode,
                cancellationToken);

        if (package is null)
        {
            return QrExportResult.Failure("PackageCode does not exist.");
        }

        var qrReference = $"QR-{Guid.NewGuid():N}".ToUpperInvariant();
        var now = DateTime.UtcNow;

        var charge = new FinancialTransaction
        {
            LibraryId = library.Id,
            TransactionType = FinancialTransactionType.OpenInvoice,
            Amount = package.PriceIqd,
            PaidAmount = 0m,
            RemainingAmount = package.PriceIqd,
            Description = $"QR export charge for {package.PackageCode} / {request.StudentName} / {posDevice.PosCode} / {qrReference}",
            TransactionDate = now,
            DueDate = now,
            Status = FinancialTransactionStatus.Open,
            CreatedByLibraryAccountId = account.Id,
            UpdatedAt = now
        };

        var qrCode = new QrCode
        {
            LibraryId = library.Id,
            PackageId = package.Id,
            PosDeviceId = posDevice.Id,
            CreatedByLibraryAccountId = account.Id,
            FinancialTransaction = charge,
            QrReference = qrReference,
            StudentName = request.StudentName,
            StudentPhoneNumber = request.StudentPhoneNumber,
            QrPayload = QrPayloadBuilder.Build(
                library.LibraryCode,
                posDevice.PosCode,
                package.PackageCode,
                package.Name,
                request.StudentName,
                request.StudentPhoneNumber),
            UpdatedAt = now
        };

        _context.FinancialTransactions.Add(charge);
        _context.QrCodes.Add(qrCode);

        // Persist the charge and QR together so EF Core keeps them in one atomic SaveChanges transaction.
        await _context.SaveChangesAsync(cancellationToken);

        return QrExportResult.Success(qrReference);
    }
}
