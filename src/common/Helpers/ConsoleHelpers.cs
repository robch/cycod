using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public class ConsoleHelpers
{
    public static void ConfigureDebug(bool debug)
    {
        _debug = _debug || debug;
        WriteDebugLine($"Debug: {_debug}");
    }

    public static void Configure(bool debug, bool verbose, bool quiet)
    {
        Console.OutputEncoding = Encoding.UTF8;

        _debug = debug;
        _verbose = verbose;
        _quiet = quiet;

        WriteDebugLine($"Debug: {_debug}");
        WriteDebugLine($"Verbose: {_verbose}");
        WriteDebugLine($"Quiet: {_quiet}");
    }

    public static bool IsQuiet()
    {
        return _quiet;
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

    public static void Write(string message, ConsoleColor? foregroundColor = null, bool overrideQuiet = false)
    {
        Write(message, foregroundColor, null, overrideQuiet);
    }

    public static void Write(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, bool overrideQuiet = false)
    {
        if (_quiet && !overrideQuiet) return;

        lock (_printLock)
        {
            WriteWithColorWithoutScrollSmear(message, foregroundColor, backgroundColor);
        }
    }

    public static void WriteLine(string message = "", ConsoleColor? color = null, bool overrideQuiet = false)
    {
        WriteLine(message, color, null, overrideQuiet);
    }

    public static void WriteLine(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, bool overrideQuiet = false)
    {
        if (_quiet && !overrideQuiet) return;
        Write(message + '\n', foregroundColor, backgroundColor, overrideQuiet);
    }

    public static void WriteLineIfNotEmpty(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        WriteLine(message);
    }

    public static void WriteWarning(string message)
    {
        Write(message, ConsoleColor.Black, ConsoleColor.Yellow, overrideQuiet: true);
    }

    public static void WriteWarningLine(string message)
    {
        WriteLine(message, ConsoleColor.Black, ConsoleColor.Yellow, overrideQuiet: true);
    }
    
    public static void WriteError(string message)
    {
        Write(message, ConsoleColor.White, ConsoleColor.Red, overrideQuiet: true);
    }

    public static void WriteErrorLine(string message)
    {
        WriteLine(message, ConsoleColor.White, ConsoleColor.Red, overrideQuiet: true);
    }

    public static void WriteDebug(string message)
    {
        if (!_debug) return;
        Write(message, ConsoleColor.Cyan);
    }

    public static void WriteDebugLine(string message = "")
    {
        if (!_debug) return;
        WriteLine(message, ConsoleColor.Cyan);
    }

    public static void WriteDebugHexDump(string message, string? title = null)
    {
        var noMessage = string.IsNullOrEmpty(message);
        if (noMessage)
        {
            WriteDebugLine($"{title}\n  0000: (empty)");
            return;
        }

        var i = 0;
        foreach (var ch in message)
        {
            if (i % 16 == 0)
            {
                title = title!.Replace("\r", "\\r").Replace("\n", "\\n");
                WriteDebug(i == 0 ? $"{title}\n" : "\n");
                WriteDebug(string.Format("  {0:x4}: ", i));
            }

            WriteDebug(string.Format("{0:X2} ", (int)ch));
            i++;
        }
        WriteDebugLine();
    }


    public static bool IsStandardInputReference(string fileName)
    {
        return fileName == "-" || fileName == "stdin" || fileName == "STDIN" || fileName == "STDIN:";
    }

    public static IEnumerable<string> GetAllLinesFromStdin()
    {
        return _allLinesFromStdin == null
            ? ReadAllLinesFromStdin()
            : _allLinesFromStdin;
    }

    public static ConsoleKeyInfo? ReadKey(bool intercept = false)
    {
        if (Console.IsInputRedirected)
        {
            var line = Console.ReadLine()?.TrimEnd();
            if (line == null) return null;

            var treatAsEnter = line.Length == 0 || line == "\n" || line == "\r" || line == "\r\n";
            if (treatAsEnter) return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);

            var isAlphaNumeric = char.IsLetterOrDigit(line.ElementAt(0)) || char.IsPunctuation(line.ElementAt(0)) || char.IsSymbol(line.ElementAt(0));
            if (isAlphaNumeric) return new ConsoleKeyInfo(line.ElementAt(0), (ConsoleKey)line.ElementAt(0), false, false, false);

            return line.ElementAt(0) switch
            {
                '\t' => new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false),
                ' ' => new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false),
                _ => null
            };
        }

        return Console.ReadKey(intercept);
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

    private static void WriteWithColorWithoutScrollSmear(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor)
    {
        var lines = message
            .Split(new[] { '\n' }, StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0) Console.WriteLine();
            WriteWithColor(lines[i], foregroundColor, backgroundColor);
        }
    }

    private static void WriteWithColor(string message, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor)
    {
        var prevForegroundColor = Console.ForegroundColor;
        if (foregroundColor != null) Console.ForegroundColor = ColorHelpers.MapColor((ConsoleColor)foregroundColor);

        var prevBackgroundColor = Console.BackgroundColor;
        if (backgroundColor != null) Console.BackgroundColor = ColorHelpers.MapColor((ConsoleColor)backgroundColor);

        Console.Write(message);
        if (foregroundColor != null) Console.ForegroundColor = prevForegroundColor;
        if (backgroundColor != null) Console.BackgroundColor = prevBackgroundColor;
    }
   
    private static bool _debug = false;
    private static bool _verbose = false;
    private static bool _quiet = false;

    private static object _printLock = new();
    private static int _cchLastStatus = 0;

    private static List<string>? _allLinesFromStdin;
}