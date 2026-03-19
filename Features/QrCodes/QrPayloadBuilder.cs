using System.Text.Json;

namespace MyApi.Services;

public static class QrPayloadBuilder
{
    public static string Build(
        string libraryCode,
        string posCode,
        string packageCode,
        string packageName,
        string studentName,
        string studentPhoneNumber)
    {
        var payload = new
        {
            libraryCode,
            posCode,
            packageCode,
            packageName,
            studentName,
            studentPhoneNumber
        };

        return JsonSerializer.Serialize(payload);
    }
}
