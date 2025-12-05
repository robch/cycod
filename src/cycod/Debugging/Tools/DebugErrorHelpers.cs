using System.Text.Json;

namespace Cycod.Debugging.Tools;

static class DebugErrorHelpers
{
    public static string Error(string code, string message)
    {
        var err = new ErrorResponse { Error = new ErrorBody { Code = code, Message = message } };
        return JsonSerializer.Serialize(err);
    }
}
