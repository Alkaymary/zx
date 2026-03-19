using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.QrCodes;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;
using MyApi.Models;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireQrRead)]
public class AdminQrCodesController : ControllerBase
{
    private readonly IAdminQrCodesAppService _appService;

    public AdminQrCodesController(IAdminQrCodesAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Returns QR metrics for one library for admin dashboard usage.
    /// </summary>
    [HttpGet("library/{libraryId:int}/metrics")]
    [ProducesResponseType(typeof(LibraryQrMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LibraryQrMetricsDto>> GetLibraryMetrics(int libraryId, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetLibraryMetricsAsync(libraryId, cancellationToken));
    }

    /// <summary>
    /// Returns QR records for one library for admin dashboard usage.
    /// </summary>
    [HttpGet("library/{libraryId:int}/items")]
    [ProducesResponseType(typeof(IEnumerable<AdminQrCodeListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AdminQrCodeListItemDto>>> GetLibraryItems(
        int libraryId,
        [FromQuery] RecordStatus? status,
        CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetLibraryItemsAsync(libraryId, status, cancellationToken));
    }

    /// <summary>
    /// Returns full QR export details by reference code for admin usage.
    /// </summary>
    [HttpGet("by-reference")]
    [ProducesResponseType(typeof(AdminQrCodeDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminQrCodeDetailsDto>> GetByReference([FromQuery] string reference, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByReferenceAsync(reference, cancellationToken));
    }
}
