using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;

class PlaywrightHelpers
{
    public static int RunCli(string[] args)
    {
        return Microsoft.Playwright.Program.Main(args);
    }

    public static async Task<List<string>> GetWebSearchResultUrlsAsync(string searchEngine, string query, int maxResults, List<Regex> excludeURLContainsPatternList, BrowserType browserType, bool interactive)
    {
        // Initialize Playwright
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await GetBrowser(browserType, interactive, playwright);
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Determine the search URL based on the chosen search engine
        var searchUrl = searchEngine switch
        {
            "bing" => $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}",
            "google" => $"https://www.google.com/search?q={Uri.EscapeDataString(query)}",
            "yahoo" => $"https://search.yahoo.com/search?p={Uri.EscapeDataString(query)}",
            "duckduckgo" => $"https://duckduckgo.com/?q={Uri.EscapeDataString(query)}",
            _ => throw new ArgumentException($"Unsupported search engine: {searchEngine}")
        };
        ConsoleHelpers.PrintDebugLine($"Navigating to {searchUrl}");

        // Navigate to the URL
        await page.GotoAsync(searchUrl);

        // Extract search result URLs
        var urls = searchEngine switch
        {
            "bing" => await ExtractBingSearchResults(page, maxResults, excludeURLContainsPatternList, interactive),
            "google" => await ExtractGoogleSearchResults(page, maxResults, excludeURLContainsPatternList, interactive),
            "yahoo" => await ExtractYahooSearchResults(page, maxResults, excludeURLContainsPatternList, interactive),
            "duckduckgo" => await ExtractDuckDuckGoSearchResults(page, maxResults, excludeURLContainsPatternList, interactive),
            _ => throw new ArgumentException($"Unsupported search engine: {searchEngine}")
        };
        
