using System.Text;

public class LogTrafficHttpMessageHandler : HttpClientHandler
{
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
        // Log the request method and URI
        ConsoleHelpers.WriteDebugLine($"===== REQUEST: {request.Method} {request.RequestUri}");

        // Log each request header
        foreach (var header in request.Headers)
        {
            var headerName = header.Key;
            var headerValue = string.Join(", ", header.Value);
            ConsoleHelpers.WriteDebugLine($"===== REQUEST HEADER: {headerName}: {headerValue}");
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
                ConsoleHelpers.WriteDebugLine($"===== REQUEST BODY: {line}");
            }
        }
    }

    private async Task LogResponseAsync(HttpResponseMessage response)
    {
        // Log the response status code and reason phrase
        ConsoleHelpers.WriteDebugLine($"===== RESPONSE: {response.StatusCode} ({response.ReasonPhrase})");

        // Log each response header
        var sb = new StringBuilder();
        foreach (var header in response.Headers)
        {
            sb.Append($"{header.Key}: {string.Join(", ", header.Value)}\n");
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
            ConsoleHelpers.WriteDebugLine($"===== RESPONSE BODY: {line}");
        }
    }
}
