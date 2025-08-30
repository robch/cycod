"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RunnableProcessResult = exports.ProcessEvent = exports.TimeoutStrategy = exports.ProcessErrorType = exports.ProcessCompletionState = void 0;
var ProcessCompletionState;
(function (ProcessCompletionState) {
    ProcessCompletionState["Completed"] = "Completed";
    ProcessCompletionState["TimedOut"] = "TimedOut";
    ProcessCompletionState["Canceled"] = "Canceled";
    ProcessCompletionState["Error"] = "Error";
})(ProcessCompletionState || (exports.ProcessCompletionState = ProcessCompletionState = {}));
var ProcessErrorType;
(function (ProcessErrorType) {
    ProcessErrorType["StartFailure"] = "StartFailure";
    ProcessErrorType["Timeout"] = "Timeout";
    ProcessErrorType["Killed"] = "Killed";
    ProcessErrorType["Crashed"] = "Crashed";
    ProcessErrorType["AccessDenied"] = "AccessDenied";
    ProcessErrorType["Other"] = "Other";
})(ProcessErrorType || (exports.ProcessErrorType = ProcessErrorType = {}));
var TimeoutStrategy;
(function (TimeoutStrategy) {
    TimeoutStrategy["ImmediateKill"] = "ImmediateKill";
    TimeoutStrategy["CtrlCOnly"] = "CtrlCOnly";
    TimeoutStrategy["KillOnly"] = "KillOnly";
    TimeoutStrategy["Progressive"] = "Progressive";
})(TimeoutStrategy || (exports.TimeoutStrategy = TimeoutStrategy = {}));
var ProcessEvent;
(function (ProcessEvent) {
    ProcessEvent["Started"] = "Started";
    ProcessEvent["Timeout"] = "Timeout";
    ProcessEvent["SendingCtrlC"] = "SendingCtrlC";
    ProcessEvent["AttemptingKill"] = "AttemptingKill";
    ProcessEvent["Killed"] = "Killed";
    ProcessEvent["Completed"] = "Completed";
    ProcessEvent["Error"] = "Error";
})(ProcessEvent || (exports.ProcessEvent = ProcessEvent = {}));
class RunnableProcessResult {
    constructor(stdout, stderr, merged, exitCode, completionState = ProcessCompletionState.Completed, duration = 0, errorType, friendlyErrorMessage, exception) {
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
    get success() {
        return this.exitCode === 0 && this.completionState === ProcessCompletionState.Completed;
    }
    get isTimeout() {
        return this.completionState === ProcessCompletionState.TimedOut;
    }
    get wasKilled() {
        return this.errorType === ProcessErrorType.Killed;
    }
    get hasError() {
        return this.errorType !== undefined || this.completionState !== ProcessCompletionState.Completed;
    }
    toString() {
        return `ExitCode: ${this.exitCode}, State: ${this.completionState}, Duration: ${this.duration}ms` +
            (this.hasError ? `, Error: ${this.friendlyErrorMessage || this.errorType || "Unknown error"}` : "");
    }
}
exports.RunnableProcessResult = RunnableProcessResult;
//# sourceMappingURL=RunnableProcessResult.js.map