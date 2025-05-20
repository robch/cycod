using Microsoft.VisualStudio.TestPlatform.ObjectModel;

public class YamlTestProperties
{
    public static void Set(TestCase test, string name, string value)
    {
        Logger.Log($"YamlTestProperties.Set('{name}'='{value.Replace("\n", "\\n")}')");
        if (!string.IsNullOrEmpty(value))
        {
            var property = properties[name];
            test.SetPropertyValue(property, value);
        }
    }

    public static string? Get(TestCase test, string name, string? defaultValue = null)
    {
        var value = test.GetPropertyValue(properties[name], defaultValue);
        Logger.LogIf(!string.IsNullOrEmpty(value), $"TestCaseProperties.Get('{name}') = '{value?.Replace("\n", "\\n")}'");
        return value;
    }

    #region private methods and data
    private static TestProperty RegisterTestCaseProperty(string name)
    {
        return TestProperty.Register($"YamlTestCase.{name}", name, typeof(string), TestPropertyAttributes.Hidden, typeof(TestCase));
    }

    private static readonly Dictionary<string, TestProperty> properties = new Dictionary<string, TestProperty>() {
        { "cli", RegisterTestCaseProperty("CLI") },
        { "run", RegisterTestCaseProperty("Run") },
        { "script", RegisterTestCaseProperty("Script") },
        { "shell", RegisterTestCaseProperty("Shell") },
        { "bash", RegisterTestCaseProperty("Bash") },
        { "cmd", RegisterTestCaseProperty("Cmd") },
        { "powershell", RegisterTestCaseProperty("Powershell") },
        { "pwsh", RegisterTestCaseProperty("Pwsh") },
        { "parallelize", RegisterTestCaseProperty("Parallelize") },
        { "nextTestCaseId", RegisterTestCaseProperty("nextTestCaseId") },
        { "afterTestCaseId", RegisterTestCaseProperty("afterTestCaseId") },
        { "matrix", RegisterTestCaseProperty("Matrix") },
        { "foreach", RegisterTestCaseProperty("ForEach") },
        { "arguments", RegisterTestCaseProperty("Arguments") },
        { "input", RegisterTestCaseProperty("Input")},
        { "expect", RegisterTestCaseProperty("ExpectGpt") },
        { "expect-regex", RegisterTestCaseProperty("Expect") },
        { "not-expect-regex", RegisterTestCaseProperty("NotExpect") },
        { "expect-exit-code", RegisterTestCaseProperty("ExpectExitCode") },
        { "skipOnFailure", RegisterTestCaseProperty("SkipOnFailure") },
        { "timeout", RegisterTestCaseProperty("Timeout") },
        { "env", RegisterTestCaseProperty("Env") },
        { "working-directory", RegisterTestCaseProperty("WorkingDirectory") }
    };

    #endregion
}
