using System;
using System.ClientModel.Primitives;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class LogTrafficEventPolicy : TrafficEventPolicy
{
    private static readonly Regex _authHeaderPattern = new(@"(Authorization|Bearer|api-key):\s*([^\s]+)", RegexOptions.IgnoreCase);
    private static readonly Regex _tokenPattern = new(@"(token|apiKey|password|secret|key)(\s*[=:]\s*)([^\s&,"";]+)", RegexOptions.IgnoreCase);
    
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
            string logHeaderValue = headerValue;
            if (headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                headerName.Contains("Key", StringComparison.OrdinalIgnoreCase) ||
                headerName.Contains("Token", StringComparison.OrdinalIgnoreCase))
            {
                logHeaderValue = "[REDACTED]";
            }
            
            ConsoleHelpers.WriteDebugLine($"===== REQUEST HEADER: {headerName}: {logHeaderValue}");
        }
        
        using MemoryStream dumpStream = new();
        request.Content?.WriteTo(dumpStream);
        dumpStream.Position = 0;
        BinaryData requestData = BinaryData.FromStream(dumpStream);

        var line = requestData.ToString().Replace("\n", "\\n").Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(line))
        {
            string logLine = MaskSensitiveData(line);
            ConsoleHelpers.WriteDebugLine($"===== REQUEST BODY: {logLine}");
        }
    }

    private static void LogResponse(PipelineResponse response)
    {
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE: {response.Status} ({response.ReasonPhrase})");
        Logger.Info($"HTTP Response: {response.Status} ({response.ReasonPhrase})");

        var sb = new StringBuilder();
        foreach ((string headerName, string headerValue) in response.Headers)
        {
            string logHeaderValue = headerValue;
            if (headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                headerName.Contains("Key", StringComparison.OrdinalIgnoreCase) ||
                headerName.Contains("Token", StringComparison.OrdinalIgnoreCase))
            {
                logHeaderValue = "[REDACTED]";
            }
            
            sb.Append($"{headerName}: {logHeaderValue}\n");
        }
        
        var headers = sb.ToString().Replace("\n", "\\n").Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(headers))
        {
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE HEADERS: {headers}");
        }

        var line = response.Content?.ToString()?.Replace("\n", "\\n")?.Replace("\r", "");
        if (line != null)
        {
            string logLine = MaskSensitiveData(line);
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE BODY: {logLine}");
            
            if (response.Status < 200 || response.Status >= 300)
            {
                Logger.Warning($"HTTP request failed with status {response.Status}: {logLine.Substring(0, Math.Min(500, logLine.Length))}");
            }
        }
    }
    
    private static string MaskSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var result = _authHeaderPattern.Replace(input, m => $"{m.Groups[1].Value}: [REDACTED]");
        result = _tokenPattern.Replace(result, m => $"{m.Groups[1].Value}{m.Groups[2].Value}[REDACTED]");
        
        return result;
    }
}
