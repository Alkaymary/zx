using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.PosDevices;

public sealed class ValidatePosDeviceDependencies
{
    private readonly AppDbContext _context;

    public ValidatePosDeviceDependencies(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<PosDeviceResponseDto>?> ExecuteAsync(
        int? libraryId,
        int? activatedByAccountId,
        string posCode,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (libraryId.HasValue && !await _context.Libraries.AnyAsync(x => x.Id == libraryId.Value, cancellationToken))
        {
            return AppResult<PosDeviceResponseDto>.BadRequest("LibraryId does not exist.");
        }

        if (activatedByAccountId.HasValue && !await _context.LibraryAccounts.AnyAsync(x => x.Id == activatedByAccountId.Value, cancellationToken))
        {
            return AppResult<PosDeviceResponseDto>.BadRequest("ActivatedByAccountId does not exist.");
        }

        var posCodeExists = await _context.PosDevices
            .AnyAsync(x => x.PosCode == posCode && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);
        if (posCodeExists)
        {
            return AppResult<PosDeviceResponseDto>.Conflict("PosCode already exists.");
        }

        return null;
    }
}

public sealed class CreatePosDeviceUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidatePosDeviceDependencies _validator;

    public CreatePosDeviceUseCase(AppDbContext context, ValidatePosDeviceDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<PosDeviceResponseDto>> ExecuteAsync(
        CreatePosDeviceDto request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ExecuteAsync(
            request.LibraryId,
            request.ActivatedByAccountId,
            request.PosCode,
            null,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        var device = new PosDevice
        {
            LibraryId = request.LibraryId,
            PosCode = request.PosCode,
            SerialNumber = request.SerialNumber,
            DeviceModel = request.DeviceModel,
            DeviceVendor = request.DeviceVendor,
            Status = request.Status,
            IsActivated = request.IsActivated,
            ActivatedByAccountId = request.ActivatedByAccountId,
            ActivationToken = request.ActivationToken,
            ActivatedAt = request.ActivatedAt,
            LastAuthenticatedAt = request.LastAuthenticatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PosDevices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);
        await PosDeviceMappings.LoadReferencesAsync(_context, device, cancellationToken);
        return AppResult<PosDeviceResponseDto>.Success(PosDeviceMappings.ToDto(device));
    }
}

public sealed class UpdatePosDeviceUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidatePosDeviceDependencies _validator;

    public UpdatePosDeviceUseCase(AppDbContext context, ValidatePosDeviceDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<PosDeviceResponseDto>> ExecuteAsync(
        int id,
        UpdatePosDeviceDto request,
        CancellationToken cancellationToken)
    {
        var device = await _context.PosDevices
            .Include(x => x.Library)
            .Include(x => x.ActivatedByAccount)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (device is null)
        {
            return AppResult<PosDeviceResponseDto>.NotFound("POS device was not found.");
        }

        var validation = await _validator.ExecuteAsync(
            request.LibraryId,
            request.ActivatedByAccountId,
            request.PosCode,
            id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        PosDeviceMappings.ApplyUpdate(device, request);
        await _context.SaveChangesAsync(cancellationToken);
        await PosDeviceMappings.LoadReferencesAsync(_context, device, cancellationToken);
        return AppResult<PosDeviceResponseDto>.Success(PosDeviceMappings.ToDto(device));
    }
}

public sealed class UpdatePosDeviceByQueryUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidatePosDeviceDependencies _validator;

    public UpdatePosDeviceByQueryUseCase(AppDbContext context, ValidatePosDeviceDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<PosDeviceResponseDto>> ExecuteAsync(
        PosDeviceQueryDto query,
        UpdatePosDeviceDto request,
        CancellationToken cancellationToken)
    {
        var matches = await PosDeviceQueryBuilder.Build(_context, query, trackChanges: true)
            .Include(x => x.Library)
            .Include(x => x.ActivatedByAccount)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult<PosDeviceResponseDto>.NotFound("No POS device matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult<PosDeviceResponseDto>.Conflict("Query matched more than one POS device. Narrow the filters before updating.");
        }

        var device = matches[0];
        var validation = await _validator.ExecuteAsync(
            request.LibraryId,
            request.ActivatedByAccountId,
            request.PosCode,
            device.Id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        PosDeviceMappings.ApplyUpdate(device, request);
        await _context.SaveChangesAsync(cancellationToken);
        await PosDeviceMappings.LoadReferencesAsync(_context, device, cancellationToken);
        return AppResult<PosDeviceResponseDto>.Success(PosDeviceMappings.ToDto(device));
    }
}

public sealed class DeletePosDeviceUseCase
{
    private readonly AppDbContext _context;

    public DeletePosDeviceUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var device = await _context.PosDevices.FindAsync([id], cancellationToken);
        if (device is null)
        {
            return AppResult.NotFound("POS device was not found.");
        }

        _context.PosDevices.Remove(device);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}

public sealed class DeletePosDeviceByQueryUseCase
{
    private readonly AppDbContext _context;

    public DeletePosDeviceByQueryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(PosDeviceQueryDto query, CancellationToken cancellationToken)
    {
        var matches = await PosDeviceQueryBuilder.Build(_context, query, trackChanges: true)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult.NotFound("No POS device matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult.Conflict("Query matched more than one POS device. Narrow the filters before deleting.");
        }

        _context.PosDevices.Remove(matches[0]);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
