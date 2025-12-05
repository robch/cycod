using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Lightweight event-driven interrupt manager for handling user interrupts during AI streaming.
/// Replaces the polling-based approach with a more efficient event-driven system.
/// </summary>
public class SimpleInterruptManager : IDisposable
{
    private readonly TaskCompletionSource<InterruptResult> _interruptSignal = new();
    private readonly CancellationTokenSource _shutdownToken = new();
    private Task? _monitorTask;
    private bool _disposed = false;

    /// <summary>
    /// Waits asynchronously for an interrupt to occur.
    /// </summary>
    /// <returns>A task that completes when an interrupt is detected.</returns>
    public Task<InterruptResult> WaitForInterruptAsync()
    {
        return _interruptSignal.Task;
    }

    /// <summary>
    /// Starts monitoring for user interrupts in a background task.
    /// </summary>
    public void StartMonitoring()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SimpleInterruptManager));

        _monitorTask = Task.Run(MonitorForInterruptsAsync, _shutdownToken.Token);
    }

    /// <summary>
    /// Background task that monitors for interrupt conditions.
    /// </summary>
    private async Task MonitorForInterruptsAsync()
    {
        var escTracker = new EscKeyTracker();
        
        try
        {
            while (!_shutdownToken.Token.IsCancellationRequested)
            {
                // Only check for input if console is available and not redirected
                if (!Console.IsInputRedirected && Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    
                    if (escTracker.ProcessKey(keyInfo))
                    {
                        var result = new InterruptResult
                        {
                            Type = InterruptType.DoubleEscape,
                            Timestamp = DateTime.UtcNow
                        };
                        
                        _interruptSignal.TrySetResult(result);
                        return;
                    }
                }
                
                // Short delay - much more responsive than 50ms polling
                await Task.Delay(10, _shutdownToken.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when shutting down
        }
        catch (Exception ex)
        {
            // Signal error through the task completion source
            _interruptSignal.TrySetException(ex);
        }
    }

    /// <summary>
    /// Disposes resources and stops monitoring.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        
        try
        {
            _shutdownToken.Cancel();
            _monitorTask?.Wait(TimeSpan.FromMilliseconds(100));
        }
        catch (Exception)
        {
            // Ignore cleanup exceptions
        }
        finally
        {
            _shutdownToken.Dispose();
            _monitorTask?.Dispose();
        }
    }
}

/// <summary>
/// Tracks ESC key presses to detect double-ESC interrupt pattern.
/// </summary>
public class EscKeyTracker
{
    private DateTime? _lastEscTime;
    private const int DoubleEscTimeoutMs = 500;

    /// <summary>
    /// Processes a key press and returns true if double-ESC is detected.
    /// </summary>
    /// <param name="keyInfo">The key that was pressed.</param>
    /// <returns>True if double-ESC pattern is detected, false otherwise.</returns>
    public bool ProcessKey(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            var currentTime = DateTime.UtcNow;
            
            if (_lastEscTime.HasValue && 
                (currentTime - _lastEscTime.Value).TotalMilliseconds <= DoubleEscTimeoutMs)
            {
                // Double ESC detected
                _lastEscTime = null; // Reset for next sequence
                return true;
            }
            
            _lastEscTime = currentTime;
        }
        else
        {
            // Reset ESC tracking if any other key is pressed
            _lastEscTime = null;
        }
        
        return false;
    }
}

/// <summary>
/// Represents the result of an interrupt detection.
/// </summary>
public class InterruptResult
{
    public InterruptType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Types of interrupts that can be detected.
/// </summary>
public enum InterruptType
{
    DoubleEscape
}