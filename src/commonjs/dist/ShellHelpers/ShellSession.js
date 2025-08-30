"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ShellSession = exports.ShellType = void 0;
const child_process_1 = require("child_process");
var ShellType;
(function (ShellType) {
    ShellType["Bash"] = "bash";
    ShellType["Cmd"] = "cmd";
    ShellType["PowerShell"] = "powershell";
})(ShellType || (exports.ShellType = ShellType = {}));
class ShellSession {
    constructor() {
        ShellSession._sessions.push(this);
    }
    ensureProcess() {
        if (this._shellProcess && !this._shellProcess.killed) {
            return;
        }
        console.log(`Starting ${this.getShellType()} shell...`);
        const shellType = this.getShellType();
        this._shellProcess = (0, child_process_1.spawn)(shellType, [], {
            stdio: ['pipe', 'pipe', 'pipe'],
            shell: false
        });
    }
    async executeCommandAsync(command) {
        this.ensureProcess();
        return new Promise((resolve, reject) => {
            if (!this._shellProcess) {
                reject(new Error('Shell process not initialized'));
                return;
            }
            let stdout = '';
            let stderr = '';
            const onData = (data) => {
                stdout += data.toString();
            };
            const onError = (data) => {
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
    dispose() {
        if (this._shellProcess && !this._shellProcess.killed) {
            this._shellProcess.kill();
        }
        const index = ShellSession._sessions.indexOf(this);
        if (index > -1) {
            ShellSession._sessions.splice(index, 1);
        }
    }
    static disposeAll() {
        for (const session of this._sessions) {
            session.dispose();
        }
        this._sessions = [];
    }
}
exports.ShellSession = ShellSession;
ShellSession._sessions = [];
//# sourceMappingURL=ShellSession.js.map