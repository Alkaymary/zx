using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.QrCodes;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "LibraryAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireLibraryAccount)]
public class QrCodesController : ControllerBase
{
    private readonly ILibraryQrCodesAppService _appService;

    public QrCodesController(ILibraryQrCodesAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Returns QR exports created for the current library.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QrCodeResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QrCodeResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetAllAsync(User.ToLibraryActorContext(), cancellationToken));
    }

    /// <summary>
    /// Returns one QR export by id for the current library.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QrCodeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QrCodeResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, User.ToLibraryActorContext(), cancellationToken));
    }

    /// <summary>
    /// Exports a new QR code, creates a linked financial charge, and returns the reference code as plain text.
    /// </summary>
    [HttpPost("export")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<string>> Export(CreateQrCodeDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.ExportAsync(request, User.ToLibraryActorContext(), cancellationToken));
    }

    /// <summary>
    /// Updates the student data and status of a QR export for the current library.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(QrCodeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QrCodeResponseDto>> Update(int id, UpdateQrCodeDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, User.ToLibraryActorContext(), cancellationToken));
    }

    /// <summary>
    /// Deletes a QR export for the current library when its linked financial charge has not been settled or paid.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, User.ToLibraryActorContext(), cancellationToken));
    }
}
