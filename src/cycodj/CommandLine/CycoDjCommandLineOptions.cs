using System;
using CycoDj.CommandLineCommands;

namespace CycoDj.CommandLine;

public class CycoDjCommandLineOptions : CommandLineOptions
{
    public static bool Parse(string[] args, out CommandLineOptions? options, out CommandLineException? ex)
    {
        options = new CycoDjCommandLineOptions();
        return options.Parse(args, out ex);
    }

    override protected Command? NewDefaultCommand()
    {
        // Default to list command if no command specified
        return new ListCommand();
    }

    override protected string PeekCommandName(string[] args, int i)
    {
        var name = base.PeekCommandName(args, i);
        
        // For single-word commands, just return the command name
        var firstWord = name.Split(' ')[0].ToLowerInvariant();
        if (firstWord == "list" || firstWord == "show" || firstWord == "journal" || 
            firstWord == "branches" || firstWord == "search" || firstWord == "export")
        {
            return firstWord;
        }
        
        return name;
    }


    override protected Command? NewCommandFromName(string commandName)
    {
        var lowerCommandName = commandName.ToLowerInvariant();
        
        if (lowerCommandName.StartsWith("list")) return new ListCommand();
        if (lowerCommandName.StartsWith("show")) return new ShowCommand();
        if (lowerCommandName.StartsWith("journal")) return new JournalCommand();
        if (lowerCommandName.StartsWith("branches")) return new BranchesCommand();
        if (lowerCommandName.StartsWith("search")) return new SearchCommand();
        if (lowerCommandName.StartsWith("export")) return new ExportCommand();
        
        return base.NewCommandFromName(commandName);
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        if (command is ListCommand listCommand)
        {
            return TryParseListCommandOptions(listCommand, args, ref i, arg);
        }
        else if (command is ShowCommand showCommand)
        {
            return TryParseShowCommandOptions(showCommand, args, ref i, arg);
        }
        else if (command is JournalCommand journalCommand)
        {
            return TryParseJournalCommandOptions(journalCommand, args, ref i, arg);
        }
        else if (command is BranchesCommand branchesCommand)
        {
            return TryParseBranchesCommandOptions(branchesCommand, args, ref i, arg);
        }
        else if (command is SearchCommand searchCommand)
        {
            return TryParseSearchCommandOptions(searchCommand, args, ref i, arg);
        }
        else if (command is ExportCommand exportCommand)
        {
            return TryParseExportCommandOptions(exportCommand, args, ref i, arg);
        }
        
        return false;
    }

