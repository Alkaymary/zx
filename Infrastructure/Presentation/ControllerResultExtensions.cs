using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Common.Results;

namespace MyApi.Infrastructure.Presentation;

public static class ControllerResultExtensions
{
    public static ActionResult ToActionResult(this ControllerBase controller, AppResult result)
    {
        return result.Status switch
        {
            AppResultStatus.Success => controller.Ok(),
            AppResultStatus.NoContent => controller.NoContent(),
            AppResultStatus.BadRequest => controller.BadRequest(result.Message),
            AppResultStatus.NotFound => controller.NotFound(result.Message),
            AppResultStatus.Conflict => controller.Conflict(result.Message),
            AppResultStatus.Unauthorized => controller.Unauthorized(result.Message),
            AppResultStatus.Forbid => controller.Forbid(),
            _ => controller.BadRequest(result.Message)
        };
    }

    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, AppResult<T> result)
    {
        return result.Status switch
        {
            AppResultStatus.Success => controller.Ok(result.Value),
            AppResultStatus.NoContent => controller.NoContent(),
            AppResultStatus.BadRequest => controller.BadRequest(result.Message),
            AppResultStatus.NotFound => controller.NotFound(result.Message),
            AppResultStatus.Conflict => controller.Conflict(result.Message),
            AppResultStatus.Unauthorized => controller.Unauthorized(result.Message),
            AppResultStatus.Forbid => controller.Forbid(),
            _ => controller.BadRequest(result.Message)
        };
    }
}
