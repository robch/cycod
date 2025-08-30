import { ProgramRunner, CommandLineOptions, CommandLineException, ShellSession } from 'cycod-common';
import { CycoDevCommandLineOptions } from './CommandLine/CycoDevCommandLineOptions';
import { CycoDevProgramInfo } from './CycoDevProgramInfo';

export class CycoDevProgramRunner extends ProgramRunner {
    private _programInfo = new CycoDevProgramInfo();

    static async runAsync(args: string[]): Promise<number> {
        try {
            const program = new CycoDevProgramRunner();
            return await program.runProgramAsync(args);
        } finally {
            ShellSession.disposeAll();
        }
    }

    protected parseCommandLine(args: string[]): { 
        success: boolean; 
        commandLineOptions?: CommandLineOptions; 
        exception?: CommandLineException;
    } {
        return CycoDevCommandLineOptions.parse(args);
    }

    protected displayBanner(): void {
        // TODO: Implement cycod banner display
        console.log('CycoDJS - AI-powered CLI');
    }
}