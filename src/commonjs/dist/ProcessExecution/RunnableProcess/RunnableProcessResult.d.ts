export declare enum ProcessCompletionState {
    Completed = "Completed",
    TimedOut = "TimedOut",
    Canceled = "Canceled",
    Error = "Error"
}
export declare enum ProcessErrorType {
    StartFailure = "StartFailure",
    Timeout = "Timeout",
    Killed = "Killed",
    Crashed = "Crashed",
    AccessDenied = "AccessDenied",
    Other = "Other"
}
export declare enum TimeoutStrategy {
    ImmediateKill = "ImmediateKill",
    CtrlCOnly = "CtrlCOnly",
    KillOnly = "KillOnly",
    Progressive = "Progressive"
}
export declare enum ProcessEvent {
    Started = "Started",
    Timeout = "Timeout",
    SendingCtrlC = "SendingCtrlC",
    AttemptingKill = "AttemptingKill",
    Killed = "Killed",
    Completed = "Completed",
    Error = "Error"
}
export declare class RunnableProcessResult {
    readonly standardOutput: string;
    readonly standardError: string;
    readonly mergedOutput: string;
    readonly exitCode: number;
    readonly completionState: ProcessCompletionState;
    readonly duration: number;
    readonly errorType?: ProcessErrorType;
    readonly friendlyErrorMessage?: string;
    readonly exception?: Error;
    constructor(stdout: string, stderr: string, merged: string, exitCode: number, completionState?: ProcessCompletionState, duration?: number, errorType?: ProcessErrorType, friendlyErrorMessage?: string, exception?: Error);
    get success(): boolean;
    get isTimeout(): boolean;
    get wasKilled(): boolean;
    get hasError(): boolean;
    toString(): string;
}
//# sourceMappingURL=RunnableProcessResult.d.ts.map