import * as fs from 'fs';
import * as os from 'os';
import { Command } from './Command';
import { CommandLineException } from './CommandLineException';
import { HelpCommand } from '../CommandLineCommands/HelpCommand';
import { VersionCommand } from '../CommandLineCommands/VersionCommand';
import { ConfigStore } from '../Configuration/ConfigStore';
import { ConfigFileScope } from '../Configuration/ConfigFileScope';
import { KnownSettingsCLIParser } from '../Configuration/KnownSettingsCLIParser';
import { KnownSettings } from '../Configuration/KnownSettings';
import { AtFileHelpers } from '../Helpers/AtFileHelpers';
import { ConsoleHelpers } from '../Helpers/ConsoleHelpers';
import { FileHelpers } from '../Helpers/FileHelpers';
import { AliasFileHelpers } from '../Helpers/AliasFileHelpers';
import { ProfileFileHelpers } from '../Helpers/ProfileFileHelpers';

/**
 * Handles command line option parsing and validation.
 * Supports various global options, command-specific options, and configuration settings.
 */
export class CommandLineOptions {
    /**
     * Creates a new instance of CommandLineOptions with default values.
     */
    protected constructor() {
        this.Interactive = true;

        this.Debug = false;
        this.Verbose = false;
        this.Quiet = false;

        this.HelpTopic = '';
        this.ExpandHelpTopics = false;

        this.Commands = [];

        this.AllOptions = [];
        this.SaveAliasName = null;

        this.ThreadCount = 0;
        this.WorkingDirectory = null;
    }

    public Interactive: boolean;

    public Debug: boolean;
    public Verbose: boolean;
    public Quiet: boolean;

    public HelpTopic: string;
    public ExpandHelpTopics: boolean;

    private _threadCount: number;
    public get ThreadCount(): number { return this._threadCount; }
    private set ThreadCount(value: number) { this._threadCount = value; }
    
    public WorkingDirectory: string | null;
    public Commands: Command[];

    public AllOptions: string[];
    public SaveAliasName: string | null;
    public SaveAliasScope: ConfigFileScope | null;

    /**
     * Parses command line arguments and populates command options.
     * @param args The command line arguments to parse.
     * @returns An object containing success status and any exception that occurred.
     */
    public Parse(args: string[]): { success: boolean; exception?: CommandLineException } {
        try {
            const allInputs = this.ExpandedInputsFromCommandLine(args);
            this.ParseInputOptions(allInputs);
            return { success: this.Commands.length > 0 };
        } catch (error) {
            if (error instanceof CommandLineException) {
                return { success: false, exception: error };
            }
            throw error;
        }
    }

    /**
     * Peeks at the command name from the arguments without consuming them.
     * @param args The command line arguments.
     * @param i The current argument index.
     * @returns The command name.
     */
    protected PeekCommandName(args: string[], i: number): string {
        const name1 = this.GetInputOptionArgs(i, args, 1)[0] || '';
        const name2 = this.GetInputOptionArgs(i + 1, args, 1)[0] || '';
        
        let commandName: string;
        switch (name1) {
            case 'version':
                commandName = 'version';
                break;
            case 'help':
                commandName = 'help';
                break;
            default:
                commandName = `${name1} ${name2}`.trim();
                break;
        }
        return commandName;
    }

    /**
     * Checks if a partial command name should trigger help display.
     * @param commandName The command name to check.
     * @returns True if help should be displayed for this partial command.
     */
    protected CheckPartialCommandNeedsHelp(commandName: string): boolean {
        return false;
    }

    /**
     * Creates a new command instance from the given command name.
     * @param commandName The name of the command to create.
     * @returns A new command instance or null if the command is not recognized.
     */
    protected NewCommandFromName(commandName: string): Command | null {
        switch (commandName) {
            case 'help':
                return new HelpCommand();
            case 'version':
                return new VersionCommand();
            default:
                return null;
        }
    }

    /**
     * Creates a default command when no specific command is provided.
     * @returns A default command instance or null.
     */
    protected NewDefaultCommand(): Command | null {
        return null;
    }

