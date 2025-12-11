using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CycoGh.Models;

class GitHubSearchHelpers
{
    public static async Task<List<RepoInfo>> SearchRepositoriesAsync(SearchCommand command)
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

    private static async Task<List<RepoInfo>> SearchRepositoriesByKeywordsAsync(SearchCommand command)
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
        args.Add("name,owner,url,description,stargazersCount,language,updatedAt,forksCount,openIssuesCount");
        
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

    private static async Task<List<RepoInfo>> SearchCodeForRepositoriesAsync(SearchCommand command)
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

    private static List<RepoInfo> ParseRepositoryUrls(string jsonOutput)
    {
        var repos = new List<RepoInfo>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;
            
            foreach (var item in root.EnumerateArray())
            {
                var repo = ParseRepoInfo(item);
                if (repo != null)
                {
                    repos.Add(repo);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing GitHub search results: {ex.Message}");
            throw;
        }
        
        return repos;
    }

    private static List<RepoInfo> ParseCodeSearchRepositoryUrls(string jsonOutput, int maxResults)
    {
        var seenUrls = new HashSet<string>();
        var repos = new List<RepoInfo>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;
            
            foreach (var result in root.EnumerateArray())
            {
                if (result.TryGetProperty("repository", out var repoElement))
                {
                    var repo = ParseRepoInfo(repoElement);
                    if (repo != null && !seenUrls.Contains(repo.Url))
                    {
                        repos.Add(repo);
                        seenUrls.Add(repo.Url);
                        
                        if (repos.Count >= maxResults)
                        {
                            break;
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
        
        return repos;
    }

    private static RepoInfo? ParseRepoInfo(JsonElement repoElement)
    {
        try
        {
            var url = repoElement.GetProperty("url").GetString();
            
            // Try to get name - might be "name" (repo search) or part of "nameWithOwner" (code search)
            string? name = null;
            string? owner = null;
            
            if (repoElement.TryGetProperty("name", out var nameElement))
            {
                name = nameElement.GetString();
            }
            else if (repoElement.TryGetProperty("nameWithOwner", out var nameWithOwnerElement))
            {
                var fullName = nameWithOwnerElement.GetString();
                if (!string.IsNullOrEmpty(fullName))
                {
                    var parts = fullName.Split('/');
                    if (parts.Length == 2)
                    {
                        owner = parts[0];
                        name = parts[1];
                    }
                }
            }
            
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(name))
            {
                return null;
            }

            // Try to get owner if we didn't get it from nameWithOwner
            if (string.IsNullOrEmpty(owner))
            {
                owner = repoElement.TryGetProperty("owner", out var ownerElement) && 
                           ownerElement.TryGetProperty("login", out var loginElement)
                    ? loginElement.GetString() ?? "unknown"
                    : "unknown";
            }

            var stars = repoElement.TryGetProperty("stargazersCount", out var starsElement)
                ? starsElement.GetInt32()
                : 0;

            var language = repoElement.TryGetProperty("language", out var langElement) && langElement.ValueKind != JsonValueKind.Null
                ? langElement.GetString()
                : null;

            var description = repoElement.TryGetProperty("description", out var descElement) && descElement.ValueKind != JsonValueKind.Null
                ? descElement.GetString()
                : null;

            DateTime? updatedAt = null;
            if (repoElement.TryGetProperty("updatedAt", out var updatedElement))
            {
                var updatedStr = updatedElement.GetString();
                if (!string.IsNullOrEmpty(updatedStr) && DateTime.TryParse(updatedStr, out var parsed))
                {
                    updatedAt = parsed;
                }
            }

            var forks = repoElement.TryGetProperty("forksCount", out var forksElement)
                ? forksElement.GetInt32()
                : 0;

            var openIssues = repoElement.TryGetProperty("openIssuesCount", out var issuesElement)
                ? issuesElement.GetInt32()
                : 0;

            return new RepoInfo
            {
                Url = url,
                Name = name,
                Owner = owner,
                Stars = stars,
                Language = language,
                Description = description,
                UpdatedAt = updatedAt,
                Forks = forks,
                OpenIssues = openIssues
            };
        }
        catch
        {
            return null;
        }
    }

    public static async Task<List<string>> CloneRepositoriesAsync(List<RepoInfo> repos, SearchCommand command)
    {
        var cloneDir = command.CloneDirectory;
        var maxClone = Math.Min(command.MaxClone, repos.Count);
        var clonedRepos = new List<string>();
        
        // Create clone directory if it doesn't exist
        if (!Directory.Exists(cloneDir))
        {
            Directory.CreateDirectory(cloneDir);
            Logger.Info($"Created directory: {cloneDir}");
        }
        
        for (int i = 0; i < maxClone; i++)
        {
            var repo = repos[i];
            var repoName = repo.Name;
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
                    await CloneAsSubmoduleAsync(repo.Url, targetPath);
                }
                else
                {
                    await CloneRepositoryAsync(repo.Url, targetPath);
                }
                
                clonedRepos.Add(targetPath);
                ConsoleHelpers.WriteLine($"Cloned: {repoName}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine($"Failed to clone {repoName}: {ex.Message}");
                Logger.Error($"Error cloning repository {repo.Url}: {ex.Message}");
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
}
