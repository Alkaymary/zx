using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyApi.Application.Auth;
using MyApi.Dtos;
using MyApi.Infrastructure.RateLimiting;
using MyApi.Infrastructure.Presentation;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
public class AuthController : ControllerBase
{
    private readonly IAdminAuthAppService _appService;

    public AuthController(IAdminAuthAppService appService)
    {
        _appService = appService;
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Login)]
    [HttpPost("admin-login")]
    public async Task<ActionResult<AuthResponseDto>> AdminLogin(AdminLoginRequestDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.AdminLoginAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicyNames.Refresh)]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.RefreshAsync(request, cancellationToken));
    }
}
