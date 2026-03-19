using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Contexts;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.QrCodes;

public sealed record LibraryQrActorData(LibraryAccount Account, Library Library);

public sealed class GetLibraryQrActorDataQuery
{
    private readonly AppDbContext _context;

    public GetLibraryQrActorDataQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LibraryQrActorData?> ExecuteAsync(
        LibraryActorContext actor,
        CancellationToken cancellationToken)
    {
        if (!actor.IsLibraryAccount || !actor.AccountId.HasValue)
        {
            return null;
        }

        var account = await _context.LibraryAccounts
            .Include(x => x.Library)
            .FirstOrDefaultAsync(x => x.Id == actor.AccountId.Value, cancellationToken);

        return account is null ? null : new LibraryQrActorData(account, account.Library);
    }
}

public sealed class ListLibraryQrCodesQuery
{
    private readonly AppDbContext _context;
    private readonly GetLibraryQrActorDataQuery _actorDataQuery;

    public ListLibraryQrCodesQuery(AppDbContext context, GetLibraryQrActorDataQuery actorDataQuery)
    {
        _context = context;
        _actorDataQuery = actorDataQuery;
    }

    public async Task<AppResult<IReadOnlyList<QrCodeResponseDto>>> ExecuteAsync(
        LibraryActorContext actor,
        CancellationToken cancellationToken)
    {
        var contextData = await _actorDataQuery.ExecuteAsync(actor, cancellationToken);
        if (contextData is null)
        {
            return AppResult<IReadOnlyList<QrCodeResponseDto>>.Unauthorized("Invalid library token.");
        }

        var items = await _context.QrCodes
            .AsNoTracking()
            .Where(x => x.LibraryId == contextData.Library.Id)
            .OrderByDescending(x => x.Id)
            .Select(QrCodeMappings.ToLibraryProjection())
            .ToListAsync(cancellationToken);

        return AppResult<IReadOnlyList<QrCodeResponseDto>>.Success(items);
    }
}

public sealed class GetLibraryQrCodeByIdQuery
{
    private readonly AppDbContext _context;
    private readonly GetLibraryQrActorDataQuery _actorDataQuery;

    public GetLibraryQrCodeByIdQuery(AppDbContext context, GetLibraryQrActorDataQuery actorDataQuery)
    {
        _context = context;
        _actorDataQuery = actorDataQuery;
    }

    public async Task<AppResult<QrCodeResponseDto>> ExecuteAsync(
        int id,
        LibraryActorContext actor,
        CancellationToken cancellationToken)
    {
        var contextData = await _actorDataQuery.ExecuteAsync(actor, cancellationToken);
        if (contextData is null)
        {
            return AppResult<QrCodeResponseDto>.Unauthorized("Invalid library token.");
        }

        var item = await _context.QrCodes
            .AsNoTracking()
            .Where(x => x.Id == id && x.LibraryId == contextData.Library.Id)
            .Select(QrCodeMappings.ToLibraryProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? AppResult<QrCodeResponseDto>.NotFound("QR export was not found.")
            : AppResult<QrCodeResponseDto>.Success(item);
    }

    public Task<QrCodeResponseDto> LoadRequiredAsync(int id, CancellationToken cancellationToken)
    {
        return _context.QrCodes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(QrCodeMappings.ToLibraryProjection())
            .FirstAsync(cancellationToken);
    }
}

public sealed class GetAdminQrLibraryMetricsQuery
{
    private readonly AppDbContext _context;

    public GetAdminQrLibraryMetricsQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LibraryQrMetricsDto> ExecuteAsync(int libraryId, CancellationToken cancellationToken)
    {
        var metrics = await _context.QrCodes
            .AsNoTracking()
            .Where(x => x.LibraryId == libraryId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalGenerated = g.Count(),
                TotalUsed = g.Count(x => x.Status == RecordStatus.Used)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new LibraryQrMetricsDto
        {
            LibraryId = libraryId,
            TotalGenerated = metrics?.TotalGenerated ?? 0,
            TotalUsed = metrics?.TotalUsed ?? 0
        };
    }
}

public sealed class ListAdminLibraryQrItemsQuery
{
    private readonly AppDbContext _context;

    public ListAdminLibraryQrItemsQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AdminQrCodeListItemDto>> ExecuteAsync(
        int libraryId,
        RecordStatus? status,
        CancellationToken cancellationToken)
    {
        var query = _context.QrCodes
            .AsNoTracking()
            .Where(x => x.LibraryId == libraryId);

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.Id)
            .Select(QrCodeMappings.ToAdminListItemProjection())
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetAdminQrByReferenceQuery
{
    private readonly AppDbContext _context;

    public GetAdminQrByReferenceQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<AdminQrCodeDetailsDto>> ExecuteAsync(string reference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return AppResult<AdminQrCodeDetailsDto>.BadRequest("Reference is required.");
        }

        var item = await _context.QrCodes
            .AsNoTracking()
            .Where(x => x.QrReference == reference.Trim())
            .Select(QrCodeMappings.ToAdminDetailsProjection())
            .FirstOrDefaultAsync(cancellationToken);

        return item is null
            ? AppResult<AdminQrCodeDetailsDto>.NotFound("QR export was not found.")
            : AppResult<AdminQrCodeDetailsDto>.Success(item);
    }
}