    private bool TryParseListCommandOptions(ListCommand command, string[] args, ref int i, string arg)
    {
        if (arg == "--date" || arg == "-d")
        {
            var date = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new CommandLineException($"Missing date value for {arg}");
            }
            command.Date = date;
            return true;
        }
        else if (arg == "--last")
        {
            var count = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(count) || !int.TryParse(count, out var n))
            {
                throw new CommandLineException($"Missing or invalid count for {arg}");
            }
            command.Last = n;
            return true;
        }
        
        return false;
    }

    private bool TryParseBranchesCommandOptions(BranchesCommand command, string[] args, ref int i, string arg)
    {
        if (arg == "--date" || arg == "-d")
        {
            var date = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new CommandLineException($"Missing date value for {arg}");
            }
            command.Date = date;
            return true;
        }
        else if (arg == "--conversation" || arg == "-c")
        {
            var conv = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(conv))
            {
                throw new CommandLineException($"Missing conversation value for {arg}");
            }
            command.Conversation = conv;
            return true;
        }
        else if (arg == "--verbose" || arg == "-v")
        {
            command.Verbose = true;
            return true;
        }
        
        return false;
    }

    private bool TryParseShowCommandOptions(ShowCommand command, string[] args, ref int i, string arg)
    {
        // First positional argument is the conversation ID
        if (!arg.StartsWith("-") && string.IsNullOrEmpty(command.ConversationId))
        {
            command.ConversationId = arg;
            return true;
        }
        
        if (arg == "--show-tool-calls")
        {
            command.ShowToolCalls = true;
            return true;
        }
        else if (arg == "--show-tool-output")
        {
            command.ShowToolOutput = true;
            return true;
        }
        else if (arg == "--max-content-length")
        {
            var length = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(length) || !int.TryParse(length, out var n))
            {
                throw new CommandLineException($"Missing or invalid length for {arg}");
            }
            command.MaxContentLength = n;
            return true;
        }
        
        return false;
    }

    private bool TryParseJournalCommandOptions(JournalCommand command, string[] args, ref int i, string arg)
    {
        if (arg == "--date" || arg == "-d")
        {
            var date = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new CommandLineException($"Missing date value for {arg}");
            }
            command.Date = date;
            return true;
        }
        else if (arg == "--last-days")
        {
            var days = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(days) || !int.TryParse(days, out var n))
            {
                throw new CommandLineException($"Missing or invalid days for {arg}");
            }
            command.LastDays = n;
            return true;
        }
        else if (arg == "--detailed")
        {
            command.Detailed = true;
            return true;
        }
        
        return false;
    }

    private bool TryParseSearchCommandOptions(SearchCommand command, string[] args, ref int i, string arg)
    {
        // First positional argument is the search query
        if (!arg.StartsWith("-") && string.IsNullOrEmpty(command.Query))
        {
            command.Query = arg;
            return true;
        }
        
        if (arg == "--date" || arg == "-d")
        {
            var date = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new CommandLineException($"Missing date value for {arg}");
            }
            command.Date = date;
            return true;
        }
        else if (arg == "--last")
        {
            var count = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(count) || !int.TryParse(count, out var n))
            {
                throw new CommandLineException($"Missing or invalid count for {arg}");
            }
            command.Last = n;
            return true;
        }
        else if (arg == "--case-sensitive" || arg == "-c")
        {
            command.CaseSensitive = true;
            return true;
        }
        else if (arg == "--regex" || arg == "-r")
        {
            command.UseRegex = true;
            return true;
        }
        else if (arg == "--user-only" || arg == "-u")
        {
            command.UserOnly = true;
            return true;
        }
        else if (arg == "--assistant-only" || arg == "-a")
        {
            command.AssistantOnly = true;
            return true;
        }
        else if (arg == "--context" || arg == "-C")
        {
            var lines = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(lines) || !int.TryParse(lines, out var n))
            {
                throw new CommandLineException($"Missing or invalid context lines for {arg}");
            }
            command.ContextLines = n;
            return true;
        }
        
        return false;
    }

    private bool TryParseExportCommandOptions(ExportCommand command, string[] args, ref int i, string arg)
    {
        if (arg == "--output" || arg == "-o")
        {
            var outputFile = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new CommandLineException($"Missing output file for {arg}");
            }
            command.OutputFile = outputFile;
            return true;
        }
        else if (arg == "--date" || arg == "-d")
        {
            var date = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(date))
            {
                throw new CommandLineException($"Missing date value for {arg}");
            }
            command.Date = date;
            return true;
        }
        else if (arg == "--last")
        {
            var count = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(count) || !int.TryParse(count, out var n))
            {
                throw new CommandLineException($"Missing or invalid count for {arg}");
            }
            command.Last = n;
            return true;
        }
        else if (arg == "--conversation" || arg == "-c")
        {
            var id = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new CommandLineException($"Missing conversation ID for {arg}");
            }
            command.ConversationId = id;
            return true;
        }
        else if (arg == "--include-tool-output")
        {
            command.IncludeToolOutput = true;
            return true;
        }
        else if (arg == "--no-branches")
        {
            command.IncludeBranches = false;
            return true;
        }
        else if (arg == "--overwrite")
        {
            command.Overwrite = true;
            return true;
        }
        
        return false;
    }


}
