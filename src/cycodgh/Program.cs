using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        CycoGhProgramInfo _programInfo = new();

        LoggingInitializer.InitializeMemoryLogger();
        LoggingInitializer.LogStartupDetails(args);
        Logger.Info($"Starting {ProgramInfo.Name}, version {VersionInfo.GetVersion()}");

        if (!CycoGhCommandLineOptions.Parse(args, out var commandLineOptions, out var ex))
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
            if (command is SearchCommand searchCommand)
            {
                await HandleSearchCommandAsync(searchCommand);
            }
        }

        return 0;
    }

    private static async Task HandleSearchCommandAsync(SearchCommand command)
    {
        try
        {
            var query = string.Join(" ", command.Keywords);
            var searchType = !string.IsNullOrEmpty(command.FileExtension) 
                ? $"code search (in .{command.FileExtension} files)" 
                : "repository search";
                
            ConsoleHelpers.WriteLine($"## GitHub {searchType} for '{query}'", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLine();

            // Search GitHub
            var repos = await GitHubSearchHelpers.SearchRepositoriesAsync(command);

            if (repos.Count == 0)
            {
                ConsoleHelpers.WriteLine("No results found", ConsoleColor.Yellow);
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
                saveOutput.AppendLine($"## GitHub {searchType} for '{query}'");
                saveOutput.AppendLine();
                saveOutput.AppendLine(output);
                
                var saveFileName = FileHelpers.GetFileNameFromTemplate("search-output.md", command.SaveOutput)!;
                FileHelpers.WriteAllText(saveFileName, saveOutput.ToString());
                ConsoleHelpers.WriteLine($"Saved output to: {saveFileName}", ConsoleColor.Green);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error: {ex.Message}");
            Logger.Error($"Search command failed: {ex.Message}");
            Logger.Error(ex.StackTrace ?? string.Empty);
        }
    }

    private static string FormatRepoOutput(List<CycoGh.Models.RepoInfo> repos, string format)
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

    private static string FormatAsUrls(List<CycoGh.Models.RepoInfo> repos)
    {
        return string.Join(Environment.NewLine, repos.Select(r => r.Url));
    }

    private static string FormatAsDetailed(List<CycoGh.Models.RepoInfo> repos)
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

    private static string FormatAsTable(List<CycoGh.Models.RepoInfo> repos)
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

    private static string FormatAsJson(List<CycoGh.Models.RepoInfo> repos)
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

    private static string FormatAsCsv(List<CycoGh.Models.RepoInfo> repos)
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

    private static void DisplayBanner()
    {
        var programNameUppercase = Name.ToUpper();
        
        if (ProgramInfo.Assembly == null)
        {
            new CycoGhProgramInfo();
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

    public const string Name = "cycodgh";
}
