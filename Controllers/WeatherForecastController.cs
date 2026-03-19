using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.WeatherForecasting;
using MyApi.Infrastructure.Presentation;
using MyApi.Infrastructure.Security;
using MyApi.Models;

namespace MyApi.Controllers;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "AdminAPI")]
[Authorize(Policy = AuthorizationPolicies.RequireAdminRead)]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastAppService _appService;

    public WeatherForecastController(IWeatherForecastAppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _appService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WeatherForecast>> GetById(int id, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await _appService.GetByIdAsync(id, cancellationToken));
    }
}
