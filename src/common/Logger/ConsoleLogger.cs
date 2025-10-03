using System;
using System.Threading;

public class ConsoleLogger : ILogger
{
    private static readonly Lazy<ConsoleLogger> _instance = new(() => new ConsoleLogger());
    public static ConsoleLogger Instance => _instance.Value;
    
    private readonly LogFilter _filter = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private bool _isEnabled;
    private bool _logToStderr = true;
    
    public string Name => "Console";
    public LogLevel Level { get; set; } = LogLevel.All;
    public bool IsLoggingEnabled 
    { 
        get
        {
            _lock.EnterReadLock();
            try { return _isEnabled; }
            finally { _lock.ExitReadLock(); }
        }
    }
    
    private ConsoleLogger()
    {
        _isEnabled = false;
    }
    
    public void Start(bool logToStderr = true)
    {
        _lock.EnterWriteLock();
        try
        {
            _isEnabled = true;
            _logToStderr = logToStderr;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public void Stop()
    {
        _lock.EnterWriteLock();
        try
        {
            _isEnabled = false;
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
            if (!_isEnabled) return;
            
            var formatted = LogFormatter.FormatMessage(level, title, fileName, lineNumber, message);
            
            if (_logToStderr)
                Console.Error.Write(formatted);
            else
                Console.Out.Write(formatted);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    public void Close() => Stop();
}