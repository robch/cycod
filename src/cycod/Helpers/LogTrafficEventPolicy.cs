using System;
using System.ClientModel.Primitives;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class LogTrafficEventPolicy : TrafficEventPolicy
{
    // Regular expressions for sensitive data masking
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
        // Log the request method and URI to both console and file
        ConsoleHelpers.WriteDebugLine($"===== REQUEST: {request.Method} {request.Uri}");
        
        // Log request headers
        foreach ((string headerName, string headerValue) in request.Headers)
        {
            // Mask sensitive values in headers
            string logHeaderValue = headerValue;
            if (headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                headerName.Contains("Key", StringComparison.OrdinalIgnoreCase) ||
                headerName.Contains("Token", StringComparison.OrdinalIgnoreCase))
            {
                logHeaderValue = "[REDACTED]";
            }
            
            // Console debug output
            ConsoleHelpers.WriteDebugLine($"===== REQUEST HEADER: {headerName}: {logHeaderValue}");
        }
        
        // No additional logging needed for sensitive headers note

        // If the request has content, log it
        using MemoryStream dumpStream = new();
        request.Content?.WriteTo(dumpStream);
        dumpStream.Position = 0;
        BinaryData requestData = BinaryData.FromStream(dumpStream);

        var line = requestData.ToString().Replace("\n", "\\n").Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(line))
        {
            // Mask sensitive data in request body
            string logLine = MaskSensitiveData(line);
            
            // Console debug output
            ConsoleHelpers.WriteDebugLine($"===== REQUEST BODY: {logLine}");
        }
    }

    private static void LogResponse(PipelineResponse response)
    {
        // Log the response status code and reason phrase
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE: {response.Status} ({response.ReasonPhrase})");
        
        // Always log HTTP responses at Info level (important for operations tracking)
        Logger.Info($"HTTP Response: {response.Status} ({response.ReasonPhrase})");

        // Log each response header
        var sb = new StringBuilder();
        foreach ((string headerName, string headerValue) in response.Headers)
        {
            // Mask sensitive values in headers
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
            // Console debug output
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE HEADERS: {headers}");
        }

        // If the response has content, log it
        var line = response.Content?.ToString()?.Replace("\n", "\\n")?.Replace("\r", "");
        if (line != null)
        {
            // Mask sensitive data in response body
            string logLine = MaskSensitiveData(line);
            
            // Console debug output
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE BODY: {logLine}");
            
            // If we got a non-success status code, log at Warning level
            if (response.Status < 200 || response.Status >= 300)
            {
                Logger.Warning($"HTTP request failed with status {response.Status}: {logLine.Substring(0, Math.Min(500, logLine.Length))}");
            }
        }
    }
    
    /// <summary>
    /// Masks sensitive data such as tokens, keys and passwords in the text to be logged
    /// </summary>
    /// <param name="input">The input text to mask</param>
    /// <returns>The masked text</returns>
    private static string MaskSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Replace Authorization headers
        var result = _authHeaderPattern.Replace(input, m => $"{m.Groups[1].Value}: [REDACTED]");
        
        // Replace tokens/keys/passwords in JSON or query strings
        result = _tokenPattern.Replace(result, m => $"{m.Groups[1].Value}{m.Groups[2].Value}[REDACTED]");
        
        return result;
    }
}
