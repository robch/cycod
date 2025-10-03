public class TestLogger
{
    public static void Log(IYamlTestFrameworkLogger logger)
    {
        _logger = logger;
    }

    public static void Log(string text)
    {
        using (var mutex = new Mutex(false, "TestLogger Mutex"))
        {
            mutex.WaitOne();
#if DEBUG
            _logger?.LogInfo(text);
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
        using (var mutex = new Mutex(false, "TestLogger Mutex"))
        {
            mutex.WaitOne();
            _logger?.LogInfo(text);
            mutex.ReleaseMutex();
        }
    }

    public static void LogWarning(string text)
    {
        using (var mutex = new Mutex(false, "TestLogger Mutex"))
        {
            mutex.WaitOne();
            _logger?.LogWarning(text);
            mutex.ReleaseMutex();
        }
    }

    public static void LogError(string text)
    {
        using (var mutex = new Mutex(false, "TestLogger Mutex"))
        {
            mutex.WaitOne();
            _logger?.LogError(text);
            mutex.ReleaseMutex();
        }
    }

    #endregion

    #region private methods and data

    private static IYamlTestFrameworkLogger? _logger = null;

    #endregion
}
