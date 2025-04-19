using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class CommandLineOptions
{
    public CommandLineOptions()
    {
        Interactive = true;

        Debug = false;
        Verbose = false;
        Quiet = false;

        HelpTopic = string.Empty;
        ExpandHelpTopics = false;

        Commands = new List<Command>();

        AllOptions = new string[0];
        SaveAliasName = null;

        ThreadCount = 0;
        WorkingDirectory = null;
    }

    public bool Interactive;

    public bool Debug;
    public bool Verbose;
    public bool Quiet;

    public string HelpTopic;
    public bool ExpandHelpTopics;

    public int ThreadCount { get; private set; }
    public string? WorkingDirectory { get; set; }
    public List<Command> Commands;

    public string[] AllOptions;
    public string? SaveAliasName;
    public ConfigFileScope? SaveAliasScope;
        
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
            var lines = ConsoleHelpers.IsStandardInputReference(fileName)
                ? ConsoleHelpers.GetAllLinesFromStdin()
                : File.ReadLines(fileName);

            return lines.SelectMany(line => ExpandedInput(line));
        }

        return [input];
    }

    private static string ExpandedAtFileInput(string input)
    {
        if (!input.StartsWith("@"))
        {
            throw new ArgumentException("Not an @ file input");
        }

        return AtFileHelpers.ExpandAtFileValue(input);
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

        var oneChatCommandWithNoInput = commandLineOptions.Commands.Count == 1 && command is ChatCommand chatCommand && chatCommand.InputInstructions.Count == 0;
        var inOrOutRedirected = Console.IsInputRedirected || Console.IsOutputRedirected;
        var implictlyUseStdIn = oneChatCommandWithNoInput && inOrOutRedirected;
        if (implictlyUseStdIn)
        {
            var stdinLines = ConsoleHelpers.GetAllLinesFromStdin();
            var joined = string.Join("\n", stdinLines);
            (command as ChatCommand)!.InputInstructions.Add(joined);
        }

        return commandLineOptions;
    }

    private static bool TryParseKnownSettingOption(string[] args, ref int i, string arg)
    {
        // Check if this is a known setting CLI option
        if (KnownSettingsCLIParser.TryParseCLIOption(arg, out string? settingName, out string? value))
        {
            // If value is in the same arg (e.g. --setting=value)
            if (value != null)
            {
                // Add to configuration store
                ConfigStore.Instance.SetFromCommandLine(settingName!, value);
                ConsoleHelpers.WriteDebugLine($"Set known setting from CLI: {settingName} = {value}");
                return true;
            }
            
            // Otherwise, the value should be the next argument
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var settingValue = max1Arg.FirstOrDefault() ?? throw new CommandLineException($"Missing value for {arg}");
            
            // Add to configuration store
            ConfigStore.Instance.SetFromCommandLine(settingName!, settingValue);
            ConsoleHelpers.WriteDebugLine($"Set known setting from CLI: {settingName} = {settingValue}");
            i += max1Arg.Count();
            return true;
        }
        
        return false;
    }

    public static bool TryParseInputOptions(CommandLineOptions commandLineOptions, ref Command? command, string[] args, ref int i, string arg)
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
                var parsedAsAlias = AliasFileHelpers.TryParseAliasOptions(commandLineOptions, ref command, args, ref i, arg.Substring(2));
                if (parsedAsAlias) return true;
            }

            var name1 = GetInputOptionArgs(i, args, max: 1).FirstOrDefault();
            var name2 = GetInputOptionArgs(i + 1, args, max: 1).FirstOrDefault();
            var commandName = name1 switch
            {
                "version" => "version",
                "help" => "help",
                "chat" => "chat",
                _ => $"{name1} {name2}".Trim()
            };

            var partialCommandNeedsHelp =
                commandName == "alias" ||
                commandName == "config" ||
                commandName == "github" ||
                commandName == "mcp" ||
                commandName == "prompt" ||
                commandName == "test";
            if (partialCommandNeedsHelp)
            {
                command = new HelpCommand();
                commandLineOptions.HelpTopic = commandName;
                return true;
            }

            command = commandName switch
            {
                "help" => new HelpCommand(),
                "version" => new VersionCommand(),
                "github login" => new GitHubLoginCommand(),
                "config list" => new ConfigListCommand(),
                "config get" => new ConfigGetCommand(),
                "config set" => new ConfigSetCommand(),
                "config clear" => new ConfigClearCommand(),
                "config add" => new ConfigAddCommand(),
                "config remove" => new ConfigRemoveCommand(),
                "alias list" => new AliasListCommand(),
                "alias get" => new AliasGetCommand(),
                "alias delete" => new AliasDeleteCommand(),
                "prompt list" => new PromptListCommand(),
                "prompt get" => new PromptGetCommand(),
                "prompt create" => new PromptCreateCommand(),
                "prompt delete" => new PromptDeleteCommand(),
                "mcp list" => new McpListCommand(),
                "mcp get" => new McpGetCommand(),
                "mcp add" => new McpAddCommand(),
                "mcp remove" => new McpRemoveCommand(),
                "test list" => new TestListCommand(),
                "test run" => new TestRunCommand(),
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
            TryParseVersionCommandOptions(commandLineOptions, command as VersionCommand, args, ref i, arg) ||
            TryParseChatCommandOptions(commandLineOptions, command as ChatCommand, args, ref i, arg) ||
            TryParseGitHubLoginCommandOptions(command as GitHubLoginCommand, args, ref i, arg) ||
            TryParseConfigCommandOptions(command as ConfigBaseCommand, args, ref i, arg) ||
            TryParseAliasCommandOptions(command as AliasBaseCommand, args, ref i, arg) ||
            TryParsePromptCommandOptions(command as PromptBaseCommand, args, ref i, arg) ||
            TryParseMcpCommandOptions(command as McpBaseCommand, args, ref i, arg) ||
            TryParseTestCommandOptions(command as TestBaseCommand, args, ref i, arg) ||
            TryParseSharedCommandOptions(command, args, ref i, arg) ||
            TryParseKnownSettingOption(args, ref i, arg);
        if (parsedOption) return true;

        if (arg == "--help")
        {
            commandLineOptions.HelpTopic = command is ChatCommand ? "usage" : command!.GetCommandName();
            command = new HelpCommand();
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
            parsedOption = AliasFileHelpers.TryParseAliasOptions(commandLineOptions, ref command, args, ref i, arg.Substring(2));
        }
        else if (command is HelpCommand helpCommand)
        {
            commandLineOptions.HelpTopic = $"{commandLineOptions.HelpTopic} {arg}".Trim();
            parsedOption = true;
        }
        else if (command is ConfigGetCommand configGetCommand && string.IsNullOrEmpty(configGetCommand.Key))
        {
            configGetCommand.Key = arg;
            parsedOption = true;
        }
        else if (command is ConfigClearCommand configClearCommand && string.IsNullOrEmpty(configClearCommand.Key))
        {
            configClearCommand.Key = arg;
            parsedOption = true;
        }
        else if (command is ConfigSetCommand configSetCommand)
        {
            if (string.IsNullOrEmpty(configSetCommand.Key))
            {
                configSetCommand.Key = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(configSetCommand.Value))
            {
                configSetCommand.Value = arg;
                parsedOption = true;
            }
        }
        else if (command is ConfigAddCommand configAddCommand)
        {
            if (string.IsNullOrEmpty(configAddCommand.Key))
            {
                configAddCommand.Key = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(configAddCommand.Value))
            {
                configAddCommand.Value = arg;
                parsedOption = true;
            }
        }
        else if (command is ConfigRemoveCommand configRemoveCommand)
        {
            if (string.IsNullOrEmpty(configRemoveCommand.Key))
            {
                configRemoveCommand.Key = arg;
                parsedOption = true;
            }
            else if (string.IsNullOrEmpty(configRemoveCommand.Value))
            {
                configRemoveCommand.Value = arg;
                parsedOption = true;
            }
        }
        else if (command is AliasGetCommand aliasGetCommand && string.IsNullOrEmpty(aliasGetCommand.AliasName))
        {
            aliasGetCommand.AliasName = arg;
            parsedOption = true;
        }
        else if (command is AliasDeleteCommand aliasDeleteCommand && string.IsNullOrEmpty(aliasDeleteCommand.AliasName))
        {
            aliasDeleteCommand.AliasName = arg;
            parsedOption = true;
        }
        else if (command is PromptCreateCommand createCommand && string.IsNullOrEmpty(createCommand.PromptName))
        {
            createCommand.PromptName = arg;
            parsedOption = true;
        }
        else if (command is PromptCreateCommand createCmd && string.IsNullOrEmpty(createCmd.PromptText))
        {
            createCmd.PromptText = arg;
            parsedOption = true;
        }
        else if (command is PromptDeleteCommand deleteCommand && string.IsNullOrEmpty(deleteCommand.PromptName))
        {
            deleteCommand.PromptName = arg;
            parsedOption = true;
        }
        else if (command is PromptGetCommand getCommand && string.IsNullOrEmpty(getCommand.PromptName))
        {
            getCommand.PromptName = arg;
            parsedOption = true;
        }
        else if (command is McpGetCommand mcpGetCommand && string.IsNullOrEmpty(mcpGetCommand.Name))
        {
            mcpGetCommand.Name = arg;
            parsedOption = true;
        }
        else if (command is McpAddCommand mcpAddCommand && string.IsNullOrEmpty(mcpAddCommand.Name))
        {
            mcpAddCommand.Name = arg;
            parsedOption = true;
        }
        else if (command is McpRemoveCommand mcpRemoveCommand && string.IsNullOrEmpty(mcpRemoveCommand.Name))
        {
            mcpRemoveCommand.Name = arg;
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
        else if (arg == "--interactive")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var interactive = max1Arg.FirstOrDefault() ?? "true";
            commandLineOptions.Interactive = interactive.ToLower() == "true" || interactive == "1";
            i += max1Arg.Count();
        }
        else if (arg == "--debug")
        {
            commandLineOptions.Debug = true;
            ConsoleHelpers.ConfigureDebug(true);
        }
        else if (arg == "--verbose")
        {
            commandLineOptions.Verbose = true;
        }
        else if (arg == "--quiet")
        {
            commandLineOptions.Quiet = true;
        }
        else if (arg == "--save-alias" || arg == "--save-local-alias")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var aliasName = max1Arg.FirstOrDefault() ?? throw new CommandLineException("Missing alias name for --save-alias");
            commandLineOptions.SaveAliasName = aliasName;
            commandLineOptions.SaveAliasScope = ConfigFileScope.Local;
            i += max1Arg.Count();
        }
        else if (arg == "--save-user-alias")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var aliasName = max1Arg.FirstOrDefault() ?? throw new CommandLineException("Missing alias name for --save-user-alias");
            commandLineOptions.SaveAliasName = aliasName;
            commandLineOptions.SaveAliasScope = ConfigFileScope.User;
            i += max1Arg.Count();
        }
        else if (arg == "--save-global-alias")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var aliasName = max1Arg.FirstOrDefault() ?? throw new CommandLineException("Missing alias name for --save-global-alias");
            commandLineOptions.SaveAliasName = aliasName;
            commandLineOptions.SaveAliasScope = ConfigFileScope.Global;
            i += max1Arg.Count();
        }
        else if (arg == "--profile")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var profileName = ValidateString(arg, max1Arg.FirstOrDefault(), "profile name");
            ProfileFileHelpers.LoadProfile(profileName!);
            i += max1Arg.Count();
        }
        else if (arg == "--config")
        {
            var configFiles = GetInputOptionArgs(i + 1, args);
            ConfigStore.Instance.LoadConfigFiles(ValidateFilesExist(configFiles));
            i += configFiles.Count();
        }
        else if (arg == "--threads")
        {
            var countStr = i + 1 < args.Count() ? args.ElementAt(++i) : null;
            commandLineOptions.ThreadCount = ValidateInt(arg, countStr, "thread count");
        }
        else if (arg == "--working-dir" || arg == "--folder" || arg == "--dir" || arg == "--cwd")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var dirPath = ValidateString(arg, max1Arg.FirstOrDefault(), "directory path");
            commandLineOptions.WorkingDirectory = dirPath;
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

    private static bool TryParseVersionCommandOptions(CommandLineOptions commandLineOptions, VersionCommand? versionCommand, string[] args, ref int i, string arg)
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

    private static bool TryParseConfigCommandOptions(ConfigBaseCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--file")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var configPath = ValidateOkFileName(max1Arg.FirstOrDefault());
            ConfigStore.Instance.LoadConfigFile(configPath);
            command.Scope = ConfigFileScope.FileName;
            command.ConfigFileName = configPath;
            i += max1Arg.Count();
        }
        else if (arg == "--global" || arg == "-g")
        {
            command.Scope = ConfigFileScope.Global;
            command.ConfigFileName = null;
        }
        else if (arg == "--user" || arg == "-u")
        {
            command.Scope = ConfigFileScope.User;
            command.ConfigFileName = null;
        }
        else if (arg == "--local" || arg == "-l")
        {
            command.Scope = ConfigFileScope.Local;
            command.ConfigFileName = null;
        }
        else if (arg == "--any" || arg == "-a")
        {
            command.Scope = ConfigFileScope.Any;
            command.ConfigFileName = null;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseAliasCommandOptions(AliasBaseCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--global" || arg == "-g")
        {
            command.Scope = ConfigFileScope.Global;
        }
        else if (arg == "--user" || arg == "-u")
        {
            command.Scope = ConfigFileScope.User;
        }
        else if (arg == "--local" || arg == "-l")
        {
            command.Scope = ConfigFileScope.Local;
        }
        else if (arg == "--any" || arg == "-a")
        {
            command.Scope = ConfigFileScope.Any;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParsePromptCommandOptions(PromptBaseCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--global" || arg == "-g")
        {
            command.Scope = ConfigFileScope.Global;
        }
        else if (arg == "--user" || arg == "-u")
        {
            command.Scope = ConfigFileScope.User;
        }
        else if (arg == "--local" || arg == "-l")
        {
            command.Scope = ConfigFileScope.Local;
        }
        else if (arg == "--any" || arg == "-a")
        {
            command.Scope = ConfigFileScope.Any;
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseMcpCommandOptions(McpBaseCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--global" || arg == "-g")
        {
            command.Scope = ConfigFileScope.Global;
        }
        else if (arg == "--user" || arg == "-u")
        {
            command.Scope = ConfigFileScope.User;
        }
        else if (arg == "--local" || arg == "-l")
        {
            command.Scope = ConfigFileScope.Local;
        }
        else if (arg == "--any" || arg == "-a")
        {
            command.Scope = ConfigFileScope.Any;
        }
        else if (command is McpAddCommand cmdAddCommand && arg == "--command")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var cmdValue = ValidateString(arg, max1Arg.FirstOrDefault(), "command");
            cmdAddCommand.Command = cmdValue;
            cmdAddCommand.Transport = "stdio";
            i += max1Arg.Count();
        }
        else if (command is McpAddCommand urlAddCommand && arg == "--url")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var urlValue = ValidateString(arg, max1Arg.FirstOrDefault(), "url");
            urlAddCommand.Url = urlValue;
            urlAddCommand.Transport = "sse";
            i += max1Arg.Count();
        }
        else if (command is McpAddCommand argsAddCommand && arg == "--arg")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var argValue = ValidateString(arg, max1Arg.FirstOrDefault(), "argument");
            argsAddCommand.Args.Add(argValue!);
            i += max1Arg.Count();
        }
        else if (command is McpAddCommand envAddCommand && (arg == "--env" || arg == "-e"))
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var env = ValidateString(arg, max1Arg.FirstOrDefault(), "environment variable");
            envAddCommand.EnvironmentVars.Add(env!);
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseTestCommandOptions(TestBaseCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--file")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var filePattern = ValidateString(arg, max1Arg.FirstOrDefault(), "file pattern");
            command.Files.Add(filePattern!);
            i += max1Arg.Count();
        }
        else if (arg == "--files")
        {
            var filePatterns = GetInputOptionArgs(i + 1, args);
            var validPatterns = ValidateStrings(arg, filePatterns, "file patterns");
            command.Files.AddRange(validPatterns);
            i += filePatterns.Count();
        }
        else if (arg == "--test")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var testName = ValidateString(arg, max1Arg.FirstOrDefault(), "test name");
            command.Tests.Add(testName!);
            i += max1Arg.Count();
        }
        else if (arg == "--tests")
        {
            var testNames = GetInputOptionArgs(i + 1, args);
            var validTests = ValidateStrings(arg, testNames, "test names");
            command.Tests.AddRange(validTests);
            i += testNames.Count();
        }
        else if (arg == "--contains")
        {
            var containPatterns = GetInputOptionArgs(i + 1, args);
            var validContains = ValidateStrings(arg, containPatterns, "contains patterns");
            command.Contains.AddRange(validContains);
            i += containPatterns.Count();
        }
        else if (arg == "--remove")
        {
            var removePatterns = GetInputOptionArgs(i + 1, args);
            var validRemove = ValidateStrings(arg, removePatterns, "remove patterns");
            command.Remove.AddRange(validRemove);
            i += removePatterns.Count();
        }
        // Handle TestRunCommand specific options
        else if (command is TestRunCommand runCommand && arg == "--output-file")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
            runCommand.OutputFile = outputFile;
            i += max1Arg.Count();
        }
        else if (command is TestRunCommand runCommand2 && arg == "--output-format")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var format = ValidateString(arg, max1Arg.FirstOrDefault(), "output format");
            var allowedFormats = new[] { "trx", "junit" };
            if (!allowedFormats.Contains(format))
            {
                throw new CommandLineException($"Invalid format for --output-format: {format}. Allowed values: trx, junit");
            }
            runCommand2.OutputFormat = format!;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseChatCommandOptions(CommandLineOptions commandLineOptions, ChatCommand? command, string[] args, ref int i, string arg)
    {
        bool parsed = true;

        if (command == null)
        {
            parsed = false;
        }
        else if (arg == "--var")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var assignment = ValidateAssignment(arg, max1Arg.FirstOrDefault());
            command.Variables[assignment.Item1] = assignment.Item2;
            ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
            i += max1Arg.Count();
        }
        else if (arg == "--vars")
        {
            var varArgs = GetInputOptionArgs(i + 1, args);
            var assignments = ValidateAssignments(arg, varArgs);
            foreach (var assignment in assignments)
            {
                command.Variables[assignment.Item1] = assignment.Item2;
                ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
            }
            i += varArgs.Count();
        }
        else if (arg == "--foreach")
        {
            var foreachArgs = GetInputOptionArgs(i + 1, args).ToArray();
            var foreachVariable = ForEachVarHelpers.ParseForeachVarOption(foreachArgs, out var skipCount);
            command.ForEachVariables.Add(foreachVariable);
            commandLineOptions.Interactive = false;
            i += skipCount;
        }
        else if (arg == "--use-templates")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var useTemplates = max1Arg.FirstOrDefault() ?? "true";
            command.UseTemplates = useTemplates.ToLower() == "true" || useTemplates == "1";
            i += max1Arg.Count();
        }
        else if (arg == "--no-templates")
        {
            command.UseTemplates = false;
        }
        else if (arg == "--system-prompt")
        {
            var promptArgs = GetInputOptionArgs(i + 1, args);
            var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "system prompt");
            command.SystemPrompt = prompt;
            i += promptArgs.Count();
        }
        else if (arg == "--add-system-prompt")
        {
            var promptArgs = GetInputOptionArgs(i + 1, args);
            var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "additional system prompt");
            if (!string.IsNullOrEmpty(prompt))
            {
                command.SystemPromptAdds.Add(prompt);
            }
            i += promptArgs.Count();
        }
        else if (arg == "--add-user-prompt" || arg == "--prompt")
        {
            var promptArgs = GetInputOptionArgs(i + 1, args);
            var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "additional user prompt");
            if (!string.IsNullOrEmpty(prompt))
            {
                var needsSlashPrefix = arg == "--prompt" && !prompt.StartsWith("/");
                var prefix = needsSlashPrefix ? "/" : string.Empty;
                command.UserPromptAdds.Add($"{prefix}{prompt}");
            }
            i += promptArgs.Count();
        }
        else if (arg == "--input" || arg == "--instruction" || arg == "--question" || arg == "-q")
        {
            var inputArgs = GetInputOptionArgs(i + 1, args)
                .Select(x => FileHelpers.FileExists(x)
                    ? FileHelpers.ReadAllText(x)
                    : x);

            var isQuietNonInteractiveAlias = arg == "--question" || arg == "-q";
            if (isQuietNonInteractiveAlias)
            {
                commandLineOptions.Quiet = true;
                commandLineOptions.Interactive = false;
            }

            var implictlyUseStdIn = isQuietNonInteractiveAlias && inputArgs.Count() == 0;
            if (implictlyUseStdIn)
            {
                inputArgs = ConsoleHelpers.GetAllLinesFromStdin();
            }

            var joined = ValidateString(arg, string.Join("\n", inputArgs), "input");
            command.InputInstructions.Add(joined!);

            i += inputArgs.Count();
        }
        else if (arg == "--inputs" || arg == "--instructions" || arg == "--questions")
        {
            var inputArgs = GetInputOptionArgs(i + 1, args)
                .Select(x => FileHelpers.FileExists(x)
                    ? FileHelpers.ReadAllText(x)
                    : x);

            var isQuietNonInteractiveAlias = arg == "--questions";
            if (isQuietNonInteractiveAlias)
            {
                commandLineOptions.Quiet = true;
                commandLineOptions.Interactive = false;
            }

            var implictlyUseStdIn = isQuietNonInteractiveAlias && inputArgs.Count() == 0;
            if (implictlyUseStdIn)
            {
                inputArgs = ConsoleHelpers.GetAllLinesFromStdin();
            }

            var inputs = ValidateStrings(arg, inputArgs, "input");
            command.InputInstructions.AddRange(inputs);

            i += inputArgs.Count();
        }
        else if (arg == "--chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var chatHistory = max1Arg.FirstOrDefault() ?? DefaultSimpleChatHistoryFileName;
            command.InputChatHistory = !FileHelpers.FileExists(chatHistory) ? command.InputChatHistory : chatHistory;
            command.OutputChatHistory = chatHistory;
            command.LoadMostRecentChatHistory = false;
            i += max1Arg.Count();
        }
        else if (arg == "--input-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
            command.InputChatHistory = inputChatHistory;
            command.LoadMostRecentChatHistory = false;
            i += max1Arg.Count();
        }
        else if (arg == "--continue")
        {
            command.LoadMostRecentChatHistory = true;
            command.InputChatHistory = null;
        }
        else if (arg == "--output-chat-history")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputChatHistory = max1Arg.FirstOrDefault() ?? DefaultOutputChatHistoryFileNameTemplate;
            command.OutputChatHistory = outputChatHistory;
            i += max1Arg.Count();
        }
        else if (arg == "--trim-token-target")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var trimTokenTarget = ValidateInt(arg, max1Arg.FirstOrDefault(), "trim token target");
            command.TrimTokenTarget = trimTokenTarget;
            i += max1Arg.Count();
        }
        else if (arg == "--output-trajectory")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputTrajectory = max1Arg.FirstOrDefault() ?? DefaultOutputTrajectoryFileNameTemplate;
            command.OutputTrajectory = outputTrajectory;
            i += max1Arg.Count();
        }
        else if (arg == "--use-openai")
        {
            ConfigStore.Instance.SetFromCommandLine(KnownSettings.AppPreferredProvider, "openai");
        }
        else if (arg == "--use-azure-openai" || arg == "--use-azure")
        {
            ConfigStore.Instance.SetFromCommandLine(KnownSettings.AppPreferredProvider, "azure-openai");
        }
        else if (arg == "--use-copilot") 
        {
            ConfigStore.Instance.SetFromCommandLine(KnownSettings.AppPreferredProvider, "copilot");
        }
        else if (arg == "--use-copilot-token") 
        {
            ConfigStore.Instance.SetFromCommandLine(KnownSettings.AppPreferredProvider, "copilot-token");
        }
        else if (arg == "--use-copilot-hmac") 
        {
            ConfigStore.Instance.SetFromCommandLine(KnownSettings.AppPreferredProvider, "copilot-hmac");
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }

    private static bool TryParseGitHubLoginCommandOptions(GitHubLoginCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--config")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var configPath = ValidateOkFileName(max1Arg.FirstOrDefault());
            ConfigStore.Instance.LoadConfigFile(configPath);
            command.Scope = ConfigFileScope.FileName;
            command.ConfigFileName = configPath;
            i += max1Arg.Count();
        }
        else if (arg == "--global" || arg == "-g")
        {
            command.Scope = ConfigFileScope.Global;
            command.ConfigFileName = null;
        }
        else if (arg == "--user" || arg == "-u")
        {
            command.Scope = ConfigFileScope.User;
            command.ConfigFileName = null;
        }
        else if (arg == "--local" || arg == "-l")
        {
            command.Scope = ConfigFileScope.Local;
            command.ConfigFileName = null;
        }
        else if (arg == "--any" || arg == "-a")
        {
            command.Scope = ConfigFileScope.Any;
            command.ConfigFileName = null;
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

    private static string? ValidateString(string arg, string? argStr, string argDescription)
    {
        if (string.IsNullOrEmpty(argStr))
        {
            throw new CommandLineException($"Missing {argDescription} for {arg}");
        }

        return argStr;
    }

    private static IEnumerable<string> ValidateStrings(string arg, IEnumerable<string> argStrs, string argDescription)
    {
        var strings = argStrs.ToList();
        if (!strings.Any())
        {
            throw new CommandLineException($"Missing {argDescription} for {arg}");
        }

        return strings.Select(x => ValidateString(arg, x, argDescription)!);
    }

    private static (string, string) ValidateAssignment(string arg, string? assignment)
    {
        assignment = ValidateString(arg, assignment, "assignment")!;
        
        var parts = assignment.Split('=', 2);
        if (parts.Length != 2)
        {
            throw new CommandLineException($"Invalid variable definition for {arg}: {assignment}. Use NAME=VALUE format.");
        }

        return (parts[0], parts[1]);
    }

    private static IEnumerable<(string, string)> ValidateAssignments(string arg, IEnumerable<string> assignments)
    {
        if (!assignments.Any())
        {
            throw new CommandLineException($"Missing variable assignments for {arg}");
        }

        return assignments.Select(x => ValidateAssignment(arg, x));
    }

    private static string ValidateOkFileName(string? arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            throw new CommandLineException("Missing file name");
        }

        return arg;
    }

    private static IEnumerable<string> ValidateFilesExist(IEnumerable<string> args)
    {
        args = args.ToList();
        foreach (var arg in args)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new CommandLineException("Missing file name");
            }

            if (!File.Exists(arg))
            {
                throw new CommandLineException($"File does not exist: {arg}");
            }
        }

        return args;
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

    private static int ValidateInt(string arg, string? countStr, string argDescription)
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

    private const string DefaultSimpleChatHistoryFileName = "chat-history.jsonl";
    private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";
    private const string DefaultOutputTrajectoryFileNameTemplate = "trajectory-{time}.md";
}
