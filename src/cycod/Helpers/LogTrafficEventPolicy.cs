using System;
using System.ClientModel.Primitives;
using System.Text;

public class LogTrafficEventPolicy : TrafficEventPolicy
{
    public LogTrafficEventPolicy()
    {
        var wrapLogRequest = TryCatchHelpers.NoThrowWrap((PipelineRequest request) => LogRequest(request))!;
        OnRequest += (sender, request) => wrapLogRequest(request);

        var wrapLogResponse = TryCatchHelpers.NoThrowWrap((PipelineResponse response) => LogResponse(response))!;
        OnResponse += (sender, response) => wrapLogResponse(response);
    }

    private static void LogRequest(PipelineRequest request)
    {
        ConsoleHelpers.WriteDebugLine($"===== REQUEST: {request.Method} {request.Uri}");

        foreach ((string headerName, string headerValue) in request.Headers)
        {
            ConsoleHelpers.WriteDebugLine($"===== REQUEST HEADER: {headerName}: {headerValue}");
        }

        using MemoryStream dumpStream = new();
        request.Content?.WriteTo(dumpStream);
        dumpStream.Position = 0;
        BinaryData requestData = BinaryData.FromStream(dumpStream);

        var line = requestData.ToString().Replace("\n", "\\n").Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(line))
        {
            ConsoleHelpers.WriteDebugLine($"===== REQUEST BODY: {line}");
        }
    }

    private static void LogResponse(PipelineResponse response)
    {
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE: {response.Status} ({response.ReasonPhrase})");

        var sb = new StringBuilder();
        foreach ((string headerName, string headerValue) in response.Headers)
        {
            sb.Append($"{headerName}: {headerValue}\n");
        }
        var headers = sb.ToString().Replace("\n", "\\n").Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(headers))
        {
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE HEADERS: {headers}");
        }

        var line = response.Content?.ToString()?.Replace("\n", "\\n")?.Replace("\r", "");
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE BODY: {line}");
    }
}
