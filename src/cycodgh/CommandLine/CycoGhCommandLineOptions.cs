using System;
using System.Collections.Generic;
using System.Linq;

public class CycoGhCommandLineOptions : CommandLineOptions
{
    public static bool Parse(string[] args, out CommandLineOptions? options, out CommandLineException? ex)
    {
        options = new CycoGhCommandLineOptions();
        return options.Parse(args, out ex);
    }

    override protected Command? NewDefaultCommand()
    {
        return new SearchCommand();
    }

    override protected Command? NewCommandFromName(string commandName)
    {
        return commandName switch
        {
            "search" => new SearchCommand(),
            _ => base.NewCommandFromName(commandName)
        };
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        return TryParseSearchCommandOptions(command as SearchCommand, args, ref i, arg) ||
               TryParseSharedCycoGhCommandOptions(command as CycoGhCommand, args, ref i, arg);
    }

    private bool TryParseSearchCommandOptions(SearchCommand? command, string[] args, ref int i, string arg)
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
        else if (arg == "--in-files" || arg == "--file-extension")
        {
            var ext = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            if (string.IsNullOrWhiteSpace(ext))
            {
                throw new CommandLineException($"Missing file extension for {arg}");
            }
            command.FileExtension = ext!;
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

    private bool TryParseSharedCycoGhCommandOptions(CycoGhCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--save-output")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var saveOutput = max1Arg.FirstOrDefault() ?? "search-output.md";
            command.SaveOutput = saveOutput;
            i += max1Arg.Count();
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
}
