using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class CommandLineException : Exception
{
    public CommandLineException() : base()
    {
    }

    public CommandLineException(string message, string? helpTopic = null) : base(message)
    {
        _helpTopic = helpTopic;
    }

    public string? GetHelpTopic()
    {
        return _helpTopic;
    }

    private string? _helpTopic;
}
