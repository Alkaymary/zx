using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.LibraryAccounts;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireLibraryRead)]
public class LibraryAccountsController : ControllerBase
{
    private readonly ILibraryAccountsAppService _appService;

    public LibraryAccountsController(ILibraryAccountsAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibraryAccountResponseDto>>> GetAll([FromQuery] LibraryAccountQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("by-query")]
    public async Task<ActionResult<IEnumerable<LibraryAccountResponseDto>>> GetByQuery([FromQuery] LibraryAccountQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibraryAccountResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("available-roles")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccountCreate)]
    public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetAvailableRoles(CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAvailableRolesAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccountCreate)]
    public async Task<ActionResult<LibraryAccountResponseDto>> Create(CreateLibraryAccountDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccountManagement)]
    public async Task<ActionResult<LibraryAccountResponseDto>> Update(int id, UpdateLibraryAccountDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPut("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccountManagement)]
    public async Task<ActionResult<LibraryAccountResponseDto>> UpdateByQuery(
        [FromQuery] LibraryAccountQueryDto query,
        [FromBody] UpdateLibraryAccountDto request,
        CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateByQueryAsync(query, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccountManagement)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }

    [HttpDelete("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccountManagement)]
    public async Task<IActionResult> DeleteByQuery([FromQuery] LibraryAccountQueryDto query, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteByQueryAsync(query, cancellationToken));
    }
}
