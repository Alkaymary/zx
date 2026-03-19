using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Roles;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireRoleManagement)]
public class RolesController : ControllerBase
{
    private readonly IRoleAppService _appService;

    public RolesController(IRoleAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponseDto>> Create(CreateRoleDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoleResponseDto>> Update(int id, UpdateRoleDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }
}
