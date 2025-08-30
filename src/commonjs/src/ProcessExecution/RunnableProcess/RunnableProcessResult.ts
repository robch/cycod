export enum ProcessCompletionState {
    Completed = "Completed",
    TimedOut = "TimedOut",
    Canceled = "Canceled",
    Error = "Error"
}

export enum ProcessErrorType {
    StartFailure = "StartFailure",
    Timeout = "Timeout",
    Killed = "Killed",
    Crashed = "Crashed",
    AccessDenied = "AccessDenied",
    Other = "Other"
}

export enum TimeoutStrategy {
    ImmediateKill = "ImmediateKill",
    CtrlCOnly = "CtrlCOnly",
    KillOnly = "KillOnly",
    Progressive = "Progressive"
}

export enum ProcessEvent {
    Started = "Started",
    Timeout = "Timeout",
    SendingCtrlC = "SendingCtrlC",
    AttemptingKill = "AttemptingKill",
    Killed = "Killed",
    Completed = "Completed",
    Error = "Error"
}

export class RunnableProcessResult {
    readonly standardOutput: string;
    readonly standardError: string;
    readonly mergedOutput: string;
    readonly exitCode: number;
    readonly completionState: ProcessCompletionState;
    readonly duration: number; // in milliseconds
    readonly errorType?: ProcessErrorType;
    readonly friendlyErrorMessage?: string;
    readonly exception?: Error;

    constructor(
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
        this.standardOutput = stdout || '';
        this.standardError = stderr || '';
        this.mergedOutput = merged || '';
        this.exitCode = exitCode;
        this.completionState = completionState;
        this.duration = duration;
        this.errorType = errorType;
        this.friendlyErrorMessage = friendlyErrorMessage;
        this.exception = exception;
    }

    get success(): boolean {
        return this.exitCode === 0 && this.completionState === ProcessCompletionState.Completed;
    }

    get isTimeout(): boolean {
        return this.completionState === ProcessCompletionState.TimedOut;
    }

    get wasKilled(): boolean {
        return this.errorType === ProcessErrorType.Killed;
    }

    get hasError(): boolean {
        return this.errorType !== undefined || this.completionState !== ProcessCompletionState.Completed;
    }

    toString(): string {
        return `ExitCode: ${this.exitCode}, State: ${this.completionState}, Duration: ${this.duration}ms` +
               (this.hasError ? `, Error: ${this.friendlyErrorMessage || this.errorType || "Unknown error"}` : "");
    }
}