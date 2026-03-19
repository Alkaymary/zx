using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.PosDevices;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequirePosRead)]
public class PosDevicesController : ControllerBase
{
    private readonly IPosDevicesAppService _appService;

    public PosDevicesController(IPosDevicesAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PosDeviceResponseDto>>> GetAll([FromQuery] PosDeviceQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("by-query")]
    public async Task<ActionResult<IEnumerable<PosDeviceResponseDto>>> GetByQuery([FromQuery] PosDeviceQueryDto query, CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PosDeviceResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequirePosWrite)]
    public async Task<ActionResult<PosDeviceResponseDto>> Create(CreatePosDeviceDto request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequirePosWrite)]
    public async Task<ActionResult<PosDeviceResponseDto>> Update(int id, UpdatePosDeviceDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPut("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequirePosWrite)]
    public async Task<ActionResult<PosDeviceResponseDto>> UpdateByQuery([FromQuery] PosDeviceQueryDto query, [FromBody] UpdatePosDeviceDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.UpdateByQueryAsync(query, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.RequirePosWrite)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteAsync(id, cancellationToken));
    }

    [HttpDelete("by-query")]
    [Authorize(Policy = AuthorizationPolicies.RequirePosWrite)]
    public async Task<IActionResult> DeleteByQuery([FromQuery] PosDeviceQueryDto query, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.DeleteByQueryAsync(query, cancellationToken));
    }
}
