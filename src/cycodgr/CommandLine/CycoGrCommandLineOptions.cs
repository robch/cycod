using System;
using System.Collections.Generic;
using System.Linq;
using CycoGr.CommandLineCommands;

namespace CycoGr.CommandLine;

public class CycoGrCommandLineOptions : CommandLineOptions
{
    public static bool Parse(string[] args, out CommandLineOptions? options, out CommandLineException? ex)
    {
        options = new CycoGrCommandLineOptions();
        return options.Parse(args, out ex);
    }

    override protected Command? NewDefaultCommand()
    {
        return new SearchCommand();
    }

    override protected Command? NewCommandFromName(string commandName)
    {
        return base.NewCommandFromName(commandName);
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        return TryParseSearchCommandOptions(command as SearchCommand, args, ref i, arg) ||
               TryParseSharedCycoGrCommandOptions(command as CycoGrCommand, args, ref i, arg);
    }

    private bool TryParseSearchCommandOptions(SearchCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        // Content search flags
        if (arg == "--contains")
        {
            var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(terms))
            {
                throw new CommandLineException($"Missing search terms for {arg}");
            }
            command.Contains = terms!;
        }
        else if (arg == "--file-contains")
        {
            var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(terms))
            {
                throw new CommandLineException($"Missing search terms for {arg}");
            }
            command.FileContains = terms!;
        }
        else if (arg == "--repo-contains")
        {
            var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(terms))
            {
                throw new CommandLineException($"Missing search terms for {arg}");
            }
            command.RepoContains = terms!;
        }
        else if (arg == "--repo-file-contains")
        {
            var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(terms))
            {
                throw new CommandLineException($"Missing search terms for {arg}");
            }
            command.RepoFileContains = terms!;
        }
        // Extension-specific repo-file-contains shortcuts
        else if (arg.StartsWith("--repo-") && arg.EndsWith("-file-contains"))
        {
            // Extract extension: --repo-csproj-file-contains → csproj
            var prefix = "--repo-";
            var suffix = "-file-contains";
            var ext = arg.Substring(prefix.Length, arg.Length - prefix.Length - suffix.Length);
            var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(terms))
            {
                throw new CommandLineException($"Missing search terms for {arg}");
            }
            command.RepoFileContains = terms!;
            command.RepoFileContainsExtension = MapExtensionToLanguage(ext);
        }
        // Extension-specific file-contains shortcuts
        else if (arg.StartsWith("--") && arg.EndsWith("-file-contains"))
        {
            // Extract extension: --cs-file-contains → cs
            var ext = arg.Substring(2, arg.Length - 2 - "-file-contains".Length);
            var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(terms))
            {
                throw new CommandLineException($"Missing search terms for {arg}");
            }
            command.FileContains = terms!;
            command.Language = MapExtensionToLanguage(ext);
        }
        // Other options
        else if (arg == "--max-results")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MaxResults = ValidatePositiveNumber(arg, countStr);
        }
        else if (arg == "--clone")
        {
            command.Clone = true;
        }
        else if (arg == "--max-clone")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MaxClone = ValidatePositiveNumber(arg, countStr);
        }
        else if (arg == "--clone-dir")
        {
            var dir = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new CommandLineException($"Missing directory path for {arg}");
            }
            command.CloneDirectory = dir!;
        }
        else if (arg == "--as-submodules")
        {
            command.AsSubmodules = true;
        }
        else if (arg == "--language")
        {
            var lang = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(lang))
            {
                throw new CommandLineException($"Missing language for {arg}");
            }
            command.Language = lang!;
        }
        // Language shortcuts - Tier 1 (Primary)
        else if (arg == "--cs" || arg == "--csharp")
        {
            command.Language = "csharp";
        }
        else if (arg == "--js" || arg == "--javascript")
        {
            command.Language = "javascript";
        }
        else if (arg == "--ts" || arg == "--typescript")
        {
            command.Language = "typescript";
        }
        else if (arg == "--py" || arg == "--python")
        {
            command.Language = "python";
        }
        else if (arg == "--java")
        {
            command.Language = "java";
        }
        else if (arg == "--go")
        {
            command.Language = "go";
        }
        else if (arg == "--md" || arg == "--markdown")
        {
            command.Language = "markdown";
        }
        // Language shortcuts - Tier 2 (Popular)
        else if (arg == "--rb" || arg == "--ruby")
        {
            command.Language = "ruby";
        }
        else if (arg == "--rs" || arg == "--rust")
        {
            command.Language = "rust";
        }
        else if (arg == "--php")
        {
            command.Language = "php";
        }
        else if (arg == "--cpp" || arg == "--c++")
        {
            command.Language = "cpp";
        }
        else if (arg == "--swift")
        {
            command.Language = "swift";
        }
        else if (arg == "--kt" || arg == "--kotlin")
        {
            command.Language = "kotlin";
        }
        // Language shortcuts - Tier 3 (Config/Markup)
        else if (arg == "--yml" || arg == "--yaml")
        {
            command.Language = "yaml";
        }
        else if (arg == "--json")
        {
            command.Language = "json";
        }
        else if (arg == "--xml")
        {
            command.Language = "xml";
        }
        else if (arg == "--html")
        {
            command.Language = "html";
        }
        else if (arg == "--css")
        {
            command.Language = "css";
        }
        else if (arg == "--owner")
        {
            var owner = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new CommandLineException($"Missing owner/organization for {arg}");
            }
            command.Owner = owner!;
        }
        else if (arg == "--sort")
        {
            var sort = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(sort))
            {
                throw new CommandLineException($"Missing sort field for {arg}");
            }
            command.SortBy = sort!;
        }
        else if (arg == "--include-forks")
        {
            command.IncludeForks = true;
        }
        else if (arg == "--exclude-fork")
        {
            command.ExcludeForks = true;
        }
        else if (arg == "--only-forks")
        {
            command.OnlyForks = true;
        }
        else if (arg == "--min-stars")
        {
            var starsStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MinStars = ValidateNonNegativeNumber(arg, starsStr);
        }
        else if (arg == "--lines-before-and-after" || arg == "--lines")
        {
            var linesStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.LinesBeforeAndAfter = ValidateNonNegativeNumber(arg, linesStr);
        }
        else if (arg == "--line-contains" && command is CycoGr.CommandLineCommands.SearchCommand searchCmd3)
        {
            var patterns = GetInputOptionArgs(i + 1, args, required: 1);
            searchCmd3.LineContainsPatterns.AddRange(patterns);
            i += patterns.Count();
        }
        else if (arg == "--extension" || arg == "--in-files")
        {
            var ext = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new CommandLineException($"Missing extension for {arg}");
            }
            command.Language = MapExtensionToLanguage(ext!);
        }
        else if (arg == "--format")
        {
            var format = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new CommandLineException($"Missing format for {arg}");
            }
            command.Format = format!;
        }
        else if (arg == "--file-instructions")
        {
            var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            command.FileInstructionsList.Add(new Tuple<string, string>(instructions!, ""));
        }
        else if (arg.StartsWith("--") && arg.EndsWith("-file-instructions"))
        {
            // Extract extension: --cs-file-instructions → cs, --md-file-instructions → md
            var ext = arg.Substring(2, arg.Length - 2 - "-file-instructions".Length);
            var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            command.FileInstructionsList.Add(new Tuple<string, string>(instructions!, ext));
        }
        else if (arg == "--repo-instructions")
        {
            var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            command.RepoInstructionsList.Add(instructions!);
        }
        else if (arg == "--instructions")
        {
            var instructions = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            command.InstructionsList.Add(instructions!);
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    override protected bool TryParseOtherCommandArg(Command? command, string arg)
    {
        if (command is SearchCommand searchCommand)
        {
            searchCommand.RepoPatterns.Add(arg);
            return true;
        }

        return false;
    }

    private string MapExtensionToLanguage(string ext)
    {
        // Map file extension shorthand to GitHub language names
        return ext.ToLower() switch
        {
            "cs" => "csharp",
            "js" => "javascript",
            "ts" => "typescript",
            "py" => "python",
            "rb" => "ruby",
            "rs" => "rust",
            "kt" => "kotlin",
            "cpp" => "cpp",
            "c++" => "cpp",
            "yml" => "yaml",
            "md" => "markdown",
            _ => ext.ToLower()  // Pass through as-is
        };
    }

    private bool TryParseSharedCycoGrCommandOptions(CycoGrCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--repo")
        {
            var repo = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(repo))
            {
                throw new CommandLineException($"Missing repository name for {arg}");
            }
            command.Repos.Add(repo!);
        }
        else if (arg == "--repos")
        {
            var repoArgs = GetInputOptionArgs(i + 1, args);
            foreach (var repoArg in repoArgs)
            {
                if (repoArg.StartsWith("@"))
                {
                    // Load repos from file
                    var fileName = repoArg.Substring(1);
                    if (!FileHelpers.FileExists(fileName))
                    {
                        throw new CommandLineException($"Repository list file not found: {fileName}");
                    }
                    var reposFromFile = FileHelpers.ReadAllLines(fileName)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim());
                    command.Repos.AddRange(reposFromFile);
                }
                else
                {
                    command.Repos.Add(repoArg);
                }
            }
            i += repoArgs.Count();
        }
        else if (arg == "--file-path" && command is CycoGr.CommandLineCommands.SearchCommand searchCmd)
        {
            var path = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new CommandLineException($"Missing file path for {arg}");
            }
            searchCmd.FilePaths.Add(path!);
        }
        else if (arg == "--file-paths" && command is CycoGr.CommandLineCommands.SearchCommand searchCmd2)
        {
            var pathArgs = GetInputOptionArgs(i + 1, args);
            
            foreach (var pathArg in pathArgs)
            {
                // Handle @ file loading (if @ wasn't already expanded)
                if (pathArg.StartsWith("@"))
                {
                    var fileName = pathArg.Substring(1);
                    if (!FileHelpers.FileExists(fileName))
                    {
                        throw new CommandLineException($"File paths file not found: {fileName}");
                    }
                    var fileContent = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                    var normalized = fileContent.Replace("\r\n", "\n").Replace("\r", "\n");
                    var pathsFromFile = normalized
                        .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim());
                    searchCmd2.FilePaths.AddRange(pathsFromFile);
                }
                // Handle case where @ was already expanded to a single string with embedded newlines
                else if (pathArg.Contains("\n") || pathArg.Contains("\r"))
                {
                    var normalized = pathArg.Replace("\r\n", "\n").Replace("\r", "\n");
                    var paths = normalized
                        .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim());
                    searchCmd2.FilePaths.AddRange(paths);
                }
                // Regular path arg
                else
                {
                    searchCmd2.FilePaths.Add(pathArg);
                }
            }
            i += pathArgs.Count();
        }
        else if (arg == "--save-output")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var saveOutput = max1Arg.FirstOrDefault() ?? "search-output.md";
            command.SaveOutput = saveOutput;
            i += max1Arg.Count();
        }
        else if (arg == "--save-json")
        {
            var file = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new CommandLineException($"Missing file path for {arg}");
            }
            command.SaveJson = file!;
        }
        else if (arg == "--save-csv")
        {
            var file = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new CommandLineException($"Missing file path for {arg}");
            }
            command.SaveCsv = file!;
        }
        else if (arg == "--save-table")
        {
            var file = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new CommandLineException($"Missing file path for {arg}");
            }
            command.SaveTable = file!;
        }
        else if (arg == "--save-urls")
        {
            var file = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new CommandLineException($"Missing file path for {arg}");
            }
            command.SaveUrls = file!;
        }
        else if (arg == "--save-repos")
        {
            var file = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new CommandLineException($"Missing file path for {arg}");
            }
            command.SaveRepos = file!;
        }
        else if (arg == "--save-file-paths")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var template = max1Arg.FirstOrDefault() ?? "files-{repo}.txt";
            command.SaveFilePaths = template;
            i += max1Arg.Count();
        }
        else if (arg == "--save-repo-urls")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var template = max1Arg.FirstOrDefault() ?? "repo-urls.txt";
            command.SaveRepoUrls = template;
            i += max1Arg.Count();
        }
        else if (arg == "--save-file-urls")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var template = max1Arg.FirstOrDefault() ?? "file-urls-{repo}.txt";
            command.SaveFileUrls = template;
            i += max1Arg.Count();
        }
        else if (arg == "--exclude")
        {
            var excludeArgs = GetInputOptionArgs(i + 1, args);
            if (excludeArgs.Count() == 0)
            {
                throw new CommandLineException($"Missing pattern(s) for {arg}");
            }
            command.Exclude.AddRange(excludeArgs);
            i += excludeArgs.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private int ValidatePositiveNumber(string arg, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CommandLineException($"Missing value for {arg}");
        }

        if (!int.TryParse(value, out var number) || number <= 0)
        {
            throw new CommandLineException($"Invalid value for {arg}: '{value}' (must be a positive integer)");
        }

        return number;
    }

    private int ValidateNonNegativeNumber(string arg, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CommandLineException($"Missing value for {arg}");
        }

        if (!int.TryParse(value, out var number) || number < 0)
        {
            throw new CommandLineException($"Invalid value for {arg}: '{value}' (must be a non-negative integer)");
        }

        return number;
    }
}