    /**
     * Attempts to parse command-specific options. Override in derived classes.
     * @param command The current command being processed.
     * @param args All command line arguments.
     * @param i The current argument index (will be modified if args are consumed).
     * @param arg The current argument being processed.
     * @returns True if the argument was successfully parsed.
     */
    protected TryParseOtherCommandOptions(command: Command | null, args: string[], i: { value: number }, arg: string): boolean {
        return false;
    }

    /**
     * Attempts to parse a command-specific argument. Override in derived classes.
     * @param command The current command being processed.
     * @param arg The argument to parse.
     * @returns True if the argument was successfully parsed.
     */
    protected TryParseOtherCommandArg(command: Command | null, arg: string): boolean {
        return false;
    }

    /**
     * Expands command line inputs, processing @file and @@file references.
     * @param args The raw command line arguments.
     * @returns An array of expanded arguments.
     */
    protected ExpandedInputsFromCommandLine(args: string[]): string[] {
        return args.flatMap(arg => this.ExpandedInput(arg));
    }

    /**
     * Expands a single input argument, handling @file and @@file references.
     * @param input The input argument to expand.
     * @returns An array of expanded arguments.
     */
    protected ExpandedInput(input: string): string[] {
        if (input.startsWith('@@')) {
            return this.ExpandedAtAtFileInput(input);
        } else if (input.startsWith('@')) {
            return [this.ExpandedAtFileInput(input)];
        } else {
            return [input];
        }
    }

    /**
     * Expands @@file input by reading the file and recursively expanding each line.
     * @param input The @@file input to expand.
     * @returns An array of expanded arguments from the file.
     */
    protected ExpandedAtAtFileInput(input: string): string[] {
        if (!input.startsWith('@@')) {
            throw new Error('Not an @@ file input');
        }

        const fileName = input.substring(2);
        const fileNameOk = FileHelpers.FileExists(fileName);
        if (fileNameOk) {
            let lines: string[];
            if (ConsoleHelpers.IsStandardInputReference(fileName)) {
                lines = ConsoleHelpers.GetAllLinesFromStdin();
            } else {
                const fileContent = fs.readFileSync(fileName, 'utf8');
                lines = fileContent.split(/\r?\n/);
            }

            return lines.flatMap(line => this.ExpandedInput(line));
        }

        return [input];
    }

    /**
     * Expands @file input by reading the file content as a single value.
     * @param input The @file input to expand.
     * @returns The expanded file content.
     */
    protected ExpandedAtFileInput(input: string): string {
        if (!input.startsWith('@')) {
            throw new Error('Not an @ file input');
        }

        return AtFileHelpers.ExpandAtFileValue(input);
    }

    /**
     * Parses the expanded input options and builds the command list.
     * @param allInputs The expanded command line arguments.
     */
    protected ParseInputOptions(allInputs: string[]): void {
        let command: Command | null = null;

        const args = this.AllOptions = [...allInputs];

        // Make a pass to dereference all aliases.
        for (let i = 0; i < args.length; i++) {
            if (args[i].startsWith('--')) {
                const result = this.ExpandAliasOptions(command, args, i, args[i].substring(2));
                command = result.command;
                // Note: In TypeScript we need to handle array modification differently
                // The ExpandAliasOptions method will return the new args array
                if (result.newArgs.length !== args.length) {
                    args.splice(0, args.length, ...result.newArgs);
                }
            }
        }

        for (let i = 0; i < args.length; i++) {
            const parseResult = this.TryParseInputOptions(command, args, i, args[i]);
            command = parseResult.command;
            i = parseResult.newIndex;
            
            if (!parseResult.parsed) {
                for (let j = 0; j < i; j++) {
                    ConsoleHelpers.WriteDebugLine(`arg[${j}] = ${args[j]}`);
                }
                ConsoleHelpers.WriteDebugLine(`(INVALID) arg[${i}] = ${args[i]}`);
                throw this.InvalidArgException(command, args[i]);
            }
        }

        if (args.length === 0) {
            command = this.NewDefaultCommand();
        }

        if (!this.HelpTopic && command !== null && command.IsEmpty()) {
            this.HelpTopic = command.GetHelpTopic();
        }

        if (command !== null && !command.IsEmpty()) {
            this.Commands.push(command.Validate());
        }
    }

