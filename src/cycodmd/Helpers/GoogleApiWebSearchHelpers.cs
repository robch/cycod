using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

public static class GoogleApiWebSearchHelpers
{
    private static string endpoint = null;
    private static string apiKey = null;
    private static string engineId = null;

    public static void ConfigureEndpoint(string apiEndpoint, string api_Key, string searchEngineId)
    {
        endpoint = apiEndpoint;
        apiKey = api_Key;
        engineId = searchEngineId;
    }

    public static async Task<List<string>> GetWebSearchResultUrlsAsync(string query, int maxResults, List<Regex> excludeURLContainsPatternList, BrowserType browserType, bool interactive)
    {
        var fallbackToPlaywright = string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey);
        if (fallbackToPlaywright) return await PlaywrightHelpers.GetWebSearchResultUrlsAsync("google", query, maxResults, excludeURLContainsPatternList, browserType, interactive);

        var urls = new List<string>();
        var httpClient = new HttpClient();
        int start = 1; // Google Custom Search JSON API uses start parameter for pagination

        while (urls.Count < maxResults)
        {
            var requestUri = $"{endpoint}?q={Uri.EscapeDataString(query)}&key={apiKey}&cx={engineId}&start={start}";
            ConsoleHelpers.PrintDebugLine($"Sending request to Google API: {requestUri}");

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var searchResults = doc.RootElement.GetProperty("items").EnumerateArray();

            foreach (var result in searchResults)
            {
                var url = result.GetProperty("link").GetString();
                if (!excludeURLContainsPatternList.Any(pattern => pattern.IsMatch(url)))
                {
                    urls.Add(url);
                    if (urls.Count == maxResults)
                    {
                        break;
                    }
                }
            }

            if (!searchResults.Any())
            {
                ConsoleHelpers.PrintDebugLine($"No more search results found, json response: {jsonResponse}");
                break;
            }

            start += searchResults.Count();
        }

        return urls.Take(maxResults).ToList();
    }
}
