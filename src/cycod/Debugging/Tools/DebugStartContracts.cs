using System.Text.Json.Serialization;

namespace Cycod.Debugging.Tools;

public class DebugStartRequest
{
    [JsonPropertyName("programPath")] public string ProgramPath { get; set; } = string.Empty;
    [JsonPropertyName("stopAtEntry")] public bool? StopAtEntry { get; set; }
    [JsonPropertyName("adapterPreference")] public string? AdapterPreference { get; set; }
}

public class DebugStartResponse
{
    [JsonPropertyName("sessionId")] public string SessionId { get; set; } = string.Empty;
    [JsonPropertyName("capabilities")] public object Capabilities { get; set; } = new { };
    [JsonPropertyName("state")] public object State { get; set; } = new { };
}

public class ErrorResponse
{
    [JsonPropertyName("error")] public ErrorBody Error { get; set; } = new();
}

public class ErrorBody
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}
