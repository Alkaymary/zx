namespace MyApi.Application.Common.Results;

public enum AppResultStatus
{
    Success,
    NoContent,
    BadRequest,
    NotFound,
    Conflict,
    Unauthorized,
    Forbid
}

public class AppResult
{
    protected AppResult(AppResultStatus status, string? message = null)
    {
        Status = status;
        Message = message;
    }

    public AppResultStatus Status { get; }

    public string? Message { get; }

    public bool IsSuccess => Status == AppResultStatus.Success || Status == AppResultStatus.NoContent;

    public static AppResult Success() => new(AppResultStatus.Success);

    public static AppResult NoContent() => new(AppResultStatus.NoContent);

    public static AppResult BadRequest(string message) => new(AppResultStatus.BadRequest, message);

    public static AppResult NotFound(string message) => new(AppResultStatus.NotFound, message);

    public static AppResult Conflict(string message) => new(AppResultStatus.Conflict, message);

    public static AppResult Unauthorized(string message) => new(AppResultStatus.Unauthorized, message);

    public static AppResult Forbid(string? message = null) => new(AppResultStatus.Forbid, message);
}

public sealed class AppResult<T> : AppResult
{
    private AppResult(AppResultStatus status, T? value = default, string? message = null)
        : base(status, message)
    {
        Value = value;
    }

    public T? Value { get; }

    public static AppResult<T> Success(T value) => new(AppResultStatus.Success, value);

    public new static AppResult<T> BadRequest(string message) => new(AppResultStatus.BadRequest, default, message);

    public new static AppResult<T> NotFound(string message) => new(AppResultStatus.NotFound, default, message);

    public new static AppResult<T> Conflict(string message) => new(AppResultStatus.Conflict, default, message);

    public new static AppResult<T> Unauthorized(string message) => new(AppResultStatus.Unauthorized, default, message);

    public new static AppResult<T> Forbid(string? message = null) => new(AppResultStatus.Forbid, default, message);
}
