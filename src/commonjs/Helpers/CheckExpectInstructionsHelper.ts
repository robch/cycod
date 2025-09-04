import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { FileHelpers } from './FileHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';
import { OS } from './OS';

export interface RunnableProcessResult {
    standardOutput: string;
    standardError: string;
    mergedOutput: string;
    exitCode: number;
    completionState: ProcessCompletionState;
}

export enum ProcessCompletionState {
    Completed,
    TimedOut,
    Killed
}

export class RunnableProcessBuilder {
    private _fileName: string = '';
    private _arguments: string = '';
    private _workingDirectory?: string;
    private _timeout: number = 60000;
    private _commandLine: string = '';

    public withCommandLine(commandLine: string): RunnableProcessBuilder {
        this._commandLine = commandLine;
        return this;
    }

    public withWorkingDirectory(workingDirectory?: string): RunnableProcessBuilder {
        this._workingDirectory = workingDirectory;
        return this;
    }

    public withTimeout(timeout: number): RunnableProcessBuilder {
        this._timeout = timeout;
        return this;
    }

    public async run(): Promise<RunnableProcessResult> {
        const { spawn } = require('child_process');
        
        // Parse command line
        if (this._commandLine) {
            const parts = this._commandLine.match(/(?:[^\s"]+|"[^"]*")+/g) || [];
            if (parts.length > 0) {
                this._fileName = parts[0].replace(/^"(.*)"$/, '$1');
                this._arguments = parts.slice(1).join(' ');
            }
        }

        return new Promise<RunnableProcessResult>((resolve) => {
            let stdOut = '';
            let stdErr = '';
            let merged = '';
            let completed = false;

            const child = spawn(this._fileName, this._arguments.split(' ').filter(arg => arg.length > 0), {
                cwd: this._workingDirectory,
                shell: true,
                stdio: ['pipe', 'pipe', 'pipe']
            });

            child.stdout?.on('data', (data) => {
                const str = data.toString();
                stdOut += str;
                merged += str;
            });

            child.stderr?.on('data', (data) => {
                const str = data.toString();
                stdErr += str;
                merged += str;
            });

            const timeoutHandle = setTimeout(() => {
                if (!completed) {
                    completed = true;
                    child.kill();
                    resolve({
                        standardOutput: stdOut,
                        standardError: stdErr,
                        mergedOutput: merged,
                        exitCode: -1,
                        completionState: ProcessCompletionState.TimedOut
                    });
                }
            }, this._timeout);

            child.on('close', (code) => {
                if (!completed) {
                    completed = true;
                    clearTimeout(timeoutHandle);
                    resolve({
                        standardOutput: stdOut,
                        standardError: stdErr,
                        mergedOutput: merged,
                        exitCode: code || 0,
                        completionState: ProcessCompletionState.Completed
                    });
                }
            });

            child.on('error', (error) => {
                if (!completed) {
                    completed = true;
                    clearTimeout(timeoutHandle);
                    resolve({
                        standardOutput: stdOut,
                        standardError: stdErr + error.message,
                        mergedOutput: merged + error.message,
                        exitCode: -1,
                        completionState: ProcessCompletionState.Killed
                    });
                }
            });
        });
    }
}

export interface CheckExpectInstructionsResult {
    passed: boolean;
    gptStdOut: string;
    gptStdErr: string;
    gptMerged: string;
}

export class CheckExpectInstructionsHelper {
    private static _cliCache: Map<string, string> = new Map();

    public static async checkExpectations(
        output: string, 
        instructions?: string, 
        workingDirectory?: string
    ): Promise<CheckExpectInstructionsResult> {
        
        const result: CheckExpectInstructionsResult = {
            passed: true,
            gptStdOut: '',
            gptStdErr: '',
            gptMerged: ''
        };

        const noInstructions = !instructions || instructions.length === 0;
        if (noInstructions) return result;

        ConsoleHelpers.writeDebugLine(`CheckExpectInstructionsHelper.checkExpectations: Checking for ${instructions} in '${output}'`);

        const prompt: string[] = [];
        prompt.push(`Here's the console output:\n\n${output}\n`);
        prompt.push(`Here's the expectation:\n\n${instructions}\n`);
        prompt.push('You **must always** answer "PASS" if the expectation is met.');
        prompt.push('You **must always** answer "FAIL" if the expectation is not met.');
        prompt.push('You **must only** answer "PASS" with no additional text if the expectation is met.');
        prompt.push('If you answer "FAIL", you **must** provide additional text to explain why the expectation was not met (without using the word "PASS" as we will interpret that as a "PASS").');
        
        const promptTempFile = FileHelpers.writeTextToTempFile(prompt.join('\n'))!;
        let passed = false;

        try {
            const startProcess = CheckExpectInstructionsHelper.findCacheCli('cycod');
            const startArgs = `--input @${promptTempFile} --interactive false --quiet`;
            const commandLine = `${startProcess} ${startArgs}`;

            ConsoleHelpers.writeDebugLine(`CheckExpectInstructionsHelper.checkExpectations: RunnableProcessBuilder executing '${commandLine}'`);
            const processResult = await new RunnableProcessBuilder()
                .withCommandLine(commandLine)
                .withWorkingDirectory(workingDirectory)
                .withTimeout(60000)
                .run();

            result.gptStdOut = processResult.standardOutput;
            result.gptStdErr = processResult.standardError;
            result.gptMerged = processResult.mergedOutput;

            const exitedNotKilled = processResult.completionState === ProcessCompletionState.Completed;
            const exitedNormally = exitedNotKilled && processResult.exitCode === 0;
            passed = exitedNormally;

            const timedoutOrKilled = !exitedNotKilled;
            if (timedoutOrKilled) {
                const message = 'CheckExpectInstructionsHelper.checkExpectations: WARNING: Timedout or killed!';
                result.gptStdErr += `\n${message}\n`;
                result.gptMerged += `\n${message}\n`;
                ConsoleHelpers.writeDebugLine(message);
            }
        } catch (ex: any) {
            const exception = `CheckExpectInstructionsHelper.checkExpectations: EXCEPTION: ${ex.message}`;
            result.gptStdErr += `\n${exception}\n`;
            result.gptMerged += `\n${exception}\n`;
            ConsoleHelpers.writeDebugLine(exception);
        }

        try {
            fs.unlinkSync(promptTempFile);
        } catch {
            // Ignore cleanup errors
        }

        if (passed) {
            ConsoleHelpers.writeDebugLine(`CheckExpectInstructionsHelper.checkExpectations: Checking for 'PASS' in '${result.gptMerged}'`);
            passed = result.gptMerged.includes('PASS') || result.gptMerged.includes('TRUE') || result.gptMerged.includes('YES');
            ConsoleHelpers.writeDebugLine(`CheckExpectInstructionsHelper.checkExpectations: ${passed}`);
        }

        result.passed = passed;
        return result;
    }

    private static findCacheCli(cli: string): string {
        if (CheckExpectInstructionsHelper._cliCache.has(cli)) {
            return CheckExpectInstructionsHelper._cliCache.get(cli)!;
        }

        const found = CheckExpectInstructionsHelper.findCli(cli);
        CheckExpectInstructionsHelper._cliCache.set(cli, found);

        return found;
    }

    private static findCli(cli: string): string {
        const specified = cli && cli.length > 0;
        if (specified) {
            const found = CheckExpectInstructionsHelper.findCliOrNull(cli);
            return found !== null
                ? CheckExpectInstructionsHelper.cliFound(cli, found)              // use what we found
                : CheckExpectInstructionsHelper.cliNotFound(cli);                 // use what was specified
        } else {
            const clis = ['cycod'];
            const found = CheckExpectInstructionsHelper.pickCliOrNull(clis);
            return found !== null
                ? CheckExpectInstructionsHelper.pickCliFound(clis, found)         // use what we found
                : CheckExpectInstructionsHelper.pickCliNotFound(clis, clis[0]);   // use cycod
        }
    }

    private static findCliOrNull(cli: string): string | null {
        const dll = `${cli}.dll`;
        const exe = OS.isWindows() ? `${cli}.exe` : cli;

        const path1 = process.env.PATH || '';
        const path2 = process.cwd();
        const path3 = FileHelpers.getProgramAssemblyFileInfo().directoryName;
        const pathVar = `${path3}${path.delimiter}${path2}${path.delimiter}${path1}`;

        const paths = pathVar.split(path.delimiter);
        for (const part2 of ['', 'net6.0']) {
            for (const part1 of paths) {
                const checkExe = path.join(part1, part2, exe);
                if (fs.existsSync(checkExe)) {
                    const checkDll = CheckExpectInstructionsHelper.findCliDllOrNull(checkExe, dll);
                    if (checkDll !== null) {
                        return checkExe;
                    }
                }
            }
        }

        return null;
    }

    private static findCliDllOrNull(cliPath: string, dll: string): string | null {
        if (!fs.existsSync(cliPath)) return null;

        const dir = path.dirname(cliPath);
        const check = path.join(dir, dll);
        if (fs.existsSync(check)) return check;

        // Recursively search subdirectories
        try {
            const files = fs.readdirSync(dir, { recursive: true, withFileTypes: true });
            const matches = files
                .filter(f => f.isFile() && f.name === dll)
                .map(f => path.join(f.path, f.name));
            
            if (matches.length === 1) return matches[0];
        } catch {
            // Ignore directory read errors
        }

        return null;
    }

    private static cliFound(cli: string, found: string): string {
        ConsoleHelpers.writeDebugLine(`CliFound: CLI specified (${cli}); found; using ${found}`);
        return found;
    }

    private static cliNotFound(cli: string): string {
        const message = `CliNotFound: CLI specified (${cli}); tried searching PATH and working directory; not found; using ${cli}`;
        ConsoleHelpers.writeWarningLine(message);
        return cli;
    }

    private static pickCliOrNull(clis: string[]): string | null {
        const cliOrNulls: (string | null)[] = [];
        for (const cli of clis) {
            cliOrNulls.push(CheckExpectInstructionsHelper.findCliOrNull(cli));
        }

        const clisFound = cliOrNulls.filter(cli => cli !== null);
        return clisFound.length === 1 ? clisFound[0] : null;
    }

    private static pickCliFound(clis: string[], cli: string): string {
        const message = `PickCliFound: found CLI, using ${cli}`;
        ConsoleHelpers.writeDebugLine(message);
        return cli;
    }

    private static pickCliNotFound(clis: string[], cli: string): string {
        const message = `PickCliNotFound: CLI not found; tried searching PATH and working directory; using ${cli}`;
        ConsoleHelpers.writeDebugLine(message);
        return cli;
    }
}