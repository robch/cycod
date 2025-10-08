using System;
using System.Collections.Generic;
using System.Threading;

public class LogConfiguration
{
    private static readonly Dictionary<string, LogConfiguration> _configs = new();
    private static readonly ReaderWriterLockSlim _configLock = new();
    
    public LogLevel Level { get; set; } = LogLevel.All;
    
    public static LogConfiguration GetConfig(string loggerName)
    {
        _configLock.EnterReadLock();
        try
        {
            if (_configs.TryGetValue(loggerName, out var config))
                return config;
        }
        finally
        {
            _configLock.ExitReadLock();
        }

        _configLock.EnterWriteLock();
        try
        {
            if (!_configs.TryGetValue(loggerName, out var config))
            {
                config = new LogConfiguration();
                _configs[loggerName] = config;
            }
            return config;
        }
        finally
        {
            _configLock.ExitWriteLock();
        }
    }
    
    private LogConfiguration()
    {
        // Get default level from environment
        var envLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
        if (!string.IsNullOrEmpty(envLevel) && Enum.TryParse<LogLevel>(envLevel, true, out var level))
        {
            Level = level;
        }
    }
}