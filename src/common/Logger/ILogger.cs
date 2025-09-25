public interface ILogger
{
    string Name { get; }
    LogLevel Level { get; set; }
    bool IsLoggingEnabled { get; }
    bool IsLevelEnabled(LogLevel level);
    void SetFilter(string filterPattern);
    void LogMessage(LogLevel level, string title, string fileName, int lineNumber, string message);
    void Close();
}