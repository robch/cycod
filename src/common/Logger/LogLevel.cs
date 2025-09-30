using System;

[Flags]
public enum LogLevel : int
{
    None = 0x00,
    Error = 0x02,
    Warning = 0x04,
    Info = 0x08,
    Verbose = 0x10,
    
    // Cumulative levels (matching original bitflag logic)
    ErrorOnly = Error,
    ErrorAndWarning = Error | Warning,
    ErrorWarningInfo = Error | Warning | Info,
    All = Error | Warning | Info | Verbose
}