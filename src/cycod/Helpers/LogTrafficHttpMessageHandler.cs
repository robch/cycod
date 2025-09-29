using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class LogTrafficHttpMessageHandler : HttpClientHandler
{
    // Regular expressions for sensitive data masking
    private static readonly Regex _authHeaderPattern = new(@"(Authorization|Bearer|api-key):\s*([^\s]+)", RegexOptions.IgnoreCase);
    private static readonly Regex _tokenPattern = new(@"(token|apiKey|password|secret|key)(\s*[=:]\s*)([^\s&,"";]+)", RegexOptions.IgnoreCase);
    
    public LogTrafficHttpMessageHandler()
    {
        base.AllowAutoRedirect = true;
        base.MaxAutomaticRedirections = 10;
        base.UseCookies = false;
        base.UseProxy = true;
        base.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await LogRequestAsync(request);
        var response = await base.SendAsync(request, cancellationToken);
        await LogResponseAsync(response);
        return response;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LogRequestAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
        var response = base.Send(request, cancellationToken);
        LogResponseAsync(response).ConfigureAwait(false).GetAwaiter().GetResult();
        return response;
    }

    private async Task LogRequestAsync(HttpRequestMessage request)
    {
        ConsoleHelpers.WriteDebugLine($"===== REQUEST: {request.Method} {request.RequestUri}");
        
        // Log request headers
        foreach (var header in request.Headers)
        {
            var headerName = header.Key;
            var headerValue = string.Join(", ", header.Value);
            
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
        if (request.Content != null)
        {
            using var dumpStream = new MemoryStream();
            await request.Content.CopyToAsync(dumpStream);
            dumpStream.Position = 0;
            var requestData = BinaryData.FromStream(dumpStream);

            var line = requestData.ToString().Replace("\n", "\\n").Replace("\r", "");
            if (!string.IsNullOrWhiteSpace(line))
            {
                // Mask sensitive data in request body
                string logLine = MaskSensitiveData(line);
                
                // Console debug output
                ConsoleHelpers.WriteDebugLine($"===== REQUEST BODY: {logLine}");
            }
        }
    }

    private async Task LogResponseAsync(HttpResponseMessage response)
    {
        // Log the response status code and reason phrase
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE: {response.StatusCode} ({response.ReasonPhrase})");
        
        // Always log HTTP responses at Info level (important for operations tracking)
        Logger.Info($"HTTP Response: {(int)response.StatusCode} {response.StatusCode} ({response.ReasonPhrase})");

        // Log each response header
        var sb = new StringBuilder();
        foreach (var header in response.Headers)
        {
            var headerValue = string.Join(", ", header.Value);
            
            // Mask sensitive values in headers
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                header.Key.Contains("Key", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Contains("Token", StringComparison.OrdinalIgnoreCase))
            {
                headerValue = "[REDACTED]";
            }
            
            sb.Append($"{header.Key}: {headerValue}\n");
        }
        
        var headers = sb.ToString().Replace("\n", "\\n").Replace("\r", "");
        if (!string.IsNullOrWhiteSpace(headers))
        {
            // Console debug output
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE HEADERS: {headers}");
        }

        // If the response has content, log it
        if (response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync();
            var line = content.Replace("\n", "\\n")?.Replace("\r", "");
            
            // Mask sensitive data in response body
            string logLine = MaskSensitiveData(line ?? string.Empty);
            
            // Console debug output
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE BODY: {logLine}");
            
            // If we got a non-success status code, log at Warning level
            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning($"HTTP request failed with status {(int)response.StatusCode} {response.StatusCode}: {logLine.Substring(0, Math.Min(500, logLine.Length))}");
            }
        }
    }
    
    /// <summary>
    /// Masks sensitive data such as tokens, keys and passwords in the text to be logged
    /// </summary>
    /// <param name="input">The input text to mask</param>
    /// <returns>The masked text</returns>
    private string MaskSensitiveData(string input)
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
