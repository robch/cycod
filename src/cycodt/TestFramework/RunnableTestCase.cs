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
        
        // Log when a RunnableTestCase is created with detailed info
        Logger.Info($"TestCase-{_test.Id}: CREATED - {_test.DisplayName} with {_items.Count} items");
        Logger.Info($"TestCase-{_test.Id}: SOURCE - File: {_test.CodeFilePath ?? "null"}, FullyQualifiedName: {_test.FullyQualifiedName ?? "null"}");
        for (int i = 0; i < _items.Count; i++)
        {
            Logger.Info($"TestCase-{_test.Id}: Item[{i}] ID: {_items[i].Id}");
        }
    }

    public TestCase Test { get { return _test; } }

    public IEnumerable<RunnableTestCaseItem> Items { get { return _items; } }

    public bool IsRunning 
    { 
        get 
        { 
            lock (_stateLock)
            {
                return _started && !_stopped; 
            }
        } 
    }
    
    public bool IsFinished 
    { 
        get 
        { 
            lock (_stateLock)
            {
                return _started && _stopped; 
            }
        } 
    }

    public void RecordStart(IYamlTestFrameworkHost host, RunnableTestCaseItem item)
    {
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        Logger.Info($"TestCase-{_test.Id}: START_REQUESTED - Thread: {threadId}, ItemId: {item?.Id ?? "null"} - {_test.DisplayName}");

        lock (_stateLock)
        {
            Logger.Info($"TestCase-{_test.Id}: INSIDE_LOCK - Thread: {threadId} - Started: {_started}, Stopped: {_stopped}, Active: {_activeItemCount}, Expected items: {_items.Count}, Recorded results: {_resultsRecorded.Count}");
            
            if (_stopped) 
            {
                Logger.Error($"TestCase-{_test.Id}: ERROR_ALREADY_STOPPED - Thread: {threadId} - Cannot start stopped test case! Expected items: {_items.Count}, Recorded results: {_resultsRecorded.Count}");
                Logger.Error($"TestCase-{_test.Id}: ERROR_ITEMS - Thread: {threadId}");
                for (int i = 0; i < _items.Count; i++)
                {
                    Logger.Error($"TestCase-{_test.Id}: ERROR_Item[{i}]: {_items[i].Id}");
                }
                throw new InvalidOperationException("Cannot start a stopped test case.");
            }
            
            _activeItemCount++;
            
            if (_started) 
            {
                Logger.Info($"TestCase-{_test.Id}: ALREADY_STARTED - Thread: {threadId} - returning early (Active count now: {_activeItemCount})");
                return;
            }

            Logger.Info($"TestCase-{_test.Id}: CALLING_HOST_RECORDSTART - Thread: {threadId}");
            host.RecordStart(_test);

            _started = true;
            Logger.Info($"TestCase-{_test.Id}: STARTED_SUCCESSFULLY - Thread: {threadId} - Active count: {_activeItemCount}");
        }
    }

    public void RecordResults(IYamlTestFrameworkHost host, RunnableTestCaseItem runnableTestCaseItem, IList<TestResult> results)
    {
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        TestLogger.Log($"RunnableTestCase.RecordResults({_test.DisplayName}) - Thread: {threadId}, ItemId: {runnableTestCaseItem?.Id ?? "null"}, Results count: {results.Count}");

        lock (_stateLock)
        {
            TestLogger.Log($"RunnableTestCase.RecordResults({_test.DisplayName}) - Thread: {threadId} - INSIDE LOCK - Started: {_started}, Stopped: {_stopped}, Active: {_activeItemCount}, Expected items: {_items.Count}, Recorded results: {_resultsRecorded.Count}");

            if (!_started) throw new InvalidOperationException("Cannot record results for a test case that has not been started.");
            if (_stopped) throw new InvalidOperationException("Cannot record results for a test case that has been stopped.");

            foreach (var result in results)
            {
                host.RecordResult(result);
            }

            _resultsRecorded.Add(results);
            TestLogger.Log($"RunnableTestCase.RecordResults({_test.DisplayName}) - Thread: {threadId} - Results recorded successfully. Total recorded batches: {_resultsRecorded.Count}");
        }
    }

    public void RecordStop(IYamlTestFrameworkHost host, RunnableTestCaseItem runnableTestCaseItem)
    {
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        Logger.Info($"TestCase-{_test.Id}: STOP_REQUESTED - Thread: {threadId}, ItemId: {runnableTestCaseItem?.Id ?? "null"} - {_test.DisplayName}");

        lock (_stateLock)
        {
            Logger.Info($"TestCase-{_test.Id}: STOP_INSIDE_LOCK - Thread: {threadId} - Started: {_started}, Stopped: {_stopped}, Active: {_activeItemCount}, Expected items: {_items.Count}, Recorded results: {_resultsRecorded.Count}");
            
            if (!_started) throw new InvalidOperationException("Cannot stop a test case that has not been started.");
            if (_stopped) 
            {
                Logger.Info($"TestCase-{_test.Id}: ALREADY_STOPPED - Thread: {threadId} - ignoring");
                return; // Already stopped, nothing to do
            }

            _activeItemCount--; // Decrement active count
            Logger.Info($"TestCase-{_test.Id}: ACTIVE_COUNT_DECREMENTED - Thread: {threadId} - to: {_activeItemCount}");
            
            var countRecorded = _resultsRecorded.Count;
            var countExpected = _items.Count;
            
            Logger.Info($"TestCase-{_test.Id}: CHECKING_COMPLETION - Thread: {threadId} - Results: {countRecorded}/{countExpected}, Active: {_activeItemCount}");
            
            if (countRecorded != countExpected)
            {
                Logger.Info($"TestCase-{_test.Id}: NOT_READY_RESULTS - Thread: {threadId} - Expected {countExpected} items; recorded {countRecorded} items - not ready to stop yet.");
                return;
            }

            if (_activeItemCount > 0)
            {
                Logger.Info($"TestCase-{_test.Id}: NOT_READY_ACTIVE - Thread: {threadId} - Still have {_activeItemCount} active items - not ready to stop yet.");
                return;
            }

            // We're the last item - stop the test case
            Logger.Info($"TestCase-{_test.Id}: READY_TO_STOP - Thread: {threadId} - All items recorded ({countRecorded}) and no active items; recording end.");
            var outcome = TestResultHelpers.TestOutcomeFromResults(_resultsRecorded.SelectMany(x => x));
            host.RecordEnd(_test, outcome);
            _stopped = true;
            Logger.Info($"TestCase-{_test.Id}: STOPPED_SUCCESSFULLY - Thread: {threadId}");
        }
    }

    public IEnumerable<TestResult> Results 
    { 
        get 
        { 
            lock (_stateLock)
            {
                return _resultsRecorded.SelectMany(x => x).ToList(); // Return a copy to avoid modification
            }
        } 
    }

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
    private readonly object _stateLock = new object(); // Thread synchronization lock
    private int _activeItemCount = 0; // Track how many items are currently executing
}
