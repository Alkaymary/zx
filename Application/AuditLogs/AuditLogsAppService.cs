using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MyApi.Data;
using MyApi.Dtos;
using MyApi.Infrastructure;
using MyApi.Models;
using MyApi.Application.Common.Results;

namespace MyApi.Application.AuditLogs;

public interface IAuditLogsAppService
{
    Task<AuditLogPagedResponseDto> GetAllAsync(AuditLogQueryDto query, CancellationToken cancellationToken);
    Task<AppResult<AuditLogDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<AuditLogMetricsDto> GetMetricsAsync(AuditLogQueryDto query, CancellationToken cancellationToken);
}

public class AuditLogsAppService : IAuditLogsAppService
{
    private readonly AppDbContext _context;

    public AuditLogsAppService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLogPagedResponseDto> GetAllAsync(AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 200);
        var auditQuery = BuildQuery(query);

        var totalCount = await auditQuery.CountAsync(cancellationToken);
        var items = await auditQuery
            .OrderByDescending(x => x.OperationDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToListItem())
            .ToListAsync(cancellationToken);

        return new AuditLogPagedResponseDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }

    public async Task<AppResult<AuditLogDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var item = await _context.AuditLogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AuditLogDetailDto
            {
                Id = x.Id,
                OperationDate = x.OperationDate,
                TraceIdentifier = x.TraceIdentifier,
                AccountType = x.AccountType,
                AccountId = x.AccountId,
                AccountUsername = x.AccountUsername,
                RoleCode = x.RoleCode,
                Endpoint = x.Endpoint,
                QueryString = x.QueryString,
                ActionName = x.ActionName,
                HttpMethod = x.HttpMethod,
                StatusCode = x.StatusCode,
                Status = x.Status,
                SecurityLevel = x.SecurityLevel,
                IpAddress = x.IpAddress,
                DurationMs = x.DurationMs,
                RequestPayload = x.RequestPayload,
                ResponsePayload = x.ResponsePayload
            })
            .SingleOrDefaultAsync(cancellationToken);

        return item is null
            ? AppResult<AuditLogDetailDto>.NotFound("Audit log was not found.")
            : AppResult<AuditLogDetailDto>.Success(item);
    }

    public async Task<AuditLogMetricsDto> GetMetricsAsync(AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        var auditQuery = BuildQuery(query);
        var today = DateTime.UtcNow.Date;

        return await auditQuery
            .GroupBy(_ => 1)
            .Select(g => new AuditLogMetricsDto
            {
                TotalLogs = g.Count(),
                TodayLogs = g.Count(x => x.OperationDate >= today),
                SuccessfulLogs = g.Count(x => x.Status == AuditLogStatus.Succeeded),
                ClientErrors = g.Count(x => x.Status == AuditLogStatus.ClientError),
                BlockedLogs = g.Count(x => x.Status == AuditLogStatus.Blocked),
                ServerErrors = g.Count(x => x.Status == AuditLogStatus.ServerError),
                PublicLogs = g.Count(x => x.SecurityLevel == AuditSecurityLevel.Public),
                ProtectedLogs = g.Count(x => x.SecurityLevel == AuditSecurityLevel.Protected),
                PrivilegedLogs = g.Count(x => x.SecurityLevel == AuditSecurityLevel.Privileged),
                AdminLogs = g.Count(x => x.AccountType == "AdminUser"),
                LibraryLogs = g.Count(x => x.AccountType == "LibraryAccount")
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? new AuditLogMetricsDto();
    }

    private IQueryable<AuditLog> BuildQuery(AuditLogQueryDto query)
    {
        var auditQuery = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (query.Id.HasValue)
        {
            auditQuery = auditQuery.Where(x => x.Id == query.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = SqlSearchPattern.Contains(query.Search);
            auditQuery = auditQuery.Where(x =>
                EF.Functions.ILike(x.Endpoint, term, "\\")
                || EF.Functions.ILike(x.ActionName, term, "\\")
                || (x.AccountUsername != null && EF.Functions.ILike(x.AccountUsername, term, "\\"))
                || (x.IpAddress != null && EF.Functions.ILike(x.IpAddress, term, "\\"))
                || EF.Functions.ILike(x.TraceIdentifier, term, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.Endpoint))
        {
            var endpoint = SqlSearchPattern.Contains(query.Endpoint);
            auditQuery = auditQuery.Where(x => EF.Functions.ILike(x.Endpoint, endpoint, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.HttpMethod))
        {
            var method = SqlSearchPattern.Exact(query.HttpMethod);
            auditQuery = auditQuery.Where(x => EF.Functions.ILike(x.HttpMethod, method, "\\"));
        }

        if (query.Status.HasValue)
        {
            auditQuery = auditQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.SecurityLevel.HasValue)
        {
            auditQuery = auditQuery.Where(x => x.SecurityLevel == query.SecurityLevel.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.AccountUsername))
        {
            var account = SqlSearchPattern.Contains(query.AccountUsername);
            auditQuery = auditQuery.Where(x => x.AccountUsername != null && EF.Functions.ILike(x.AccountUsername, account, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(query.IpAddress))
        {
            var ip = SqlSearchPattern.Contains(query.IpAddress);
            auditQuery = auditQuery.Where(x => x.IpAddress != null && EF.Functions.ILike(x.IpAddress, ip, "\\"));
        }

        if (query.FromDate.HasValue)
        {
            var from = DateTime.SpecifyKind(query.FromDate.Value, DateTimeKind.Utc);
            auditQuery = auditQuery.Where(x => x.OperationDate >= from);
        }

        if (query.ToDate.HasValue)
        {
            var to = DateTime.SpecifyKind(query.ToDate.Value, DateTimeKind.Utc);
            auditQuery = auditQuery.Where(x => x.OperationDate <= to);
        }

        return auditQuery;
    }

    private static Expression<Func<AuditLog, AuditLogListItemDto>> MapToListItem()
    {
        return x => new AuditLogListItemDto
        {
            Id = x.Id,
            OperationDate = x.OperationDate,
            TraceIdentifier = x.TraceIdentifier,
            AccountType = x.AccountType,
            AccountId = x.AccountId,
            AccountUsername = x.AccountUsername,
            RoleCode = x.RoleCode,
            Endpoint = x.Endpoint,
            QueryString = x.QueryString,
            ActionName = x.ActionName,
            HttpMethod = x.HttpMethod,
            StatusCode = x.StatusCode,
            Status = x.Status,
            SecurityLevel = x.SecurityLevel,
            IpAddress = x.IpAddress,
            DurationMs = x.DurationMs
        };
    }
}
