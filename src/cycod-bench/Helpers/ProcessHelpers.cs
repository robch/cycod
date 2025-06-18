using System.Diagnostics;
using System.Text;

namespace CycodBench.Helpers;

/// <summary>
/// Helper methods for process execution.
/// </summary>
public static class ProcessHelpers
{
    /// <summary>
    /// Executes a process asynchronously with the specified timeout.
    /// </summary>
    /// <param name="processStartInfo">The process start info.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A tuple containing the standard output and standard error.</returns>
    public static async Task<(string StdOut, string StdErr)> ExecuteProcessAsync(
        ProcessStartInfo processStartInfo,
        int timeoutMs = 30000,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process { StartInfo = processStartInfo };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        process.OutputDataReceived += (sender, e) => 
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        
        process.ErrorDataReceived += (sender, e) => 
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };
        
        process.Start();
        
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
        
        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best effort to kill the process
                }
                
                if (timeoutCts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException($"Process execution timed out after {timeoutMs}ms");
                }
                
                throw;
            }
        }
        
        return (outputBuilder.ToString(), errorBuilder.ToString());
    }
}