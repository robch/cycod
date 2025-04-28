public class CycoDtCommandLineOptions : CommandLineOptions
{
    public static bool Parse(string[] args, out CycoDtCommandLineOptions? options, out CommandLineException? ex)
    {
        options = new CycoDtCommandLineOptions();
        return options.Parse(args, out ex);
    }
    override protected Command? CommandFromName(string commandName)
    {
        return commandName switch
        {
            "list" => new TestListCommand(),
            "run" => new TestRunCommand(),
            _ => base.CommandFromName(commandName)
        };
    }

    override protected bool TryParseOtherCommandOptions(Command? command, string[] args, ref int i, string arg)
    {
        return TryParseTestCommandOptions(command as TestBaseCommand, args, ref i, arg);
    }

    private bool TryParseTestCommandOptions(TestBaseCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

        bool parsed = true;

        if (arg == "--file")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var filePattern = ValidateString(arg, max1Arg.FirstOrDefault(), "file pattern");
            command.Files.Add(filePattern!);
            i += max1Arg.Count();
        }
        else if (arg == "--files")
        {
            var filePatterns = GetInputOptionArgs(i + 1, args);
            var validPatterns = ValidateStrings(arg, filePatterns, "file patterns");
            command.Files.AddRange(validPatterns);
            i += filePatterns.Count();
        }
        else if (arg == "--test")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var testName = ValidateString(arg, max1Arg.FirstOrDefault(), "test name");
            command.Tests.Add(testName!);
            i += max1Arg.Count();
        }
        else if (arg == "--tests")
        {
            var testNames = GetInputOptionArgs(i + 1, args);
            var validTests = ValidateStrings(arg, testNames, "test names");
            command.Tests.AddRange(validTests);
            i += testNames.Count();
        }
        else if (arg == "--contains")
        {
            var containPatterns = GetInputOptionArgs(i + 1, args);
            var validContains = ValidateStrings(arg, containPatterns, "contains patterns");
            command.Contains.AddRange(validContains);
            i += containPatterns.Count();
        }
        else if (arg == "--remove")
        {
            var removePatterns = GetInputOptionArgs(i + 1, args);
            var validRemove = ValidateStrings(arg, removePatterns, "remove patterns");
            command.Remove.AddRange(validRemove);
            i += removePatterns.Count();
        }
        else if (command is TestRunCommand runCommand && arg == "--output-file")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var outputFile = ValidateString(arg, max1Arg.FirstOrDefault(), "output file");
            runCommand.OutputFile = outputFile;
            i += max1Arg.Count();
        }
        else if (command is TestRunCommand runCommand2 && arg == "--output-format")
        {
            var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
            var format = ValidateString(arg, max1Arg.FirstOrDefault(), "output format");
            var allowedFormats = new[] { "trx", "junit" };
            if (!allowedFormats.Contains(format))
            {
                throw new CommandLineException($"Invalid format for --output-format: {format}. Allowed values: trx, junit");
            }
            runCommand2.OutputFormat = format!;
            i += max1Arg.Count();
        }
        else
        {
            parsed = false;
        }

        return parsed;
    }
}
