using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net;
using System.Text.Json;

[McpServerToolType]
public sealed class GeolocationTools
{
    [McpServerTool, Description("Get geolocation information for the current machine's IP address.")]
    public static async Task<string> GetCurrentLocationInfo(HttpClient client)
    {
        try
        {
            // First get the current IP address (using a service that returns the public IP)
            using var ipResponse = await new HttpClient().GetAsync("https://api.ipify.org");
            ipResponse.EnsureSuccessStatusCode();
            var ip = await ipResponse.Content.ReadAsStringAsync();
            
            // Now use that IP to get geolocation info
            return await GetLocationInfoForIp(client, ip);
        }
        catch (Exception ex)
        {
            return $"Error retrieving geolocation: {ex.Message}";
        }
    }
    
    [McpServerTool, Description("Get geolocation information for a specific IP address.")]
    public static async Task<string> GetLocationInfoForIp(
        HttpClient client,
        [Description("The IP address to get geolocation information for.")] string ipAddress)
    {
        try
        {
            // Validate the IP address
            if (!IPAddress.TryParse(ipAddress, out _))
            {
                return "Error: Invalid IP address format.";
            }
            
            // Make request to the IP-API service
            using var jsonDocument = await client.ReadJsonDocumentAsync($"/json/{ipAddress}");
            var root = jsonDocument.RootElement;
            
            // Check if the request was successful
            var status = root.GetProperty("status").GetString();
            if (status != "success")
            {
                var message = root.TryGetProperty("message", out var msgElement) 
                    ? msgElement.GetString() 
                    : "Unknown error occurred";
                return $"Error: {message}";
            }
            
            // Extract relevant geolocation data
            var latitude = root.GetProperty("lat").GetDouble();
            var longitude = root.GetProperty("lon").GetDouble();
            var country = root.GetProperty("country").GetString();
            var region = root.GetProperty("regionName").GetString();
            var city = root.GetProperty("city").GetString();
            var isp = root.GetProperty("isp").GetString();
            var timezone = root.GetProperty("timezone").GetString();
            
            // Format the response
            return $"""
                IP Address: {ipAddress}
                Location: {city}, {region}, {country}
                Coordinates: {latitude}, {longitude}
                ISP: {isp}
                Timezone: {timezone}
                """;
        }
        catch (Exception ex)
        {
            return $"Error retrieving geolocation: {ex.Message}";
        }
    }
}