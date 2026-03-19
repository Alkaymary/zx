using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.AdminUsers;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireAdminRead)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUsersAppService _appService;

    public AdminUsersController(IAdminUsersAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Returns admin users with optional filters.
    /// </summary>
    [ProducesResponseType(typeof(IEnumerable<AdminUserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminUserResponseDto>>> GetAll([FromQuery] AdminUserQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    /// <summary>
    /// Returns admin users using query string filters.
    /// </summary>
    [ProducesResponseType(typeof(IEnumerable<AdminUserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("by-query")]
    public async Task<ActionResult<IEnumerable<AdminUserResponseDto>>> GetByQuery([FromQuery] AdminUserQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    /// <summary>
    /// Returns one admin user by id.
    /// </summary>
    [ProducesResponseType(typeof(AdminUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Creates a new admin user.
    /// </summary>
    [ProducesResponseType(typeof(AdminUserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminManagement)]
    public async Task<ActionResult<AdminUserResponseDto>> Create(CreateAdminUserDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing admin user.
    /// </summary>
    [ProducesResponseType(typeof(AdminUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminManagement)]
    public async Task<ActionResult<AdminUserResponseDto>> Update(int id, UpdateAdminUserDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Updates one admin user selected by query string filters.
    /// </summary>
    [ProducesResponseType(typeof(AdminUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminManagement)]
    public async Task<ActionResult<AdminUserResponseDto>> UpdateByQuery([FromQuery] AdminUserQueryDto query, [FromBody] UpdateAdminUserDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateByQueryAsync(query, request, cancellationToken));
    }

    /// <summary>
    /// Deletes an admin user by id.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminManagement)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }

    /// <summary>
    /// Deletes one admin user selected by query string filters.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpDelete("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdminManagement)]
    public async Task<IActionResult> DeleteByQuery([FromQuery] AdminUserQueryDto query, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteByQueryAsync(query, cancellationToken));
    }
}
