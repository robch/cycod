/// <summary>
/// Simple state machine for tracking content generation operations.
/// Thread-safe implementation with proper state transition validation.
/// </summary>
public class GenerationStateMachine
{
    private GenerationState _state = GenerationState.Idle;
    private readonly object _lock = new object();
    private string? _lastError;
    private DateTime? _startedAt;
    
    /// <summary>
    /// Gets the current generation state.
    /// </summary>
    public GenerationState CurrentState 
    { 
        get { lock (_lock) return _state; } 
    }
    
    /// <summary>
    /// Gets the error message from the last failed generation, if any.
    /// </summary>
    public string? LastError
    {
        get { lock (_lock) return _lastError; }
    }
    
    /// <summary>
    /// Gets when the current generation started, if applicable.
    /// </summary>
    public DateTime? StartedAt
    {
        get { lock (_lock) return _startedAt; }
    }
    
    /// <summary>
    /// Attempts to start a new generation operation.
    /// </summary>
    /// <returns>True if generation was started, false if already in progress</returns>
    public bool TryStartGeneration()
    {
        lock (_lock)
        {
            if (_state != GenerationState.Idle)
            {
                return false; // Can't start - already busy
            }
            
            _state = GenerationState.Generating;
            _startedAt = DateTime.UtcNow;
            _lastError = null;
            return true;
        }
    }
    
    /// <summary>
    /// Marks the generation as completed successfully.
    /// </summary>
    /// <returns>True if state was updated, false if not currently generating</returns>
    public bool MarkCompleted()
    {
        lock (_lock)
        {
            if (_state != GenerationState.Generating)
            {
                return false; // Invalid state transition
            }
            
            _state = GenerationState.CompletedWithSuccess;
            return true;
        }
    }
    
    /// <summary>
    /// Marks the generation as failed with an error message.
    /// </summary>
    /// <param name="errorMessage">The error that caused the failure</param>
    /// <returns>True if state was updated, false if not currently generating</returns>
    public bool MarkFailed(string errorMessage)
    {
        lock (_lock)
        {
            if (_state != GenerationState.Generating)
            {
                return false; // Invalid state transition
            }
            
            _state = GenerationState.CompletedWithFailure;
            _lastError = errorMessage;
            return true;
        }
    }
    
    /// <summary>
    /// Resets the state machine to idle, ready for a new generation.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _state = GenerationState.Idle;
            _startedAt = null;
            _lastError = null;
        }
    }
    
    /// <summary>
    /// Gets a user-friendly description of the current state.
    /// </summary>
    public string GetStatusDescription()
    {
        lock (_lock)
        {
            return _state switch
            {
                GenerationState.Idle => "Ready",
                GenerationState.Generating => $"Generating... (started {GetElapsedTime()})",
                GenerationState.CompletedWithSuccess => "Generation completed successfully",
                GenerationState.CompletedWithFailure => $"Generation failed: {_lastError ?? "unknown error"}",
                _ => "Unknown state"
            };
        }
    }
    
    private string GetElapsedTime()
    {
        if (_startedAt == null) return "unknown time";
        
        var elapsed = DateTime.UtcNow - _startedAt.Value;
        if (elapsed.TotalSeconds < 60)
            return $"{elapsed.TotalSeconds:F0}s ago";
        else
            return $"{elapsed.TotalMinutes:F0}m ago";
    }
}