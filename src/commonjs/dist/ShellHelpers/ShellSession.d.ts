import { ChildProcess } from 'child_process';
export declare enum ShellType {
    Bash = "bash",
    Cmd = "cmd",
    PowerShell = "powershell"
}
export declare abstract class ShellSession {
    private static _sessions;
    protected _shellProcess?: ChildProcess;
    constructor();
    protected abstract getShellType(): ShellType;
    protected ensureProcess(): void;
    executeCommandAsync(command: string): Promise<{
        stdout: string;
        stderr: string;
        exitCode: number;
    }>;
    dispose(): void;
    static disposeAll(): void;
}
//# sourceMappingURL=ShellSession.d.ts.map