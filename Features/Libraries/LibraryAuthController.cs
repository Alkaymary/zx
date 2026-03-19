using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyApi.Application.Auth;
using MyApi.Dtos;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.RateLimiting;
using MyApi.Infrastructure.Security;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "LibraryAPI")]
public class LibraryAuthController : ControllerBase
{
    private readonly ILibraryAuthAppService _appService;

    public LibraryAuthController(ILibraryAuthAppService appService)
    {
        _appService = appService;
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Login)]
    [HttpPost("login")]
    public async Task<ActionResult<LibraryAuthResponseDto>> Login(LibraryLoginRequestDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.LoginAsync(request, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.RequireLibraryAccount)]
    [HttpGet("me")]
    public async Task<ActionResult<LibraryMeResponseDto>> GetMe(CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetMeAsync(User.ToLibraryActorContext(), cancellationToken));
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Refresh)]
    [HttpPost("refresh")]
    public async Task<ActionResult<LibraryAuthResponseDto>> Refresh(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.RefreshAsync(request, cancellationToken));
    }
}
