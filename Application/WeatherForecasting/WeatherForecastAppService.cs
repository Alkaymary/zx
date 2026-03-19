using MyApi.Application.Common.Results;
using MyApi.Models;

namespace MyApi.Application.WeatherForecasting;

public interface IWeatherForecastAppService
{
    Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken);
    Task<AppResult<WeatherForecast>> GetByIdAsync(int id, CancellationToken cancellationToken);
}

public class WeatherForecastAppService : IWeatherForecastAppService
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<IReadOnlyList<WeatherForecast>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var forecast = Enumerable.Range(1, 5)
            .Select(index => CreateForecast(index))
            .ToArray();

        return Task.FromResult<IReadOnlyList<WeatherForecast>>(forecast);
    }

    public Task<AppResult<WeatherForecast>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (id <= 0)
        {
            return Task.FromResult(AppResult<WeatherForecast>.BadRequest("Id must be greater than 0."));
        }

        return Task.FromResult(AppResult<WeatherForecast>.Success(CreateForecast(id)));
    }

    private static WeatherForecast CreateForecast(int dayOffset)
    {
        return new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(dayOffset)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };
    }
}
