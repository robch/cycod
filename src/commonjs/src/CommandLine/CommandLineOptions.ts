import * as fs from 'fs';
import { Command } from './Command';
import { CommandLineException } from './CommandLineException';
import { HelpCommand } from '../CommandLineCommands/HelpCommand';
import { VersionCommand } from '../CommandLineCommands/VersionCommand';

export class CommandLineOptions {
    interactive: boolean = true;
    debug: boolean = false;
    verbose: boolean = false;
    quiet: boolean = false;
    helpTopic: string = '';
    expandHelpTopics: boolean = false;
    threadCount: number = 0;
    workingDirectory?: string = undefined;
    commands: Command[] = [];
    allOptions: string[] = [];
    saveAliasName?: string = undefined;
    saveAliasScope?: string = undefined;

    protected constructor() {}

    parse(args: string[]): { success: boolean; exception?: CommandLineException } {
        try {
            const allInputs = this.expandedInputsFromCommandLine(args);
            this.parseInputOptions(allInputs);
            return { success: this.commands.length > 0 };
        } catch (error) {
            if (error instanceof CommandLineException) {
                return { success: false, exception: error };
            }
            throw error;
        }
    }

    protected peekCommandName(args: string[], i: number): string {
        const name1 = this.getInputOptionArgs(i, args, 1)[0] || '';
        const name2 = this.getInputOptionArgs(i + 1, args, 1)[0] || '';
        
        switch (name1) {
            case "version":
                return "version";
            case "help":
                return "help";
            default:
                return `${name1} ${name2}`.trim();
        }
    }

    protected checkPartialCommandNeedsHelp(commandName: string): boolean {
        return false;
    }

    protected newCommandFromName(commandName: string): Command | null {
        switch (commandName) {
            case "help":
                return new HelpCommand();
            case "version":
                return new VersionCommand();
            default:
                return null;
        }
    }

    protected newDefaultCommand(): Command | null {
        return null;
    }

    protected tryParseOtherCommandOptions(command: Command | null, args: string[], i: { value: number }, arg: string): boolean {
        return false;
    }

    protected tryParseOtherCommandArg(command: Command | null, arg: string): boolean {
        return false;
    }

    protected expandedInputsFromCommandLine(args: string[]): string[] {
        return args.flatMap(arg => this.expandedInput(arg));
    }

    protected expandedInput(input: string): string[] {
        if (input.startsWith("@@")) {
            return this.expandedAtAtFileInput(input);
        } else if (input.startsWith("@")) {
            return [this.expandedAtFileInput(input)];
        } else {
            return [input];
        }
    }

    protected expandedAtAtFileInput(input: string): string[] {
        if (!input.startsWith("@@")) {
            throw new Error("Not an @@ file input");
        }

        const fileName = input.substring(2);
        if (fs.existsSync(fileName)) {
            const content = fs.readFileSync(fileName, 'utf-8');
            const lines = content.split(/\r?\n/).filter(line => line.trim() !== '');
            return lines.flatMap(line => this.expandedInput(line));
        }

        return [input];
    }

    protected expandedAtFileInput(input: string): string {
        if (!input.startsWith("@")) {
            throw new Error("Not an @ file input");
        }

        const fileName = input.substring(1);
        if (fs.existsSync(fileName)) {
            return fs.readFileSync(fileName, 'utf-8').trim();
        }

        return input;
    }

    protected parseInputOptions(allInputs: string[]): void {
        let command: Command | null = null;
        const args = this.allOptions = allInputs;

        // Expand aliases
        for (let i = 0; i < args.length; i++) {
            if (args[i].startsWith("--")) {
                this.expandAliasOptions(command, args, i, args[i].substring(2));
            }
        }

        for (let i = 0; i < args.length; i++) {
            const iRef = { value: i };
            const parsed = this.tryParseInputOptions(command, args, iRef, args[i]);
            i = iRef.value;
            
            if (!parsed) {
                throw this.invalidArgException(command, args[i]);
            }

            if (command && !command.isEmpty()) {
                command = this.updateCommand(command);
            }
        }

        if (args.length === 0) {
            command = this.newDefaultCommand();
        }

        if (!this.helpTopic && command && command.isEmpty()) {
            this.helpTopic = command.getHelpTopic();
        }

        if (command && !command.isEmpty()) {
            this.commands.push(command.validate());
        }
    }

    private updateCommand(command: Command): Command {
        // Logic for updating command would go here
        return command;
    }

