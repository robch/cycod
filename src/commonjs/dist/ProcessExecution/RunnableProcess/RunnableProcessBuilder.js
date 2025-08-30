"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.RunnableProcessBuilder = void 0;
const child_process_1 = require("child_process");
const path = __importStar(require("path"));
const RunnableProcessResult_1 = require("./RunnableProcessResult");
class RunnableProcessBuilder {
    constructor() {
        this._timeoutStrategy = RunnableProcessResult_1.TimeoutStrategy.Progressive;
        this._encoding = 'utf8';
    }
    withFileName(fileName) {
        this._fileName = this.resolveExecutablePath(fileName);
        return this;
    }
    withArguments(args) {
        if (Array.isArray(args)) {
            this._arguments = args.join(' ');
        }
        else {
            this._arguments = args;
        }
        return this;
    }
    withWorkingDirectory(directory) {
        this._workingDirectory = directory;
        return this;
    }
    withEnvironmentVariables(envVars) {
        this._environmentVariables = envVars;
        return this;
    }
    withStandardInput(input) {
        this._standardInput = input;
        return this;
    }
    withTimeout(timeoutMs) {
        this._timeout = timeoutMs;
        return this;
    }
    withTimeoutStrategy(strategy) {
        this._timeoutStrategy = strategy;
        return this;
    }
    withEncoding(encoding) {
        this._encoding = encoding;
        return this;
    }
    async runAsync() {
        if (!this._fileName) {
            throw new Error('FileName must be specified');
        }
        const startTime = Date.now();
        let stdout = '';
        let stderr = '';
        let merged = '';
        return new Promise((resolve) => {
            const env = { ...process.env, ...(this._environmentVariables || {}) };
            const child = (0, child_process_1.spawn)(this._fileName, this._arguments?.split(' ') || [], {
                cwd: this._workingDirectory || process.cwd(),
                env,
                stdio: ['pipe', 'pipe', 'pipe']
            });
            let timeoutHandle;
            // Set up timeout if specified
            if (this._timeout) {
                timeoutHandle = setTimeout(() => {
                    this.handleTimeout(child, resolve, startTime, stdout, stderr, merged);
                }, this._timeout);
            }
            // Handle stdout
            child.stdout?.on('data', (data) => {
                const text = data.toString('utf8');
                stdout += text;
                merged += text;
            });
            // Handle stderr  
            child.stderr?.on('data', (data) => {
                const text = data.toString('utf8');
                stderr += text;
                merged += text;
            });
            // Handle process completion
            child.on('close', (code) => {
                if (timeoutHandle) {
                    clearTimeout(timeoutHandle);
                }
                const duration = Date.now() - startTime;
                const exitCode = code ?? -1;
                resolve(new RunnableProcessResult_1.RunnableProcessResult(stdout, stderr, merged, exitCode, RunnableProcessResult_1.ProcessCompletionState.Completed, duration));
            });
            // Handle errors
            child.on('error', (error) => {
                if (timeoutHandle) {
                    clearTimeout(timeoutHandle);
                }
                const duration = Date.now() - startTime;
                resolve(new RunnableProcessResult_1.RunnableProcessResult(stdout, stderr, merged, -1, RunnableProcessResult_1.ProcessCompletionState.Error, duration, RunnableProcessResult_1.ProcessErrorType.StartFailure, error.message, error));
            });
            // Send input if provided
            if (this._standardInput) {
                child.stdin?.write(this._standardInput);
                child.stdin?.end();
            }
        });
    }
    handleTimeout(child, resolve, startTime, stdout, stderr, merged) {
        const duration = Date.now() - startTime;
        // Kill the process based on strategy
        switch (this._timeoutStrategy) {
            case RunnableProcessResult_1.TimeoutStrategy.ImmediateKill:
            case RunnableProcessResult_1.TimeoutStrategy.KillOnly:
                child.kill('SIGKILL');
                break;
            case RunnableProcessResult_1.TimeoutStrategy.CtrlCOnly:
                child.kill('SIGINT');
                break;
            case RunnableProcessResult_1.TimeoutStrategy.Progressive:
                child.kill('SIGTERM');
                // Could add logic to escalate to SIGKILL after a delay
                break;
        }
        resolve(new RunnableProcessResult_1.RunnableProcessResult(stdout, stderr, merged, -1, RunnableProcessResult_1.ProcessCompletionState.TimedOut, duration, RunnableProcessResult_1.ProcessErrorType.Timeout, `Process timed out after ${this._timeout}ms`));
    }
    resolveExecutablePath(fileName) {
        // Simple resolution - in practice this would be more sophisticated
        if (path.isAbsolute(fileName)) {
            return fileName;
        }
        // For relative paths or just executable names, return as-is
        // The system will resolve using PATH
        return fileName;
    }
}
exports.RunnableProcessBuilder = RunnableProcessBuilder;
//# sourceMappingURL=RunnableProcessBuilder.js.map