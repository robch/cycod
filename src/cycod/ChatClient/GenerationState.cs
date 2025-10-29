/// <summary>
/// Represents the current state of a content generation operation.
/// </summary>
public enum GenerationState
{
    /// <summary>
    /// Ready to start a new generation.
    /// </summary>
    Idle,
    
    /// <summary>
    /// Currently generating content.
    /// </summary>
    Generating,
    
    /// <summary>
    /// Generation completed successfully - notification may be pending.
    /// </summary>
    CompletedWithSuccess,
    
    /// <summary>
    /// Generation completed with failure - error may be pending.
    /// </summary>
    CompletedWithFailure
}