    /**
     * Attempts to parse a known setting option from the command line.
     * @param args All command line arguments.
     * @param i The current argument index (will be modified if args are consumed).
     * @param arg The current argument being processed.
     * @returns An object containing parsing success and the updated index.
     */
    protected TryParseKnownSettingOption(args: string[], i: { value: number }, arg: string): { parsed: boolean; newIndex: number } {
        // Check if this is a known setting CLI option
        const parseResult = KnownSettingsCLIParser.TryParseCLIOption(arg);
        if (parseResult.success) {
            const { settingName, value } = parseResult;
            
            // If value is in the same arg (e.g. --setting=value)
            if (value !== null) {
                // Add to configuration store
                ConfigStore.Instance.SetFromCommandLine(settingName!, value);
                ConsoleHelpers.WriteDebugLine(`Set known setting from CLI: ${settingName} = ${value}`);
                return { parsed: true, newIndex: i.value };
            }

            // Otherwise, get one or more args
            const allowMultipleArgs = KnownSettings.IsMultiValue(settingName!);
            const maxArgs = allowMultipleArgs ? Number.MAX_SAFE_INTEGER : 1;
            const argumentsArray = this.GetInputOptionArgs(i.value + 1, args, maxArgs);

            if (argumentsArray.length === 0) {
                throw new CommandLineException(`Missing value for ${arg}`);
            } else if (argumentsArray.length === 1) {
                // If there's only one argument, use it directly as a string
                const settingValue = argumentsArray[0];

                // Add to configuration store
                ConfigStore.Instance.SetFromCommandLine(settingName!, settingValue);
                ConsoleHelpers.WriteDebugLine(`Set known setting from CLI: ${settingName} = ${settingValue}`);
            } else {
                // If there are multiple arguments, use them as a list
                ConfigStore.Instance.SetFromCommandLine(settingName!, argumentsArray);
                ConsoleHelpers.WriteDebugLine(`Set known setting from CLI: ${settingName} = [${argumentsArray.join(', ')}]`);
            }

            return { parsed: true, newIndex: i.value + argumentsArray.length };
        }

        return { parsed: false, newIndex: i.value };
    }

    /**
     * Attempts to parse input options and update the current command and parsing state.
     * @param command The current command being processed.
     * @param args All command line arguments.
     * @param i The current argument index.
     * @param arg The current argument being processed.
     * @returns An object containing the updated command, parsing success, and new index.
     */
    protected TryParseInputOptions(command: Command | null, args: string[], i: number, arg: string): { command: Command | null; parsed: boolean; newIndex: number } {
        const isEndOfCommand = arg === '--' && command !== null && !command.IsEmpty();
        if (isEndOfCommand) {
            this.Commands.push(command!.Validate());
            return { command: null, parsed: true, newIndex: i };
        }

        const needNewCommand = command === null;
        if (needNewCommand) {
            const commandName = this.PeekCommandName(args, i);
            const partialCommandNeedsHelp = this.CheckPartialCommandNeedsHelp(commandName);
            if (partialCommandNeedsHelp) {
                command = new HelpCommand();
                this.HelpTopic = commandName;
                return { command, parsed: true, newIndex: i };
            }

            command = this.NewCommandFromName(commandName);

            const parsedCommand = command !== null;
            if (parsedCommand) {
                const skipHowManyExtraArgs = (commandName.match(/ /g) || []).length;
                return { command, parsed: true, newIndex: i + skipHowManyExtraArgs };
            }

            command = this.NewDefaultCommand();
        }

        // Try parsing different option types
        const indexRef = { value: i };
        let parsedOption = false;
        let newIndex = i;

        // Try each parser type
        const globalResult = this.TryParseGlobalCommandLineOptions(args, indexRef, arg);
        if (globalResult.parsed) {
            parsedOption = true;
            newIndex = globalResult.newIndex;
        }
        
        if (!parsedOption) {
            const helpResult = this.TryParseHelpCommandOptions(command instanceof HelpCommand ? command : null, args, indexRef, arg);
            if (helpResult.parsed) {
                parsedOption = true;
                newIndex = helpResult.newIndex;
            }
        }
        
        if (!parsedOption) {
            const versionResult = this.TryParseVersionCommandOptions(command instanceof VersionCommand ? command : null, args, indexRef, arg);
            if (versionResult.parsed) {
                parsedOption = true;
                newIndex = versionResult.newIndex;
            }
        }
        
        if (!parsedOption) {
            const otherResult = this.TryParseOtherCommandOptions(command, args, indexRef, arg);
            if (otherResult) {
                parsedOption = true;
                newIndex = indexRef.value;
            }
        }
        
        if (!parsedOption) {
            const sharedResult = this.TryParseSharedCommandOptions(command, args, indexRef, arg);
            if (sharedResult.parsed) {
                parsedOption = true;
                newIndex = sharedResult.newIndex;
            }
        }
        
        if (!parsedOption) {
            const knownResult = this.TryParseKnownSettingOption(args, indexRef, arg);
            if (knownResult.parsed) {
                parsedOption = true;
                newIndex = knownResult.newIndex;
            }
        }
        
        if (parsedOption) {
            return { command, parsed: true, newIndex };
        }

        if (arg === '--help') {
            this.HelpTopic = command?.GetHelpTopic() ?? 'usage';
            command = new HelpCommand();
            return { command, parsed: true, newIndex: args.length };
        } else if (arg === '--version') {
            command = new VersionCommand();
            return { command, parsed: true, newIndex: args.length };
        } else if (command instanceof HelpCommand) {
            this.HelpTopic = `${this.HelpTopic} ${arg}`.trim();
            parsedOption = true;
        }

        if (!parsedOption && this.TryParseOtherCommandArg(command, arg)) {
            parsedOption = true;
        }

        if (!parsedOption) {
            ConsoleHelpers.WriteDebugLine(`Unknown command line option: ${arg}`);
        }

        return { command, parsed: parsedOption, newIndex: i };
    }

