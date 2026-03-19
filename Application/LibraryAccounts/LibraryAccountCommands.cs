using Microsoft.EntityFrameworkCore;
using MyApi.Application.Common.Results;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Models;

namespace MyApi.Application.LibraryAccounts;

public sealed class ValidateLibraryAccountDependencies
{
    private readonly AppDbContext _context;

    public ValidateLibraryAccountDependencies(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult<LibraryAccountResponseDto>?> ExecuteAsync(
        int libraryId,
        int roleId,
        string username,
        int? currentId,
        CancellationToken cancellationToken)
    {
        if (!await _context.Libraries.AnyAsync(x => x.Id == libraryId, cancellationToken))
        {
            return AppResult<LibraryAccountResponseDto>.BadRequest("LibraryId does not exist.");
        }

        if (!await _context.Roles.AnyAsync(x => x.Id == roleId && x.GuardName == GuardName.Office, cancellationToken))
        {
            return AppResult<LibraryAccountResponseDto>.BadRequest("RoleId does not exist or is not an office role.");
        }

        var usernameExists = await _context.LibraryAccounts
            .AnyAsync(x => x.Username == username && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);
        if (usernameExists)
        {
            return AppResult<LibraryAccountResponseDto>.Conflict("Username already exists.");
        }

        return null;
    }
}

public sealed class CreateLibraryAccountUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidateLibraryAccountDependencies _validator;

    public CreateLibraryAccountUseCase(AppDbContext context, ValidateLibraryAccountDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<LibraryAccountResponseDto>> ExecuteAsync(
        CreateLibraryAccountDto request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ExecuteAsync(
            request.LibraryId,
            request.RoleId,
            request.Username,
            null,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        var account = new LibraryAccount
        {
            LibraryId = request.LibraryId,
            RoleId = request.RoleId,
            FullName = request.FullName,
            Username = request.Username,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = request.PasswordHash,
            Status = request.Status,
            LastLoginAt = request.LastLoginAt,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LibraryAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        await LibraryAccountMappings.LoadReferencesAsync(_context, account, cancellationToken);

        return AppResult<LibraryAccountResponseDto>.Success(LibraryAccountMappings.ToDto(account));
    }
}

public sealed class UpdateLibraryAccountUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidateLibraryAccountDependencies _validator;

    public UpdateLibraryAccountUseCase(AppDbContext context, ValidateLibraryAccountDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<LibraryAccountResponseDto>> ExecuteAsync(
        int id,
        UpdateLibraryAccountDto request,
        CancellationToken cancellationToken)
    {
        var account = await _context.LibraryAccounts
            .Include(x => x.Library)
            .Include(x => x.Role)
            .Include(x => x.ActivatedDevices)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (account is null)
        {
            return AppResult<LibraryAccountResponseDto>.NotFound("Library account was not found.");
        }

        var validation = await _validator.ExecuteAsync(
            request.LibraryId,
            request.RoleId,
            request.Username,
            id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        LibraryAccountMappings.ApplyUpdate(account, request);
        await _context.SaveChangesAsync(cancellationToken);
        await LibraryAccountMappings.LoadReferencesAsync(_context, account, cancellationToken);
        return AppResult<LibraryAccountResponseDto>.Success(LibraryAccountMappings.ToDto(account));
    }
}

public sealed class UpdateLibraryAccountByQueryUseCase
{
    private readonly AppDbContext _context;
    private readonly ValidateLibraryAccountDependencies _validator;

    public UpdateLibraryAccountByQueryUseCase(AppDbContext context, ValidateLibraryAccountDependencies validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<AppResult<LibraryAccountResponseDto>> ExecuteAsync(
        LibraryAccountQueryDto query,
        UpdateLibraryAccountDto request,
        CancellationToken cancellationToken)
    {
        var matches = await LibraryAccountQueryBuilder.Build(_context, query, trackChanges: true)
            .Include(x => x.Library)
            .Include(x => x.Role)
            .Include(x => x.ActivatedDevices)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult<LibraryAccountResponseDto>.NotFound("No library account matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult<LibraryAccountResponseDto>.Conflict("Query matched more than one library account. Narrow the filters before updating.");
        }

        var account = matches[0];
        var validation = await _validator.ExecuteAsync(
            request.LibraryId,
            request.RoleId,
            request.Username,
            account.Id,
            cancellationToken);

        if (validation is not null)
        {
            return validation;
        }

        LibraryAccountMappings.ApplyUpdate(account, request);
        await _context.SaveChangesAsync(cancellationToken);
        await LibraryAccountMappings.LoadReferencesAsync(_context, account, cancellationToken);
        return AppResult<LibraryAccountResponseDto>.Success(LibraryAccountMappings.ToDto(account));
    }
}

public sealed class DeleteLibraryAccountUseCase
{
    private readonly AppDbContext _context;

    public DeleteLibraryAccountUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var account = await _context.LibraryAccounts.FindAsync([id], cancellationToken);
        if (account is null)
        {
            return AppResult.NotFound("Library account was not found.");
        }

        _context.LibraryAccounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}

public sealed class DeleteLibraryAccountByQueryUseCase
{
    private readonly AppDbContext _context;

    public DeleteLibraryAccountByQueryUseCase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppResult> ExecuteAsync(LibraryAccountQueryDto query, CancellationToken cancellationToken)
    {
        var matches = await LibraryAccountQueryBuilder.Build(_context, query, trackChanges: true)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return AppResult.NotFound("No library account matched the query filters.");
        }

        if (matches.Count > 1)
        {
            return AppResult.Conflict("Query matched more than one library account. Narrow the filters before deleting.");
        }

        _context.LibraryAccounts.Remove(matches[0]);
        await _context.SaveChangesAsync(cancellationToken);
        return AppResult.NoContent();
    }
}
