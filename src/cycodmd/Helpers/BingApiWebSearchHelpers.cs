using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

public static class BingApiWebSearchHelpers
{
    private static string endpoint = null;
    private static string apiKey = null;

    public static void ConfigureEndpoint(string apiEndpoint, string api_Key)
    {
        endpoint = apiEndpoint;
        apiKey = api_Key;
    }

    public static async Task<List<string>> GetWebSearchResultUrlsAsync(string query, int maxResults, List<Regex> excludeURLContainsPatternList, BrowserType browserType, bool interactive)
    {
        var fallbackToPlaywright = string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey);
        if (fallbackToPlaywright) return await PlaywrightHelpers.GetWebSearchResultUrlsAsync("bing", query, maxResults, excludeURLContainsPatternList, browserType, interactive);

        var urls = new List<string>();
        var httpClient = new HttpClient();
        int offset = 0;
        int count = 50; // Bing API allows up to 50 results per request

        while (urls.Count < maxResults)
        {
            var requestUri = $"{endpoint}?q={Uri.EscapeDataString(query)}&count={count}&offset={offset}";
            ConsoleHelpers.PrintDebugLine($"Sending request to Bing API: {requestUri}");

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var searchResults = doc.RootElement.GetProperty("webPages").GetProperty("value").EnumerateArray();

            foreach (var result in searchResults)
            {
                var url = result.GetProperty("url").GetString();
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

            offset += count;
        }

        return urls.Take(maxResults).ToList();
    }
}