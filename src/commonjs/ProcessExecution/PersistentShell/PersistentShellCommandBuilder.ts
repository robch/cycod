import { PersistentShellProcess } from './PersistentShellProcess';
import { PersistentShellCommandResult } from './PersistentShellCommandResult';
import { PersistentShellType } from './PersistentShellType';
import { ProcessHelpers } from '../../Helpers/ProcessHelpers';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Builder for configuring and executing commands in an existing shell process.
 */
export class PersistentShellCommandBuilder {
    private readonly shellProcess: PersistentShellProcess;
    private command?: string;
    private workingDirectory?: string;
    private timeoutMs?: number;
    private abortController?: AbortController;
    
    /**
     * Creates a new PersistentShellCommandBuilder using the specified shell process.
     * @param shellProcess The shell process to use.
     */
    public constructor(shellProcess: PersistentShellProcess) {
        if (!shellProcess) {
            throw new Error('shellProcess cannot be null');
        }
        this.shellProcess = shellProcess;
    }
    
    /**
     * Sets the command to execute.
     * @param command The command string.
     * @returns This builder instance for method chaining.
     */
    public WithCommand(command: string): PersistentShellCommandBuilder {
        this.command = command;
        return this;
    }
    
    /**
     * Sets the working directory for the command.
     * @param workingDirectory The working directory.
     * @returns This builder instance for method chaining.
     */
    public WithWorkingDirectory(workingDirectory?: string): PersistentShellCommandBuilder {
        if (workingDirectory && workingDirectory.trim() !== '') {
            this.workingDirectory = workingDirectory;
        }
        return this;
    }
    
    /**
     * Sets a timeout for the command.
     * @param timeoutMs Timeout in milliseconds.
     * @returns This builder instance for method chaining.
     */
    public WithTimeout(timeoutMs?: number): PersistentShellCommandBuilder {
        if (timeoutMs !== undefined) {
            this.timeoutMs = timeoutMs;
        }
        return this;
    }
    
    /**
     * Sets an abort controller for the command.
     * @param abortController The abort controller.
     * @returns This builder instance for method chaining.
     */
    public WithAbortController(abortController: AbortController): PersistentShellCommandBuilder {
        this.abortController = abortController;
        return this;
    }
    
    /**
     * Runs the command and returns the result.
     * @returns The result of the command execution.
     */
    public Run(): Promise<PersistentShellCommandResult> {
        return this.RunAsync();
    }
    
    /**
     * Runs the command asynchronously and returns the result.
     * @returns A promise that completes with the command result.
     */
    public async RunAsync(): Promise<PersistentShellCommandResult> {
        if (!this.command) {
            throw new Error('Command must be specified');
        }
        
        if (this.shellProcess.HasExited) {
            throw new Error('Shell process has exited');
        }
        
        // Change working directory if specified
        let result: PersistentShellCommandResult;
        
        if (this.workingDirectory) {
            // Prepend a cd command to change directory
            const cdCommand = this.getChangeDirectoryCommand(this.shellProcess.PersistentShellType, this.workingDirectory);
            await this.shellProcess.RunCommandAsync(cdCommand, this.abortController?.signal);
        }
        
        try {
            if (this.timeoutMs !== undefined) {
                ConsoleHelpers.WriteDebugLine(`Running command with timeout: ${this.timeoutMs}ms`);
                result = await this.shellProcess.RunCommandAsync(this.command, this.timeoutMs);
            } else {
                ConsoleHelpers.WriteDebugLine('Running command with abort signal');
                result = await this.shellProcess.RunCommandAsync(this.command, this.abortController?.signal);
            }
            
            return result;
        } finally {
            // If we changed directory, we could restore it here if needed
        }
    }
    
    /**
     * Gets the appropriate change directory command for the specified shell.
     * @param shellType The shell type.
     * @param directory The directory to change to.
     * @returns A shell-specific command to change directory.
     */
    private getChangeDirectoryCommand(shellType: PersistentShellType, directory: string): string {
        switch (shellType) {
            case PersistentShellType.Bash:
                return `cd ${ProcessHelpers.EscapeBashArgument(directory)}`;
                
            case PersistentShellType.Cmd:
                return `cd /d ${ProcessHelpers.EscapeCmdArgument(directory)}`;
                
            case PersistentShellType.PowerShell:
                return `Set-Location ${ProcessHelpers.EscapePowerShellArgument(directory)}`;
                
            default:
                return `cd ${directory}`;
        }
    }
}