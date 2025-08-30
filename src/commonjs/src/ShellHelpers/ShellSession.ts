import { spawn, ChildProcess } from 'child_process';

export enum ShellType {
    Bash = "bash",
    Cmd = "cmd", 
    PowerShell = "powershell"
}

export abstract class ShellSession {
    private static _sessions: ShellSession[] = [];
    protected _shellProcess?: ChildProcess;

    constructor() {
        ShellSession._sessions.push(this);
    }

    protected abstract getShellType(): ShellType;

    protected ensureProcess(): void {
        if (this._shellProcess && !this._shellProcess.killed) {
            return;
        }

        console.log(`Starting ${this.getShellType()} shell...`);
        
        const shellType = this.getShellType();
        this._shellProcess = spawn(shellType, [], {
            stdio: ['pipe', 'pipe', 'pipe'],
            shell: false
        });
    }

    async executeCommandAsync(command: string): Promise<{ stdout: string; stderr: string; exitCode: number }> {
        this.ensureProcess();

        return new Promise((resolve, reject) => {
            if (!this._shellProcess) {
                reject(new Error('Shell process not initialized'));
                return;
            }

            let stdout = '';
            let stderr = '';

            const onData = (data: Buffer) => {
                stdout += data.toString();
            };

            const onError = (data: Buffer) => {
                stderr += data.toString();
            };

            this._shellProcess.stdout?.on('data', onData);
            this._shellProcess.stderr?.on('data', onError);

            this._shellProcess.stdin?.write(command + '\n');

            // Simple implementation - in practice would be more sophisticated
            setTimeout(() => {
                this._shellProcess?.stdout?.off('data', onData);
                this._shellProcess?.stderr?.off('data', onError);
                resolve({ stdout, stderr, exitCode: 0 });
            }, 1000);
        });
    }

    dispose(): void {
        if (this._shellProcess && !this._shellProcess.killed) {
            this._shellProcess.kill();
        }

        const index = ShellSession._sessions.indexOf(this);
        if (index > -1) {
            ShellSession._sessions.splice(index, 1);
        }
    }

    static disposeAll(): void {
        for (const session of this._sessions) {
            session.dispose();
        }
        this._sessions = [];
    }
}