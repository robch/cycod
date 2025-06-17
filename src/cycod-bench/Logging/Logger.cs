using Microsoft.Extensions.Logging;
using System.Text;

namespace CycodBench.Logging;

/// <summary>
/// Implementation of the CycodBench logging service.
/// </summary>
public class Logger : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly string? _logFilePath;
    private readonly object _fileLock = new object();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="factory">The logger factory.</param>
    /// <param name="category">The logger category.</param>
    /// <param name="logFilePath">Optional file path for logging to a file.</param>
    public Logger(ILoggerFactory factory, string category, string? logFilePath = null)
    {
        _logger = factory.CreateLogger(category);
        _logFilePath = logFilePath;
        
        if (!string.IsNullOrEmpty(_logFilePath))
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    /// <inheritdoc/>
    public void Debug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
        WriteToFile(LogLevel.Debug, message, args);
    }

    /// <inheritdoc/>
    public void Info(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
        WriteToFile(LogLevel.Information, message, args);
    }

    /// <inheritdoc/>
    public void Warning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
        WriteToFile(LogLevel.Warning, message, args);
    }

    /// <inheritdoc/>
    public void Error(string message, params object[] args)
    {
        _logger.LogError(message, args);
        WriteToFile(LogLevel.Error, message, args);
    }

    /// <inheritdoc/>
    public void Error(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
        WriteToFile(LogLevel.Error, exception, message, args);
    }

    /// <inheritdoc/>
    public void Critical(string message, params object[] args)
    {
        _logger.LogCritical(message, args);
        WriteToFile(LogLevel.Critical, message, args);
    }

    /// <inheritdoc/>
    public void Critical(Exception exception, string message, params object[] args)
    {
        _logger.LogCritical(exception, message, args);
        WriteToFile(LogLevel.Critical, exception, message, args);
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope(object state)
    {
        return _logger.BeginScope(state);
    }
    
    private void WriteToFile(LogLevel level, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(_logFilePath))
        {
            return;
        }
        
        try
        {
            string formattedMessage = FormatMessage(level, message, args);
            lock (_fileLock)
            {
                File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            // Log to console if file logging fails
            Console.Error.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }
    
    private void WriteToFile(LogLevel level, Exception exception, string message, params object[] args)
    {
        if (string.IsNullOrEmpty(_logFilePath))
        {
            return;
        }
        
        try
        {
            string formattedMessage = FormatMessage(level, exception, message, args);
            lock (_fileLock)
            {
                File.AppendAllText(_logFilePath, formattedMessage + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            // Log to console if file logging fails
            Console.Error.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }
    
    private string FormatMessage(LogLevel level, string message, params object[] args)
    {
        string formattedMessage;
        try
        {
            formattedMessage = string.Format(message, args);
        }
        catch (FormatException)
        {
            formattedMessage = message;
        }
        
        return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {formattedMessage}";
    }
    
    private string FormatMessage(LogLevel level, Exception exception, string message, params object[] args)
    {
        string formattedMessage = FormatMessage(level, message, args);
        
        var sb = new StringBuilder();
        sb.AppendLine(formattedMessage);
        sb.AppendLine($"Exception: {exception.GetType().FullName}");
        sb.AppendLine($"Message: {exception.Message}");
        sb.AppendLine($"StackTrace: {exception.StackTrace}");
        
        var innerException = exception.InnerException;
        while (innerException != null)
        {
            sb.AppendLine($"Inner Exception: {innerException.GetType().FullName}");
            sb.AppendLine($"Message: {innerException.Message}");
            sb.AppendLine($"StackTrace: {innerException.StackTrace}");
            innerException = innerException.InnerException;
        }
        
        return sb.ToString().TrimEnd();
    }
}