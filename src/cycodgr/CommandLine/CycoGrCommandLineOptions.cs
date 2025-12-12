using System;
using System.Collections.Generic;
using System.Linq;

public class CycoGrCommandLineOptions : CommandLineOptions
{
    public static bool Parse(string[] args, out CommandLineOptions? options, out CommandLineException? ex)
    {
        options = new CycoGrCommandLineOptions();
        return options.Parse(args, out ex);
    }

    override protected Command? NewDefaultCommand()
    {
        return new RepoCommand();
    }

    override protected string PeekCommandName(string[] args, int i)
    {
        var name1 = GetInputOptionArgs(i, args, max: 1).FirstOrDefault();
        return name1 switch
        {
            "repo" => "repo",
            "code" => "code",
            _ => base.PeekCommandName(args, i)
        };
    }

    override protected Command? NewCommandFromName(string commandName)
    {
        return commandName switch
        {
            "repo" => new RepoCommand(),
            "code" => new CodeCommand(),
            _ => base.NewCommandFromName(commandName)
        };
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        return TryParseRepoCommandOptions(command as RepoCommand, args, ref i, arg) ||
               TryParseCodeCommandOptions(command as CodeCommand, args, ref i, arg) ||
               TryParseSharedCycoGrCommandOptions(command as CycoGrCommand, args, ref i, arg);
    }

    private bool TryParseRepoCommandOptions(RepoCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
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
        else if (arg == "--format")
        {
            var format = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new CommandLineException($"Missing format for {arg}");
            }
            var validFormats = new[] { "url", "table", "json", "csv", "detailed" };
            if (!validFormats.Contains(format.ToLower()))
            {
                throw new CommandLineException($"Invalid format '{format}'. Valid formats: {string.Join(", ", validFormats)}");
            }
            command.Format = format.ToLower();
        }
        else if (!arg.StartsWith("--"))
        {
            // Positional arguments are keywords
            command.Keywords.Add(arg);
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private bool TryParseCodeCommandOptions(CodeCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--max-results")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MaxResults = ValidatePositiveNumber(arg, countStr);
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
        else if (arg == "--min-stars")
        {
            var starsStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.MinStars = ValidateNonNegativeNumber(arg, starsStr);
        }
        else if (arg == "--in-files" || arg == "--file-extension" || arg == "--extension")
        {
            var ext = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new CommandLineException($"Missing file extension for {arg}");
            }
            command.FileExtension = ext!;
        }
        else if (arg == "--format")
        {
            var format = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new CommandLineException($"Missing format for {arg}");
            }
            var validFormats = new[] { "detailed", "filenames", "files", "repos", "urls", "json", "csv" };
            if (!validFormats.Contains(format.ToLower()))
            {
                throw new CommandLineException($"Invalid format '{format}'. Valid formats: {string.Join(", ", validFormats)}");
            }
            command.Format = format.ToLower();
        }
        else if (arg == "--lines-before-and-after" || arg == "--lines")
        {
            var linesStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.LinesBeforeAndAfter = ValidateNonNegativeNumber(arg, linesStr);
        }
        else if (!arg.StartsWith("--"))
        {
            // Positional arguments are keywords
            command.Keywords.Add(arg);
        }
        else
        {
            parsed = false;
        }

        return parsed;
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
