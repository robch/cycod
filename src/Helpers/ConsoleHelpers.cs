using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

class ConsoleHelpers
{
    public static void Configure(bool debug, bool verbose)
    {
        Console.OutputEncoding = Encoding.UTF8;

        _debug = debug;
        _verbose = verbose;

        WriteDebugLine($"Debug: {_debug}");
        WriteDebugLine($"Verbose: {_verbose}");
    }

    public static bool IsVerbose()
    {
        return _verbose;
    }

    public static bool IsDebug()
    {
        return _debug;
    }

    public static void DisplayStatus(string status)
    {
        if (!_debug && !_verbose) return;
        if (Console.IsOutputRedirected) return;

        lock (_printLock)
        {
            DisplayStatusErase();
            Console.Write("\r" + status);
            _cchLastStatus = status.Length;
            if (_debug) Thread.Sleep(1);
        }
    }

    public static void DisplayStatusErase()
    {
        if (!_debug && !_verbose) return;
        if (_cchLastStatus <= 0) return;

        lock (_printLock)
        {
            var eraseLastStatus = "\r" + new string(' ', _cchLastStatus) + "\r";
            Console.Write(eraseLastStatus);
            _cchLastStatus = 0;
        }
    }

    public static void Write(string message = "", ConsoleColor? color = null)
    {
        lock (_printLock)
        {
            var prevColor = Console.ForegroundColor;
            if (color != null) Console.ForegroundColor = (ConsoleColor)color;

            Console.Write(message);
            if (color != null) Console.ForegroundColor = prevColor;
        }
    }

    public static void WriteLine(string message = "", ConsoleColor? color = null)
    {
        lock (_printLock)
        {
            DisplayStatusErase();

            var prevColor = Console.ForegroundColor;
            if (color != null) Console.ForegroundColor = (ConsoleColor)color;

            Console.WriteLine(message);
            if (color != null) Console.ForegroundColor = prevColor;
        }
    }

    public static void WriteLineIfNotEmpty(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        WriteLine(message);
    }

    public static void WriteDebugLine(string message)
    {
        if (!_debug) return;
        WriteLine(message, ConsoleColor.Cyan);
    }

    public static IEnumerable<string> GetAllLinesFromStdin()
    {
        return _allLinesFromStdin == null
            ? ReadAllLinesFromStdin()
            : _allLinesFromStdin;
    }

    private static List<string> ReadAllLinesFromStdin()
    {
        _allLinesFromStdin = new();
        while (true)
        {
            var line = Console.ReadLine();
            if (line == null) break;

            _allLinesFromStdin.Add(line);
        }

        return _allLinesFromStdin;
    }
    
    private static bool _debug = false;
    private static bool _verbose = false;
    private static object _printLock = new();
    private static int _cchLastStatus = 0;

    private static List<string>? _allLinesFromStdin;
}