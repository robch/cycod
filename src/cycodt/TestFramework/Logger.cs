public class Logger
{
    public static void Log(IYamlTestFrameworkLogger logger)
    {
        Logger.logger = logger;
    }

    public static void Log(string text)
    {
        using (var mutex = new Mutex(false, "Logger Mutex"))
        {
            mutex.WaitOne();
#if DEBUG
            logger?.LogInfo(text);
#endif
            mutex.ReleaseMutex();
        }
    }

    public static void LogIf(bool log, string text)
    {
        if (log) Log(text);
    }

    #region log methods

    public static void LogInfo(string text)
    {
        using (var mutex = new Mutex(false, "Logger Mutex"))
        {
            mutex.WaitOne();
            logger?.LogInfo(text);
            mutex.ReleaseMutex();
        }
    }

    public static void LogWarning(string text)
    {
        using (var mutex = new Mutex(false, "Logger Mutex"))
        {
            mutex.WaitOne();
            logger?.LogWarning(text);
            mutex.ReleaseMutex();
        }
    }

    public static void LogError(string text)
    {
        using (var mutex = new Mutex(false, "Logger Mutex"))
        {
            mutex.WaitOne();
            logger?.LogError(text);
            mutex.ReleaseMutex();
        }
    }

    #endregion

    #region private methods and data

    private static IYamlTestFrameworkLogger? logger = null;

    #endregion
}
