using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CycoGr.Models;

class GitHubSearchHelpers
{
    public static async Task<List<RepoInfo>> SearchRepositoriesAsync(RepoCommand command)
    {
        return await SearchRepositoriesByKeywordsAsync(command);
    }

    public static async Task<List<CodeMatch>> SearchCodeAsync(CodeCommand command)
    {
        return await SearchCodeForMatchesAsync(command);
    }

    private static async Task<List<RepoInfo>> SearchRepositoriesByKeywordsAsync(RepoCommand command)
    {
        var query = string.Join(" ", command.Keywords);
        
        // Add repo qualifiers if specified
        if (command.Repos.Any())
        {
            var repoQualifiers = string.Join(" ", command.Repos.Select(r => $"repo:{r}"));
            query = $"{query} {repoQualifiers}";
        }
        
        // Add owner qualifier if specified
        if (!string.IsNullOrEmpty(command.Owner))
        {
            query = $"{query} user:{command.Owner}";
        }
        
        // Add fork filter
        if (command.OnlyForks)
        {
            query = $"{query} fork:only";
        }
        else if (command.ExcludeForks)
        {
            query = $"{query} fork:false";
        }
        else if (command.IncludeForks)
        {
            query = $"{query} fork:true";
        }
        
        // Add stars filter if specified
        if (command.MinStars > 0)
        {
            query = $"{query} stars:>={command.MinStars}";
        }
        
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

    private static async Task<List<CodeMatch>> SearchCodeForMatchesAsync(CodeCommand command)
    {
        var query = string.Join(" ", command.Keywords);
        
        // Add repo qualifiers if specified
        if (command.Repos.Any())
        {
            var repoQualifiers = string.Join(" ", command.Repos.Select(r => $"repo:{r}"));
            query = $"{query} {repoQualifiers}";
        }
        
        // Add owner qualifier if specified
        if (!string.IsNullOrEmpty(command.Owner))
        {
            query = $"{query} user:{command.Owner}";
        }
        
        // Add stars filter if specified
        if (command.MinStars > 0)
        {
            query = $"{query} stars:>={command.MinStars}";
        }
        
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
        args.Add("path,repository,sha,textMatches,url");
        
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
            
            return ParseCodeSearchResults(result.StandardOutput);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error searching GitHub code: {ex.Message}");
            throw;
        }
    }

    private static List<CodeMatch> ParseCodeSearchResults(string jsonOutput)
    {
        var codeMatches = new List<CodeMatch>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;
            
            foreach (var item in root.EnumerateArray())
            {
                var match = ParseCodeMatch(item);
                if (match != null)
                {
                    codeMatches.Add(match);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing GitHub code search results: {ex.Message}");
            throw;
        }
        
        return codeMatches;
    }

    private static CodeMatch? ParseCodeMatch(JsonElement element)
    {
        try
        {
            var path = element.GetProperty("path").GetString();
            var url = element.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
            var sha = element.GetProperty("sha").GetString();
            
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(sha))
            {
                return null;
            }

            var repoElement = element.GetProperty("repository");
            var repo = ParseRepoInfo(repoElement);
            if (repo == null)
            {
                return null;
            }

            var textMatches = new List<TextMatch>();
            if (element.TryGetProperty("textMatches", out var textMatchesElement))
            {
                foreach (var tmElement in textMatchesElement.EnumerateArray())
                {
                    var fragment = tmElement.GetProperty("fragment").GetString();
                    if (string.IsNullOrEmpty(fragment)) continue;

                    var textMatch = new TextMatch
                    {
                        Fragment = fragment,
                        Property = tmElement.TryGetProperty("property", out var propEl) ? propEl.GetString() ?? "" : "",
                        Type = tmElement.TryGetProperty("type", out var typeEl) ? typeEl.GetString() ?? "" : ""
                    };

                    if (tmElement.TryGetProperty("matches", out var matchesElement))
                    {
                        foreach (var matchEl in matchesElement.EnumerateArray())
                        {
                            var indicesEl = matchEl.GetProperty("indices");
                            var indices = new List<int>();
                            foreach (var indexEl in indicesEl.EnumerateArray())
                            {
                                indices.Add(indexEl.GetInt32());
                            }

                            var matchText = matchEl.TryGetProperty("text", out var textEl) ? textEl.GetString() : null;
                            if (matchText != null)
                            {
                                textMatch.Matches.Add(new MatchIndices
                                {
                                    Indices = indices.ToArray(),
                                    Text = matchText
                                });
                            }
                        }
                    }

                    textMatches.Add(textMatch);
                }
            }

            return new CodeMatch
            {
                Path = path,
                Repository = repo,
                Sha = sha,
                Url = url ?? "",
                TextMatches = textMatches
            };
        }
        catch
        {
            return null;
        }
    }

    // TODO: This will be used for CodeCommand in Phase 1.3
    /*
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
    */

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

    // TODO: This will be used for CodeCommand in Phase 1.3
    /*
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
    */

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

    public static async Task<List<string>> CloneRepositoriesAsync(List<RepoInfo> repos, RepoCommand command)
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
