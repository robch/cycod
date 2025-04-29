using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class CommandLineOptions
{
    public const string DefaultSaveFileOutputTemplate = "{filePath}/{fileBase}-output.md";
    public const string DefaultSavePageOutputTemplate = "{filePath}/{fileBase}-output.md";
    public const string DefaultSaveOutputTemplate = "output.md";

    public CommandLineOptions()
    {
        Debug = false;
        Verbose = false;

        HelpTopic = string.Empty;
        ExpandHelpTopics = false;

        Commands = new List<Command>();

        AllOptions = null;
        SaveAliasName = null;
    }

    public bool Debug;
    public bool Verbose;

    public string HelpTopic;
    public bool ExpandHelpTopics;

    public List<Command> Commands;

    public string[] AllOptions;
    public string SaveAliasName;

    public static bool Parse(string[] args, out CommandLineOptions options, out CommandLineException ex)
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
        var aliasDirectory = FindAliasDirectory(create: true);
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
        var additionalFileName = FileHelpers.GetFileNameFromTemplate(baseFileName, "{filepath}/{filebase}-" + additionalFileCount + "{fileext}");
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
        Command command = null;

        var args = commandLineOptions.AllOptions = allInputs.ToArray();
        for (int i = 0; i < args.Count(); i++)
        {
            var parsed = TryParseInputOptions(commandLineOptions, ref command, args, ref i, args[i]);
            if (!parsed)
            {
                throw InvalidArgException(command, args[i]);
            }
        }

        if (string.IsNullOrEmpty(commandLineOptions.HelpTopic) && command != null && command.IsEmpty())
        {
            commandLineOptions.HelpTopic = command.GetCommandName();
        }

        if (command != null && !command.IsEmpty())
        {
            commandLineOptions.Commands.Add(command.Validate());
        }

        return commandLineOptions;
    }

    private static bool TryParseInputOptions(CommandLineOptions commandLineOptions, ref Command command, string[] args, ref int i, string arg)
    {
        var isEndOfCommand = arg == "--" && command != null && !command.IsEmpty();
        if (isEndOfCommand)
        {
            commandLineOptions.Commands.Add(command.Validate());
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
                "version" => "version",
                "help" => "help",
                "run" => "run",
                _ => $"{name1} {name2}".Trim()
            };

            command = commandName switch
            {
                "web search" => new WebSearchCommand(),
                "web get" => new WebGetCommand(),
                "version" => new VersionCommand(),
                "help" => new HelpCommand(),
                "run" => new RunCommand(),
                _ => new FindFilesCommand()
            };

            var needToRestartLoop = command is not FindFilesCommand;
            if (needToRestartLoop)
            {
                var skipHowManyExtraArgs = commandName.Count(x => x == ' ');
                i += skipHowManyExtraArgs;
                return true;
            }
        }

        var parsedOption = TryParseGlobalCommandLineOptions(commandLineOptions, args, ref i, arg) ||
            TryParseHelpCommandOptions(commandLineOptions, command as HelpCommand, args, ref i, arg) ||
            TryParseVersionCommandOptions(commandLineOptions, command as VersionCommand, args, ref i, arg) ||
            TryParseFindFilesCommandOptions(command as FindFilesCommand, args, ref i, arg) ||
            TryParseWebCommandOptions(command as WebCommand, args, ref i, arg) ||
            TryParseRunCommandOptions(command as RunCommand, args, ref i, arg) ||
            TryParseSharedCommandOptions(command, args, ref i, arg);
        if (parsedOption) return true;

        if (arg == "--help")
        {
            commandLineOptions.HelpTopic = command.GetCommandName();
            i = args.Count();
            parsedOption = true;
        }
        else if (arg == "--version")
        {
            command = new VersionCommand();
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
        else if (command is FindFilesCommand findFilesCommand)
        {
            findFilesCommand.Globs.Add(arg);
            parsedOption = true;
        }
        else if (command is RunCommand runCommand)
        {
            runCommand.ScriptToRun = $"{runCommand.ScriptToRun}\n{arg}".Trim();
            parsedOption = true;
        }
        else if (command is WebSearchCommand webSearchCommand)
        {
            webSearchCommand.Terms.Add(arg);
            parsedOption = true;
        }
        else if (command is WebGetCommand webGetCommand)
        {
            webGetCommand.Urls.Add(arg);
            parsedOption = true;
        }

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

    private static bool TryParseHelpCommandOptions(CommandLineOptions commandLineOptions, HelpCommand helpCommand, string[] args, ref int i, string arg)
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

    private static bool TryParseVersionCommandOptions(CommandLineOptions commandLineOptions, VersionCommand versionCommand, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (versionCommand == null)
        {
            parsed = false;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseRunCommandOptions(RunCommand command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--script")
        {
            var scriptArgs = GetInputOptionArgs(i + 1, args);
            command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
            command.Type = RunCommand.ScriptType.Default;
            i += scriptArgs.Count();
        }
        else if (arg == "--cmd")
        {
            var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
            command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
            command.Type = RunCommand.ScriptType.Cmd;
            i += scriptArgs.Count();
        }
        else if (arg == "--bash")
        {
            var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
            command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
            command.Type = RunCommand.ScriptType.Bash;
            i += scriptArgs.Count();
        }
        else if (arg == "--powershell")
        {
            var scriptArgs = GetInputOptionArgs(i + 1, args, 1);
            command.ScriptToRun = ValidateJoinedString(arg, command.ScriptToRun, scriptArgs, "\n", "command");
            command.Type = RunCommand.ScriptType.PowerShell;
            i += scriptArgs.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseFindFilesCommandOptions(FindFilesCommand command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--contains")
        {
            var patterns = GetInputOptionArgs(i + 1, args, required: 1);
            var asRegExs = ValidateRegExPatterns(arg, patterns);
            command.IncludeFileContainsPatternList.AddRange(asRegExs);
            command.IncludeLineContainsPatternList.AddRange(asRegExs);
            i += patterns.Count();
        }
        else if (arg == "--file-contains")
        {
            var patterns = GetInputOptionArgs(i + 1, args, required: 1);
            var asRegExs = ValidateRegExPatterns(arg, patterns);
            command.IncludeFileContainsPatternList.AddRange(asRegExs);
            i += patterns.Count();
        }
        else if (arg == "--file-not-contains")
        {
            var patterns = GetInputOptionArgs(i + 1, args, required: 1);
            var asRegExs = ValidateRegExPatterns(arg, patterns);
            command.ExcludeFileContainsPatternList.AddRange(asRegExs);
            i += patterns.Count();
        }
        else if (arg == "--line-contains")
        {
            var patterns = GetInputOptionArgs(i + 1, args, required: 1);
            var asRegExs = ValidateRegExPatterns(arg, patterns);
            command.IncludeLineContainsPatternList.AddRange(asRegExs);
            i += patterns.Count();
        }
        else if (arg == "--lines")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            var count = ValidateLineCount(arg, countStr);
            command.IncludeLineCountBefore = count;
            command.IncludeLineCountAfter = count;
        }
        else if (arg == "--lines-before")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.IncludeLineCountBefore = ValidateLineCount(arg, countStr);
        }
        else if (arg == "--lines-after")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.IncludeLineCountAfter = ValidateLineCount(arg, countStr);
        }
        else if (arg == "--remove-all-lines")
        {
            var patterns = GetInputOptionArgs(i + 1, args);
            var asRegExs = ValidateRegExPatterns(arg, patterns);
            command.RemoveAllLineContainsPatternList.AddRange(asRegExs);
            i += patterns.Count();
        }
        else if (arg == "--line-numbers")
        {
            command.IncludeLineNumbers = true;
        }
        else if (arg.StartsWith("--") && arg.EndsWith("file-instructions"))
        {
            var instructions = GetInputOptionArgs(i + 1, args);
            if (instructions.Count() == 0)
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            var fileNameCriteria = arg != "--file-instructions"
                ? arg.Substring(2, arg.Length - 20)
                : string.Empty;
            var withCriteria = instructions.Select(x => Tuple.Create(x, fileNameCriteria));
            command.FileInstructionsList.AddRange(withCriteria);
            i += instructions.Count();
        }
        else if (arg == "--exclude")
        {
            var patterns = GetInputOptionArgs(i + 1, args);
            if (patterns.Count() == 0)
            {
                throw new CommandLineException($"Missing patterns for {arg}");
            }

            var containsSlash = (string x) => x.Contains('/') || x.Contains('\\');
            var asRegExs = patterns
                .Where(x => !containsSlash(x))
                .Select(x => ValidateFilePatternToRegExPattern(arg, x));
            var asGlobs = patterns
                .Where(x => containsSlash(x))
                .ToList();

            command.ExcludeFileNamePatternList.AddRange(asRegExs);
            command.ExcludeGlobs.AddRange(asGlobs);
            i += patterns.Count();
        }
        else if (arg == "--save-file-output")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var saveFileOutput = max1Arg.FirstOrDefault() ?? DefaultSaveFileOutputTemplate;
            command.SaveFileOutput = saveFileOutput;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseWebCommandOptions(WebCommand command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--interactive")
        {
            command.Interactive = true;
        }
        else if (arg == "--chromium")
        {
            command.Browser = BrowserType.Chromium;
        }
        else if (arg == "--firefox")
        {
            command.Browser = BrowserType.Firefox;
        }
        else if (arg == "--webkit")
        {
            command.Browser = BrowserType.Webkit;
        }
        else if (arg == "--strip")
        {
            command.StripHtml = true;
        }
        else if (arg == "--save-page-folder")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, 1);
            command.SaveFolder = max1Arg.FirstOrDefault() ?? "web-pages";
            i += max1Arg.Count();
        }
        else if (arg == "--bing")
        {
            command.SearchProvider = WebSearchProvider.Bing;
        }
        else if (arg == "--duck-duck-go" || arg == "--duckduckgo")
        {
            command.SearchProvider = WebSearchProvider.DuckDuckGo;
        }
        else if (arg == "--google")
        {
            command.SearchProvider = WebSearchProvider.Google;
        }
        else if (arg == "--yahoo")
        {
            command.SearchProvider = WebSearchProvider.Yahoo;
        }
        else if (arg == "--bing-api")
        {
            command.SearchProvider = WebSearchProvider.BingAPI;
        }
        else if (arg == "--google-api")
        {
            command.SearchProvider = WebSearchProvider.GoogleAPI;
        }
        else if (arg == "--get")
        {
            command.GetContent = true;
        }
        else if (arg == "--max")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, 1);
            command.MaxResults = ValidateInt(arg, max1Arg.FirstOrDefault(), "max results");
            i += max1Arg.Count();
        }
        else if (arg == "--exclude")
        {
            var patterns = GetInputOptionArgs(i + 1, args);
            var asRegExs = ValidateRegExPatterns(arg, patterns);
            command.ExcludeURLContainsPatternList.AddRange(asRegExs);
            i += patterns.Count();
        }
        else if (arg.StartsWith("--") && arg.EndsWith("page-instructions"))
        {
            var instructions = GetInputOptionArgs(i + 1, args);
            if (instructions.Count() == 0)
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            var webPageCriteria = arg != "--page-instructions"
                ? arg.Substring(2, arg.Length - 20)
                : string.Empty;
            var withCriteria = instructions.Select(x => Tuple.Create(x, webPageCriteria));
            command.PageInstructionsList.AddRange(withCriteria);
            i += instructions.Count();
        }
        else if (arg == "--save-page-output" ||arg == "--save-web-output" || arg == "--save-web-page-output")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var savePageOutput = max1Arg.FirstOrDefault() ?? DefaultSavePageOutputTemplate;
            command.SavePageOutput = savePageOutput;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseSharedCommandOptions(Command command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--instructions")
        {
            var instructions = GetInputOptionArgs(i + 1, args);
            if (instructions.Count() == 0)
            {
                throw new CommandLineException($"Missing instructions for {arg}");
            }
            command.InstructionsList.AddRange(instructions);
            i += instructions.Count();
        }
        else if (arg == "--save-output")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var saveOutput = max1Arg.FirstOrDefault() ?? DefaultSaveOutputTemplate;
            command.SaveOutput = saveOutput;
            i += max1Arg.Count();
        }
        else if (arg == "--save-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var saveChatHistory = max1Arg.FirstOrDefault() ?? AiInstructionProcessor.DefaultSaveChatHistoryTemplate;
            command.SaveChatHistory = saveChatHistory;
            i += max1Arg.Count();
        }
        else if (arg == "--built-in-functions")
        {
            command.UseBuiltInFunctions = true;
        }
        else if (arg == "--threads")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            command.ThreadCount = ValidateInt(arg, countStr, "thread count");
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseAliasOptions(CommandLineOptions commandLineOptions, ref Command command, string[] args, ref int i, string alias)
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

    private static IEnumerable<string> GetInputOptionArgs(int startAt, string[] args, int max = int.MaxValue, int required = 0)
    {
        var found = 0;

        for (int i = startAt; i < args.Length && i - startAt < max; i++)
        {
            if (args[i].StartsWith("--") && found >= required)
            {
                yield break;
            }

            yield return args[i];
        }
    }

    private static IEnumerable<Regex> ValidateRegExPatterns(string arg, IEnumerable<string> patterns)
    {
        patterns = patterns.ToList();
        if (!patterns.Any())
        {
            throw new CommandLineException($"Missing regular expression patterns for {arg}");
        }

        return patterns.Select(x => ValidateRegExPattern(arg, x));
    }

    private static Regex ValidateRegExPattern(string arg, string pattern)
    {
        try
        {
            return new Regex(pattern);
        }
        catch (Exception)
        {
            throw new CommandLineException($"Invalid regular expression pattern for {arg}: {pattern}");
        }
    }

    private static Regex ValidateFilePatternToRegExPattern(string arg, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            throw new CommandLineException($"Missing file pattern for {arg}");
        }

        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        var patternPrefix = isWindows ? "(?i)^" : "^";
        var regexPattern = patternPrefix + pattern
            .Replace(".", "\\.")
            .Replace("*", ".*")
            .Replace("?", ".") + "$";

        try
        {
            return new Regex(regexPattern);
        }
        catch (Exception)
        {
            throw new CommandLineException($"Invalid file pattern for {arg}: {pattern}");
        }
    }

    private static int ValidateLineCount(string arg, string countStr)
    {
        return ValidateInt(arg, countStr, "line count");
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

    private static string ValidateJoinedString(string arg, string seed, IEnumerable<string> values, string separator, string argDescription)
    {
        seed = string.Join(separator, values.Prepend(seed)).Trim();
        if (string.IsNullOrEmpty(seed))
        {
            throw new CommandLineException($"Missing {argDescription} for {arg}");
        }

        return seed;
    }

    private static CommandLineException InvalidArgException(Command command, string arg)
    {
        var message = $"Invalid argument: {arg}";
        var ex = command is WebSearchCommand ? new WebSearchCommandLineException(message)
            : command is WebGetCommand ? new WebGetCommandLineException(message)
            : new CommandLineException(message);
        return ex;
    }

    private static string FindAliasDirectory(bool create = false)
    {
        return create
            ? FileHelpers.FindOrCreateDirectory(".mdx", "aliases")
            : FileHelpers.FindDirectory(".mdx", "aliases");

    }
}
