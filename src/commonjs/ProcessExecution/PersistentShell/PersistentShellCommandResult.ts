import { RunnableProcessResult } from '../RunnableProcess/RunnableProcessResult';
import { ProcessCompletionState, ProcessErrorType } from '../RunnableProcess/RunnableProcessResult';

/**
 * Represents the result of a shell command execution.
 */
export class PersistentShellCommandResult extends RunnableProcessResult {
    /**
     * Gets the original command as provided before wrapping.
     */
    public readonly OriginalCommand: string;
    
    /**
     * Gets the duration of just the command execution.
     */
    public readonly CommandDuration: number;
    
    /**
     * Gets any shell-specific error information, if available.
     */
    public readonly ShellSpecificError?: string;
    
    /**
     * Gets whether the command produced a syntax error in the shell.
     */
    public readonly IsSyntaxError: boolean;
    
    /**
     * Creates a new PersistentShellCommandResult extending the base RunnableProcessResult.
     * @param stdout The standard output.
     * @param stderr The standard error output.
     * @param merged The merged stdout and stderr.
     * @param exitCode The command/shell exit code.
     * @param completionState The state in which the command completed.
     * @param duration The overall duration including shell startup if applicable.
     * @param errorType The type of error that occurred, if any.
     * @param friendlyErrorMessage A user-friendly error message.
     * @param exception The exception that occurred, if any.
     * @param originalCommand The original command as provided.
     * @param commandDuration The duration of just the command execution.
     * @param shellSpecificError Any shell-specific error information.
     * @param isSyntaxError Whether the error was a syntax error.
     */
    public constructor(
        stdout: string,
        stderr: string,
        merged: string,
        exitCode: number,
        completionState: ProcessCompletionState = ProcessCompletionState.Completed,
        duration: number = 0,
        errorType?: ProcessErrorType,
        friendlyErrorMessage?: string,
        exception?: Error,
        originalCommand: string = '',
        commandDuration: number = 0,
        shellSpecificError?: string,
        isSyntaxError: boolean = false
    ) {
        super(stdout, stderr, merged, exitCode, completionState, duration, errorType, friendlyErrorMessage, exception);
        
        this.OriginalCommand = originalCommand;
        this.CommandDuration = commandDuration;
        this.ShellSpecificError = shellSpecificError;
        this.IsSyntaxError = isSyntaxError;
    }
    
    /**
     * Returns a string representation of the shell command result.
     */
    public toString(): string {
        const baseString = super.toString();
        const command = this.OriginalCommand.length > 50 
            ? this.OriginalCommand.substring(0, 47) + '...' 
            : this.OriginalCommand;
        const shellError = this.ShellSpecificError ? `, Shell Error: ${this.ShellSpecificError}` : '';
        return `${baseString}, Command: ${command}${shellError}`;
    }
    
    /**
     * Creates a PersistentShellCommandResult from an existing RunnableProcessResult.
     * @param processResult The base RunnableProcessResult.
     * @param originalCommand The original command as provided.
     * @param commandDuration The duration of just the command execution.
     * @param shellSpecificError Any shell-specific error information.
     * @param isSyntaxError Whether the error was a syntax error.
     * @returns A new PersistentShellCommandResult.
     */
    public static FromProcessResult(
        processResult: RunnableProcessResult,
        originalCommand: string = '',
        commandDuration: number = 0,
        shellSpecificError?: string,
        isSyntaxError: boolean = false
    ): PersistentShellCommandResult {
        if (!processResult) {
            throw new Error('processResult cannot be null');
        }
            
        const result = new PersistentShellCommandResult(
            processResult.StandardOutput,
            processResult.StandardError,
            processResult.MergedOutput,
            processResult.ExitCode,
            processResult.CompletionState,
            processResult.Duration,
            processResult.ErrorType,
            processResult.FriendlyErrorMessage,
            processResult.Exception,
            originalCommand,
            commandDuration,
            shellSpecificError,
            isSyntaxError
        );

        return result;
    }
}