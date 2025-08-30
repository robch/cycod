import { ProgramRunner, CommandLineOptions, CommandLineException } from 'cycod-common';
export declare class CycoDevProgramRunner extends ProgramRunner {
    private _programInfo;
    static runAsync(args: string[]): Promise<number>;
    protected parseCommandLine(args: string[]): {
        success: boolean;
        commandLineOptions?: CommandLineOptions;
        exception?: CommandLineException;
    };
    protected displayBanner(): void;
}
//# sourceMappingURL=CycoDevProgramRunner.d.ts.map