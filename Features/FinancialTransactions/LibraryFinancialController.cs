using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.LibraryFinancial;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "LibraryAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireLibraryAccount)]
public class LibraryFinancialController : ControllerBase
{
    private readonly ILibraryFinancialAppService _appService;

    public LibraryFinancialController(ILibraryFinancialAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Returns the financial summary for the current library account.
    /// </summary>
    [HttpGet("me/summary")]
    [ProducesResponseType(typeof(LibraryFinancialSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LibraryFinancialSummaryDto>> GetMySummary(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetMySummaryAsync(User.ToLibraryActorContext(), cancellationToken));
    }

    /// <summary>
    /// Returns the financial statement for the current library account.
    /// </summary>
    [HttpGet("me/statement")]
    [ProducesResponseType(typeof(LibraryFinancialStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LibraryFinancialStatementDto>> GetMyStatement(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetMyStatementAsync(User.ToLibraryActorContext(), cancellationToken));
    }
}