    /**
     * Expands alias options by reading alias files and replacing the alias with its content.
     * @param command The current command being processed.
     * @param args The command line arguments array.
     * @param currentIndex The current argument index.
     * @param alias The alias name to expand.
     * @returns An object containing the updated command and new args array.
     */
    protected ExpandAliasOptions(command: Command | null, args: string[], currentIndex: number, alias: string): { command: Command | null; newArgs: string[] } {
        const aliasFilePath = AliasFileHelpers.FindAliasFile(alias);
        if (aliasFilePath && fs.existsSync(aliasFilePath)) {
            const aliasContent = fs.readFileSync(aliasFilePath, 'utf8');
            const aliasLines = aliasContent.split(/\r?\n/).filter(line => line.trim() !== '');

            const aliasArgs = aliasLines.map(x => 
                x.startsWith('@') ? AtFileHelpers.ExpandAtFileValue(x) : x
            );

            const newArgs = [
                ...args.slice(0, currentIndex),
                ...aliasArgs,
                ...args.slice(currentIndex + 1)
            ];
            
            return { command, newArgs };
        }
        
        return { command, newArgs: args };
    }

    /**
     * Attempts to parse global command line options.
     * @param args All command line arguments.
     * @param i The current argument index (will be modified if args are consumed).
     * @param arg The current argument being processed.
     * @returns An object containing parsing success and the updated index.
     */
    protected TryParseGlobalCommandLineOptions(args: string[], i: { value: number }, arg: string): { parsed: boolean; newIndex: number } {
        let parsed = true;
        let argsConsumed = 0;

        if (arg === '--and') {
            // ignore --and ... used when combining @@ files
        } else if (arg === '--interactive') {
            const max1Arg = this.GetInputOptionArgs(i.value + 1, args, 1);
            const interactive = max1Arg[0] || 'true';
            this.Interactive = interactive.toLowerCase() === 'true' || interactive === '1';
            argsConsumed = max1Arg.length;
        } else if (arg === '--debug') {
            this.Debug = true;
            ConsoleHelpers.ConfigureDebug(true);
        } else if (arg === '--verbose') {
            this.Verbose = true;
        } else if (arg === '--quiet') {
            this.Quiet = true;
        } else if (arg === '--save-local-alias') {
            const max1Arg = this.GetInputOptionArgs(i.value + 1, args, 1);
            const aliasName = max1Arg[0];
            if (!aliasName) {
                throw new CommandLineException('Missing alias name for --save-local-alias');
            }
            this.SaveAliasName = aliasName;
            this.SaveAliasScope = ConfigFileScope.Local;
            argsConsumed = max1Arg.length;
        } else if (arg === '--save-alias' || arg === '--save-user-alias') {
            const max1Arg = this.GetInputOptionArgs(i.value + 1, args, 1);
            const aliasName = max1Arg[0];
            if (!aliasName) {
                throw new CommandLineException('Missing alias name for --save-user-alias');
            }
            this.SaveAliasName = aliasName;
            this.SaveAliasScope = ConfigFileScope.User;
            argsConsumed = max1Arg.length;
        } else if (arg === '--save-global-alias') {
            const max1Arg = this.GetInputOptionArgs(i.value + 1, args, 1);
            const aliasName = max1Arg[0];
            if (!aliasName) {
                throw new CommandLineException('Missing alias name for --save-global-alias');
            }
            this.SaveAliasName = aliasName;
            this.SaveAliasScope = ConfigFileScope.Global;
            argsConsumed = max1Arg.length;
        } else if (arg === '--profile') {
            const max1Arg = this.GetInputOptionArgs(i.value + 1, args, 1);
            const profileName = this.ValidateString(arg, max1Arg[0], 'profile name');
            ProfileFileHelpers.LoadProfile(profileName!);
            argsConsumed = max1Arg.length;
        } else if (arg === '--config') {
            const configFiles = this.GetInputOptionArgs(i.value + 1, args);
            ConfigStore.Instance.LoadConfigFiles(this.ValidateFilesExist(configFiles));
            argsConsumed = configFiles.length;
        } else if (arg === '--threads') {
            const countStr = i.value + 1 < args.length ? args[i.value + 1] : null;
            this.ThreadCount = this.ValidateInt(arg, countStr, 'thread count');
            argsConsumed = countStr ? 1 : 0;
        } else if (arg === '--working-dir' || arg === '--folder' || arg === '--dir' || arg === '--cwd') {
            const max1Arg = this.GetInputOptionArgs(i.value + 1, args, 1);
            const dirPath = this.ValidateString(arg, max1Arg[0], 'directory path');
            this.WorkingDirectory = dirPath;
            argsConsumed = max1Arg.length;
        } else {
            parsed = false;
        }

        return { parsed, newIndex: i.value + argsConsumed };
    }

