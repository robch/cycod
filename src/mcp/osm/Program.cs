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
    var client = new HttpClient() { BaseAddress = new Uri("https://nominatim.openstreetmap.org") };
    // Set a user agent as required by Nominatim's usage policy
    // Set a user agent with contact information as required by Nominatim's usage policy
    client.DefaultRequestHeaders.UserAgent.ParseAdd("cycod-mcp-osm/1.0 (your-email@example.com)");
    // Add rate limiting via HttpClient configuration
    client.Timeout = TimeSpan.FromSeconds(10);
    return client;
});

await builder.Build().RunAsync();