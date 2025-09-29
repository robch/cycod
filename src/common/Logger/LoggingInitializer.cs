using System;
using System.IO;

public static class LoggingInitializer
{
    private static bool _isInitialized = false;
    private static bool _memoryLogInitialized = false;
    private static readonly object _initLock = new object();

    /// <summary>
    /// Initialize the memory logger to start capturing logs immediately.
    /// This should be called as early as possible in the application.
    /// </summary>
    public static void InitializeMemoryLogger()
    {
        if (_memoryLogInitialized)
            return;
            
        lock (_initLock)
        {
            if (_memoryLogInitialized)
                return;
                
            try
            {
                // Configure memory logger
                Logger.ConfigureMemoryLogger(true);
                Logger.Info("Memory logger initialized.");
                _memoryLogInitialized = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to initialize memory logger: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Dump memory logs to the configured exception file
    /// </summary>
    /// <param name="exception">Optional exception that triggered the error</param>
    public static void DumpMemoryLogsOnError(Exception? exception = null)
    {
        try
        {
            // Access the memory logger instance directly
            var memory = (MemoryLogger)MemoryLogger.Instance;
            
            // Get the current memory log contents
            var logsDir = Path.Combine(Path.GetTempPath(), "cycod-logs");
            string dumpFileName = Path.Combine(
                logsDir, 
                $"exception-log-{ProgramInfo.Name}-{DateTime.Now:yyyyMMddHHmmss}.log");
                
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(dumpFileName) ?? "");
            
            // If we have an exception, ensure it gets logged before dumping
            if (exception != null)
            {
                // Access Exit method using reflection as it's private
                var exitMethod = typeof(MemoryLogger).GetMethod("Exit", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (exitMethod != null)
                {
                    exitMethod.Invoke(memory, new object[] { exception });
                    return; // Exit already calls Dump, so we're done
                }
                
                // If reflection failed, log directly before dumping
                Logger.Error($"Unhandled exception: {exception.Message}\n{exception.StackTrace}");
            }
            
            // Dump the memory logs to the file
            memory.Dump(dumpFileName, "LOG", emitToStdOut: false, emitToStdErr: false);
            
            Console.Error.WriteLine($"Exception logs written to: {dumpFileName}");
        }
        catch
        {
            // Ignore errors in error handler
        }
    }
    
    /// <summary>
    /// Grounds an auto-save log filename if auto-save is enabled.
    /// </summary>
    /// <returns>The grounded auto-save filename, or null if auto-save is disabled</returns>
    public static string? GroundAutoSaveLogFileName()
    {
        var shouldAutoSave = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoSaveLog).AsBool(true);
        if (shouldAutoSave)
        {
            var logsDir = EnsureLogsDirectory();
            var fileName = Path.Combine(logsDir, "log-{ProgramName}-{time}.log");
            return FileHelpers.GetFileNameFromTemplate("log.log", fileName);
        }
        return null;
    }

    /// <summary>
    /// Ensures the logs directory exists and returns its path.
    /// </summary>
    /// <returns>The path to the logs directory.</returns>
    private static string EnsureLogsDirectory()
    {
        return ScopeFileHelpers.EnsureDirectoryInScope("logs", ConfigFileScope.User);
    }
    
    /// <summary>
    /// Check if a file is locked (in use by another process)
    /// </summary>
    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                // File is not locked
                stream.Close();
            }
            return false;
        }
        catch (IOException)
        {
            // File is locked
            return true;
        }
        catch (Exception)
        {
            // If we can't determine if it's locked, assume it's not
            return false;
        }
    }

    /// <summary>
    /// Initialize the full logging system with file and console loggers.
    /// This should be called after command line parsing is complete.
    /// </summary>
    /// <param name="logFileName">Path to log file, or null if file logging is not desired</param>
    /// <param name="debugMode">Whether debug mode is enabled</param>
    public static void InitializeLogging(string? logFileName, bool debugMode)
    {
        if (_isInitialized)
            return;
            
        lock (_initLock)
        {
            if (_isInitialized)
                return;
                
            try
            {
                // If memory logger not yet initialized, do it now
                if (!_memoryLogInitialized)
                {
                    InitializeMemoryLogger();
                }
                
                // Get log level configuration from environment variable
                var logConfig = LogConfiguration.GetConfig("default");
                
                // Apply log level configuration to all loggers immediately
                if (logConfig.Level != LogLevel.All)
                {
                    FileLogger.Instance.Level = logConfig.Level;
                    ConsoleLogger.Instance.Level = logConfig.Level;
                    MemoryLogger.Instance.Level = logConfig.Level;
                }
                
                // If no explicit log file is provided, check if auto-save is enabled
                if (string.IsNullOrEmpty(logFileName))
                {
                    logFileName = GroundAutoSaveLogFileName();
                }
                
                // Configure file logger if filename was provided or auto-save is enabled
                if (!string.IsNullOrEmpty(logFileName))
                {
                    try
                    {
                        // Create directory for log file if it doesn't exist
                        var directory = Path.GetDirectoryName(logFileName);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        
                        // Process any templates in the log filename
                        var processedLogFileName = FileHelpers.GetFileNameFromTemplate("log.log", logFileName) ?? logFileName;
                        
                        // For relative paths, they will be resolved relative to the current working directory
                        // For absolute paths, they will be used as-is
                        
                        Logger.ConfigureFileLogger(processedLogFileName, append: true);
                        Logger.Info($"File logger initialized with file: {processedLogFileName}");
                        
                        // Update the logFileName to the processed version
                        logFileName = processedLogFileName;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to initialize file logger: {ex.Message}");
                        // Continue without file logging, but keep memory logging active
                    }
                }
                
                // Configure console logger based on debug mode
                Logger.ConfigureConsoleLogger(debugMode);
                if (debugMode)
                {
                    Logger.Info("Console logger initialized in debug mode.");
                }
                
                // Configure memory logger to dump to file on crash with a default filename
                string crashLogFile;
                
                if (!string.IsNullOrEmpty(logFileName))
                {
                    // Use the same directory as the log file
                    string logDirectory = Path.GetDirectoryName(logFileName) ?? "";
                    string exceptionFileName = $"exception-{Path.GetFileName(logFileName)}";
                    crashLogFile = Path.Combine(logDirectory, exceptionFileName);
                }
                else
                {
                    // Create a default exception log filename with template processing
                    string exceptionLogTemplate = "exception-log-{ProgramName}-{time}.log";
                    crashLogFile = FileHelpers.GetFileNameFromTemplate("exception.log", exceptionLogTemplate) ?? "exception-log.log";
                    
                    // Use the current working directory for exception logs when no explicit log file is provided
                }
                
                // Access the MemoryLogger directly to configure it with exactly the settings we want
                MemoryLogger.Instance.EnableLogging(true);
                MemoryLogger.Instance.DumpOnExit(crashLogFile, emitToStdOut: false, emitToStdErr: false);
                Logger.Info($"Memory logger configured to dump to {crashLogFile} only on abnormal exit.");
                
                _isInitialized = true;
                Logger.Info("Logging system fully initialized.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to initialize logging system: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Logs detailed startup information for debugging purposes
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void LogStartupDetails(string[] args)
    {
        var processId = Environment.ProcessId;
        var currentDir = Directory.GetCurrentDirectory();
        var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        var argsString = string.Join(" ", args);
        
        Logger.Info($"Process Details - PID: {processId}, Name: {processName}, ThreadID: {threadId}");
        Logger.Info($"Working Directory: {currentDir}");
        Logger.Info($"Command Line Args: {argsString}");
        Logger.Info($"Environment Variables: LOG_LEVEL={Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "not set"}");
    }
}