    /**
     * Attempts to parse help command specific options.
     * @param helpCommand The help command instance, or null if not a help command.
     * @param args All command line arguments.
     * @param i The current argument index.
     * @param arg The current argument being processed.
     * @returns An object containing parsing success and the updated index.
     */
    protected TryParseHelpCommandOptions(helpCommand: HelpCommand | null, args: string[], i: { value: number }, arg: string): { parsed: boolean; newIndex: number } {
        let parsed = true;

        if (helpCommand === null) {
            parsed = false;
        } else if (arg === '--expand') {
            this.ExpandHelpTopics = true;
        } else {
            parsed = false;
        }

        return { parsed, newIndex: i.value };
    }

    /**
     * Attempts to parse version command specific options.
     * @param versionCommand The version command instance, or null if not a version command.
     * @param args All command line arguments.
     * @param i The current argument index.
     * @param arg The current argument being processed.
     * @returns An object containing parsing success and the updated index.
     */
    protected TryParseVersionCommandOptions(versionCommand: VersionCommand | null, args: string[], i: { value: number }, arg: string): { parsed: boolean; newIndex: number } {
        let parsed = true;

        if (versionCommand === null) {
            parsed = false;
        } else {
            parsed = false;
        }

        return { parsed, newIndex: i.value };
    }


    /**
     * Attempts to parse shared command options that apply to multiple command types.
     * @param command The current command instance.
     * @param args All command line arguments.
     * @param i The current argument index.
     * @param arg The current argument being processed.
     * @returns An object containing parsing success and the updated index.
     */
    protected TryParseSharedCommandOptions(command: Command | null, args: string[], i: { value: number }, arg: string): { parsed: boolean; newIndex: number } {
        let parsed = true;

        if (command === null) {
            parsed = false;
        } else {
            parsed = false;
        }

        return { parsed, newIndex: i.value };
    }

