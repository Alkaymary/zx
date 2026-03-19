using System.Linq.Expressions;
using MyApi.Dtos;
using MyApi.Models;
using MyApi.Services;

namespace MyApi.Application.QrCodes;

public static class QrCodeMappings
{
    public static Expression<Func<QrCode, QrCodeResponseDto>> ToLibraryProjection()
    {
        return x => new QrCodeResponseDto
        {
            Id = x.Id,
            QrReference = x.QrReference,
            LibraryId = x.LibraryId,
            LibraryCode = x.Library.LibraryCode,
            LibraryName = x.Library.LibraryName,
            PackageId = x.PackageId,
            PackageCode = x.Package.PackageCode,
            PackageName = x.Package.Name,
            PosDeviceId = x.PosDeviceId,
            PosCode = x.PosDevice.PosCode,
            StudentName = x.StudentName,
            StudentPhoneNumber = x.StudentPhoneNumber,
            QrPayload = x.QrPayload,
            FinancialTransactionId = x.FinancialTransactionId,
            ChargeAmountIqd = x.FinancialTransaction.Amount,
            FinancialStatus = x.FinancialTransaction.Status,
            RemainingAmountIqd = x.FinancialTransaction.RemainingAmount,
            Status = x.Status,
            CreatedByLibraryAccountId = x.CreatedByLibraryAccountId,
            CreatedByUsername = x.CreatedByLibraryAccount.Username,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static Expression<Func<QrCode, AdminQrCodeListItemDto>> ToAdminListItemProjection()
    {
        return x => new AdminQrCodeListItemDto
        {
            Id = x.Id,
            QrReference = x.QrReference,
            LibraryId = x.LibraryId,
            LibraryCode = x.Library.LibraryCode,
            LibraryName = x.Library.LibraryName,
            PosCode = x.PosDevice.PosCode,
            PackageCode = x.Package.PackageCode,
            PackageName = x.Package.Name,
            StudentName = x.StudentName,
            StudentPhoneNumber = x.StudentPhoneNumber,
            Status = x.Status,
            ChargeAmountIqd = x.FinancialTransaction.Amount,
            RemainingAmountIqd = x.FinancialTransaction.RemainingAmount,
            CreatedAt = x.CreatedAt
        };
    }

    public static Expression<Func<QrCode, AdminQrCodeDetailsDto>> ToAdminDetailsProjection()
    {
        return x => new AdminQrCodeDetailsDto
        {
            Id = x.Id,
            QrReference = x.QrReference,
            LibraryId = x.LibraryId,
            LibraryCode = x.Library.LibraryCode,
            LibraryName = x.Library.LibraryName,
            PosDeviceId = x.PosDeviceId,
            PosCode = x.PosDevice.PosCode,
            PackageId = x.PackageId,
            PackageCode = x.Package.PackageCode,
            PackageName = x.Package.Name,
            PackagePriceIqd = x.Package.PriceIqd,
            StudentName = x.StudentName,
            StudentPhoneNumber = x.StudentPhoneNumber,
            QrPayload = x.QrPayload,
            Status = x.Status,
            CreatedByLibraryAccountId = x.CreatedByLibraryAccountId,
            CreatedByUsername = x.CreatedByLibraryAccount.Username,
            CreatedByFullName = x.CreatedByLibraryAccount.FullName,
            FinancialTransactionId = x.FinancialTransactionId,
            FinancialTransactionType = x.FinancialTransaction.TransactionType,
            ChargeAmountIqd = x.FinancialTransaction.Amount,
            PaidAmountIqd = x.FinancialTransaction.PaidAmount,
            RemainingAmountIqd = x.FinancialTransaction.RemainingAmount,
            FinancialStatus = x.FinancialTransaction.Status,
            FinancialDescription = x.FinancialTransaction.Description,
            FinancialTransactionDate = x.FinancialTransaction.TransactionDate,
            FinancialDueDate = x.FinancialTransaction.DueDate,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static void ApplyUpdate(QrCode item, UpdateQrCodeDto request)
    {
        item.StudentName = request.StudentName;
        item.StudentPhoneNumber = request.StudentPhoneNumber;
        item.Status = request.Status;
        item.QrPayload = QrPayloadBuilder.Build(
            item.Library.LibraryCode,
            item.PosDevice.PosCode,
            item.Package.PackageCode,
            item.Package.Name,
            item.StudentName,
            item.StudentPhoneNumber);
        item.UpdatedAt = DateTime.UtcNow;
    }
}
