"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ProcessHelpers = void 0;
const child_process_1 = require("child_process");
const FileHelpers_1 = require("./FileHelpers");
class ProcessHelpers {
    static async runShellScript(shell, script, scriptArgs, workingDirectory, envVars, input, timeout) {
        const filesToDelete = [];
        try {
            const scriptFileExt = this.getShellScriptFileExtension(shell);
            const scriptWrapped = this.wrapScriptContent(shell, script);
            const scriptFileName = FileHelpers_1.FileHelpers.writeTextToTempFile(scriptWrapped, scriptFileExt);
            filesToDelete.push(scriptFileName);
            const { processName, processArgsFormat } = this.getShellProcessNameAndArgsFormat(shell);
            const shellArgsFormatted = processArgsFormat
                .replace('{0}', scriptFileName)
                .replace('{1}', scriptArgs || '')
                .trim();
            return await this.runProcess(`${processName} ${shellArgsFormatted}`, workingDirectory, envVars, input, timeout);
        }
        finally {
            // Clean up temp files unless in debug mode
            filesToDelete.forEach(file => FileHelpers_1.FileHelpers.deleteFile(file));
        }
    }
    static async runProcess(command, workingDirectory, envVars, input, timeout) {
        return new Promise((resolve, reject) => {
            const env = { ...process.env, ...(envVars || {}) };
            const options = {
                cwd: workingDirectory || process.cwd(),
                env,
                shell: true
            };
            const child = (0, child_process_1.exec)(command, options, (error, stdout, stderr) => {
                if (error && error.code !== null) {
                    resolve({
                        exitCode: error.code || 1,
                        stdout: (stdout?.toString() || ''),
                        stderr: (stderr?.toString() || error.message)
                    });
                }
                else {
                    resolve({
                        exitCode: 0,
                        stdout: (stdout?.toString() || ''),
                        stderr: (stderr?.toString() || '')
                    });
                }
            });
            if (input) {
                child.stdin?.write(input);
                child.stdin?.end();
            }
            if (timeout) {
                setTimeout(() => {
                    child.kill();
                    reject(new Error(`Process timed out after ${timeout}ms`));
                }, timeout);
            }
        });
    }
    static getShellScriptFileExtension(shell) {
        switch (shell.toLowerCase()) {
            case 'powershell':
            case 'pwsh':
                return '.ps1';
            case 'cmd':
            case 'batch':
                return '.bat';
            case 'bash':
            case 'sh':
            default:
                return '.sh';
        }
    }
    static wrapScriptContent(shell, script) {
        switch (shell.toLowerCase()) {
            case 'powershell':
            case 'pwsh':
                return `${script}\nif ($LASTEXITCODE) { exit $LASTEXITCODE }`;
            case 'cmd':
            case 'batch':
                return `@echo off\n${script}`;
            case 'bash':
            case 'sh':
            default:
                return `#!/bin/bash\nset -e\n${script}`;
        }
    }
    static getShellProcessNameAndArgsFormat(shell) {
        switch (shell.toLowerCase()) {
            case 'powershell':
                return {
                    processName: 'powershell',
                    processArgsFormat: '-ExecutionPolicy Bypass -File "{0}" {1}'
                };
            case 'pwsh':
                return {
                    processName: 'pwsh',
                    processArgsFormat: '-ExecutionPolicy Bypass -File "{0}" {1}'
                };
            case 'cmd':
            case 'batch':
                return {
                    processName: 'cmd',
                    processArgsFormat: '/c "{0}" {1}'
                };
            case 'bash':
                return {
                    processName: 'bash',
                    processArgsFormat: '"{0}" {1}'
                };
            case 'sh':
            default:
                return {
                    processName: 'sh',
                    processArgsFormat: '"{0}" {1}'
                };
        }
    }
}
exports.ProcessHelpers = ProcessHelpers;
//# sourceMappingURL=ProcessHelpers.js.map