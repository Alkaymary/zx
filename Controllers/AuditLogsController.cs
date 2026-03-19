using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.AuditLogs;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireAuditAccess)]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogsAppService _appService;

    public AuditLogsController(IAuditLogsAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Returns audit logs with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AuditLogPagedResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuditLogPagedResponseDto>> GetAll([FromQuery] AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    /// <summary>
    /// Returns one audit log with the full request and response payloads.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AuditLogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuditLogDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Returns summary metrics for the audit log dashboard.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AuditLogMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuditLogMetricsDto>> GetMetrics([FromQuery] AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetMetricsAsync(query, cancellationToken));
    }
}
