using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Libraries;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireLibraryRead)]
public class LibrariesController : ControllerBase
{
    private readonly ILibrariesAppService _appService;

    public LibrariesController(ILibrariesAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibraryResponseDto>>> GetAll([FromQuery] LibraryQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, User.ToAdminActorContext(), cancellationToken));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<LibraryResponseDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _appService.SearchAsync(q, limit, User.ToAdminActorContext(), cancellationToken));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<LibraryStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetStatsAsync(cancellationToken));
    }

    [HttpGet("by-query")]
    public async Task<ActionResult<IEnumerable<LibraryResponseDto>>> GetByQuery([FromQuery] LibraryQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, User.ToAdminActorContext(), cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibraryResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, User.ToAdminActorContext(), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryWrite)]
    public async Task<ActionResult<LibraryResponseDto>> Create(CreateLibraryDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryWrite)]
    public async Task<ActionResult<LibraryResponseDto>> Update(int id, UpdateLibraryDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPut("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryWrite)]
    public async Task<ActionResult<LibraryResponseDto>> UpdateByQuery(
        [FromQuery] LibraryQueryDto query,
        [FromBody] UpdateLibraryDto request,
        CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateByQueryAsync(query, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }

    [HttpDelete("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryDelete)]
    public async Task<IActionResult> DeleteByQuery([FromQuery] LibraryQueryDto query, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteByQueryAsync(query, cancellationToken));
    }
}
