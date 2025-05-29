using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace CycodMcp.OpenStreetMap
{
    [McpServerToolType]
    public sealed class OpenStreetMapTools
    {
        [McpServerTool, Description("Convert a place name to latitude and longitude coordinates.")]
        public static async Task<string> GeocodePlace(
            HttpClient client,
            [Description("The name of the place or address to geocode.")] string placeName,
            [Description("Maximum number of results to return (default: 1).")] int limit = 1)
        {
            if (string.IsNullOrWhiteSpace(placeName))
            {
                return "Error: Place name cannot be empty.";
            }

            var encodedPlaceName = Uri.EscapeDataString(placeName);
            var requestUri = $"/search?q={encodedPlaceName}&format=json&limit={limit}";
            
            using var jsonDocument = await client.ReadJsonDocumentAsync(requestUri);
            var results = jsonDocument.RootElement.EnumerateArray();
            
            if (!results.Any())
            {
                return "No locations found matching the provided place name.";
            }

            var sb = new StringBuilder();
            
            foreach (var result in results)
            {
                var displayName = result.GetProperty("display_name").GetString() ?? "Unknown Location";
                var latitude = result.GetProperty("lat").GetString() ?? "Unknown";
                var longitude = result.GetProperty("lon").GetString() ?? "Unknown";
                var type = result.GetProperty("type").GetString() ?? "Unknown";
                var importance = result.TryGetProperty("importance", out var importanceElement) 
                    ? importanceElement.GetDouble().ToString("F2") 
                    : "N/A";

                sb.AppendLine($"Location: {displayName}");
                sb.AppendLine($"Coordinates: {latitude}, {longitude}");
                sb.AppendLine($"Type: {type}");
                sb.AppendLine($"Importance: {importance}");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }
    }
}