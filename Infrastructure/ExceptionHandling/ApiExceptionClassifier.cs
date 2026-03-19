using System.Net.Sockets;
using Npgsql;

namespace MyApi.Infrastructure.ExceptionHandling;

internal static class ApiExceptionClassifier
{
    public static bool IsRequestAborted(Exception exception, HttpContext context)
    {
        if (!context.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        return Enumerate(exception).Any(static item =>
            item is OperationCanceledException or IOException);
    }

    public static bool IsTransient(Exception exception, HttpContext context)
    {
        if (IsRequestAborted(exception, context))
        {
            return false;
        }

        return Enumerate(exception).Any(static item =>
            item is TimeoutException
            || item is TaskCanceledException
            || item is OperationCanceledException
            || item is SocketException
            || item is IOException
            || item is NpgsqlException npgsqlException && npgsqlException.IsTransient
            || item is InvalidOperationException invalidOperationException
                && invalidOperationException.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Exception> Enumerate(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            yield return current;
        }
    }
}
