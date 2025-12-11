using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

class GitHubSearchHelpers
{
    public static async Task<List<string>> SearchRepositoriesAsync(SearchCommand command)
    {
        var useCodeSearch = !string.IsNullOrEmpty(command.FileExtension);
        
        if (useCodeSearch)
        {
            return await SearchCodeForRepositoriesAsync(command);
        }
        else
        {
            return await SearchRepositoriesByKeywordsAsync(command);
        }
    }

    private static async Task<List<string>> SearchRepositoriesByKeywordsAsync(SearchCommand command)
    {
        var query = string.Join(" ", command.Keywords);
        var args = new List<string>();
        
        args.Add("search");
        args.Add("repos");
        args.Add($"\"{query}\"");
        args.Add("--limit");
        args.Add(command.MaxResults.ToString());
        
        if (!string.IsNullOrEmpty(command.Language))
        {
            args.Add("--language");
            args.Add(command.Language);
        }
        
        if (!string.IsNullOrEmpty(command.SortBy))
        {
            args.Add("--sort");
            args.Add(command.SortBy);
        }
        
        if (command.IncludeForks)
        {
            args.Add("--include-forks");
        }
        
        args.Add("--json");
        args.Add("name,owner,url");
        
        var ghCommand = $"gh {string.Join(" ", args)}";
        
        try
        {
            var result = await ProcessHelpers.RunProcessAsync(ghCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
            
            if (result.ExitCode != 0)
            {
                var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                    ? result.StandardError 
                    : "Unknown error executing gh command";
                throw new Exception($"GitHub search failed: {errorMsg}");
            }
            
            return ParseRepositoryUrls(result.StandardOutput);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error searching GitHub: {ex.Message}");
            throw;
        }
    }

    private static async Task<List<string>> SearchCodeForRepositoriesAsync(SearchCommand command)
    {
        var query = string.Join(" ", command.Keywords);
        var args = new List<string>();
        
        args.Add("search");
        args.Add("code");
        args.Add($"\"{query}\"");
        args.Add("--limit");
        args.Add(command.MaxResults.ToString());
        
        if (!string.IsNullOrEmpty(command.FileExtension))
        {
            args.Add("--extension");
            args.Add(command.FileExtension.TrimStart('.'));
        }
        
        if (!string.IsNullOrEmpty(command.Language))
        {
            args.Add("--language");
            args.Add(command.Language);
        }
        
        args.Add("--json");
        args.Add("repository");
        
        var ghCommand = $"gh {string.Join(" ", args)}";
        
        try
        {
            var result = await ProcessHelpers.RunProcessAsync(ghCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
            
            if (result.ExitCode != 0)
            {
                var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                    ? result.StandardError 
                    : "Unknown error executing gh command";
                throw new Exception($"GitHub code search failed: {errorMsg}");
            }
            
            return ParseCodeSearchRepositoryUrls(result.StandardOutput, command.MaxResults);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error searching GitHub code: {ex.Message}");
            throw;
        }
    }

    private static List<string> ParseRepositoryUrls(string jsonOutput)
    {
        var urls = new List<string>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;
            
            foreach (var repo in root.EnumerateArray())
            {
                if (repo.TryGetProperty("url", out var urlElement))
                {
                    var url = urlElement.GetString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        urls.Add(url);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing GitHub search results: {ex.Message}");
            throw;
        }
        
        return urls;
    }

    private static List<string> ParseCodeSearchRepositoryUrls(string jsonOutput, int maxResults)
    {
        var urls = new HashSet<string>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;
            
            foreach (var result in root.EnumerateArray())
            {
                if (result.TryGetProperty("repository", out var repoElement))
                {
                    if (repoElement.TryGetProperty("url", out var urlElement))
                    {
                        var url = urlElement.GetString();
                        if (!string.IsNullOrEmpty(url))
                        {
                            urls.Add(url);
                            
                            // Stop when we reach max unique repos
                            if (urls.Count >= maxResults)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing GitHub code search results: {ex.Message}");
            throw;
        }
        
        return urls.ToList();
    }

    public static async Task<List<string>> CloneRepositoriesAsync(List<string> repoUrls, SearchCommand command)
    {
        var cloneDir = command.CloneDirectory;
        var maxClone = Math.Min(command.MaxClone, repoUrls.Count);
        var clonedRepos = new List<string>();
        
        // Create clone directory if it doesn't exist
        if (!Directory.Exists(cloneDir))
        {
            Directory.CreateDirectory(cloneDir);
            Logger.Info($"Created directory: {cloneDir}");
        }
        
        for (int i = 0; i < maxClone; i++)
        {
            var url = repoUrls[i];
            var repoName = GetRepoNameFromUrl(url);
            var targetPath = Path.Combine(cloneDir, repoName);
            
            // Skip if already exists
            if (Directory.Exists(targetPath))
            {
                ConsoleHelpers.WriteLine($"Skipping {repoName} (already exists)", ConsoleColor.Yellow);
                Logger.Warning($"Repository already exists: {targetPath}");
                clonedRepos.Add(targetPath);
                continue;
            }
            
            try
            {
                ConsoleHelpers.DisplayStatus($"Cloning {repoName} ({i + 1}/{maxClone})...");
                
                if (command.AsSubmodules)
                {
                    await CloneAsSubmoduleAsync(url, targetPath);
                }
                else
                {
                    await CloneRepositoryAsync(url, targetPath);
                }
                
                clonedRepos.Add(targetPath);
                ConsoleHelpers.WriteLine($"Cloned: {repoName}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine($"Failed to clone {repoName}: {ex.Message}");
                Logger.Error($"Error cloning repository {url}: {ex.Message}");
            }
        }
        
        ConsoleHelpers.DisplayStatusErase();
        return clonedRepos;
    }

    private static async Task CloneRepositoryAsync(string url, string targetPath)
    {
        var gitCommand = $"git clone {url} \"{targetPath}\"";
        var result = await ProcessHelpers.RunProcessAsync(gitCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
        
        if (result.ExitCode != 0)
        {
            var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                ? result.StandardError 
                : "Unknown error executing git clone";
            throw new Exception($"Git clone failed: {errorMsg}");
        }
    }

    private static async Task CloneAsSubmoduleAsync(string url, string targetPath)
    {
        var gitCommand = $"git submodule add {url} \"{targetPath}\"";
        var result = await ProcessHelpers.RunProcessAsync(gitCommand, workingDirectory: null, envVars: null, input: null, timeout: null);
        
        if (result.ExitCode != 0)
        {
            var errorMsg = !string.IsNullOrEmpty(result.StandardError) 
                ? result.StandardError 
                : "Unknown error executing git submodule add";
            throw new Exception($"Git submodule add failed: {errorMsg}");
        }
    }

    private static string GetRepoNameFromUrl(string url)
    {
        // Extract repo name from URL like "https://github.com/owner/repo"
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Trim('/').Split('/');
        return segments.Length > 0 ? segments.Last() : "unknown-repo";
    }
}
