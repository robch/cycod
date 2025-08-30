import { spawn, exec, ChildProcess } from 'child_process';
import * as path from 'path';
import { FileHelpers } from './FileHelpers';

export interface ProcessResult {
    exitCode: number;
    stdout: string;
    stderr: string;
}

export class ProcessHelpers {
    static async runShellScript(
        shell: string,
        script: string,
        scriptArgs?: string,
        workingDirectory?: string,
        envVars?: Record<string, string>,
        input?: string,
        timeout?: number
    ): Promise<ProcessResult> {
        const filesToDelete: string[] = [];

        try {
            const scriptFileExt = this.getShellScriptFileExtension(shell);
            const scriptWrapped = this.wrapScriptContent(shell, script);
            const scriptFileName = FileHelpers.writeTextToTempFile(scriptWrapped, scriptFileExt);
            filesToDelete.push(scriptFileName);

            const { processName, processArgsFormat } = this.getShellProcessNameAndArgsFormat(shell);
            const shellArgsFormatted = processArgsFormat
                .replace('{0}', scriptFileName)
                .replace('{1}', scriptArgs || '')
                .trim();

            return await this.runProcess(
                `${processName} ${shellArgsFormatted}`,
                workingDirectory,
                envVars,
                input,
                timeout
            );
        } finally {
            // Clean up temp files unless in debug mode
            filesToDelete.forEach(file => FileHelpers.deleteFile(file));
        }
    }

    static async runProcess(
        command: string,
        workingDirectory?: string,
        envVars?: Record<string, string>,
        input?: string,
        timeout?: number
    ): Promise<ProcessResult> {
        return new Promise((resolve, reject) => {
            const env = { ...process.env, ...(envVars || {}) };
            const options: any = {
                cwd: workingDirectory || process.cwd(),
                env,
                shell: true
            };

            const child = exec(command, options, (error, stdout, stderr) => {
                if (error && error.code !== null) {
                    resolve({
                        exitCode: error.code || 1,
                        stdout: (stdout?.toString() || ''),
                        stderr: (stderr?.toString() || error.message)
                    });
                } else {
                    resolve({
                        exitCode: 0,
                        stdout: (stdout?.toString() || ''),
                        stderr: (stderr?.toString() || '')
                    });
                }
            });

            if (input) {
                child.stdin?.write(input);
                child.stdin?.end();
            }

            if (timeout) {
                setTimeout(() => {
                    child.kill();
                    reject(new Error(`Process timed out after ${timeout}ms`));
                }, timeout);
            }
        });
    }

    private static getShellScriptFileExtension(shell: string): string {
        switch (shell.toLowerCase()) {
            case 'powershell':
            case 'pwsh':
                return '.ps1';
            case 'cmd':
            case 'batch':
                return '.bat';
            case 'bash':
            case 'sh':
            default:
                return '.sh';
        }
    }

    private static wrapScriptContent(shell: string, script: string): string {
        switch (shell.toLowerCase()) {
            case 'powershell':
            case 'pwsh':
                return `${script}\nif ($LASTEXITCODE) { exit $LASTEXITCODE }`;
            case 'cmd':
            case 'batch':
                return `@echo off\n${script}`;
            case 'bash':
            case 'sh':
            default:
                return `#!/bin/bash\nset -e\n${script}`;
        }
    }

    private static getShellProcessNameAndArgsFormat(shell: string): { processName: string; processArgsFormat: string } {
        switch (shell.toLowerCase()) {
            case 'powershell':
                return {
                    processName: 'powershell',
                    processArgsFormat: '-ExecutionPolicy Bypass -File "{0}" {1}'
                };
            case 'pwsh':
                return {
                    processName: 'pwsh',
                    processArgsFormat: '-ExecutionPolicy Bypass -File "{0}" {1}'
                };
            case 'cmd':
            case 'batch':
                return {
                    processName: 'cmd',
                    processArgsFormat: '/c "{0}" {1}'
                };
            case 'bash':
                return {
                    processName: 'bash',
                    processArgsFormat: '"{0}" {1}'
                };
            case 'sh':
            default:
                return {
                    processName: 'sh',
                    processArgsFormat: '"{0}" {1}'
                };
        }
    }
}