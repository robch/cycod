using Microsoft.VisualStudio.TestPlatform.ObjectModel;

public class RunnableTestCase
{
    public RunnableTestCase(TestCase test) :
        this(test, MatrixFromTest(test))
    {
    }

    public RunnableTestCase(TestCase test, List<Dictionary<string, string>> matrix)
    {
        _test = test;
        _matrix = matrix;

        foreach (var matrixItem in _matrix)
        {
            _items.Add(new RunnableTestCaseItem(this, matrixItem));
        }
    }

    public TestCase Test { get { return _test; } }

    public IEnumerable<RunnableTestCaseItem> Items { get { return _items; } }

    public bool IsRunning { get { return _started && !_stopped; } }
    public bool IsFinished { get { return _started && _stopped; } }

    public void RecordStart(IYamlTestFrameworkHost host, RunnableTestCaseItem item)
    {
        TestLogger.Log($"RunnableTestCase.RecordStart({_test.DisplayName})");

        if (_stopped) throw new InvalidOperationException("Cannot start a stopped test case.");
        if (_started) return;

        host.RecordStart(_test);

        _started = true;
    }

    public void RecordResults(IYamlTestFrameworkHost host, RunnableTestCaseItem runnableTestCaseItem, IList<TestResult> results)
    {
        TestLogger.Log($"RunnableTestCase.RecordResults({_test.DisplayName})");

        if (!_started) throw new InvalidOperationException("Cannot record results for a test case that has not been started.");
        if (_stopped) throw new InvalidOperationException("Cannot record results for a test case that has been stopped.");

        foreach (var result in results)
        {
            host.RecordResult(result);
        }

        _resultsRecorded.Add(results);
    }

    public void RecordStop(IYamlTestFrameworkHost host, RunnableTestCaseItem runnableTestCaseItem)
    {
        TestLogger.Log($"RunnableTestCase.RecordStop({_test.DisplayName})");

        if (!_started) throw new InvalidOperationException("Cannot stop a test case that has not been started.");
        if (_stopped) throw new InvalidOperationException("Cannot stop a test case that has already been stopped.");

        var countRecorded = _resultsRecorded.Count;
        var countExpected = _items.Count;
        if (countRecorded != countExpected)
        {
            TestLogger.Log($"RunnableTestCase.RecordStop({_test.DisplayName}): Expected {countExpected} items; recorded {countRecorded} items.");
            return;
        }

        TestLogger.Log($"RunnableTestCase.RecordStop({_test.DisplayName}): All items recorded ({countRecorded}); recording end.");
        var outcome = TestResultHelpers.TestOutcomeFromResults(_resultsRecorded.SelectMany(x => x));
        host.RecordEnd(_test, outcome);
        _stopped = true;
    }

    public IEnumerable<TestResult> Results { get { return _resultsRecorded.SelectMany(x => x); } }

    private static List<Dictionary<string, string>> MatrixFromTest(TestCase test)
    {
        var matrix = YamlTestProperties.Get(test, "matrix");
        TestLogger.Log($"RunnableTestCase.MatrixFromTest({test.DisplayName}): {matrix}");
        return !string.IsNullOrEmpty(matrix)
            ? JsonHelpers.FromJsonArrayText(matrix!)
            : new List<Dictionary<string, string>>();
    }

    private TestCase _test;
    private List<Dictionary<string, string>> _matrix;
    private List<RunnableTestCaseItem> _items = new();
    private List<IList<TestResult>> _resultsRecorded = new();
    private bool _started = false;
    private bool _stopped = false;
}
