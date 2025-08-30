import { spawn, ChildProcess } from 'child_process';
import * as path from 'path';
import { RunnableProcessResult, ProcessCompletionState, ProcessErrorType, TimeoutStrategy } from './RunnableProcessResult';

export class RunnableProcessBuilder {
    private _fileName?: string;
    private _arguments?: string;
    private _workingDirectory?: string;
    private _environmentVariables?: Record<string, string>;
    private _standardInput?: string;
    private _timeout?: number;
    private _timeoutStrategy: TimeoutStrategy = TimeoutStrategy.Progressive;
    private _encoding: BufferEncoding = 'utf8';

    withFileName(fileName: string): RunnableProcessBuilder {
        this._fileName = this.resolveExecutablePath(fileName);
        return this;
    }

    withArguments(args: string | string[]): RunnableProcessBuilder {
        if (Array.isArray(args)) {
            this._arguments = args.join(' ');
        } else {
            this._arguments = args;
        }
        return this;
    }

    withWorkingDirectory(directory: string): RunnableProcessBuilder {
        this._workingDirectory = directory;
        return this;
    }

    withEnvironmentVariables(envVars: Record<string, string>): RunnableProcessBuilder {
        this._environmentVariables = envVars;
        return this;
    }

    withStandardInput(input: string): RunnableProcessBuilder {
        this._standardInput = input;
        return this;
    }

    withTimeout(timeoutMs: number): RunnableProcessBuilder {
        this._timeout = timeoutMs;
        return this;
    }

    withTimeoutStrategy(strategy: TimeoutStrategy): RunnableProcessBuilder {
        this._timeoutStrategy = strategy;
        return this;
    }

    withEncoding(encoding: BufferEncoding): RunnableProcessBuilder {
        this._encoding = encoding;
        return this;
    }

    async runAsync(): Promise<RunnableProcessResult> {
        if (!this._fileName) {
            throw new Error('FileName must be specified');
        }

        const startTime = Date.now();
        let stdout = '';
        let stderr = '';
        let merged = '';

        return new Promise<RunnableProcessResult>((resolve) => {
            const env = { ...process.env, ...(this._environmentVariables || {}) };
            
            const child = spawn(this._fileName!, this._arguments?.split(' ') || [], {
                cwd: this._workingDirectory || process.cwd(),
                env,
                stdio: ['pipe', 'pipe', 'pipe']
            });

            let timeoutHandle: NodeJS.Timeout | undefined;

            // Set up timeout if specified
            if (this._timeout) {
                timeoutHandle = setTimeout(() => {
                    this.handleTimeout(child, resolve, startTime, stdout, stderr, merged);
                }, this._timeout);
            }

            // Handle stdout
            child.stdout?.on('data', (data: Buffer) => {
                const text = data.toString('utf8');
                stdout += text;
                merged += text;
            });

            // Handle stderr  
            child.stderr?.on('data', (data: Buffer) => {
                const text = data.toString('utf8');
                stderr += text;
                merged += text;
            });

            // Handle process completion
            child.on('close', (code: number | null) => {
                if (timeoutHandle) {
                    clearTimeout(timeoutHandle);
                }

                const duration = Date.now() - startTime;
                const exitCode = code ?? -1;

                resolve(new RunnableProcessResult(
                    stdout,
                    stderr,
                    merged,
                    exitCode,
                    ProcessCompletionState.Completed,
                    duration
                ));
            });

            // Handle errors
            child.on('error', (error: Error) => {
                if (timeoutHandle) {
                    clearTimeout(timeoutHandle);
                }

                const duration = Date.now() - startTime;
                
                resolve(new RunnableProcessResult(
                    stdout,
                    stderr,
                    merged,
                    -1,
                    ProcessCompletionState.Error,
                    duration,
                    ProcessErrorType.StartFailure,
                    error.message,
                    error
                ));
            });

            // Send input if provided
            if (this._standardInput) {
                child.stdin?.write(this._standardInput);
                child.stdin?.end();
            }
        });
    }

    private handleTimeout(
        child: ChildProcess,
        resolve: (result: RunnableProcessResult) => void,
        startTime: number,
        stdout: string,
        stderr: string,
        merged: string
    ): void {
        const duration = Date.now() - startTime;

        // Kill the process based on strategy
        switch (this._timeoutStrategy) {
            case TimeoutStrategy.ImmediateKill:
            case TimeoutStrategy.KillOnly:
                child.kill('SIGKILL');
                break;
            case TimeoutStrategy.CtrlCOnly:
                child.kill('SIGINT');
                break;
            case TimeoutStrategy.Progressive:
                child.kill('SIGTERM');
                // Could add logic to escalate to SIGKILL after a delay
                break;
        }

        resolve(new RunnableProcessResult(
            stdout,
            stderr,
            merged,
            -1,
            ProcessCompletionState.TimedOut,
            duration,
            ProcessErrorType.Timeout,
            `Process timed out after ${this._timeout}ms`
        ));
    }

    private resolveExecutablePath(fileName: string): string {
        // Simple resolution - in practice this would be more sophisticated
        if (path.isAbsolute(fileName)) {
            return fileName;
        }

        // For relative paths or just executable names, return as-is
        // The system will resolve using PATH
        return fileName;
    }
}