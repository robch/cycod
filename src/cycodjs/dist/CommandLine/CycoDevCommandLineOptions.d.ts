import { CommandLineOptions, CommandLineException } from 'cycod-common';
export declare class CycoDevCommandLineOptions extends CommandLineOptions {
    static parse(args: string[]): {
        success: boolean;
        commandLineOptions?: CommandLineOptions;
        exception?: CommandLineException;
    };
    protected peekCommandName(args: string[], i: number): string;
    protected checkPartialCommandNeedsHelp(commandName: string): boolean;
    protected newCommandFromName(commandName: string): any;
    protected newDefaultCommand(): any;
}
//# sourceMappingURL=CycoDevCommandLineOptions.d.ts.map