    /**
     * Gets input option arguments starting from a specific index.
     * @param startAt The starting index in the arguments array.
     * @param args All command line arguments.
     * @param max The maximum number of arguments to consume.
     * @param required The minimum number of arguments required.
     * @returns An array of argument strings.
     */
    protected GetInputOptionArgs(startAt: number, args: string[], max: number = Number.MAX_SAFE_INTEGER, required: number = 0): string[] {
        const result: string[] = [];
        let found = 0;
        
        for (let i = startAt; i < args.length && i - startAt < max; i++, found++) {
            if (args[i].startsWith('--') && found >= required) {
                break;
            }
            result.push(args[i]);
        }
        
        return result;
    }

    /**
     * Validates that a string argument is not null or empty.
     * @param arg The option name for error reporting.
     * @param argStr The string value to validate.
     * @param argDescription Description of the argument for error messages.
     * @returns The validated string.
     */
    protected ValidateString(arg: string, argStr: string | undefined, argDescription: string): string | null {
        if (!argStr || argStr.trim() === '') {
            throw new CommandLineException(`Missing ${argDescription} for ${arg}`);
        }

        return argStr;
    }

    /**
     * Validates that a collection of string arguments is not empty.
     * @param arg The option name for error reporting.
     * @param argStrs The string values to validate.
     * @param argDescription Description of the arguments for error messages.
     * @param allowEmptyStrings Whether to allow empty strings in the collection.
     * @returns The validated array of strings.
     */
    protected ValidateStrings(arg: string, argStrs: string[], argDescription: string, allowEmptyStrings: boolean = false): string[] {
        const strings = [...argStrs];
        if (strings.length === 0) {
            throw new CommandLineException(`Missing ${argDescription} for ${arg}`);
        }

        return strings.map(x => allowEmptyStrings ? x : this.ValidateString(arg, x, argDescription)!);
    }

    /**
     * Validates and joins strings with a separator.
     * @param arg The option name for error reporting.
     * @param seed The initial string value.
     * @param values The values to join with the seed.
     * @param separator The separator to use for joining.
     * @param argDescription Description for error messages.
     * @returns The joined string.
     */
    protected static ValidateJoinedString(arg: string, seed: string, values: string[], separator: string, argDescription: string): string {
        const result = [seed, ...values].join(separator).trim();
        if (!result) {
            throw new CommandLineException(`Missing ${argDescription} for ${arg}`);
        }

        return result;
    }

    /**
     * Validates and parses a variable assignment in NAME=VALUE format.
     * @param arg The option name for error reporting.
     * @param assignment The assignment string to validate.
     * @returns A tuple containing the variable name and value.
     */
    protected ValidateAssignment(arg: string, assignment: string | undefined): [string, string] {
        const validatedAssignment = this.ValidateString(arg, assignment, 'assignment')!;

        const parts = validatedAssignment.split('=');
        if (parts.length !== 2) {
            throw new CommandLineException(`Invalid variable definition for ${arg}: ${validatedAssignment}. Use NAME=VALUE format.`);
        }

        return [parts[0], parts[1]];
    }

    /**
     * Validates multiple variable assignments.
     * @param arg The option name for error reporting.
     * @param assignments The assignment strings to validate.
     * @returns An array of tuples containing variable names and values.
     */
    protected ValidateAssignments(arg: string, assignments: string[]): [string, string][] {
        if (assignments.length === 0) {
            throw new CommandLineException(`Missing variable assignments for ${arg}`);
        }

        return assignments.map(x => this.ValidateAssignment(arg, x));
    }

    /**
     * Validates that a file name is provided.
     * @param arg The file name to validate.
     * @returns The validated file name.
     */
    protected ValidateOkFileName(arg: string | undefined): string {
        if (!arg || arg.trim() === '') {
            throw new CommandLineException('Missing file name');
        }

        return arg;
    }

    /**
     * Validates that all files in the collection exist.
     * @param args The file paths to validate.
     * @returns The validated array of file paths.
     */
    protected ValidateFilesExist(args: string[]): string[] {
        const files = [...args];
        for (const arg of files) {
            if (!arg || arg.trim() === '') {
                throw new CommandLineException('Missing file name');
            }

            if (!fs.existsSync(arg)) {
                throw new CommandLineException(`File does not exist: ${arg}`);
            }
        }

        return files;
    }

    /**
     * Validates that a single file exists.
     * @param arg The file path to validate.
     * @returns The validated file path.
     */
    protected ValidateFileExists(arg: string | undefined): string | null {
        if (!arg || arg.trim() === '') {
            throw new CommandLineException('Missing file name');
        }

        if (!fs.existsSync(arg)) {
            throw new CommandLineException(`File does not exist: ${arg}`);
        }

        return arg;
    }

