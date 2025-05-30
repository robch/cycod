using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using CycodMcp.OpenStreetMap;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<OpenStreetMapTools>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(_ =>
{
    var handler = new HttpClientHandler { AllowAutoRedirect = true, UseCookies = false };
    var client = new HttpClient(handler) { BaseAddress = new Uri("https://nominatim.openstreetmap.org") };
    client.DefaultRequestHeaders.UserAgent.ParseAdd("cycod-mcp-osm/1.0 (your-email@example.com)");
    client.Timeout = TimeSpan.FromSeconds(10);
    return client;
});

await builder.Build().RunAsync();