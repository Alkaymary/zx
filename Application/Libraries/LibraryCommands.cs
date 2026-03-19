using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.Libraries;

public sealed class CreateLibraryUseCase
{
    private readonly AppDbContext _context;

    public CreateLibraryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryResponseDto>> ExecuteAsync(CreateLibraryDto request, CancellationToken cancellationToken)
    {
        if (await _context.Libraries.AnyAsync(x => x.LibraryCode == request.LibraryCode, cancellationToken))
        {
            return AppResult<LibraryResponseDto>.Conflict("Library code already exists.");
        }

        var library = new Library
        {
            LibraryCode = request.LibraryCode,
            LibraryName = request.LibraryName,
            OwnerName = request.OwnerName,
            OwnerPhone = request.OwnerPhone,
            OwnerPhone2 = request.OwnerPhone2,
            Address = request.Address,
            Province = request.Province,
            City = request.City,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = request.Status,
            CreditLimit = request.CreditLimit,
            CurrentBalance = request.CurrentBalance,
            Notes = request.Notes,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Libraries.Add(library);
        await _context.SaveChangesAsync(cancellationToken);
        await LibraryMappings.LoadReferencesAsync(_context, library, cancellationToken);

        return AppResult<LibraryResponseDto>.Success(LibraryMappings.ToDto(library));
    }
}

public sealed class UpdateLibraryUseCase
{
    private readonly AppDbContext _context;

    public UpdateLibraryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryResponseDto>> ExecuteAsync(
        int id,
        UpdateLibraryDto request,
        CancellationToken cancellationToken)
    {
        var library = await _context.Libraries
            .Include(x => x.Accounts)
            .Include(x => x.PosDevices)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (library is null)
        {
            return AppResult<LibraryResponseDto>.NotFound("Library was not found.");
        }

        if (await _context.Libraries.AnyAsync(x => x.Id != id && x.LibraryCode == request.LibraryCode, cancellationToken))
        {
            return AppResult<LibraryResponseDto>.Conflict("Library code already exists.");
        }

        LibraryMappings.ApplyUpdate(library, request);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult<LibraryResponseDto>.Success(LibraryMappings.ToDto(library));
    }
}

public sealed class UpdateLibraryByQueryUseCase
{
    private readonly AppDbContext _context;

    public UpdateLibraryByQueryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryResponseDto>> ExecuteAsync(
        LibraryQueryDto query,
        UpdateLibraryDto request,
        CancellationToken cancellationToken)
    {
        var matches = await LibraryQueryBuilder.Build(_context, query, trackChanges: true)
            .Include(x => x.Accounts)
            .Include(x => x.PosDevices)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult<LibraryResponseDto>.NotFound("No library matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult<LibraryResponseDto>.Conflict("Query matched more than one library. Narrow the filters before updating.");
        }

        var library = matches[0];
        if (await _context.Libraries.AnyAsync(x => x.Id != library.Id && x.LibraryCode == request.LibraryCode, cancellationToken))
        {
            return AppResult<LibraryResponseDto>.Conflict("Library code already exists.");
        }

        LibraryMappings.ApplyUpdate(library, request);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult<LibraryResponseDto>.Success(LibraryMappings.ToDto(library));
    }
}

public sealed class DeleteLibraryUseCase
{
    private readonly AppDbContext _context;

    public DeleteLibraryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var library = await _context.Libraries.FindAsync([id], cancellationToken);
        if (library is null)
        {
            return AppResult.NotFound("Library was not found.");
        }

        _context.Libraries.Remove(library);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}

public sealed class DeleteLibraryByQueryUseCase
{
    private readonly AppDbContext _context;

    public DeleteLibraryByQueryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(LibraryQueryDto query, CancellationToken cancellationToken)
    {
        var matches = await LibraryQueryBuilder.Build(_context, query, trackChanges: true)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult.NotFound("No library matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult.Conflict("Query matched more than one library. Narrow the filters before deleting.");
        }

        _context.Libraries.Remove(matches[0]);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
