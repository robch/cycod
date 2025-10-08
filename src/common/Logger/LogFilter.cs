using System;
using System.Linq;
using System.Threading;

public class LogFilter
{
    private string[] _filters = Array.Empty<string>();
    private readonly ReaderWriterLockSlim _lock = new();
    
    public void SetFilter(string filterPattern)
    {
        _lock.EnterWriteLock();
        try
        {
            if (string.IsNullOrWhiteSpace(filterPattern))
            {
                _filters = Array.Empty<string>();
            }
            else
            {
                _filters = filterPattern.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(f => f.Trim())
                                        .Where(f => !string.IsNullOrEmpty(f))
                                        .ToArray();
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public bool ShouldLog(string logLine)
    {
        _lock.EnterReadLock();
        try
        {
            if (_filters.Length == 0)
                return true;
                
            return _filters.Any(filter => logLine.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}