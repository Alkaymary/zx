using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.PosDevices;

internal static class PosDeviceMappings
{
    public static Expression<Func<PosDevice, PosDeviceResponseDto>> ToProjection()
    {
        return x => new PosDeviceResponseDto
        {
            Id = x.Id,
            LibraryId = x.LibraryId,
            LibraryName = x.Library != null ? x.Library.LibraryName : null,
            PosCode = x.PosCode,
            SerialNumber = x.SerialNumber,
            DeviceModel = x.DeviceModel,
            DeviceVendor = x.DeviceVendor,
            Status = x.Status,
            IsActivated = x.IsActivated,
            ActivatedByAccountId = x.ActivatedByAccountId,
            ActivatedByUsername = x.ActivatedByAccount != null ? x.ActivatedByAccount.Username : null,
            ActivationToken = x.ActivationToken,
            ActivatedAt = x.ActivatedAt,
            LastAuthenticatedAt = x.LastAuthenticatedAt,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static void ApplyUpdate(PosDevice device, UpdatePosDeviceDto request)
    {
        device.LibraryId = request.LibraryId;
        device.PosCode = request.PosCode;
        device.SerialNumber = request.SerialNumber;
        device.DeviceModel = request.DeviceModel;
        device.DeviceVendor = request.DeviceVendor;
        device.Status = request.Status;
        device.IsActivated = request.IsActivated;
        device.ActivatedByAccountId = request.ActivatedByAccountId;
        device.ActivationToken = request.ActivationToken;
        device.ActivatedAt = request.ActivatedAt;
        device.LastAuthenticatedAt = request.LastAuthenticatedAt;
        device.UpdatedAt = DateTime.UtcNow;
    }

    public static async Task LoadReferencesAsync(AppDbContext context, PosDevice device, CancellationToken cancellationToken)
    {
        await context.Entry(device).Reference(x => x.Library).LoadAsync(cancellationToken);
        await context.Entry(device).Reference(x => x.ActivatedByAccount).LoadAsync(cancellationToken);
    }

    public static PosDeviceResponseDto ToDto(PosDevice device) => new()
    {
        Id = device.Id,
        LibraryId = device.LibraryId,
        LibraryName = device.Library?.LibraryName,
        PosCode = device.PosCode,
        SerialNumber = device.SerialNumber,
        DeviceModel = device.DeviceModel,
        DeviceVendor = device.DeviceVendor,
        Status = device.Status,
        IsActivated = device.IsActivated,
        ActivatedByAccountId = device.ActivatedByAccountId,
        ActivatedByUsername = device.ActivatedByAccount?.Username,
        ActivationToken = device.ActivationToken,
        ActivatedAt = device.ActivatedAt,
        LastAuthenticatedAt = device.LastAuthenticatedAt,
        CreatedAt = device.CreatedAt,
        UpdatedAt = device.UpdatedAt
    };
}
