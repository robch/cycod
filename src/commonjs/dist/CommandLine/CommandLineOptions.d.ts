import { Command } from './Command';
import { CommandLineException } from './CommandLineException';
import { HelpCommand } from '../CommandLineCommands/HelpCommand';
import { VersionCommand } from '../CommandLineCommands/VersionCommand';
export declare class CommandLineOptions {
    interactive: boolean;
    debug: boolean;
    verbose: boolean;
    quiet: boolean;
    helpTopic: string;
    expandHelpTopics: boolean;
    threadCount: number;
    workingDirectory?: string;
    commands: Command[];
    allOptions: string[];
    saveAliasName?: string;
    saveAliasScope?: string;
    protected constructor();
    parse(args: string[]): {
        success: boolean;
        exception?: CommandLineException;
    };
    protected peekCommandName(args: string[], i: number): string;
    protected checkPartialCommandNeedsHelp(commandName: string): boolean;
    protected newCommandFromName(commandName: string): Command | null;
    protected newDefaultCommand(): Command | null;
    protected tryParseOtherCommandOptions(command: Command | null, args: string[], i: {
        value: number;
    }, arg: string): boolean;
    protected tryParseOtherCommandArg(command: Command | null, arg: string): boolean;
    protected expandedInputsFromCommandLine(args: string[]): string[];
    protected expandedInput(input: string): string[];
    protected expandedAtAtFileInput(input: string): string[];
    protected expandedAtFileInput(input: string): string;
    protected parseInputOptions(allInputs: string[]): void;
    private updateCommand;
    protected tryParseInputOptions(command: Command | null, args: string[], i: {
        value: number;
    }, arg: string): boolean;
    protected expandAliasOptions(command: Command | null, args: string[], currentIndex: number, alias: string): void;
    protected tryParseGlobalCommandLineOptions(args: string[], i: {
        value: number;
    }, arg: string): boolean;
    protected tryParseHelpCommandOptions(helpCommand: HelpCommand | null, args: string[], i: {
        value: number;
    }, arg: string): boolean;
    protected tryParseVersionCommandOptions(versionCommand: VersionCommand | null, args: string[], i: {
        value: number;
    }, arg: string): boolean;
    protected getInputOptionArgs(startAt: number, args: string[], max?: number): string[];
    protected validateString(arg: string, argStr: string | undefined, argDescription: string): string;
    protected validateInt(arg: string, countStr: string | undefined, argDescription: string): number;
    protected invalidArgException(command: Command | null, arg: string): CommandLineException;
}
//# sourceMappingURL=CommandLineOptions.d.ts.map