import { CommandLineOptions } from './CommandLine/CommandLineOptions';
import { CommandLineException } from './CommandLine/CommandLineException';
export declare abstract class ProgramRunner {
    protected runProgramAsync(args: string[]): Promise<number>;
    protected abstract parseCommandLine(args: string[]): {
        success: boolean;
        commandLineOptions?: CommandLineOptions;
        exception?: CommandLineException;
    };
    private doProgram;
    private runCommands;
    protected processDirectives(args: string[]): string[];
    protected shouldShowHelp(commandLineOptions: CommandLineOptions): boolean;
    protected shouldShowVersion(commandLineOptions: CommandLineOptions): boolean;
    protected showHelp(commandLineOptions: CommandLineOptions): Promise<number>;
    protected showVersion(commandLineOptions: CommandLineOptions): Promise<number>;
    protected displayBanner(): void;
    protected configureConsoleHelpers(commandLineOptions: CommandLineOptions): void;
    private saveConsoleColor;
    private restoreConsoleColor;
}
//# sourceMappingURL=ProgramRunner.d.ts.map