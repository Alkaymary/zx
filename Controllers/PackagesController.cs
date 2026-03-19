using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Packages;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequirePackageRead)]
public class PackagesController : ControllerBase
{
    private readonly IPackagesAppService _appService;

    public PackagesController(IPackagesAppService appService)
    {
        _appService = appService;
    }

    /// <summary>
    /// Returns all packages.
    /// </summary>
    [ProducesResponseType(typeof(IEnumerable<PackageResponseDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackageResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Returns one package by id.
    /// </summary>
    [ProducesResponseType(typeof(PackageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PackageResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Creates a new package.
    /// </summary>
    [ProducesResponseType(typeof(PackageResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequirePackageWrite)]
    public async Task<ActionResult<PackageResponseDto>> Create(CreatePackageDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, User.ToAdminActorContext(), cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing package.
    /// </summary>
    [ProducesResponseType(typeof(PackageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequirePackageWrite)]
    public async Task<ActionResult<PackageResponseDto>> Update(int id, UpdatePackageDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes a package by id.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequirePackageWrite)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }
}
