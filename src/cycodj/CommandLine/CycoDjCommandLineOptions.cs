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
        if (firstWord == "list" || firstWord == "show" || 
            firstWord == "branches" || firstWord == "search" || 
            firstWord == "stats" || firstWord == "cleanup")
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
        if (lowerCommandName.StartsWith("branches")) return new BranchesCommand();
        if (lowerCommandName.StartsWith("search")) return new SearchCommand();
        if (lowerCommandName.StartsWith("stats")) return new StatsCommand();
        if (lowerCommandName.StartsWith("cleanup")) return new CleanupCommand();
        
        return base.NewCommandFromName(commandName);
    }

    /// <summary>
    /// Try to parse common instruction-related options for all cycodj commands
    /// </summary>
    private bool TryParseCommonInstructionOptions(CycoDjCommand command, string[] args, ref int i, string arg)
    {
        if (arg == "--instructions")
        {
            var instructions = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new CommandLineException($"Missing instructions value for {arg}");
            }
            command.Instructions = instructions;
            return true;
        }
        else if (arg == "--use-built-in-functions")
        {
            command.UseBuiltInFunctions = true;
            return true;
        }
        else if (arg == "--save-chat-history")
        {
            var savePath = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(savePath))
            {
                throw new CommandLineException($"Missing path value for {arg}");
            }
            command.SaveChatHistory = savePath;
            return true;
        }
        
        return false;
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        // Try common instruction options first for all cycodj commands
        if (command is CycoDjCommand cycodjCommand && TryParseCommonInstructionOptions(cycodjCommand, args, ref i, arg))
        {
            return true;
        }
        
        if (command is ListCommand listCommand)
        {
            return TryParseListCommandOptions(listCommand, args, ref i, arg);
        }
        else if (command is ShowCommand showCommand)
        {
            return TryParseShowCommandOptions(showCommand, args, ref i, arg);
        }
        else if (command is BranchesCommand branchesCommand)
        {
            return TryParseBranchesCommandOptions(branchesCommand, args, ref i, arg);
        }
        else if (command is SearchCommand searchCommand)
        {
            return TryParseSearchCommandOptions(searchCommand, args, ref i, arg);
        }
        else if (command is StatsCommand statsCommand)
        {
            return TryParseStatsCommandOptions(statsCommand, args, ref i, arg);
        }
        else if (command is CleanupCommand cleanupCommand)
        {
            return TryParseCleanupCommandOptions(cleanupCommand, args, ref i, arg);
        }
        
        return false;
    }

    /// <summary>
    /// Try to parse common display options (--messages, --stats, --branches)
    /// Returns true if option was handled
    /// </summary>
    private bool TryParseDisplayOptions(CycoDjCommand command, string[] args, ref int i, string arg)
    {
        // --messages [N|all]
        if (arg == "--messages")
        {
            // Check if next arg is a value (not another option)
            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                var value = args[++i];
                if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // Set to a large number (all messages)
                    SetMessageCount(command, int.MaxValue);
                }
                else if (int.TryParse(value, out var count))
                {
                    SetMessageCount(command, count);
                }
                else
                {
                    throw new CommandLineException($"Invalid value for --messages: {value}");
                }
            }
            else
            {
                // No value provided, set to null (use command default)
                SetMessageCount(command, null);
            }
            return true;
        }
        
        // --stats
        else if (arg == "--stats")
        {
            SetShowStats(command, true);
            return true;
        }
        
        // --branches (for list/search commands)
        else if (arg == "--branches")
        {
            SetShowBranches(command, true);
            return true;
        }
        
        // --save-output <file>
        else if (arg == "--save-output")
        {
            var outputFile = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new CommandLineException($"Missing file path for --save-output");
            }
            command.SaveOutput = outputFile;
            return true;
        }
        
        return false;
    }

    private void SetMessageCount(CycoDjCommand command, int? value)
    {
        var prop = command.GetType().GetProperty("MessageCount");
        if (prop != null)
        {
            prop.SetValue(command, value);
        }
    }

    private void SetShowStats(CycoDjCommand command, bool value)
    {
        var prop = command.GetType().GetProperty("ShowStats");
        if (prop != null)
        {
            prop.SetValue(command, value);
        }
    }

    private void SetShowBranches(CycoDjCommand command, bool value)
    {
        var prop = command.GetType().GetProperty("ShowBranches");
        if (prop != null)
        {
            prop.SetValue(command, value);
        }
    }

    /// <summary>
    /// Try to parse common time filtering options (--today, --yesterday, --last, --after, --before, --date-range)
    /// Returns true if option was handled
    /// </summary>
    private bool TryParseTimeOptions(CycoDjCommand command, string[] args, ref int i, string arg)
    {
        // --today shortcut (calendar day)
        if (arg == "--today")
        {
            command.After = DateTime.Today;
            command.Before = DateTime.Now;
            return true;
        }
        
        // --yesterday shortcut (calendar day)
        else if (arg == "--yesterday")
        {
            command.After = DateTime.Today.AddDays(-1);
            command.Before = DateTime.Today.AddTicks(-1);
            return true;
        }
        
        // --after <timespec>
        else if (arg == "--after" || arg == "--time-after")
        {
            var timeSpec = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(timeSpec))
            {
                throw new CommandLineException($"Missing timespec value for {arg}");
            }
            command.After = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: true);
            return true;
        }
        
        // --before <timespec>
        else if (arg == "--before" || arg == "--time-before")
        {
            var timeSpec = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(timeSpec))
            {
                throw new CommandLineException($"Missing timespec value for {arg}");
            }
            command.Before = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: false);
            return true;
        }
        
        // --date-range <range> or --time-range <range>
        else if (arg == "--date-range" || arg == "--time-range")
        {
            var timeSpec = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(timeSpec))
            {
                throw new CommandLineException($"Missing timespec range for {arg}");
            }
            var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
            command.After = after;
            command.Before = before;
            return true;
        }
        
        return false;
    }

    private bool TryParseListCommandOptions(ListCommand command, string[] args, ref int i, string arg)
    {
        // Try common display options first
        if (TryParseDisplayOptions(command, args, ref i, arg))
        {
            return true;
        }
        
        // Try common time options
        if (TryParseTimeOptions(command, args, ref i, arg))
        {
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
            var value = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandLineException($"Missing value for {arg}");
            }
            
            ParseLastValue(command, arg, value);
            return true;
        }
        
        return false;
    }

    private bool TryParseBranchesCommandOptions(BranchesCommand command, string[] args, ref int i, string arg)
    {
        // Try common display options first
        if (TryParseDisplayOptions(command, args, ref i, arg))
        {
            return true;
        }
        
        // Try common time options
        if (TryParseTimeOptions(command, args, ref i, arg))
        {
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
            var value = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandLineException($"Missing value for {arg}");
            }
            
            ParseLastValue(command, arg, value);
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
        
        // Try common display options first
        if (TryParseDisplayOptions(command, args, ref i, arg))
        {
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

    private bool TryParseSearchCommandOptions(SearchCommand command, string[] args, ref int i, string arg)
    {
        // First positional argument is the search query
        if (!arg.StartsWith("-") && string.IsNullOrEmpty(command.Query))
        {
            command.Query = arg;
            return true;
        }
        
        // Try common display options first
        if (TryParseDisplayOptions(command, args, ref i, arg))
        {
            return true;
        }
        
        // Try common time options
        if (TryParseTimeOptions(command, args, ref i, arg))
        {
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
            var value = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandLineException($"Missing value for {arg}");
            }
            
            ParseLastValue(command, arg, value);
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

    private bool TryParseStatsCommandOptions(StatsCommand command, string[] args, ref int i, string arg)
    {
        // Try common display options first
        if (TryParseDisplayOptions(command, args, ref i, arg))
        {
            return true;
        }
        
        // Try common time options
        if (TryParseTimeOptions(command, args, ref i, arg))
        {
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
            var value = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new CommandLineException($"Missing value for {arg}");
            }
            
            ParseLastValue(command, arg, value);
            return true;
        }
        else if (arg == "--show-tools")
        {
            command.ShowTools = true;
            return true;
        }
        else if (arg == "--no-dates")
        {
            command.ShowDates = false;
            return true;
        }
        
        return false;
    }

    private bool TryParseCleanupCommandOptions(CleanupCommand command, string[] args, ref int i, string arg)
    {
        if (arg == "--find-duplicates") { command.FindDuplicates = true; return true; }
        else if (arg == "--remove-duplicates") { command.RemoveDuplicates = true; command.FindDuplicates = true; return true; }
        else if (arg == "--find-empty") { command.FindEmpty = true; return true; }
        else if (arg == "--remove-empty") { command.RemoveEmpty = true; command.FindEmpty = true; return true; }
        else if (arg == "--older-than-days") 
        { 
            var days = i + 1 < args.Length ? args[++i] : null;
            if (string.IsNullOrWhiteSpace(days) || !int.TryParse(days, out var n))
                throw new CommandLineException($"Missing or invalid days for {arg}");
            command.OlderThanDays = n;
            return true;
        }
        else if (arg == "--execute") { command.DryRun = false; return true; }
        return false;
    }

    /// <summary>
    /// Parse a value for --last: either TIMESPEC or conversation count
    /// For TIMESPEC like "7d", automatically makes it negative (7 days ago) and creates range
    /// </summary>
    private void ParseLastValue(CycoDjCommand command, string arg, string value)
    {
        // Smart detection: TIMESPEC vs conversation count
        if (IsTimeSpec(value))
        {
            // Parse as TIMESPEC
            try
            {
                // For --last context, relative times should go BACKWARD (ago)
                // If value is like "7d", convert to "-7d.." (7 days ago to now)
                var timeSpec = value;
                if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d+[dhms]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    timeSpec = "-" + value + ".."; // Make it a range from N ago to now
                }
                
                var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
                command.After = after;
                command.Before = before;
            }
            catch (Exception ex)
            {
                throw new CommandLineException($"Invalid time specification for --last: {value}\n{ex.Message}");
            }
        }
        else
        {
            // Parse as conversation count (for ListCommand, SearchCommand, etc.)
            if (!int.TryParse(value, out var count))
            {
                throw new CommandLineException($"Invalid number for --last: {value}");
            }
            
            // Set Last property if it exists on the command
            var lastProp = command.GetType().GetProperty("Last");
            if (lastProp != null)
            {
                lastProp.SetValue(command, count);
            }
        }
    }

    /// <summary>
    /// Determines if a value is a TIMESPEC (vs. a plain number for conversation count)
    /// </summary>
    private static bool IsTimeSpec(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        
        // Has range syntax?
        if (value.Contains("..")) return true;
        
        // Is keyword?
        if (value.Equals("today", StringComparison.OrdinalIgnoreCase)) return true;
        if (value.Equals("yesterday", StringComparison.OrdinalIgnoreCase)) return true;
        
        // Has time units (d, h, m, s)?
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"[dhms]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) 
            return true;
        
        // Pure number = conversation count
        return false;
    }

}
