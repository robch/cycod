using Microsoft.Extensions.Logging;

namespace CycodBench.Logging;

/// <summary>
/// Interface for the CycodBench logging service.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Debug(string message, params object[] args);
    
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Info(string message, params object[] args);
    
    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Warning(string message, params object[] args);
    
    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Error(string message, params object[] args);
    
    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Error(Exception exception, string message, params object[] args);
    
    /// <summary>
    /// Logs a critical message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Critical(string message, params object[] args);
    
    /// <summary>
    /// Logs a critical message with an exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Critical(Exception exception, string message, params object[] args);
    
    /// <summary>
    /// Creates a scope for logging.
    /// </summary>
    /// <param name="state">The state object for the scope.</param>
    /// <returns>A disposable object that ends the scope when disposed.</returns>
    IDisposable? BeginScope(object state);
}