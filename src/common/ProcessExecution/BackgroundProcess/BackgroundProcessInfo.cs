using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents information about a background process.
/// </summary>
public class BackgroundProcessInfo
{
    /// <summary>
    /// Gets the unique handle/identifier for this process.
    /// </summary>
    public string Handle { get; }

    /// <summary>
    /// Gets or sets the display name for this process.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the process has a custom name.
    /// </summary>
    public bool HasCustomName { get; set; }

    /// <summary>
    /// Gets the time when the process was started.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// Gets a value indicating whether the process is still running.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            if (Process == null) 
            {
                ConsoleHelpers.WriteDebugLine($"[PROCESS] {Name} IsRunning: false (Process is null)");
                return false;
            }
            
            try
            {
                var hasExited = Process.HasExited;
                ConsoleHelpers.WriteDebugLine($"[PROCESS] {Name} IsRunning: {!hasExited} (HasExited: {hasExited})");
                return !hasExited;
            }
            catch (InvalidOperationException ex)
            {
                ConsoleHelpers.WriteDebugLine($"[PROCESS] {Name} IsRunning: true (starting state, exception: {ex.Message})");
                return true;
            }
        }
    }

    /// <summary>
    /// Gets the underlying process.
    /// </summary>
    internal RunnableProcess Process { get; }

    /// <summary>
    /// Gets the accumulated standard output.
    /// </summary>
    private StringBuilder StdoutBuffer { get; } = new StringBuilder();

    /// <summary>
    /// Gets the accumulated standard error.
    /// </summary>
    private StringBuilder StderrBuffer { get; } = new StringBuilder();

    /// <summary>
    /// Gets the accumulated merged output.
    /// </summary>
    private StringBuilder MergedBuffer { get; } = new StringBuilder();

    /// <summary>
    /// Lock object for thread synchronization.
    /// </summary>
    private readonly object _lockObj = new object();

    /// <summary>
    /// Creates a new instance of the <see cref="BackgroundProcessInfo"/> class.
    /// </summary>
    /// <param name="handle">The unique handle for this process.</param>
    /// <param name="process">The underlying process.</param>
    public BackgroundProcessInfo(string handle, RunnableProcess process)
    {
        Handle = handle ?? throw new ArgumentNullException(nameof(handle));
        Process = process ?? throw new ArgumentNullException(nameof(process));
        Name = handle;  // Default name is the handle
        HasCustomName = false;
        StartTime = DateTime.Now;

        // Set up callbacks to capture output in real-time
        process.SetStdoutCallback(line => {
            lock (_lockObj) {
                StdoutBuffer.AppendLine(line);
                MergedBuffer.AppendLine(line);
            }
        });
        
        process.SetStderrCallback(line => {
            lock (_lockObj) {
                StderrBuffer.AppendLine(line);
                MergedBuffer.AppendLine(line);
            }
        });
    }

    /// <summary>
    /// Gets the accumulated output from the process and optionally clears the buffers.
    /// </summary>
    /// <param name="clearBuffers">Whether to clear the output buffers after retrieving.</param>
    /// <returns>A dictionary containing stdout, stderr, and merged output.</returns>
    public Dictionary<string, string> GetOutput(bool clearBuffers = false)
    {
        lock (_lockObj)
        {
            var result = new Dictionary<string, string>
            {
                { "stdout", StdoutBuffer.ToString() },
                { "stderr", StderrBuffer.ToString() },
                { "merged", MergedBuffer.ToString() }
            };

            if (clearBuffers)
            {
                StdoutBuffer.Clear();
                StderrBuffer.Clear();
                MergedBuffer.Clear();
            }

            return result;
        }
    }
}