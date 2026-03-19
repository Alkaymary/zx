namespace MyApi.Infrastructure;

public static class SqlSearchPattern
{
    public static string Contains(string value)
    {
        return $"%{EscapeLike(value.Trim())}%";
    }

    public static string Exact(string value)
    {
        return EscapeLike(value.Trim());
    }

    private static string EscapeLike(string value)
    {
        return value
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
    }
}
