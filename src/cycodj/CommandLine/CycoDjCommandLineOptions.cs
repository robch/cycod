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

    override protected Command? NewCommandFromName(string commandName)
    {
        return commandName.ToLowerInvariant() switch
        {
            "list" => new ListCommand(),
            "branches" => new BranchesCommand(),
            _ => base.NewCommandFromName(commandName)
        };
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        if (command is ListCommand listCommand)
        {
            return TryParseListCommandOptions(listCommand, args, ref i, arg);
        }
        else if (command is BranchesCommand branchesCommand)
        {
            return TryParseBranchesCommandOptions(branchesCommand, args, ref i, arg);
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
}
