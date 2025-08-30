import { CommandLineOptions, CommandLineException, ConsoleHelpers } from 'cycod-common';
import { ChatCommand } from '../CommandLineCommands/ChatCommand';

export class CycoDevCommandLineOptions extends CommandLineOptions {
    static parse(args: string[]): { 
        success: boolean; 
        commandLineOptions?: CommandLineOptions; 
        exception?: CommandLineException;
    } {
        const options = new CycoDevCommandLineOptions();
        const result = options.parse(args);

        if (result.success && options.commands.length === 1) {
            const command = options.commands[0];
            const oneChatCommandWithNoInput = command instanceof ChatCommand && 
                (command as ChatCommand).inputInstructions.length === 0;
            
            const inOrOutRedirected = process.stdin.isTTY === false || process.stdout.isTTY === false;
            const implicitlyUseStdIn = oneChatCommandWithNoInput && inOrOutRedirected;
            
            if (implicitlyUseStdIn) {
                const stdinLines = ConsoleHelpers.getAllLinesFromStdin();
                const joined = stdinLines.join('\n');
                (command as ChatCommand).inputInstructions.push(joined);
            }
        }

        return { success: result.success, commandLineOptions: options, exception: result.exception };
    }

    protected peekCommandName(args: string[], i: number): string {
        const name1 = this.getInputOptionArgs(i, args, 1)[0];
        
        switch (name1) {
            case "chat":
                return "chat";
            default:
                return super.peekCommandName(args, i);
        }
    }

    protected checkPartialCommandNeedsHelp(commandName: string): boolean {
        return commandName === "alias" ||
               commandName === "config" ||
               commandName === "github" ||
               commandName === "mcp" ||
               commandName === "prompt";
    }

    protected newCommandFromName(commandName: string): any {
        switch (commandName) {
            case "chat":
            case "":
                return new ChatCommand();
            default:
                return super.newCommandFromName(commandName);
        }
    }

    protected newDefaultCommand(): any {
        return new ChatCommand();
    }
}