    /**
     * Validates regular expression patterns.
     * @param arg The option name for error reporting.
     * @param patterns The regex patterns to validate.
     * @returns An array of compiled RegExp objects.
     */
    protected ValidateRegExPatterns(arg: string, patterns: string[]): RegExp[] {
        const patternArray = [...patterns];
        if (patternArray.length === 0) {
            throw new CommandLineException(`Missing regular expression patterns for ${arg}`);
        }

        return patternArray.map(x => this.ValidateRegExPattern(arg, x));
    }

    /**
     * Validates and compiles a single regular expression pattern.
     * @param arg The option name for error reporting.
     * @param pattern The regex pattern to validate.
     * @returns A compiled RegExp object.
     */
    protected ValidateRegExPattern(arg: string, pattern: string): RegExp {
        try {
            return new RegExp(pattern, 'i');
        } catch (error) {
            throw new CommandLineException(`Invalid regular expression pattern for ${arg}: ${pattern}`);
        }
    }

    /**
     * Validates and separates patterns into regex and glob patterns.
     * @param arg The option name for error reporting.
     * @param patterns The patterns to validate and separate.
     * @returns An object containing separated regex patterns and glob patterns.
     */
    protected ValidateExcludeRegExAndGlobPatterns(arg: string, patterns: string[]): { asRegExs: RegExp[]; asGlobs: string[] } {
        if (patterns.length === 0) {
            throw new CommandLineException(`Missing patterns for ${arg}`);
        }

        const containsSlash = (x: string) => x.includes('/') || x.includes('\\');
        const asRegExs = patterns
            .filter(x => !containsSlash(x))
            .map(x => this.ValidateFilePatternToRegExPattern(arg, x));
        const asGlobs = patterns
            .filter(x => containsSlash(x));
            
        return { asRegExs, asGlobs };
    }

    /**
     * Validates and converts a file pattern to a regular expression.
     * @param arg The option name for error reporting.
     * @param pattern The file pattern to convert.
     * @returns A compiled RegExp object.
     */
    protected ValidateFilePatternToRegExPattern(arg: string, pattern: string): RegExp {
        if (!pattern || pattern.trim() === '') {
            throw new CommandLineException(`Missing file pattern for ${arg}`);
        }

        const isWindows = os.platform() === 'win32';
        const patternPrefix = isWindows ? '(?i)^' : '^';
        const regexPattern = patternPrefix + pattern
            .replace(/\./g, '\\.')
            .replace(/\*/g, '.*')
            .replace(/\?/g, '.') + '$';

        try {
            return new RegExp(regexPattern);
        } catch (error) {
            throw new CommandLineException(`Invalid file pattern for ${arg}: ${pattern}`);
        }
    }

    /**
     * Validates a line count argument.
     * @param arg The option name for error reporting.
     * @param countStr The line count string to validate.
     * @returns The validated line count as a number.
     */
    protected ValidateLineCount(arg: string, countStr: string | undefined): number {
        return this.ValidateInt(arg, countStr, 'line count');
    }

    /**
     * Validates an integer argument.
     * @param arg The option name for error reporting.
     * @param countStr The integer string to validate.
     * @param argDescription Description of the argument for error messages.
     * @returns The validated integer.
     */
    protected ValidateInt(arg: string, countStr: string | undefined, argDescription: string): number {
        if (!countStr || countStr.trim() === '') {
            throw new CommandLineException(`Missing ${argDescription} for ${arg}`);
        }

        const count = parseInt(countStr, 10);
        if (isNaN(count)) {
            throw new CommandLineException(`Invalid ${argDescription} for ${arg}: ${countStr}`);
        }

        return count;
    }

    /**
     * Creates a CommandLineException for invalid arguments.
     * @param command The current command being processed.
     * @param arg The invalid argument.
     * @returns A new CommandLineException instance.
     */
    protected InvalidArgException(command: Command | null, arg: string): CommandLineException {
        const message = `Invalid argument: ${arg}`;
        return new CommandLineException(message, command?.GetHelpTopic() ?? '');
    }
}
}
