using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure;
using MyApi.Models;

namespace MyApi.Application.PosDevices;

internal static class PosDeviceQueryBuilder
{
    public static IQueryable<PosDevice> Build(AppDbContext context, PosDeviceQueryDto query, bool trackChanges = false)
    {
        IQueryable<PosDevice> deviceQuery = context.PosDevices;
        if (!trackChanges)
        {
            deviceQuery = deviceQuery.AsNoTracking();
        }

        if (query.Id.HasValue)
        {
            deviceQuery = deviceQuery.Where(x => x.Id == query.Id.Value);
        }

        if (query.LibraryId.HasValue)
        {
            deviceQuery = deviceQuery.Where(x => x.LibraryId == query.LibraryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.LibraryName))
        {
            var libraryName = SqlSearchPattern.Contains(query.LibraryName);
            deviceQuery = deviceQuery.Where(x => x.Library != null && EF.Functions.ILike(x.Library.LibraryName, libraryName, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.PosCode))
        {
            var posCode = SqlSearchPattern.Contains(query.PosCode);
            deviceQuery = deviceQuery.Where(x => EF.Functions.ILike(x.PosCode, posCode, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.SerialNumber))
        {
            var serialNumber = SqlSearchPattern.Contains(query.SerialNumber);
            deviceQuery = deviceQuery.Where(x => x.SerialNumber != null && EF.Functions.ILike(x.SerialNumber, serialNumber, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.DeviceModel))
        {
            var deviceModel = SqlSearchPattern.Contains(query.DeviceModel);
            deviceQuery = deviceQuery.Where(x => x.DeviceModel != null && EF.Functions.ILike(x.DeviceModel, deviceModel, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.DeviceVendor))
        {
            var deviceVendor = SqlSearchPattern.Contains(query.DeviceVendor);
            deviceQuery = deviceQuery.Where(x => x.DeviceVendor != null && EF.Functions.ILike(x.DeviceVendor, deviceVendor, "\\"));
        }

        if (query.Status.HasValue)
        {
            deviceQuery = deviceQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.IsActivated.HasValue)
        {
            deviceQuery = deviceQuery.Where(x => x.IsActivated == query.IsActivated.Value);
        }

        if (query.ActivatedByAccountId.HasValue)
        {
            deviceQuery = deviceQuery.Where(x => x.ActivatedByAccountId == query.ActivatedByAccountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ActivatedByUsername))
        {
            var activatedByUsername = SqlSearchPattern.Contains(query.ActivatedByUsername);
            deviceQuery = deviceQuery.Where(x => x.ActivatedByAccount != null && EF.Functions.ILike(x.ActivatedByAccount.Username, activatedByUsername, "\\"));
        }

        return deviceQuery;
    }
}

public sealed class ListPosDevicesQuery
{
    private readonly AppDbContext _context;

    public ListPosDevicesQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PosDeviceResponseDto>> ExecuteAsync(
        PosDeviceQueryDto query,
        CancellationToken cancellationToken)
    {
        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200);

        return await PosDeviceQueryBuilder.Build(_context, query)
            .OrderBy(x => x.Id)
            .Take(limit)
            .Select(PosDeviceMappings.ToProjection())
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetPosDeviceByIdQuery
{
    private readonly AppDbContext _context;

    public GetPosDeviceByIdQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<PosDeviceResponseDto>> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var device = await _context.PosDevices
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(PosDeviceMappings.ToProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return device is null
            ? AppResult<PosDeviceResponseDto>.NotFound("POS device was not found.")
            : AppResult<PosDeviceResponseDto>.Success(device);
    }
}