        return urls;
    }

    public static async Task<(string, string)> GetPageAndTitle(string url, bool stripHtml, string saveToFolder, BrowserType browserType, bool interactive)
    {
        // Initialize Playwright
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await GetBrowser(browserType, interactive, playwright);
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Navigate to the URL
        await page.GotoAsync(url);

        // Fetch the page content and title
        var content = await FetchPageContent(page, url, stripHtml, saveToFolder);
        var title = await page.TitleAsync();

        // Return the content and title
        return (content, title);
    }

    private static async Task<List<string>> ExtractGoogleSearchResults(IPage page, int maxResults, List<Regex> excludeURLContainsPatternList, bool interactive)
    {
        var urls = new List<string>();
        while (urls.Count < maxResults)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var elements = await page.QuerySelectorAllAsync("div#search a[href]");
            foreach (var element in elements)
            {
                var href = await element.GetAttributeAsync("href");
                if (href != null && href.StartsWith("http") && !href.Contains("google"))
                {
                    if (!urls.Contains(href) && !excludeURLContainsPatternList.Any(pattern => pattern.IsMatch(href)))
                    {
                        urls.Add(href);
                    }
                }
                if (urls.Count >= maxResults) break;
            }

            if (urls.Count >= maxResults) break;

            var clicked = await TryClickWithRetriesAsync(page, "a#pnnext");
            if (!clicked) break;
        }

        if (urls.Count == 0)
        {
            var content = await FetchPageContentWithRetries(page);
            var title = await page.TitleAsync();
            ConsoleHelpers.PrintDebugLine($"No search results found, page title: {title}\n\n{content}");
            if (interactive) Task.Delay(10000).Wait();
        }

        return urls.Take(maxResults).ToList();
    }

    private static async Task<List<string>> ExtractBingSearchResults(IPage page, int maxResults, List<Regex> excludeURLContainsPatternList, bool interactive)
    {
        var urls = new List<string>();
        while (urls.Count < maxResults)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var elements = await page.QuerySelectorAllAsync("li.b_algo a[href]");
            foreach (var element in elements)
            {
                var href = await element.GetAttributeAsync("href");
                if (href != null && href.StartsWith("http"))
                {
                    if (!urls.Contains(href) && !excludeURLContainsPatternList.Any(pattern => pattern.IsMatch(href)))
                    {
                        urls.Add(href);
                    }
                }
                if (urls.Count >= maxResults) break;
            }

            if (urls.Count >= maxResults) break;

            var clicked = await TryClickWithRetriesAsync(page, "a.sb_pagN");
            if (!clicked) break;
        }

        if (urls.Count == 0)
        {
            var content = await FetchPageContentWithRetries(page);
            var title = await page.TitleAsync();
            ConsoleHelpers.PrintDebugLine($"No search results found, page title: {title}\n\n{content}");
            if (interactive) Task.Delay(10000).Wait();
        }

        return urls.Take(maxResults).ToList();
    }

    private static async Task<List<string>> ExtractDuckDuckGoSearchResults(IPage page, int maxResults, List<Regex> excludeURLContainsPatternList, bool interactive)
    {
        var urls = new List<string>();
        while (urls.Count < maxResults)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var elements = await page.QuerySelectorAllAsync("a[data-testid=result-title-a][href]");
            foreach (var element in elements)
            {
                var href = await element.GetAttributeAsync("href");
                if (href != null && href.StartsWith("http") && !href.Contains("duckduckgo.com"))
                {
                    if (!urls.Contains(href) && !excludeURLContainsPatternList.Any(pattern => pattern.IsMatch(href)))
                    {
                        urls.Add(href);
                    }
                }
                if (urls.Count >= maxResults) break;
            }

            if (urls.Count >= maxResults) break;

            var clicked = await TryClickWithRetriesAsync(page, "button#more-results");
            if (!clicked) break;
        }

        if (urls.Count == 0)
        {
            var content = await FetchPageContentWithRetries(page);
            var title = await page.TitleAsync();
            ConsoleHelpers.PrintDebugLine($"No search results found, page title: {title}\n\n{content}");
            if (interactive) Task.Delay(10000).Wait();
        }

        return urls.Take(maxResults).ToList();
    }

    private static async Task<List<string>> ExtractYahooSearchResults(IPage page, int maxResults, List<Regex> excludeURLContainsPatternList, bool interactive)
    {
        var urls = new List<string>();
        while (urls.Count < maxResults)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var elements = await page.QuerySelectorAllAsync("div#web a[href]");
            foreach (var element in elements)
            {
                var href = await element.GetAttributeAsync("href");
                if (href != null && href.StartsWith("http"))
                {
                    if (!urls.Contains(href) && !excludeURLContainsPatternList.Any(pattern => pattern.IsMatch(href)))
                    {
                        urls.Add(href);
                    }
                }
                if (urls.Count >= maxResults) break;
            }

            if (urls.Count >= maxResults) break;

            var clicked = await TryClickWithRetriesAsync(page, "a.next");
            if (!clicked) break;
        }

        if (urls.Count == 0)
        {
            var content = await FetchPageContentWithRetries(page);
            var title = await page.TitleAsync();
            ConsoleHelpers.PrintDebugLine($"No search results found, page title: {title}\n\n{content}");
            if (interactive) Task.Delay(10000).Wait();
        }

        return urls.Take(maxResults).ToList();
    }

    private static async Task<IBrowser> GetBrowser(BrowserType browserType, bool interactive, IPlaywright playwright)
    {
        try
        {
            return browserType switch
            {
                BrowserType.Chromium => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
                BrowserType.Firefox => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
                BrowserType.Webkit => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = !interactive }),
                _ => throw new ArgumentOutOfRangeException(nameof(browserType), browserType, null)
            };
        }
        catch (Exception)
        {
            var browserArg = browserType.ToString().ToLower();
            if (RunCli(["install", browserArg]) == 0)
            {
                return await GetBrowser(browserType, interactive, playwright);
            }
            throw;
        }
    }

    private static async Task<string> FetchPageContent(IPage page, string url, bool stripHtml, string saveToFolder)
    {
        try
        {
            // Navigate to the URL
            await page.GotoAsync(url);

            // Get the main content text
            var content = await FetchPageContentWithRetries(page);

            if (content.Contains("Rate limit is exceeded. Try again in"))
            {
                // Rate limit exceeded, wait and try again
                var seconds = int.Parse(content.Split("Try again in ")[1].Split(" seconds.")[0]);
                await Task.Delay(seconds * 1000);
                return await FetchPageContent(page, url, stripHtml, saveToFolder);
            }

            if (stripHtml)
            {
                content = HtmlHelpers.StripHtmlContent(content);
            }

            if (!string.IsNullOrEmpty(saveToFolder))
            {
                var fileName = FileHelpers.GenerateUniqueFileNameFromUrl(url, saveToFolder);
                FileHelpers.WriteAllText(fileName, content);
            }

            return content;
        }
        catch (Exception ex)
        {
            return $"Error fetching content from {url}: {ex.Message}\n{ex.StackTrace}";
        }
    }

    private static async Task<string> FetchPageContentWithRetries(IPage page, int timeoutInMs = 10000, int retries = 3)
    {
        var timeoutTime = DateTime.Now.AddMilliseconds(timeoutInMs);

        var tryCount = retries + 1;
        while (true)
        {
            try
            {
                var waitForLoadTask = page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                var delayTask = Task.Delay((int)timeoutTime.Subtract(DateTime.Now).TotalMilliseconds);
                await Task.WhenAny(waitForLoadTask, delayTask);

                var content = await page.ContentAsync();
                return content;
            }
            catch (Exception ex)
            {
                var rethrow = --tryCount == 0 || !ex.Message.Contains("navigating");
                if (rethrow) throw;

                await Task.Delay(1000);
            }
        }
    }

    private static async Task<bool> TryClickWithRetriesAsync(IPage page, string selector, int retryForMilliseconds = 10000, int retryInterval = 200)
    {
        var clicked = false;
        var timeout = DateTime.Now.AddMilliseconds(retryForMilliseconds);

        while (!clicked && DateTime.Now < timeout)
        {
            var element = await page.QuerySelectorAsync(selector);
            if (element == null)
            {
                ConsoleHelpers.PrintDebugLine($"Element {selector} not found, will scroll and retry...");
                await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                await Task.Delay(retryInterval);
                continue;
            }

            try
            {
                await element.ScrollIntoViewIfNeededAsync();
                await element.ClickAsync();
                clicked = true;
            }
            catch (Exception)
            {
                ConsoleHelpers.PrintDebugLine($"Failed to click element {selector}, will retry...");
                await Task.Delay(retryInterval);
            }
        }

        return clicked;
    }
}
