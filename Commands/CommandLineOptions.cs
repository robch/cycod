using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class CommandLineOptions
{
    public CommandLineOptions()
    {
        Debug = false;
        Verbose = false;

        HelpTopic = string.Empty;
        ExpandHelpTopics = false;

        Commands = new List<Command>();

        AllOptions = new string[0];
        SaveAliasName = null;
    }

    public bool Debug;
    public bool Verbose;

    public string HelpTopic;
    public bool ExpandHelpTopics;

    public List<Command> Commands;

    public string[] AllOptions;
    public string? SaveAliasName;
    private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";

    public static bool Parse(string[] args, out CommandLineOptions? options, out CommandLineException? ex)
    {
        options = null;
        ex = null;

        try
        {
            var allInputs = ExpandedInputsFromCommandLine(args);
            options = ParseInputOptions(allInputs);
            return options.Commands.Any();
        }
        catch (CommandLineException e)
        {
            ex = e;
            return false;
        }
    }

    public List<string> SaveAlias(string aliasName)
    {
        var filesSaved = new List<string>();
        var aliasDirectory = FindAliasDirectory(create: true)!;
        var fileName = Path.Combine(aliasDirectory, aliasName + ".alias");

        var options = AllOptions
            .Where(x => x != "--save-alias" && x != aliasName)
            .Select(x => SingleLineOrNewAtFile(x, fileName, ref filesSaved));

        var asMultiLineString = string.Join('\n', options);
        FileHelpers.WriteAllText(fileName, asMultiLineString);

        filesSaved.Insert(0, fileName);
        return filesSaved;
    }

    private string SingleLineOrNewAtFile(string text, string baseFileName, ref List<string> additionalFiles)
    {
        var isMultiLine = text.Contains('\n') || text.Contains('\r');
        if (!isMultiLine) return text;

        var additionalFileCount = additionalFiles.Count + 1;
        var additionalFileName = FileHelpers.GetFileNameFromTemplate(baseFileName, "{filepath}/{filebase}-" + additionalFileCount + "{fileext}")!;
        additionalFiles.Add(additionalFileName);

        FileHelpers.WriteAllText(additionalFileName, text);

        return "@" + additionalFileName;
    }

    private static IEnumerable<string> ExpandedInputsFromCommandLine(string[] args)
    {
        return args.SelectMany(arg => ExpandedInput(arg));
    }
    
    private static IEnumerable<string> ExpandedInput(string input)
    {
        return input.StartsWith("@@")
            ? ExpandedAtAtFileInput(input)
            : input.StartsWith("@")
                ? [ExpandedAtFileInput(input)]
                : [input];
    }

    private static IEnumerable<string> ExpandedAtAtFileInput(string input)
    {
        if (!input.StartsWith("@@")) throw new ArgumentException("Not an @@ file input");

        var fileName = input.Substring(2);
        var fileNameOk = fileName == "-" || File.Exists(fileName);
        if (fileNameOk)
        {
            var lines = fileName == "-"
                ? ConsoleHelpers.GetAllLinesFromStdin()
                : File.ReadLines(fileName);

            return lines.SelectMany(line => ExpandedInput(line));
        }

        return [input];
    }

    private static string ExpandedAtFileInput(string input)
    {
        if (!input.StartsWith("@")) throw new ArgumentException("Not an @ file input");

        var fileName = input.Substring(1);
        var fileNameOk = fileName == "-" || File.Exists(fileName);
        return fileNameOk
            ? FileHelpers.ReadAllText(fileName)
            : input;
    }

    private static CommandLineOptions ParseInputOptions(IEnumerable<string> allInputs)
    {
        CommandLineOptions commandLineOptions = new();
        Command? command = null;

        var args = commandLineOptions.AllOptions = allInputs.ToArray();
        for (int i = 0; i < args.Count(); i++)
        {
            var parsed = TryParseInputOptions(commandLineOptions, ref command, args, ref i, args[i]);
            if (!parsed)
            {
                throw InvalidArgException(command, args[i]);
            }
        }

        if (args.Count() == 0)
        {
            command = new ChatCommand();
        }

        if (string.IsNullOrEmpty(commandLineOptions.HelpTopic) && command != null && command.IsEmpty())
        {
            commandLineOptions.HelpTopic = command.GetCommandName();
        }

        if (command != null && !command.IsEmpty())
        {
            commandLineOptions.Commands.Add(command);
        }

        return commandLineOptions;
    }

    private static bool TryParseInputOptions(CommandLineOptions commandLineOptions, ref Command? command, string[] args, ref int i, string arg)
    {
        var isEndOfCommand = arg == "--" && command != null && !command.IsEmpty();
        if (isEndOfCommand)
        {
            commandLineOptions.Commands.Add(command!);
            command = null;
            return true;
        }

        var needNewCommand = command == null;
        if (needNewCommand)
        {
            if (arg.StartsWith("--"))
            {
                var parsedAsAlias = TryParseAliasOptions(commandLineOptions, ref command, args, ref i, arg.Substring(2));
                if (parsedAsAlias) return true;
            }

            var name1 = GetInputOptionArgs(i, args, max: 1).FirstOrDefault();
            var name2 = GetInputOptionArgs(i + 1, args, max: 1).FirstOrDefault();
            var commandName = name1 switch
            {
                "help" => "help",
                "chat" => "chat",
                _ => $"{name1} {name2}".Trim()
            };

            command = commandName switch
            {
                "help" => new HelpCommand(),
                _ => new ChatCommand()
            };

            var needToRestartLoop = command is not ChatCommand;
            if (needToRestartLoop)
            {
                var skipHowManyExtraArgs = commandName.Count(x => x == ' ');
                i += skipHowManyExtraArgs;
                return true;
            }
        }

        var parsedOption = TryParseGlobalCommandLineOptions(commandLineOptions, args, ref i, arg) ||
            TryParseHelpCommandOptions(commandLineOptions, command as HelpCommand, args, ref i, arg) ||
            TryParseChatCommandOptions(command as ChatCommand, args, ref i, arg) ||
            TryParseSharedCommandOptions(command, args, ref i, arg);
        if (parsedOption) return true;

        if (arg == "--help")
        {
            commandLineOptions.HelpTopic = command is ChatCommand ? "usage" : command!.GetCommandName();
            command = new HelpCommand();
            i = args.Count();
            parsedOption = true;
        }
        else if (arg.StartsWith("--"))
        {
            parsedOption = TryParseAliasOptions(commandLineOptions, ref command, args, ref i, arg.Substring(2));
        }
        else if (command is HelpCommand helpCommand)
        {
            commandLineOptions.HelpTopic = $"{commandLineOptions.HelpTopic} {arg}".Trim();
            parsedOption = true;
        }
        // else if (command is ChatCommand chatCommand)
        // {
        //     // TODO: If there are "default" options, add them to the command here
        //     parsedOption = true;
        // }

        return parsedOption;
    }

    private static bool TryParseGlobalCommandLineOptions(CommandLineOptions commandLineOptions, string[] args, ref int i, string arg)
    {
        var parsed = true;

        if (arg == "--and")
        {
            // ignore --and ... used when combining @@ files
        }
        else if (arg == "--debug")
        {
            commandLineOptions.Debug = true;
        }
        else if (arg == "--verbose")
        {
            commandLineOptions.Verbose = true;
        }
        else if (arg == "--save-alias")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var aliasName = max1Arg.FirstOrDefault() ?? throw new CommandLineException("Missing alias name for --save-alias");
            commandLineOptions.SaveAliasName = aliasName;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseHelpCommandOptions(CommandLineOptions commandLineOptions, HelpCommand? helpCommand, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (helpCommand == null)
        {
            parsed = false;
        }
        else if (arg == "--expand")
        {
            commandLineOptions.ExpandHelpTopics = true;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseChatCommandOptions(ChatCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--input-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
            command.InputChatHistory = inputChatHistory;
            i += max1Arg.Count();
        }
        else if (arg == "--output-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputChatHistory = max1Arg.FirstOrDefault() ?? DefaultOutputChatHistoryFileNameTemplate;
            command.OutputChatHistory = outputChatHistory;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseSharedCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseAliasOptions(CommandLineOptions commandLineOptions, ref Command? command, string[] args, ref int i, string alias)
    {
        var aliasDirectory = FindAliasDirectory(create: false) ?? ".";
        var aliasFilePath = Path.Combine(aliasDirectory, $"{alias}.alias");

        if (File.Exists(aliasFilePath))
        {
            var aliasArgs = File.ReadAllLines(aliasFilePath);
            for (var j = 0; j < aliasArgs.Length; j++)
            {
                var parsed = TryParseInputOptions(commandLineOptions, ref command, aliasArgs, ref j, aliasArgs[j]);
                if (!parsed)
                {
                    throw InvalidArgException(command, aliasArgs[j]);
                }
            }
            return true;
        }
        return false;
    }

    private static IEnumerable<string> GetInputOptionArgs(int startAt, string[] args, int max = int.MaxValue)
    {
        for (int i = startAt; i < args.Length && i - startAt < max; i++)
        {
            if (args[i].StartsWith("--"))
            {
                yield break;
            }

            yield return args[i];
        }
    }

    private static string? ValidateFileExists(string? arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            throw new CommandLineException("Missing file name");
        }

        if (!File.Exists(arg))
        {
            throw new CommandLineException($"File does not exist: {arg}");
        }

        return arg;
    }

    private static int ValidateInt(string arg, string countStr, string argDescription)
    {
        if (string.IsNullOrEmpty(countStr))
        {
            throw new CommandLineException($"Missing {argDescription} for {arg}");
        }

        if (!int.TryParse(countStr, out var count))
        {
            throw new CommandLineException($"Invalid {argDescription} for {arg}: {countStr}");
        }

        return count;
    }

    private static CommandLineException InvalidArgException(Command? command, string arg)
    {
        var message = $"Invalid argument: {arg}";
        var ex = new CommandLineException(message);
        return ex;
    }

    private static string? FindAliasDirectory(bool create = false)
    {
        return create
            ? FileHelpers.FindOrCreateDirectory(".chatx", "aliases")
            : FileHelpers.FindDirectory(".chatx", "aliases");

    }
}
