/// <summary>
/// Represents the result of a generation operation with success/failure information.
/// Provides a consistent pattern for handling both successful and failed operations.
/// </summary>
/// <typeparam name="T">The type of content being generated</typeparam>
public class GenerationResult<T>
{
    /// <summary>
    /// Whether the generation operation succeeded.
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// The generated content, available when Success is true.
    /// </summary>
    public T? Value { get; init; }
    
    /// <summary>
    /// User-friendly error message, available when Success is false.
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// The original exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; init; }
    
    /// <summary>
    /// Creates a successful result with the generated value.
    /// </summary>
    /// <param name="value">The successfully generated content</param>
    /// <returns>A success result containing the value</returns>
    public static GenerationResult<T> FromSuccess(T value)
    {
        return new GenerationResult<T> { Success = true, Value = value };
    }
    
    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    /// <param name="errorMessage">User-friendly error description</param>
    /// <param name="exception">The original exception, if any</param>
    /// <returns>A failure result containing error information</returns>
    public static GenerationResult<T> FromError(string errorMessage, Exception? exception = null)
    {
        return new GenerationResult<T> 
        { 
            Success = false, 
            ErrorMessage = errorMessage, 
            Exception = exception 
        };
    }
    
    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A failure result with error message extracted from the exception</returns>
    public static GenerationResult<T> FromException(Exception exception)
    {
        return new GenerationResult<T>
        {
            Success = false,
            ErrorMessage = exception.Message,
            Exception = exception
        };
    }
}