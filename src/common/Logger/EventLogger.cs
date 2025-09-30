using System;
using System.Threading;

public class EventLogger : ILogger
{
    private static readonly Lazy<EventLogger> _instance = new(() => new EventLogger());
    public static EventLogger Instance => _instance.Value;
    
    private readonly LogFilter _filter = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private Action<string>? _callback;
    
    public string Name => "Event";
    public LogLevel Level { get; set; } = LogLevel.All;
    public bool IsLoggingEnabled
    {
        get
        {
            _lock.EnterReadLock();
            try { return _callback != null; }
            finally { _lock.ExitReadLock(); }
        }
    }
    
    private EventLogger() { }
    
    public void AttachLogTarget(Action<string> callback)
    {
        _lock.EnterWriteLock();
        try
        {
            _callback = callback;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public bool IsLevelEnabled(LogLevel level) => (Level & level) != 0;
    
    public void SetFilter(string filterPattern) => _filter.SetFilter(filterPattern);
    
    public void LogMessage(LogLevel level, string title, string fileName, int lineNumber, string message)
    {
        if (!IsLevelEnabled(level) || !_filter.ShouldLog(message))
            return;
            
        _lock.EnterReadLock();
        try
        {
            if (_callback == null) return;
            
            var formatted = LogFormatter.FormatMessage(level, title, fileName, lineNumber, message);
            try
            {
                _callback(formatted);
            }
            catch (Exception ex)
            {
                // Don't let callback exceptions crash the logging system
                // Try to log the error, but don't throw if that fails too
                try
                {
                    Console.WriteLine($"WARNING: EventLogger callback threw exception: {ex.Message}");
                }
                catch { /* Ignore console errors */ }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    public void Close()
    {
        _lock.EnterWriteLock();
        try
        {
            _callback = null;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}