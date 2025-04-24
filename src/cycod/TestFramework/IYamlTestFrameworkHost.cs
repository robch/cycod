using Microsoft.VisualStudio.TestPlatform.ObjectModel;

public interface IYamlTestFrameworkHost
{
    void RecordStart(TestCase testCase);
    void RecordResult(TestResult testResult);
    void RecordEnd(TestCase testCase, TestOutcome outcome);
}
