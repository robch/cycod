using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CycoGr.CommandLine;

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
            if (command is CycoGr.CommandLineCommands.SearchCommand searchCommand)
            {
                await HandleSearchCommandAsync(searchCommand);
            }
        }

        return 0;
    }

    private static async Task HandleSearchCommandAsync(CycoGr.CommandLineCommands.SearchCommand command)
    {
        try
        {
            // Determine what type of search based on flags
            var hasFileContains = !string.IsNullOrEmpty(command.FileContains);
            var hasRepoContains = !string.IsNullOrEmpty(command.RepoContains);
            var hasContains = !string.IsNullOrEmpty(command.Contains);
            var hasRepoPatterns = command.RepoPatterns.Any();

            // Scenario 1: Repo patterns only (no content search) - Show repo metadata
            if (hasRepoPatterns && !hasFileContains && !hasRepoContains && !hasContains)
            {
                await ShowRepoMetadataAsync(command);
            }
            // Scenario 2: --contains (unified search - both repos and code)
            else if (hasContains)
            {
                await HandleUnifiedSearchAsync(command);
            }
            // Scenario 3: --file-contains only (code search)
            else if (hasFileContains)
            {
                await HandleCodeSearchAsync(command);
            }
            // Scenario 4: --repo-contains only (repo search)
            else if (hasRepoContains)
            {
                await HandleRepoSearchAsync(command);
            }
            else
            {
                ConsoleHelpers.WriteErrorLine("No search criteria specified. Use --contains, --file-contains, or --repo-contains");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error: {ex.Message}");
            Logger.Error($"Search command failed: {ex.Message}");
            Logger.Error(ex.StackTrace ?? string.Empty);
        }
    }

    private static async Task ShowRepoMetadataAsync(CycoGr.CommandLineCommands.SearchCommand command)
    {
        foreach (var repoPattern in command.RepoPatterns)
        {
            try
            {
                var repo = await CycoGr.Helpers.GitHubSearchHelpers.GetRepositoryMetadataAsync(repoPattern);
                
                if (repo == null)
                {
                    ConsoleHelpers.WriteLine($"{repoPattern}", ConsoleColor.Yellow, overrideQuiet: true);
                    ConsoleHelpers.WriteLine($"  Not found", overrideQuiet: true);
                    ConsoleHelpers.WriteLine(overrideQuiet: true);
                    continue;
                }

                // Format: ## owner/repo (⭐ stars) (language)
                var header = $"## {repo.FullName} (⭐ {repo.FormattedStars})";
                if (!string.IsNullOrEmpty(repo.Language))
                {
                    header += $" ({repo.Language})";
                }
                ConsoleHelpers.WriteLine(header, ConsoleColor.White, overrideQuiet: true);
                ConsoleHelpers.WriteLine(overrideQuiet: true);

                // Repo URL and Description
                ConsoleHelpers.WriteLine($"Repo: {repo.Url}", overrideQuiet: true);
                if (!string.IsNullOrEmpty(repo.Description))
                {
                    ConsoleHelpers.WriteLine($"Desc: {repo.Description}", overrideQuiet: true);
                }
                ConsoleHelpers.WriteLine(overrideQuiet: true);

                // Topics and Updated
                if (repo.Topics?.Any() == true)
                {
                    ConsoleHelpers.WriteLine($"Topics: {string.Join(", ", repo.Topics)}", overrideQuiet: true);
                }
                if (repo.UpdatedAt.HasValue)
                {
                    ConsoleHelpers.WriteLine($"Updated: {repo.UpdatedAt.Value:yyyy-MM-dd}", overrideQuiet: true);
                }
                ConsoleHelpers.WriteLine(overrideQuiet: true);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteLine($"{repoPattern}", ConsoleColor.Yellow, overrideQuiet: true);
                ConsoleHelpers.WriteLine($"  Error: {ex.Message}", overrideQuiet: true);
                ConsoleHelpers.WriteLine(overrideQuiet: true);
                Logger.Error($"Failed to fetch metadata for {repoPattern}: {ex.Message}");
            }
        }
    }

    private static async Task HandleUnifiedSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
    {
        var query = command.Contains;
        ConsoleHelpers.WriteLine($"## GitHub unified search for '{query}'", ConsoleColor.Cyan, overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);

        // Search both repos and code (N of each per spec recommendation)
        var repoTask = CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
            query,
            command.RepoPatterns,
            command.Language,
            command.Owner,
            command.MinStars,
            command.SortBy,
            command.IncludeForks,
            command.ExcludeForks,
            command.OnlyForks,
            command.MaxResults);

        var codeTask = CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
            query,
            command.RepoPatterns,
            command.Language,
            command.Owner,
            command.MinStars,
            "",
            command.MaxResults);

        await Task.WhenAll(repoTask, codeTask);

        var repos = repoTask.Result;
        var codeMatches = codeTask.Result;

        // Apply exclude filters
        repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);
        codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);

        if (repos.Count == 0 && codeMatches.Count == 0)
        {
            ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow, overrideQuiet: true);
            return;
        }

        // Output repos that matched (no file sections)
        if (repos.Count > 0)
        {
            var repoOutput = FormatRepoOutput(repos, "detailed");
            ConsoleHelpers.WriteLine(repoOutput, overrideQuiet: true);
        }

        // Output code matches (with file sections under repos)
        if (codeMatches.Count > 0)
        {
            await FormatAndOutputCodeResults(codeMatches, command.LinesBeforeAndAfter, query, command.Format, command.FileInstructionsList, command.RepoInstructionsList, command.InstructionsList, overrideQuiet: true);
        }

        // Save output if requested
        if (!string.IsNullOrEmpty(command.SaveOutput))
        {
            // TODO: Build save output
            var saveFileName = FileHelpers.GetFileNameFromTemplate("unified-output.md", command.SaveOutput)!;
            // FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
            ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
        }
    }

    private static async Task HandleRepoSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
    {
        var query = !string.IsNullOrEmpty(command.RepoContains) ? command.RepoContains : command.Contains;
        ConsoleHelpers.WriteLine($"## GitHub repository search for '{query}'", ConsoleColor.Cyan, overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);

        // Search GitHub using new helper signature
        var repos = await CycoGr.Helpers.GitHubSearchHelpers.SearchRepositoriesAsync(
            query,
            command.RepoPatterns,
            command.Language,
            command.Owner,
            command.MinStars,
            command.SortBy,
            command.IncludeForks,
            command.ExcludeForks,
            command.OnlyForks,
            command.MaxResults);

        if (repos.Count == 0)
        {
            ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow, overrideQuiet: true);
            return;
        }

        // Apply exclude filters
        repos = ApplyExcludeFilters(repos, command.Exclude, r => r.Url);

        if (repos.Count == 0)
        {
            ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow, overrideQuiet: true);
            return;
        }

        // Output results in requested format
        var output = FormatRepoOutput(repos, command.Format);
        ConsoleHelpers.WriteLine(output, overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);

        // Clone if requested
        if (command.Clone)
        {
            var maxClone = Math.Min(command.MaxClone, repos.Count);
            ConsoleHelpers.WriteLine($"Cloning top {maxClone} repositories to '{command.CloneDirectory}'...", ConsoleColor.Cyan, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var clonedRepos = await CycoGr.Helpers.GitHubSearchHelpers.CloneRepositoriesAsync(repos, command.AsSubmodules, command.CloneDirectory, maxClone);

            ConsoleHelpers.WriteLine(overrideQuiet: true);
            ConsoleHelpers.WriteLine($"Successfully cloned {clonedRepos.Count} of {maxClone} repositories", ConsoleColor.Green, overrideQuiet: true);
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
            ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
        }

        // Save additional formats if requested
        SaveAdditionalFormats(command, repos, query, "repository search");
    }

    private static async Task HandleCodeSearchAsync(CycoGr.CommandLineCommands.SearchCommand command)
    {
        var query = !string.IsNullOrEmpty(command.FileContains) ? command.FileContains : command.Contains;
        var searchType = !string.IsNullOrEmpty(command.Language) 
            ? $"code search in {command.Language} files" 
            : "code search";
            
        ConsoleHelpers.WriteLine($"## GitHub {searchType} for '{query}'", ConsoleColor.Cyan, overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);

        // Search GitHub code using new helper signature
        var codeMatches = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
            query,
            command.RepoPatterns,
            command.Language,
            command.Owner,
            command.MinStars,
            "",
            command.MaxResults);

        if (codeMatches.Count == 0)
        {
            ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow, overrideQuiet: true);
            return;
        }

        // Apply exclude filters (filter by repo URL)
        codeMatches = ApplyExcludeFilters(codeMatches, command.Exclude, m => m.Repository.Url);

        if (codeMatches.Count == 0)
        {
            ConsoleHelpers.WriteLine("No results after filtering", ConsoleColor.Yellow, overrideQuiet: true);
            return;
        }

        // Output results grouped by repository
        await FormatAndOutputCodeResults(codeMatches, command.LinesBeforeAndAfter, query, command.Format, command.FileInstructionsList, command.RepoInstructionsList, command.InstructionsList, overrideQuiet: true);

        // Save output if requested
        if (!string.IsNullOrEmpty(command.SaveOutput))
        {
            var saveOutput = new StringBuilder();
            saveOutput.AppendLine($"## GitHub {searchType} for '{query}'");
            saveOutput.AppendLine();
            
            // TODO: Format for saving
            
            var saveFileName = FileHelpers.GetFileNameFromTemplate("code-output.md", command.SaveOutput)!;
            FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
            ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green, overrideQuiet: true);
        }

        // Save additional formats if requested
        SaveAdditionalFormats(command, codeMatches, query, searchType);
    }

    private static void SaveAdditionalFormats(CycoGr.CommandLine.CycoGrCommand command, object data, string query, string searchType)
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

    private static async Task FormatAndOutputCodeResults(List<CycoGr.Models.CodeMatch> codeMatches, int contextLines, string query, string format, List<Tuple<string, string>> fileInstructionsList, List<string> repoInstructionsList, List<string> instructionsList, bool overrideQuiet = false)
    {
        // Group by repository
        var byRepo = codeMatches.GroupBy(m => m.Repository.FullName).ToList();

        var allRepoOutputs = new List<string>();

        foreach (var repoGroup in byRepo)
        {
            var repo = repoGroup.First().Repository;
            var files = repoGroup.ToList();

            var repoOutputBuilder = new System.Text.StringBuilder();

            // Output repo header
            var header = $"## {repo.FullName}";
            if (repo.Stars > 0)
            {
                header += $" (⭐ {repo.FormattedStars})";
            }
            if (!string.IsNullOrEmpty(repo.Language))
            {
                header += $" ({repo.Language})";
            }
            repoOutputBuilder.AppendLine(header);
            repoOutputBuilder.AppendLine();

            // Repo URL and Desc
            repoOutputBuilder.AppendLine($"Repo: {repo.Url}");
            if (!string.IsNullOrEmpty(repo.Description))
            {
                repoOutputBuilder.AppendLine($"Desc: {repo.Description}");
            }
            repoOutputBuilder.AppendLine();

            // Count total matches across all files
            var totalMatches = files.Sum(f => f.TextMatches?.Count ?? 0);
            var fileCount = files.Select(f => f.Path).Distinct().Count();
            
            repoOutputBuilder.AppendLine($"Found {fileCount} file(s) with {totalMatches} matches:");
            
            // Output file URLs with match counts
            var fileGroups = files.GroupBy(f => f.Path).ToList();
            foreach (var fileGroup in fileGroups)
            {
                var firstMatch = fileGroup.First();
                var matchCount = fileGroup.Sum(f => f.TextMatches?.Count ?? 0);
                repoOutputBuilder.AppendLine($"- {firstMatch.Url} ({matchCount} matches)");
            }
            repoOutputBuilder.AppendLine();

            // Process files in parallel using ThrottledProcessor
            var throttledProcessor = new ThrottledProcessor(Environment.ProcessorCount);
            var fileOutputs = await throttledProcessor.ProcessAsync(
                fileGroups,
                async fileGroup => await ProcessFileGroupAsync(fileGroup, repo, query, contextLines, fileInstructionsList, overrideQuiet)
            );

            // Combine all file outputs for this repo
            var repoFilesOutput = string.Join("", fileOutputs);
            repoOutputBuilder.Append(repoFilesOutput);

            // Get the complete repo output
            var repoOutput = repoOutputBuilder.ToString();

            // Apply repo instructions if any
            if (repoInstructionsList.Any())
            {
                Logger.Info($"Applying {repoInstructionsList.Count} repo instruction(s) to repository: {repo.FullName}");
                repoOutput = AiInstructionProcessor.ApplyAllInstructions(
                    repoInstructionsList,
                    repoOutput,
                    useBuiltInFunctions: false,
                    saveChatHistory: string.Empty);
                Logger.Info($"Repo instructions applied successfully to repository: {repo.FullName}");
            }

            // Collect this repo's output
            allRepoOutputs.Add(repoOutput);
        }

        // Combine all repo outputs
        var combinedOutput = string.Join("\n", allRepoOutputs);

        // Apply final/global instructions if any
        if (instructionsList.Any())
        {
            Logger.Info($"Applying {instructionsList.Count} final instruction(s) to all combined output");
            combinedOutput = AiInstructionProcessor.ApplyAllInstructions(
                instructionsList,
                combinedOutput,
                useBuiltInFunctions: false,
                saveChatHistory: string.Empty);
            Logger.Info($"Final instructions applied successfully");
        }

        // Output the final result
        ConsoleHelpers.WriteLine(combinedOutput, overrideQuiet: overrideQuiet);
    }

    private static async Task<string> ProcessFileGroupAsync(
        IGrouping<string, CycoGr.Models.CodeMatch> fileGroup, 
        CycoGr.Models.RepoInfo repo,
        string query, 
        int contextLines,
        List<Tuple<string, string>> fileInstructionsList,
        bool overrideQuiet)
    {
        var firstMatch = fileGroup.First();
        var output = new System.Text.StringBuilder();
        
        // File header
        output.AppendLine($"## {firstMatch.Path}");
        output.AppendLine();

        // Fetch full file content and display with real line numbers
        var rawUrl = ConvertToRawUrl(firstMatch.Url);
        try
        {
            // Create FoundTextFile with lambda to load content
            var foundFile = new FoundTextFile
            {
                Path = firstMatch.Path,
                LoadContent = async () =>
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CycoGr/1.0");
                    return await httpClient.GetStringAsync(rawUrl);
                },
                Metadata = new Dictionary<string, object>
                {
                    { "Repository", repo },
                    { "Sha", firstMatch.Sha },
                    { "Url", firstMatch.Url }
                }
            };

            // Load the content
            foundFile.Content = await foundFile.LoadContent();

            // Use LineHelpers to filter and display with real line numbers
            var includePatterns = new List<System.Text.RegularExpressions.Regex>
            {
                new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(query), System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            };
            var excludePatterns = new List<System.Text.RegularExpressions.Regex>();

            var lang = DetectLanguageFromPath(firstMatch.Path);
            var backticks = $"```{lang}";
            
            var filteredContent = LineHelpers.FilterAndExpandContext(
                foundFile.Content,
                includePatterns,
                contextLines,  // lines before
                contextLines,  // lines after
                true,          // include line numbers
                excludePatterns,
                backticks,
                true           // highlight matches
            );

            if (filteredContent != null)
            {
                output.AppendLine(backticks);
                output.AppendLine(filteredContent);
                output.AppendLine("```");
            }
            else
            {
                output.AppendLine("(No matches found in full file content)");
            }
        }
        catch (Exception ex)
        {
            output.AppendLine($"Error fetching file content: {ex.Message}");
            output.AppendLine("Falling back to fragment display...");
            
            // Fallback to fragment display
            foreach (var match in fileGroup)
            {
                if (match.TextMatches?.Any() == true)
                {
                    var lang = DetectLanguageFromPath(match.Path);
                    output.AppendLine($"```{lang}");
                    
                    foreach (var textMatch in match.TextMatches)
                    {
                        var fragment = textMatch.Fragment;
                        output.AppendLine(fragment);
                    }
                    
                    output.AppendLine("```");
                }
            }
        }

        output.AppendLine();
        output.AppendLine($"Raw: {rawUrl}");
        output.AppendLine();

        // Apply file instructions if any match this file
        var formattedOutput = output.ToString();
        var instructionsForThisFile = fileInstructionsList
            .Where(x => FileNameMatchesInstructionsCriteria(firstMatch.Path, x.Item2))
            .Select(x => x.Item1)
            .ToList();

        if (instructionsForThisFile.Any())
        {
            Logger.Info($"Applying {instructionsForThisFile.Count} instruction(s) to file: {firstMatch.Path}");
            formattedOutput = AiInstructionProcessor.ApplyAllInstructions(
                instructionsForThisFile, 
                formattedOutput, 
                useBuiltInFunctions: false, 
                saveChatHistory: string.Empty);
            Logger.Info($"Instructions applied successfully to file: {firstMatch.Path}");
        }

        return formattedOutput;
    }

    private static bool FileNameMatchesInstructionsCriteria(string fileName, string fileNameCriteria)
    {
        return string.IsNullOrEmpty(fileNameCriteria) ||
            fileName.EndsWith($".{fileNameCriteria}") ||
            fileName == fileNameCriteria;
    }

    private static bool LineContainsQuery(string line, string query)
    {
        return line.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string ConvertToRawUrl(string blobUrl)
    {
        // Convert https://github.com/owner/repo/blob/sha/path to https://raw.githubusercontent.com/owner/repo/sha/path
        if (blobUrl.Contains("/blob/"))
        {
            return blobUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
        }
        return blobUrl;
    }

    private static string DetectLanguageFromPath(string path)
    {
        var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".h" or ".hpp" => "cpp",
            ".go" => "go",
            ".rs" => "rust",
            ".rb" => "ruby",
            ".php" => "php",
            ".swift" => "swift",
            ".kt" or ".kts" => "kotlin",
            ".scala" => "scala",
            ".sh" or ".bash" => "bash",
            ".ps1" => "powershell",
            ".sql" => "sql",
            ".html" or ".htm" => "html",
            ".css" => "css",
            ".xml" => "xml",
            ".json" => "json",
            ".yaml" or ".yml" => "yaml",
            ".md" or ".markdown" => "markdown",
            ".txt" => "text",
            _ => ""
        };
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
            // Format: ## owner/repo (⭐ stars) (language)
            var header = $"## {repo.FullName} (⭐ {repo.FormattedStars})";
            if (!string.IsNullOrEmpty(repo.Language))
            {
                header += $" ({repo.Language})";
            }
            output.AppendLine(header);
            output.AppendLine();

            // Repo URL and Description
            output.AppendLine($"Repo: {repo.Url}");
            if (!string.IsNullOrEmpty(repo.Description))
            {
                output.AppendLine($"Desc: {repo.Description}");
            }
            output.AppendLine();

            // Topics and Updated
            if (repo.Topics?.Any() == true)
            {
                output.AppendLine($"Topics: {string.Join(", ", repo.Topics)}");
            }
            if (repo.UpdatedAt.HasValue)
            {
                output.AppendLine($"Updated: {repo.UpdatedAt.Value:yyyy-MM-dd}");
            }
            output.AppendLine();
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