    protected tryParseInputOptions(command: Command | null, args: string[], i: { value: number }, arg: string): boolean {
        const isEndOfCommand = arg === "--" && command && !command.isEmpty();
        if (isEndOfCommand) {
            this.commands.push(command!.validate());
            command = null;
            return true;
        }

        const needNewCommand = command === null;
        if (needNewCommand) {
            const commandName = this.peekCommandName(args, i.value);
            const partialCommandNeedsHelp = this.checkPartialCommandNeedsHelp(commandName);
            
            if (partialCommandNeedsHelp) {
                command = new HelpCommand();
                this.helpTopic = commandName;
                return true;
            }

            command = this.newCommandFromName(commandName);

            if (command) {
                const skipCount = commandName.split(' ').length - 1;
                i.value += skipCount;
                return true;
            }

            command = this.newDefaultCommand();
        }

        const parsedOption = 
            this.tryParseGlobalCommandLineOptions(args, i, arg) ||
            this.tryParseHelpCommandOptions(command as HelpCommand, args, i, arg) ||
            this.tryParseVersionCommandOptions(command as VersionCommand, args, i, arg) ||
            this.tryParseOtherCommandOptions(command, args, i, arg);

        if (parsedOption) return true;

        if (arg === "--help") {
            this.helpTopic = command?.getHelpTopic() || "usage";
            command = new HelpCommand();
            i.value = args.length;
            return true;
        } else if (arg === "--version") {
            command = new VersionCommand();
            i.value = args.length;
            return true;
        } else if (command instanceof HelpCommand) {
            this.helpTopic = `${this.helpTopic} ${arg}`.trim();
            return true;
        }

        return this.tryParseOtherCommandArg(command, arg);
    }

    protected expandAliasOptions(command: Command | null, args: string[], currentIndex: number, alias: string): void {
        // TODO: Implement alias expansion
    }

    protected tryParseGlobalCommandLineOptions(args: string[], i: { value: number }, arg: string): boolean {
        switch (arg) {
            case "--and":
                // ignore --and ... used when combining @@ files
                return true;
            case "--interactive":
                const interactiveArgs = this.getInputOptionArgs(i.value + 1, args, 1);
                const interactive = interactiveArgs[0] || "true";
                this.interactive = interactive.toLowerCase() === "true" || interactive === "1";
                i.value += interactiveArgs.length;
                return true;
            case "--debug":
                this.debug = true;
                return true;
            case "--verbose":
                this.verbose = true;
                return true;
            case "--quiet":
                this.quiet = true;
                return true;
            case "--threads":
                if (i.value + 1 < args.length) {
                    this.threadCount = this.validateInt(arg, args[++i.value], "thread count");
                }
                return true;
            case "--working-dir":
            case "--folder":
            case "--dir":
            case "--cwd":
                const dirArgs = this.getInputOptionArgs(i.value + 1, args, 1);
                this.workingDirectory = this.validateString(arg, dirArgs[0], "directory path");
                i.value += dirArgs.length;
                return true;
            default:
                return false;
        }
    }

    protected tryParseHelpCommandOptions(helpCommand: HelpCommand | null, args: string[], i: { value: number }, arg: string): boolean {
        if (!helpCommand) return false;

        if (arg === "--expand") {
            this.expandHelpTopics = true;
            return true;
        }

        return false;
    }

    protected tryParseVersionCommandOptions(versionCommand: VersionCommand | null, args: string[], i: { value: number }, arg: string): boolean {
        if (!versionCommand) return false;
        return false;
    }

    protected getInputOptionArgs(startAt: number, args: string[], max: number = Number.MAX_SAFE_INTEGER): string[] {
        const result: string[] = [];
        let found = 0;

        for (let i = startAt; i < args.length && found < max; i++, found++) {
            if (args[i].startsWith("--") && found >= 0) {
                break;
            }
            result.push(args[i]);
        }

        return result;
    }

    protected validateString(arg: string, argStr: string | undefined, argDescription: string): string {
        if (!argStr || !argStr.trim()) {
            throw new CommandLineException(`Missing ${argDescription} for ${arg}`);
        }
        return argStr;
    }

    protected validateInt(arg: string, countStr: string | undefined, argDescription: string): number {
        if (!countStr || !countStr.trim()) {
            throw new CommandLineException(`Missing ${argDescription} for ${arg}`);
        }

        const count = parseInt(countStr, 10);
        if (isNaN(count)) {
            throw new CommandLineException(`Invalid ${argDescription} for ${arg}: ${countStr}`);
        }

        return count;
    }

    protected invalidArgException(command: Command | null, arg: string): CommandLineException {
        const message = `Invalid argument: ${arg}`;
        return new CommandLineException(message, command?.getHelpTopic() || "");
    }
}