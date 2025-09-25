using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public class MemoryLogger : ILogger
{
    private static readonly Lazy<MemoryLogger> _instance = new(() => new MemoryLogger());
    public static MemoryLogger Instance => _instance.Value;
    
    private const int MAX_LINES = 10000;
    
    private readonly LogFilter _filter = new();
    private readonly CircularBuffer<string> _lines = new(MAX_LINES);
    private readonly MultiStepTicketQueue<int> _tickets = new();
    
    private long _startedCount = 0;
    private bool _dumpOnExit = false;
    private string _dumpFileName = string.Empty;
    private string _dumpLinePrefix = string.Empty;
    private bool _dumpToStdOut = false;
    private bool _dumpToStdErr = false;
    
    // Ticket steps for lock-free operations
    private const byte LOG_STEP_UPDATE_BUFFER = 2;
    private const byte LOG_STEP_UPDATE_LINES = 3;
    
    public string Name => "Memory";
    public LogLevel Level { get; set; } = LogLevel.All;
    public bool IsLoggingEnabled => Volatile.Read(ref _startedCount) > 0;
    
    private MemoryLogger()
    {
        // Enhanced handler to capture exception details before dumping logs
        AppDomain.CurrentDomain.UnhandledException += (s, e) => 
        {
            // Extract and log the exception details first
            if (e.ExceptionObject is Exception ex)
            {
                // Log directly to memory buffer to avoid potential circular references
                LogExceptionDirectly(ex);
                
                // Then exit with the exception to dump logs
                Exit(ex);
            }
            else
            {
                // Unknown exception type, log what we can
                LogMessage(LogLevel.Error, "FATAL:", 
                          "UnhandledException", 
                          0, 
                          $"Unhandled exception of unknown type: {e.ExceptionObject}");
                Exit();
            }
        };
    }
    
    public void EnableLogging(bool enable)
    {
        if (enable)
        {
            Interlocked.Increment(ref _startedCount);
        }
        else if (_startedCount > 0)
        {
            Interlocked.Decrement(ref _startedCount);
        }
    }
    
    public void DumpOnExit(string? fileName, string linePrefix = "LOG", bool emitToStdOut = false, bool emitToStdErr = false)
    {
        _dumpOnExit = !string.IsNullOrEmpty(fileName) || emitToStdOut || emitToStdErr;
        _dumpFileName = fileName ?? string.Empty;
        _dumpLinePrefix = linePrefix ?? "LOG";
        _dumpToStdOut = emitToStdOut;
        _dumpToStdErr = emitToStdErr;
    }
    
    public void Dump(string? fileName = null, string linePrefix = "LOG", bool emitToStdOut = false, bool emitToStdErr = false)
    {
        bool emitToFile = !string.IsNullOrEmpty(fileName);
        
        if (!emitToFile && !emitToStdOut && !emitToStdErr)
            return;
            
        StreamWriter? fileWriter = null;
        
        // Lock range to prevent new log entries during dump
        var lockTickets = LockAllSteps();
        
        try
        {
            if (emitToFile && fileName != null)
            {
                var directory = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                fileWriter = new StreamWriter(fileName, false, Encoding.UTF8);
            }
            
            var allLines = _lines.ReadAll().ToList();
            
            foreach (var line in allLines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                
                var formattedLine = $"{linePrefix}: {line}";
                
                if (emitToStdOut)
                    Console.Out.Write(formattedLine);
                    
                if (emitToStdErr)
                    Console.Error.Write(formattedLine);
                    
                if (fileWriter != null)
                    fileWriter.Write(formattedLine);
            }
        }
        finally
        {
            // Dispose lock tickets in reverse order (LIFO)
            for (int i = lockTickets.Length - 1; i >= 0; i--)
            {
                lockTickets[i].Dispose();
            }
            fileWriter?.Dispose();
        }
    }
    
    private MultiStepTicketQueue<int>.TicketGuard[] LockAllSteps()
    {
        // Create two back-to-back tickets for range locking
        var ticket1 = _tickets.CreateTicketGuard();
        var ticket2 = _tickets.CreateTicketGuard();
        
        // Advance to lock the range
        ticket1.AdvanceToStep(254); // Lock high step
        ticket2.AdvanceToStep(LOG_STEP_UPDATE_LINES); // Lock low step
        
        return new[] { ticket2, ticket1 }; // Return in reverse order for proper disposal
    }
    
    public bool IsLevelEnabled(LogLevel level) => (Level & level) != 0;
    
    public void SetFilter(string filterPattern) => _filter.SetFilter(filterPattern);
    
    // Helper method to log exception directly to memory buffer to avoid circular references
    private void LogExceptionDirectly(Exception ex)
    {
        if (!IsLoggingEnabled)
            return;
            
        var formatted = $"FATAL: Unhandled exception: {ex.Message}\n{ex.StackTrace}";
        
        // Lock-free logging using ticket system
        using var ticket = _tickets.CreateTicketGuard();
        ticket.AdvanceToStep(LOG_STEP_UPDATE_BUFFER);
        
        // Write to circular buffer
        _lines.Write(formatted);
        
        ticket.AdvanceToStep(LOG_STEP_UPDATE_LINES);
        
        // Log inner exceptions if any
        var innerEx = ex.InnerException;
        int depth = 0;
        while (innerEx != null && depth < 5) // Limit depth to prevent infinite loops
        {
            var innerFormatted = $"INNER: Inner exception ({depth}): {innerEx.Message}\n{innerEx.StackTrace}";
            
            using var innerTicket = _tickets.CreateTicketGuard();
            innerTicket.AdvanceToStep(LOG_STEP_UPDATE_BUFFER);
            _lines.Write(innerFormatted);
            innerTicket.AdvanceToStep(LOG_STEP_UPDATE_LINES);
            
            innerEx = innerEx.InnerException;
            depth++;
        }
    }
    
    public void LogMessage(LogLevel level, string title, string fileName, int lineNumber, string message)
    {
        if (!IsLevelEnabled(level) || !_filter.ShouldLog(message))
            return;
            
        if (!IsLoggingEnabled)
            return;
            
        var formatted = LogFormatter.FormatMessage(level, title, fileName, lineNumber, message);
        
        // Lock-free logging using ticket system
        using var ticket = _tickets.CreateTicketGuard();
        ticket.AdvanceToStep(LOG_STEP_UPDATE_BUFFER);
        
        // Write to circular buffer
        _lines.Write(formatted);
        
        ticket.AdvanceToStep(LOG_STEP_UPDATE_LINES);
    }
    
    // Updated to accept an optional exception parameter
    private void Exit(Exception? exception = null)
    {
        if (!_dumpOnExit) return;
        
        try
        {
            // If exception is provided and not already logged, log it first
            if (exception != null)
            {
                LogExceptionDirectly(exception);
            }
            
            Dump(_dumpFileName, _dumpLinePrefix, _dumpToStdOut, _dumpToStdErr);
        }
        catch
        {
            // Ignore exceptions during exit
        }
    }
    
    public void Close() 
    {
        EnableLogging(false);
        _lines.Clear(); // Clear the circular buffer
    }
}