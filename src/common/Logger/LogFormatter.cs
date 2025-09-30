using System;
using System.IO;
using System.Threading;

public static class LogFormatter
{
    private static readonly DateTime _startTime = DateTime.UtcNow;
    
    public static string FormatMessage(LogLevel level, string title, string fileName, int lineNumber, string format, params object[] args)
    {
        var message = args?.Length > 0 ? string.Format(format, args) : format;
        var threadId = Thread.CurrentThread.ManagedThreadId;
        var elapsed = (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        var fileNameOnly = Path.GetFileName(fileName);
        
        var formattedMessage = $"[{threadId:D6}]: {elapsed}ms {title} {fileNameOnly}:{lineNumber} {message}";
        
        if (!formattedMessage.EndsWith('\n'))
            formattedMessage += '\n';
            
        return formattedMessage;
    }
}