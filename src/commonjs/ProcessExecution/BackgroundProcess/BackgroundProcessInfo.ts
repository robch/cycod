import { RunnableProcess } from '../RunnableProcess/RunnableProcess';

/**
 * Represents information about a background process.
 */
export class BackgroundProcessInfo {
    /**
     * Gets the unique handle/identifier for this process.
     */
    public readonly Handle: string;

    /**
     * Gets the time when the process was started.
     */
    public readonly StartTime: Date;

    /**
     * Gets the underlying process.
     */
    public readonly Process: RunnableProcess;

    /**
     * Gets the accumulated standard output.
     */
    private stdoutBuffer: string[] = [];

    /**
     * Gets the accumulated standard error.
     */
    private stderrBuffer: string[] = [];

    /**
     * Gets the accumulated merged output.
     */
    private mergedBuffer: string[] = [];

    /**
     * Lock object for thread synchronization (using a simple flag in Node.js).
     */
    private isLocked = false;

    /**
     * Creates a new instance of the BackgroundProcessInfo class.
     * @param handle The unique handle for this process.
     * @param process The underlying process.
     */
    public constructor(handle: string, process: RunnableProcess) {
        if (!handle) {
            throw new Error('handle cannot be null or empty');
        }
        if (!process) {
            throw new Error('process cannot be null');
        }

        this.Handle = handle;
        this.Process = process;
        this.StartTime = new Date();

        // Set up callbacks to capture output in real-time
        process.SetStdoutCallback((line: string) => {
            this.withLock(() => {
                this.stdoutBuffer.push(line);
                this.mergedBuffer.push(line);
            });
        });
        
        process.SetStderrCallback((line: string) => {
            this.withLock(() => {
                this.stderrBuffer.push(line);
                this.mergedBuffer.push(line);
            });
        });
    }

    /**
     * Gets a value indicating whether the process is still running.
     */
    public get IsRunning(): boolean {
        return this.Process && !this.Process.HasExited;
    }

    /**
     * Gets the accumulated output from the process and optionally clears the buffers.
     * @param clearBuffers Whether to clear the output buffers after retrieving.
     * @returns An object containing stdout, stderr, and merged output.
     */
    public GetOutput(clearBuffers: boolean = false): { stdout: string; stderr: string; merged: string } {
        return this.withLock(() => {
            const result = {
                stdout: this.stdoutBuffer.join('\n'),
                stderr: this.stderrBuffer.join('\n'),
                merged: this.mergedBuffer.join('\n')
            };

            if (clearBuffers) {
                this.stdoutBuffer = [];
                this.stderrBuffer = [];
                this.mergedBuffer = [];
            }

            return result;
        });
    }

    /**
     * Simple synchronous lock mechanism for Node.js single-threaded environment.
     * @param action The action to execute within the lock.
     */
    private withLock<T>(action: () => T): T {
        // In Node.js, we don't need complex locking since it's single-threaded
        // But we'll maintain the pattern for consistency
        if (this.isLocked) {
            // In a real scenario, we might want to queue operations
            // For now, just execute immediately
        }
        
        this.isLocked = true;
        try {
            return action();
        } finally {
            this.isLocked = false;
        }
    }
}