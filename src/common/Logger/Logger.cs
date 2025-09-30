using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public static class Logger
{
    private static readonly Dictionary<string, ILogger> _loggers = new()
    {
        ["Console"] = ConsoleLogger.Instance,
        ["File"] = FileLogger.Instance,
        ["Event"] = EventLogger.Instance,
        ["Memory"] = MemoryLogger.Instance
    };
    
    public static bool IsLogLevelEnabled(LogLevel level)
    {
        return _loggers.Values.Any(logger => logger.IsLoggingEnabled && logger.IsLevelEnabled(level));
    }
    
    public static void LogMessage(LogLevel level, string title, 
        [CallerFilePath] string fileName = "", 
        [CallerLineNumber] int lineNumber = 0, 
        string format = "", 
        params object[] args)
    {
        if (!IsLogLevelEnabled(level))
            return;
            
        var message = args?.Length > 0 ? string.Format(format, args) : format;
        
        foreach (var logger in _loggers.Values)
        {
            if (logger.IsLoggingEnabled && logger.IsLevelEnabled(level))
            {
                logger.LogMessage(level, title, fileName, lineNumber, message);
            }
        }
    }
    
    // Convenience methods
    public static void Error(string message, params object[] args) => 
        LogMessage(LogLevel.Error, "ERROR:", format: message, args: args);
        
    public static void Error(string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0) => 
        LogMessage(LogLevel.Error, "ERROR:", fileName, lineNumber, message);
        
    public static void Warning(string message, params object[] args) => 
        LogMessage(LogLevel.Warning, "WARNING:", format: message, args: args);
        
    public static void Warning(string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0) => 
        LogMessage(LogLevel.Warning, "WARNING:", fileName, lineNumber, message);
        
    public static void Info(string message, params object[] args) => 
        LogMessage(LogLevel.Info, "INFO:", format: message, args: args);
        
    public static void Info(string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0) => 
        LogMessage(LogLevel.Info, "INFO:", fileName, lineNumber, message);
        
    public static void Verbose(string message, params object[] args) => 
        LogMessage(LogLevel.Verbose, "VERBOSE:", format: message, args: args);
        
    public static void Verbose(string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0) => 
        LogMessage(LogLevel.Verbose, "VERBOSE:", fileName, lineNumber, message);
    
    // Conditional logging
    public static void ErrorIf(bool condition, string message, params object[] args)
    {
        if (condition) Error(message, args);
    }
    
    public static void ErrorIf(bool condition, string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (condition) Error(message, fileName, lineNumber);
    }
    
    public static void WarningIf(bool condition, string message, params object[] args)
    {
        if (condition) Warning(message, args);
    }
    
    public static void WarningIf(bool condition, string message, [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (condition) Warning(message, fileName, lineNumber);
    }
    
    // Configuration
    public static void ConfigureFileLogger(string fileName, uint durationSeconds = 0, uint sizeMB = 0, bool append = false)
    {
        ((FileLogger)_loggers["File"]).SetFileOptions(fileName, durationSeconds, sizeMB, append);
    }
    
    public static void ConfigureConsoleLogger(bool enable, bool logToStderr = true)
    {
        var console = (ConsoleLogger)_loggers["Console"];
        if (enable) console.Start(logToStderr);
        else console.Stop();
    }
    
    public static void ConfigureEventLogger(Action<string> callback)
    {
        ((EventLogger)_loggers["Event"]).AttachLogTarget(callback);
    }
    
    public static void ConfigureMemoryLogger(bool enable, bool dumpOnExit = false, string? dumpFile = null)
    {
        var memory = (MemoryLogger)_loggers["Memory"];
        memory.EnableLogging(enable);
        if (dumpOnExit)
        {
            memory.DumpOnExit(dumpFile, emitToStdErr: true);
        }
    }
    
    public static void DumpMemoryLogs(string? fileName = null) => 
        ((MemoryLogger)_loggers["Memory"]).Dump(fileName, emitToStdOut: true);
        
    // Set log levels for individual loggers
    public static void SetLogLevel(string loggerName, LogLevel level)
    {
        if (_loggers.TryGetValue(loggerName, out var logger))
        {
            logger.Level = level;
        }
    }
    
    // Set filters for individual loggers
    public static void SetLogFilter(string loggerName, string filterPattern)
    {
        if (_loggers.TryGetValue(loggerName, out var logger))
        {
            logger.SetFilter(filterPattern);
        }
    }
    
    // Close all loggers
    public static void CloseAll()
    {
        foreach (var logger in _loggers.Values)
        {
            logger.Close();
        }
    }
}