using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class LogTrafficHttpMessageHandler : HttpClientHandler
{
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
            
            string logHeaderValue = headerValue;
            if (headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) || 
                headerName.Contains("Key", StringComparison.OrdinalIgnoreCase) ||
                headerName.Contains("Token", StringComparison.OrdinalIgnoreCase))
            {
                logHeaderValue = "[REDACTED]";
            }
            
            ConsoleHelpers.WriteDebugLine($"===== REQUEST HEADER: {headerName}: {logHeaderValue}");
        }

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
                string logLine = MaskSensitiveData(line);
                ConsoleHelpers.WriteDebugLine($"===== REQUEST BODY: {logLine}");
            }
        }
    }

    private async Task LogResponseAsync(HttpResponseMessage response)
    {
        // Log the response status code and reason phrase
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE: {response.StatusCode} ({response.ReasonPhrase})");
        Logger.Info($"HTTP Response: {(int)response.StatusCode} {response.StatusCode} ({response.ReasonPhrase})");

        // Log each response header
        var sb = new StringBuilder();
        foreach (var header in response.Headers)
        {
            var headerValue = string.Join(", ", header.Value);
            
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
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE HEADERS: {headers}");
        }

        // If the response has content, log it
        if (response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync();
            var line = content.Replace("\n", "\\n")?.Replace("\r", "");
            
            string logLine = MaskSensitiveData(line ?? string.Empty);
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE BODY: {logLine}");
            
            if (!response.IsSuccessStatusCode)
            {
                Logger.Warning($"HTTP request failed with status {(int)response.StatusCode} {response.StatusCode}: {logLine.Substring(0, Math.Min(500, logLine.Length))}");
            }
        }
    }
    
    private string MaskSensitiveData(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var result = _authHeaderPattern.Replace(input, m => $"{m.Groups[1].Value}: [REDACTED]");
        result = _tokenPattern.Replace(result, m => $"{m.Groups[1].Value}{m.Groups[2].Value}[REDACTED]");
        
        return result;
    }
}
