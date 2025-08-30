import { CommandLineOptions } from './CommandLine/CommandLineOptions';
import { CommandLineException } from './CommandLine/CommandLineException';
import { ConsoleHelpers } from './Helpers/ConsoleHelpers';

export abstract class ProgramRunner {
    protected async runProgramAsync(args: string[]): Promise<number> {
        try {
            this.saveConsoleColor();
            return await this.doProgram(this.processDirectives(args));
        } catch (error) {
            if (error instanceof Error) {
                ConsoleHelpers.writeError(`Error: ${error.message}`);
                return 1;
            }
            return 1;
        } finally {
            this.restoreConsoleColor();
        }
    }

    protected abstract parseCommandLine(args: string[]): { 
        success: boolean; 
        commandLineOptions?: CommandLineOptions; 
        exception?: CommandLineException;
    };

    private async doProgram(args: string[]): Promise<number> {
        const parseResult = this.parseCommandLine(args);
        
        if (!parseResult.success) {
            this.displayBanner();
            
            if (parseResult.exception) {
                ConsoleHelpers.writeError(parseResult.exception.message);
                
                const helpTopic = parseResult.exception.getHelpTopic();
                if (helpTopic) {
                    // Display help for the specific topic
                    console.log(`\nFor more help, see: ${helpTopic}`);
                }
            }
            
            return parseResult.exception ? 2 : 1;
        }

        const commandLineOptions = parseResult.commandLineOptions!;
        
        this.displayBanner();
        this.configureConsoleHelpers(commandLineOptions);

        if (this.shouldShowHelp(commandLineOptions)) {
            return await this.showHelp(commandLineOptions);
        }

        if (this.shouldShowVersion(commandLineOptions)) {
            return await this.showVersion(commandLineOptions);
        }

        return await this.runCommands(commandLineOptions);
    }

    private async runCommands(commandLineOptions: CommandLineOptions): Promise<number> {
        let exitCode = 0;

        for (const command of commandLineOptions.commands) {
            try {
                const result = await command.executeAsync(commandLineOptions.interactive);
                
                if (typeof result === 'number') {
                    exitCode = result;
                    if (exitCode !== 0) break;
                }
            } catch (error) {
                ConsoleHelpers.writeError(`Command execution failed: ${(error as Error).message}`);
                exitCode = 1;
                break;
            }
        }

        return exitCode;
    }

    protected processDirectives(args: string[]): string[] {
        // Simple implementation - in C# this was more complex
        return args;
    }

    protected shouldShowHelp(commandLineOptions: CommandLineOptions): boolean {
        return !!commandLineOptions.helpTopic || commandLineOptions.commands.length === 0;
    }

    protected shouldShowVersion(commandLineOptions: CommandLineOptions): boolean {
        return commandLineOptions.commands.some(cmd => cmd.getCommandName() === 'version');
    }

    protected async showHelp(commandLineOptions: CommandLineOptions): Promise<number> {
        // TODO: Implement help display logic
        console.log('Help information would be displayed here');
        return 0;
    }

    protected async showVersion(commandLineOptions: CommandLineOptions): Promise<number> {
        // TODO: Implement version display logic
        console.log('Version: 1.0.0');
        return 0;
    }

    protected displayBanner(): void {
        // Override in derived classes to show application banner
    }

    protected configureConsoleHelpers(commandLineOptions: CommandLineOptions): void {
        ConsoleHelpers.configure(
            commandLineOptions.debug,
            commandLineOptions.verbose,
            commandLineOptions.quiet
        );
    }

    private saveConsoleColor(): void {
        // TODO: Implement console color saving if needed
    }

    private restoreConsoleColor(): void {
        // TODO: Implement console color restoration if needed
    }
}