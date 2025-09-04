/**
 * Represents the result of a process execution.
 */
export class RunnableProcessResult {
    /**
     * Gets the standard output from the process.
     */
    public readonly StandardOutput: string;
    
    /**
     * Gets the standard error output from the process.
     */
    public readonly StandardError: string;
    
    /**
     * Gets the combined output from both stdout and stderr.
     */
    public readonly MergedOutput: string;
    
    /**
     * Gets the process exit code.
     */
    public readonly ExitCode: number;
    
    /**
     * Gets the state in which the process execution completed.
     */
    public readonly CompletionState: ProcessCompletionState;
    
    /**
     * Gets the duration of the process execution in milliseconds.
     */
    public readonly Duration: number;
    
    /**
     * Gets the type of error that occurred, if any.
     */
    public readonly ErrorType?: ProcessErrorType;
    
    /**
     * Gets a user-friendly error message if an error occurred.
     */
    public readonly FriendlyErrorMessage?: string;
    
    /**
     * Gets the exception that occurred during execution, if any.
     */
    public readonly Exception?: Error;

    /**
     * Creates a new RunnableProcessResult.
     * @param stdout The standard output.
     * @param stderr The standard error output.
     * @param merged The merged stdout and stderr.
     * @param exitCode The process exit code.
     * @param completionState The state in which the process completed.
     * @param duration The duration of the process execution in milliseconds.
     * @param errorType The type of error that occurred, if any.
     * @param friendlyErrorMessage A user-friendly error message.
     * @param exception The exception that occurred, if any.
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
        exception?: Error
    ) {
        this.StandardOutput = stdout || '';
        this.StandardError = stderr || '';
        this.MergedOutput = merged || '';
        this.ExitCode = exitCode;
        this.CompletionState = completionState;
        this.Duration = duration;
        this.ErrorType = errorType;
        this.FriendlyErrorMessage = friendlyErrorMessage;
        this.Exception = exception;
    }

    /**
     * Gets whether the process completed successfully (exit code 0 and completed normally).
     */
    public get Success(): boolean {
        return this.ExitCode === 0 && this.CompletionState === ProcessCompletionState.Completed;
    }
    
    /**
     * Gets whether the process timed out.
     */
    public get IsTimeout(): boolean {
        return this.CompletionState === ProcessCompletionState.TimedOut;
    }
    
    /**
     * Gets whether the process was killed.
     */
    public get WasKilled(): boolean {
        return this.ErrorType === ProcessErrorType.Killed;
    }
    
    /**
     * Gets whether any error occurred during the process execution.
     */
    public get HasError(): boolean {
        return this.ErrorType !== undefined || this.CompletionState !== ProcessCompletionState.Completed;
    }
    
    /**
     * Returns a string representation of the process result.
     */
    public toString(): string {
        const errorInfo = this.HasError 
            ? `, Error: ${this.FriendlyErrorMessage || this.ErrorType?.toString() || 'Unknown error'}`
            : '';
        return `ExitCode: ${this.ExitCode}, State: ${this.CompletionState}, Duration: ${this.Duration}ms${errorInfo}`;
    }
}

/**
 * Represents the state in which a process execution completed.
 */
export enum ProcessCompletionState {
    /**
     * The process completed normally.
     */
    Completed = 'completed',
    
    /**
     * The process timed out and was terminated.
     */
    TimedOut = 'timedOut',
    
    /**
     * The process was canceled by the user.
     */
    Canceled = 'canceled',
    
    /**
     * An error occurred during execution.
     */
    Error = 'error'
}

/**
 * Represents different types of errors that can occur during process execution.
 */
export enum ProcessErrorType {
    /**
     * The process failed to start.
     */
    StartFailure = 'startFailure',
    
    /**
     * The process timed out.
     */
    Timeout = 'timeout',
    
    /**
     * The process was forcefully killed.
     */
    Killed = 'killed',
    
    /**
     * The process crashed.
     */
    Crashed = 'crashed',
    
    /**
     * Access was denied.
     */
    AccessDenied = 'accessDenied',
    
    /**
     * Another type of error occurred.
     */
    Other = 'other'
}

/**
 * Represents different strategies for handling process timeouts.
 */
export enum TimeoutStrategy {
    /**
     * Immediately kill the process when it times out.
     */
    ImmediateKill = 'immediateKill',
    
    /**
     * Send Ctrl+C to the process when it times out, but don't kill it.
     */
    CtrlCOnly = 'ctrlCOnly',
    
    /**
     * Kill the process without trying Ctrl+C first.
     */
    KillOnly = 'killOnly',
    
    /**
     * Try Ctrl+C first, then kill if the process doesn't exit.
     */
    Progressive = 'progressive'
}

/**
 * Represents process lifecycle events.
 */
export enum ProcessEvent {
    /**
     * Process has started.
     */
    Started = 'started',
    
    /**
     * Process has timed out.
     */
    Timeout = 'timeout',
    
    /**
     * Sending Ctrl+C to the process.
     */
    SendingCtrlC = 'sendingCtrlC',
    
    /**
     * Attempting to kill the process.
     */
    AttemptingKill = 'attemptingKill',
    
    /**
     * Process has been killed.
     */
    Killed = 'killed',
    
    /**
     * Process has completed.
     */
    Completed = 'completed',
    
    /**
     * An error occurred during process execution.
     */
    Error = 'error'
}