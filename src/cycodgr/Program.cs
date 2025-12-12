using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        CycoGrProgramInfo _programInfo = new();

        LoggingInitializer.InitializeMemoryLogger();
        LoggingInitializer.LogStartupDetails(args);
        Logger.Info($"Starting {ProgramInfo.Name}, version {VersionInfo.GetVersion()}");

        if (!CycoGrCommandLineOptions.Parse(args, out var commandLineOptions, out var ex))
        {
            DisplayBanner();
            if (ex != null)
            {
                Logger.Error($"Command line error: {ex.Message}");
                DisplayException(ex);
                HelpHelpers.DisplayUsage(ex.GetHelpTopic());
                return 2;
            }
            else
            {
                Logger.Warning("Displaying help due to command line parsing issue");
                HelpHelpers.DisplayUsage(commandLineOptions!.HelpTopic);
                return 1;
            }
        }

        var debug = ConsoleHelpers.IsDebug() || commandLineOptions!.Debug;
        var verbose = ConsoleHelpers.IsVerbose() || commandLineOptions!.Verbose;
        var quiet = ConsoleHelpers.IsQuiet() || commandLineOptions!.Quiet;
        ConsoleHelpers.Configure(debug, verbose, quiet);
        
        LoggingInitializer.InitializeLogging(commandLineOptions?.LogFile, debug);

        var helpCommand = commandLineOptions!.Commands.OfType<HelpCommand>().FirstOrDefault();
        if (helpCommand != null)
        {
            DisplayBanner();
            HelpHelpers.DisplayHelpTopic(commandLineOptions.HelpTopic, commandLineOptions.ExpandHelpTopics);
            return 0;
        }

        var versionCommand = commandLineOptions!.Commands.OfType<VersionCommand>().FirstOrDefault();
        if (versionCommand != null)
        {
            DisplayBanner();
            var version = await versionCommand.ExecuteAsync(false);
            ConsoleHelpers.WriteLine(version.ToString()!);
            return 0;
        }

        foreach (var command in commandLineOptions.Commands)
        {
            if (command is RepoCommand repoCommand)
            {
                await HandleRepoCommandAsync(repoCommand);
            }
            else if (command is CodeCommand codeCommand)
            {
                await HandleCodeCommandAsync(codeCommand);
            }
        }

        return 0;
    }

    private static async Task HandleRepoCommandAsync(RepoCommand command)
    {
        try
        {
            var query = string.Join(" ", command.Keywords);
            ConsoleHelpers.WriteLine($"## GitHub repository search for '{query}'", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLine();

            // Search GitHub
            var repos = await GitHubSearchHelpers.SearchRepositoriesAsync(command);

            if (repos.Count == 0)
            {
                ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow);
                return;
            }

            // Apply exclude filters
            repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);

            if (repos.Count == 0)
            {
                ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow);
                return;
            }

            // Output results in requested format
            var output = FormatRepoOutput(repos, command.Format);
            ConsoleHelpers.WriteLine(output);
            ConsoleHelpers.WriteLine();

            // Clone if requested
            if (command.Clone)
            {
                var maxClone = Math.Min(command.MaxClone, repos.Count);
                ConsoleHelpers.WriteLine($"Cloning top {maxClone} repositories to '{command.CloneDirectory}'...", ConsoleColor.Cyan);
                ConsoleHelpers.WriteLine();

                var clonedRepos = await GitHubSearchHelpers.CloneRepositoriesAsync(repos, command);

                ConsoleHelpers.WriteLine();
                ConsoleHelpers.WriteLine($"Successfully cloned {clonedRepos.Count} of {maxClone} repositories", ConsoleColor.Green);
            }

            // Save output if requested
            if (!string.IsNullOrEmpty(command.SaveOutput))
            {
                var saveOutput = new StringBuilder();
                saveOutput.AppendLine($"## GitHub repository search for '{query}'");
                saveOutput.AppendLine();
                saveOutput.AppendLine(output);
                
                var saveFileName = FileHelpers.GetFileNameFromTemplate("repo-output.md", command.SaveOutput)!;
                FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
                ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green);
            }

            // Save additional formats if requested
            SaveAdditionalFormats(command, repos, query, "repository search");
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error: {ex.Message}");
            Logger.Error($"Repo command failed: {ex.Message}");
            Logger.Error(ex.StackTrace ?? string.Empty);
        }
    }

    private static void SaveAdditionalFormats(CycoGrCommand command, object data, string query, string searchType)
    {
        var savedFiles = new List<string>();

        if (!string.IsNullOrEmpty(command.SaveJson))
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("output.json", command.SaveJson)!;
            string jsonContent;
            
            if (data is List<CycoGr.Models.RepoInfo> repos)
            {
                jsonContent = FormatAsJson(repos);
            }
            else if (data is List<CycoGr.Models.CodeMatch> matches)
            {
                jsonContent = FormatCodeAsJson(matches);
            }
            else
            {
                return;
            }
            
            FileHelpers.WriteAllText(fileName, jsonContent);
            savedFiles.Add(fileName);
        }

        if (!string.IsNullOrEmpty(command.SaveCsv))
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("output.csv", command.SaveCsv)!;
            string csvContent;
            
            if (data is List<CycoGr.Models.RepoInfo> repos)
            {
                csvContent = FormatAsCsv(repos);
            }
            else if (data is List<CycoGr.Models.CodeMatch> matches)
            {
                csvContent = FormatCodeAsCsv(matches);
            }
            else
            {
                return;
            }
            
            FileHelpers.WriteAllText(fileName, csvContent);
            savedFiles.Add(fileName);
        }

        if (!string.IsNullOrEmpty(command.SaveTable))
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("output.md", command.SaveTable)!;
            string tableContent;
            
            if (data is List<CycoGr.Models.RepoInfo> repos)
            {
                tableContent = FormatAsTable(repos);
            }
            else
            {
                // Code search doesn't have table format, skip
                return;
            }
            
            var saveOutput = new StringBuilder();
            saveOutput.AppendLine($"## GitHub {searchType} for '{query}'");
            saveOutput.AppendLine();
            saveOutput.AppendLine(tableContent);
            
            FileHelpers.WriteAllText(fileName, saveOutput.ToString());
            savedFiles.Add(fileName);
        }

        if (!string.IsNullOrEmpty(command.SaveUrls))
        {
            var fileName = FileHelpers.GetFileNameFromTemplate("output.txt", command.SaveUrls)!;
            string urlsContent;
            
            if (data is List<CycoGr.Models.RepoInfo> repos)
            {
                urlsContent = FormatAsUrls(repos);
            }
            else if (data is List<CycoGr.Models.CodeMatch> matches)
            {
                urlsContent = FormatCodeAsUrls(matches);
            }
            else
            {
                return;
            }
            
            FileHelpers.WriteAllText(fileName, urlsContent);
            savedFiles.Add(fileName);
        }

        if (savedFiles.Any())
        {
            foreach (var file in savedFiles)
            {
                ConsoleHelpers.WriteLine($"Saved to: {file}", ConsoleColor.Green);
            }
        }
    }

    private static async Task HandleCodeCommandAsync(CodeCommand command)
    {
        try
        {
            var query = string.Join(" ", command.Keywords);
            var searchType = !string.IsNullOrEmpty(command.FileExtension) 
                ? $"code search in .{command.FileExtension} files" 
                : "code search";
                
            ConsoleHelpers.WriteLine($"## GitHub {searchType} for '{query}'", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLine();

            // Search GitHub code
            var codeMatches = await GitHubSearchHelpers.SearchCodeAsync(command);

            if (codeMatches.Count == 0)
            {
                ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow);
                return;
            }

            // Apply exclude filters (filter by repo URL)
            codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);

            if (codeMatches.Count == 0)
            {
                ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow);
                return;
            }

            // Output results in requested format
            var output = FormatCodeOutput(codeMatches, command.Format, command.LinesBeforeAndAfter, query);
            ConsoleHelpers.WriteLine(output);
            ConsoleHelpers.WriteLine();

            // Save output if requested
            if (!string.IsNullOrEmpty(command.SaveOutput))
            {
                var saveOutput = new StringBuilder();
                saveOutput.AppendLine($"## GitHub {searchType} for '{query}'");
                saveOutput.AppendLine();
                saveOutput.AppendLine(output);
                
                var saveFileName = FileHelpers.GetFileNameFromTemplate("code-output.md", command.SaveOutput)!;
                FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
                ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green);
            }

            // Save additional formats if requested
            SaveAdditionalFormats(command, codeMatches, query, searchType);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error: {ex.Message}");
            Logger.Error($"Code command failed: {ex.Message}");
            Logger.Error(ex.StackTrace ?? string.Empty);
        }
    }

    private static string FormatCodeOutput(List<CycoGr.Models.CodeMatch> matches, string format, int contextLines, string query)
    {
        return format switch
        {
            "filenames" => FormatCodeAsFilenames(matches),
            "files" => FormatCodeAsFiles(matches),
            "repos" => FormatCodeAsRepos(matches),
            "urls" => FormatCodeAsUrls(matches),
            "json" => FormatCodeAsJson(matches),
            "csv" => FormatCodeAsCsv(matches),
            _ => FormatCodeAsDetailed(matches, contextLines, query) // Default: detailed
        };
    }

    private static string FormatCodeAsDetailed(List<CycoGr.Models.CodeMatch> matches, int contextLines, string query)
    {
        var output = new StringBuilder();
        var repoGroups = matches.GroupBy(m => m.Repository.Url).OrderBy(g => g.Key);

        foreach (var repoGroup in repoGroups)
        {
            var firstMatch = repoGroup.First();
            var repo = firstMatch.Repository;
            var lang = repo.Language ?? "unknown";
            
            // Repo header: # owner/repo (⭐ stars | language)
            output.AppendLine($"# {repo.FullName} (⭐ {repo.FormattedStars} | {lang})");
            output.AppendLine();

            foreach (var match in repoGroup.OrderBy(m => m.Path))
            {
                // File header: ## path/to/file
                output.AppendLine($"## {match.Path}");
                output.AppendLine();

                // Get language for code fence from file extension
                var fileExt = Path.GetExtension(match.Path).TrimStart('.');
                var fenceLanguage = GetLanguageForExtension(fileExt);

                // Code fence with matches
                output.AppendLine($"```{fenceLanguage}");

                foreach (var textMatch in match.TextMatches)
                {
                    var fragment = textMatch.Fragment;
                    var lines = fragment.Split(new[] { '\n' }, StringSplitOptions.None);

                    // Find matching lines
                    var matchingLineIndices = new HashSet<int>();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (LineContainsMatch(lines[i], query))
                        {
                            matchingLineIndices.Add(i);
                        }
                    }

                    // Determine which lines to show (matching lines + context)
                    var linesToShow = new HashSet<int>();
                    foreach (var matchIdx in matchingLineIndices)
                    {
                        // Add context lines before and after
                        for (int i = Math.Max(0, matchIdx - contextLines); 
                             i <= Math.Min(lines.Length - 1, matchIdx + contextLines); 
                             i++)
                        {
                            linesToShow.Add(i);
                        }
                    }

                    // Process each line, but only show lines in context window
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!linesToShow.Contains(i))
                        {
                            continue; // Skip lines outside context window
                        }

                        var line = lines[i];
                        
                        // Check if this line contains a match
                        var hasMatch = matchingLineIndices.Contains(i);
                        var marker = hasMatch ? "* " : "  ";
                        
                        // For simplicity, just show line numbers starting from 1
                        // In a real implementation, we'd extract actual line numbers from GitHub
                        var lineNum = (i + 1).ToString().PadLeft(3);
                        
                        output.AppendLine($"{marker}{lineNum}: {line}");
                    }
                }

                output.AppendLine("```");
                output.AppendLine();
            }
        }

        return output.ToString().TrimEnd();
    }

    private static bool LineContainsMatch(string line, string query)
    {
        // Simple case-insensitive search
        var keywords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return keywords.Any(k => line.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetLanguageForExtension(string extension)
    {
        // Map file extensions to code fence languages
        return extension.ToLower() switch
        {
            "cs" => "csharp",
            "js" => "javascript",
            "ts" => "typescript",
            "py" => "python",
            "rb" => "ruby",
            "java" => "java",
            "cpp" or "cc" or "cxx" => "cpp",
            "c" => "c",
            "h" or "hpp" => "cpp",
            "go" => "go",
            "rs" => "rust",
            "php" => "php",
            "swift" => "swift",
            "kt" or "kts" => "kotlin",
            "sh" or "bash" => "bash",
            "ps1" => "powershell",
            "sql" => "sql",
            "html" or "htm" => "html",
            "css" => "css",
            "scss" or "sass" => "scss",
            "json" => "json",
            "xml" => "xml",
            "yaml" or "yml" => "yaml",
            "md" or "markdown" => "markdown",
            "txt" => "text",
            _ => extension
        };
    }

    private static string FormatCodeAsFilenames(List<CycoGr.Models.CodeMatch> matches)
    {
        var output = new StringBuilder();
        var repoGroups = matches.GroupBy(m => m.Repository.Url).OrderBy(g => g.Key);

        foreach (var repoGroup in repoGroups)
        {
            var repo = repoGroup.First().Repository;
            output.AppendLine($"# {repo.FullName}");
            output.AppendLine();

            foreach (var match in repoGroup.OrderBy(m => m.Path))
            {
                output.AppendLine(match.Path);
            }

            output.AppendLine();
        }

        return output.ToString().TrimEnd();
    }

    private static string FormatCodeAsFiles(List<CycoGr.Models.CodeMatch> matches)
    {
        var output = new StringBuilder();

        foreach (var match in matches.OrderBy(m => m.Repository.Url).ThenBy(m => m.Path))
        {
            // Raw file URL: https://raw.githubusercontent.com/owner/repo/branch/path
            var rawUrl = $"https://raw.githubusercontent.com/{match.Repository.FullName}/HEAD/{match.Path}";
            output.AppendLine(rawUrl);
        }

        return output.ToString().TrimEnd();
    }

    private static string FormatCodeAsRepos(List<CycoGr.Models.CodeMatch> matches)
    {
        var uniqueRepos = matches.Select(m => m.Repository.Url).Distinct().OrderBy(u => u);
        return string.Join(Environment.NewLine, uniqueRepos);
    }

    private static string FormatCodeAsUrls(List<CycoGr.Models.CodeMatch> matches)
    {
        var output = new StringBuilder();
        var repoGroups = matches.GroupBy(m => m.Repository.Url).OrderBy(g => g.Key);

        foreach (var repoGroup in repoGroups)
        {
            var repo = repoGroup.First().Repository;
            output.AppendLine(repo.Url);

            foreach (var match in repoGroup.OrderBy(m => m.Path))
            {
                // GitHub file URL
                var fileUrl = $"{repo.Url}/blob/HEAD/{match.Path}";
                output.AppendLine($"  {fileUrl}");
            }

            output.AppendLine();
        }

        return output.ToString().TrimEnd();
    }

    private static string FormatCodeAsJson(List<CycoGr.Models.CodeMatch> matches)
    {
        var items = matches.Select(m => new
        {
            repository = new
            {
                url = m.Repository.Url,
                name = m.Repository.Name,
                owner = m.Repository.Owner,
                fullName = m.Repository.FullName,
                stars = m.Repository.Stars,
                language = m.Repository.Language
            },
            path = m.Path,
            sha = m.Sha,
            url = m.Url,
            textMatches = m.TextMatches.Select(tm => new
            {
                fragment = tm.Fragment,
                property = tm.Property,
                type = tm.Type,
                matches = tm.Matches.Select(mi => new
                {
                    indices = mi.Indices,
                    text = mi.Text
                })
            })
        });

        return System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    private static string FormatCodeAsCsv(List<CycoGr.Models.CodeMatch> matches)
    {
        var output = new StringBuilder();
        output.AppendLine("repo_url,repo_name,repo_owner,repo_stars,file_path,file_url");

        foreach (var match in matches.OrderBy(m => m.Repository.Url).ThenBy(m => m.Path))
        {
            var fileUrl = $"{match.Repository.Url}/blob/HEAD/{match.Path}";
            output.AppendLine($"\"{match.Repository.Url}\",\"{match.Repository.Name}\",\"{match.Repository.Owner}\",{match.Repository.Stars},\"{match.Path}\",\"{fileUrl}\"");
        }

        return output.ToString().TrimEnd();
    }

    private static string FormatRepoOutput(List<CycoGr.Models.RepoInfo> repos, string format)
    {
        return format switch
        {
            "table" => FormatAsTable(repos),
            "json" => FormatAsJson(repos),
            "csv" => FormatAsCsv(repos),
            "detailed" => FormatAsDetailed(repos),
            _ => FormatAsUrls(repos) // Default: url format
        };
    }

    private static string FormatAsUrls(List<CycoGr.Models.RepoInfo> repos)
    {
        return string.Join(Environment.NewLine, repos.Select(r => r.Url));
    }

    private static string FormatAsDetailed(List<CycoGr.Models.RepoInfo> repos)
    {
        var output = new StringBuilder();
        foreach (var repo in repos)
        {
            var lang = repo.Language ?? "unknown";
            var desc = string.IsNullOrWhiteSpace(repo.Description) 
                ? "(no description)" 
                : (repo.Description.Length > 80 ? repo.Description.Substring(0, 77) + "..." : repo.Description);
            
            output.AppendLine($"{repo.Url} | ⭐ {repo.FormattedStars} | {lang} | {desc}");
        }
        return output.ToString().TrimEnd();
    }

    private static string FormatAsTable(List<CycoGr.Models.RepoInfo> repos)
    {
        var output = new StringBuilder();
        output.AppendLine("| Repository | Stars | Language | Description |");
        output.AppendLine("|------------|-------|----------|-------------|");
        
        foreach (var repo in repos)
        {
            var lang = repo.Language ?? "unknown";
            var desc = string.IsNullOrWhiteSpace(repo.Description) 
                ? "(no description)" 
                : (repo.Description.Length > 50 ? repo.Description.Substring(0, 47) + "..." : repo.Description);
            
            output.AppendLine($"| {repo.FullName} | {repo.FormattedStars} | {lang} | {desc} |");
        }
        
        return output.ToString().TrimEnd();
    }

    private static string FormatAsJson(List<CycoGr.Models.RepoInfo> repos)
    {
        var items = repos.Select(r => new
        {
            url = r.Url,
            name = r.Name,
            owner = r.Owner,
            fullName = r.FullName,
            stars = r.Stars,
            language = r.Language,
            description = r.Description,
            forks = r.Forks,
            openIssues = r.OpenIssues,
            updatedAt = r.UpdatedAt?.ToString("yyyy-MM-dd")
        });

        return System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    private static string FormatAsCsv(List<CycoGr.Models.RepoInfo> repos)
    {
        var output = new StringBuilder();
        output.AppendLine("url,name,owner,stars,language,description");
        
        foreach (var repo in repos)
        {
            var desc = repo.Description?.Replace("\"", "\"\"") ?? "";
            var lang = repo.Language ?? "";
            output.AppendLine($"\"{repo.Url}\",\"{repo.Name}\",\"{repo.Owner}\",{repo.Stars},\"{lang}\",\"{desc}\"");
        }
        
        return output.ToString().TrimEnd();
    }

    private static List<T> ApplyExcludeFilters<T>(List<T> items, List<string> excludePatterns, Func<T, string> urlGetter)
    {
        if (excludePatterns.Count == 0)
        {
            return items;
        }

        var filtered = items.Where(item =>
        {
            var url = urlGetter(item);
            foreach (var pattern in excludePatterns)
            {
                try
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(url, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        return false; // Exclude this item
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Invalid regex pattern '{pattern}': {ex.Message}");
                }
            }
            return true; // Keep this item
        }).ToList();

        var excludedCount = items.Count - filtered.Count;
        if (excludedCount > 0)
        {
            ConsoleHelpers.WriteLine($"Excluded {excludedCount} result(s) matching exclude pattern(s)", ConsoleColor.Yellow);
        }

        return filtered;
    }

    private static void DisplayBanner()
    {
        var programNameUppercase = Name.ToUpper();
        
        if (ProgramInfo.Assembly == null)
        {
            new CycoGrProgramInfo();
        }
        
        ConsoleHelpers.WriteLine(
            $"{programNameUppercase} - GitHub Search and Repository Management CLI, Version {VersionInfo.GetVersion()}\n" +
            "Copyright(c) 2025, Rob Chambers. All rights reserved.\n");
    }

    private static void DisplayException(CommandLineException ex)
    {
        var printMessage = !string.IsNullOrEmpty(ex.Message);
        if (printMessage) ConsoleHelpers.WriteLine($"  {ex.Message}\n\n");
    }

    public const string Name = "cycodgr";
}
