import { BackgroundProcessInfo } from './BackgroundProcessInfo';
import { RunnableProcessBuilder } from '../RunnableProcess/RunnableProcessBuilder';
import { ConsoleHelpers } from '../../Helpers/ConsoleHelpers';

/**
 * Manages long-running background processes.
 */
export class BackgroundProcessManager {
    private static readonly processes = new Map<string, BackgroundProcessInfo>();
    private static readonly defaultCleanupInterval = 60 * 60 * 1000; // 1 hour in ms
    private static readonly defaultMaxProcessAge = 8 * 60 * 60 * 1000; // 8 hours in ms
    private static cleanupTimer: NodeJS.Timeout | null = null;

    /**
     * Static initialization to set up the cleanup timer.
     */
    private static initialize(): void {
        if (this.cleanupTimer === null) {
            // Create a timer to periodically clean up old processes
            this.cleanupTimer = setInterval(() => {
                this.CleanupOldProcesses();
            }, this.defaultCleanupInterval);

            // Register for application exit to clean up all processes
            process.on('exit', () => this.ShutdownAllProcesses());
            process.on('SIGINT', () => {
                this.ShutdownAllProcesses();
                process.exit(0);
            });
            process.on('SIGTERM', () => {
                this.ShutdownAllProcesses();
                process.exit(0);
            });
        }
    }

    /**
     * Starts a new background process.
     * @param processName The name of the executable to run.
     * @param processArguments Optional arguments to pass to the process.
     * @param workingDirectory Optional working directory.
     * @returns A handle to reference the process.
     */
    public static StartLongRunningProcess(
        processName: string, 
        processArguments?: string, 
        workingDirectory?: string
    ): string {
        this.initialize();

        if (!processName) {
            throw new Error('Process name cannot be null or empty');
        }

        // Create process using RunnableProcessBuilder
        const processBuilder = new RunnableProcessBuilder()
            .WithFileName(processName)
            .WithTimeout(Number.MAX_SAFE_INTEGER); // Very long timeout since this is a background process
        
        if (processArguments) {
            processBuilder.WithArguments(processArguments);
        }
        
        if (workingDirectory) {
            processBuilder.WithWorkingDirectory(workingDirectory);
        }

        const process = processBuilder.Build();

        // Generate a unique handle for this process
        const handle = this.generateUniqueHandle();

        // Create the process info and store it
        const processInfo = new BackgroundProcessInfo(handle, process);
        this.processes.set(handle, processInfo);

        // Start the process asynchronously
        (async () => {
            try {
                // Start the process but don't await it - let it run in the background
                await process.StartAsync();
            } catch (error) {
                const errorMessage = error instanceof Error ? error.message : String(error);
                ConsoleHelpers.WriteDebugLine(`Error in background process ${handle}: ${errorMessage}`);
            }
        })();

        // Return the handle to the caller
        return handle;
    }

    /**
     * Checks if a process with the specified handle is running.
     * @param handle The process handle.
     * @returns True if the process is running, false otherwise.
     */
    public static IsLongRunningProcessRunning(handle: string): boolean {
        if (!handle || !this.processes.has(handle)) {
            return false;
        }

        const processInfo = this.processes.get(handle)!;
        return processInfo.IsRunning;
    }

    /**
     * Gets the output from a background process.
     * @param handle The process handle.
     * @param clearBuffer Whether to clear the output buffer after retrieving.
     * @returns An object containing stdout, stderr, and merged output.
     */
    public static GetLongRunningProcessOutput(
        handle: string, 
        clearBuffer: boolean = false
    ): { stdout: string; stderr: string; merged: string } {
        if (!handle || !this.processes.has(handle)) {
            return {
                stdout: '',
                stderr: `Process with handle ${handle} not found.`,
                merged: `Process with handle ${handle} not found.`
            };
        }

        const processInfo = this.processes.get(handle)!;
        return processInfo.GetOutput(clearBuffer);
    }

    /**
     * Terminates a background process.
     * @param handle The process handle.
     * @param force Whether to force kill the process.
     * @returns True if the process was terminated, false otherwise.
     */
    public static KillLongRunningProcess(handle: string, force: boolean = false): boolean {
        if (!handle || !this.processes.has(handle)) {
            return false;
        }

        const processInfo = this.processes.get(handle)!;

        try {
            if (processInfo.IsRunning) {
                // Send Ctrl+C if not forcing
                if (!force) {
                    processInfo.Process.SendCtrlCAsync().then(() => {
                        // Wait a bit for graceful shutdown
                        setTimeout(() => {
                            if (processInfo.IsRunning && force) {
                                processInfo.Process.ForceShutdown();
                            }
                        }, 500);
                    });
                } else {
                    // Force kill immediately
                    processInfo.Process.ForceShutdown();
                }
            }

            // Remove from our collection
            this.processes.delete(handle);
            return true;
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : String(error);
            ConsoleHelpers.WriteDebugLine(`Error killing process ${handle}: ${errorMessage}`);
            return false;
        }
    }

    /**
     * Gets a list of all running background processes.
     * @returns A list of process information.
     */
    public static GetAllProcesses(): BackgroundProcessInfo[] {
        return Array.from(this.processes.values());
    }

    /**
     * Terminates all background processes.
     */
    public static ShutdownAllProcesses(): void {
        const handles = Array.from(this.processes.keys());
        for (const handle of handles) {
            this.KillLongRunningProcess(handle, true);
        }

        this.processes.clear();

        if (this.cleanupTimer) {
            clearInterval(this.cleanupTimer);
            this.cleanupTimer = null;
        }
    }

    /**
     * Cleans up old processes that have been running for too long.
     */
    private static CleanupOldProcesses(): void {
        const now = new Date();
        const oldProcesses = Array.from(this.processes.values())
            .filter(p => (now.getTime() - p.StartTime.getTime()) > this.defaultMaxProcessAge);

        for (const process of oldProcesses) {
            ConsoleHelpers.WriteDebugLine(`Cleaning up old process: ${process.Handle} running since ${process.StartTime}`);
            this.KillLongRunningProcess(process.Handle, true);
        }
    }

    /**
     * Generates a unique handle for a process.
     */
    private static generateUniqueHandle(): string {
        return Math.random().toString(36).substring(2) + Date.now().toString(36);
    }
}