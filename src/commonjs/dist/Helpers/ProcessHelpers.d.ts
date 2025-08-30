export interface ProcessResult {
    exitCode: number;
    stdout: string;
    stderr: string;
}
export declare class ProcessHelpers {
    static runShellScript(shell: string, script: string, scriptArgs?: string, workingDirectory?: string, envVars?: Record<string, string>, input?: string, timeout?: number): Promise<ProcessResult>;
    static runProcess(command: string, workingDirectory?: string, envVars?: Record<string, string>, input?: string, timeout?: number): Promise<ProcessResult>;
    private static getShellScriptFileExtension;
    private static wrapScriptContent;
    private static getShellProcessNameAndArgsFormat;
}
//# sourceMappingURL=ProcessHelpers.d.ts.map