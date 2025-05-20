using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

public class RunnableTestCaseItem
{
    public RunnableTestCaseItem(RunnableTestCase runnableTest, Dictionary<string, string>? properties = null)
    {
        var matrixId = properties != null ? YamlTestCaseMatrixHelpers.GetMatrixId(properties) : null;
        _matrixId = matrixId ?? Guid.NewGuid().ToString();

        var testId = runnableTest.Test.Id.ToString();
        _id = ItemIdFromIds(testId, _matrixId);

        _runnableTest = runnableTest;
        _properties = properties;

        _cli = GetInterpolatedProperty("cli") ?? "";
        _runProcess = GetInterpolatedProperty("run");

        _script = GetInterpolatedProperty("script");
        _shell = GetInterpolatedProperty("shell");

        var bash = GetInterpolatedProperty("bash");
        var cmd = GetInterpolatedProperty("cmd");
        var powershell = GetInterpolatedProperty("powershell");
        var pwsh = GetInterpolatedProperty("pwsh");
        
        if (!string.IsNullOrEmpty(bash))
        {
            _script = bash;
            _shell = "bash";
        }
        else if (!string.IsNullOrEmpty(cmd))
        {
            _script = cmd;
            _shell = "cmd";
        }
        else if (!string.IsNullOrEmpty(powershell))
        {
            _script = powershell;
            _shell = "powershell";
        }
        else if (!string.IsNullOrEmpty(pwsh))
        {
            _script = pwsh;
            _shell = "pwsh";
        }
        
        if (string.IsNullOrEmpty(_shell))
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            _shell = isWindows ? "cmd" : "bash";
        }

        _arguments = GetInterpolatedProperty("arguments", escapeJson: true);
        _input = GetInterpolatedProperty("input");
        if (_input != null && _input.StartsWith('"') && _input.EndsWith('"')) _input = _input[1..^1];

        _expectGpt = GetInterpolatedProperty("expect");
        _expectRegex = GetInterpolatedProperty("expect-regex");
        _notExpectRegex = GetInterpolatedProperty("not-expect-regex");
        _expectExitCode = int.Parse(GetInterpolatedProperty("expect-exit-code", "0")!);

        _env = GetInterpolatedProperty("env");
        _workingDirectory = GetInterpolatedProperty("working-directory");

        _timeout = int.Parse(GetInterpolatedProperty("timeout", YamlTestFramework.DefaultTimeout)!);
        _skipOnFailure = GetInterpolatedProperty("skipOnFailure") switch { "true" => true, _ => false };

        var basePath = new FileInfo(_runnableTest.Test.CodeFilePath).DirectoryName!;
        _workingDirectory = Path.Combine(basePath, _workingDirectory ?? "");
        var tryCreateWorkingDirectory = !string.IsNullOrEmpty(_workingDirectory) && !Directory.Exists(_workingDirectory);
        if (tryCreateWorkingDirectory) Directory.CreateDirectory(_workingDirectory!);

        _foreach = GetInterpolatedProperty("foreach", escapeJson: true);
    }

    public string Id { get { return _id; } }
    public string MatrixId { get { return _matrixId; } }

    public static string ItemIdFromIds(string testId, string matrixId)
    {
        return $"{testId}.{matrixId}";
    }

    public RunnableTestCase RunnableTest { get { return _runnableTest; } }

    public IList<TestResult> RunAndRecord(IYamlTestFrameworkHost host)
    {
        _runnableTest.RecordStart(host, this);

        // run the test case, getting all the results, prior to recording any of those results
        // (not doing this in this order seems to, for some reason, cause "foreach" test cases to run 5 times!?)
        var results = YamlTestCaseRunner.TestCaseGetResults(_runnableTest.Test, _cli, _runProcess, _script, _shell, _arguments, _input, _expectGpt, _expectRegex, _notExpectRegex, _env, _workingDirectory, _timeout, _expectExitCode, _skipOnFailure, _foreach);
        _results = results.ToList();

        _runnableTest.RecordResults(host, this, _results);
        _runnableTest.RecordStop(host, this);

        return _results;
    }

    private string? GetInterpolatedProperty(string key, string? defaultValue = null, bool escapeJson = false)
    {
        var value = YamlTestProperties.Get(_runnableTest.Test, key, defaultValue);
        return Interpolate(value, escapeJson);
    }

    public string? Interpolate(string? text, bool escapeJson = false)
    {
        return _properties != null
            ? PropertyInterpolationHelpers.Interpolate(text!, _properties, escapeJson)
            : text;
    }

    private string _id;
    private string _matrixId;
    private RunnableTestCase _runnableTest;
    private Dictionary<string, string>? _properties;

    private string _cli;
    private string? _runProcess;
    private string? _script;
    private string? _shell;
    private string? _arguments;
    private string? _input;
    private string? _expectGpt;
    private string? _expectRegex;
    private string? _notExpectRegex;
    private int _expectExitCode;
    private string? _env;
    private string? _workingDirectory;
    private int _timeout;
    private bool _skipOnFailure;
    private string? _foreach;

    private List<TestResult>? _results;
}
