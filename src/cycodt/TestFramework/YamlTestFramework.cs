using Microsoft.VisualStudio.TestPlatform.ObjectModel;

public class YamlTestFramework
{
    public static IEnumerable<TestCase> GetTestsFromYaml(string source, FileInfo file)
    {
        TestLogger.Log($"YamlTestFramework.GetTestsFromYaml('{source}', '{file.FullName}'): ENTER");
        Logger.Info($"YamlTestFramework.GetTestsFromYaml: PARSING file: {file.FullName}");
        
        var tests = YamlTestCaseParser.TestCasesFromYaml(source, file);
        var testsList = tests.ToList(); // Force enumeration to get count
        
        Logger.Info($"YamlTestFramework.GetTestsFromYaml: PARSED {testsList.Count} tests from {file.FullName}");
        
        // Log each TestCase created from this file
        for (int i = 0; i < testsList.Count; i++)
        {
            Logger.Verbose($"YamlTestFramework.GetTestsFromYaml: File={file.Name} TestCase[{i}] ID={testsList[i].Id}, Name={testsList[i].DisplayName}");
        }

        TestLogger.Log($"YamlTestFramework.GetTestsFromYaml('{source}', '{file.FullName}'): EXIT");
        return testsList;
    }

    public static IDictionary<string, IList<TestResult>> RunTests(IEnumerable<TestCase> tests, IYamlTestFrameworkHost host)
    {
        // Keep test run start/end at Info, demote intermediate steps to Debug
        TestLogger.Log($"YamlTestFramework.RunTests(): ENTER (test count: {tests.Count()})");
        Logger.Info($"Test run started: {tests.Count()} tests");

        tests = tests.ToList(); // force enumeration
        ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: About to create RunnableTestCase objects for {tests.Count()} tests");
        
        // Log each incoming TestCase at Debug level
        var testIndex = 0;
        foreach (var test in tests)
        {
            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: Input[{testIndex}] TestCase ID: {test.Id}, Name: {test.DisplayName}");
            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: Input[{testIndex}] Source: File: {test.CodeFilePath ?? "null"}, FullyQualifiedName: {test.FullyQualifiedName ?? "null"}");
            testIndex++;
        }
        
        var runnableTests = tests.Select(test => new RunnableTestCase(test)).ToList();
        
        ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: Created {runnableTests.Count} RunnableTestCase objects");
        for (int i = 0; i < runnableTests.Count; i++)
        {
            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunTests: RunnableTest[{i}] TestCase ID: {runnableTests[i].Test.Id}, Name: {runnableTests[i].Test.DisplayName}, Items: {runnableTests[i].Items.Count()}");
        }

        var runnableTestItems = runnableTests.SelectMany(x => x.Items).ToList();
        var groups = GetPriorityGroups(runnableTestItems);

        var resultsByTestCaseIdMap = InitResultsByTestCaseIdMap(tests);
        foreach (var group in groups)
        {
            if (group.Count == 0) continue;

            var resultsByTestCaseIdForGroup = RunAndRecordTests(host, group);
            foreach (var resultsForTestCase in resultsByTestCaseIdForGroup)
            {
                var testCaseId = resultsForTestCase.Key;
                var testResults = resultsForTestCase.Value;
                foreach (var result in testResults)
                {
                    resultsByTestCaseIdMap[testCaseId].Add(result);
                }
            }
        }

        // Count pass/fail results for the summary
        int passed = 0, failed = 0, skipped = 0;
        foreach (var testResults in resultsByTestCaseIdMap.Values)
        {
            foreach (var result in testResults)
            {
                switch (result.Outcome)
                {
                    case TestOutcome.Passed: passed++; break;
                    case TestOutcome.Failed: failed++; break;
                    case TestOutcome.Skipped: skipped++; break;
                }
            }
        }
        
        // Log test run completion at Info level
        Logger.Info($"Test run completed: {passed} passed, {failed} failed, {skipped} skipped");
        TestLogger.Log($"YamlTestFramework.RunTests(): EXIT");
        return resultsByTestCaseIdMap;
    }

    #region private methods

    private static Dictionary<string, IList<TestResult>> InitResultsByTestCaseIdMap(IEnumerable<TestCase> tests)
    {
        var testIds = tests
            .Select(test => test.Id.ToString())
            .Distinct()
            .ToList();

        var resultsMap = new Dictionary<string, IList<TestResult>>();
        foreach (var id in testIds)
        {
            resultsMap[id] = new List<TestResult>();
        }

        return resultsMap;
    }

    private static IDictionary<string, IList<TestResult>> RunAndRecordTests(IYamlTestFrameworkHost host, IEnumerable<RunnableTestCaseItem> items)
    {
        InitFromItemIdMaps(items, out var itemFromItemIdMap, out var itemCompletionFromItemIdMap);

        RunAndRecordRunnableTestCaseItems(host, itemFromItemIdMap, itemCompletionFromItemIdMap, onlyWithNextSteps: true, onlyParallel: true);
        RunAndRecordRunnableTestCaseItems(host, itemFromItemIdMap, itemCompletionFromItemIdMap, onlyWithNextSteps: true, onlyParallel: false);
        RunAndRecordRunnableTestCaseItems(host, itemFromItemIdMap, itemCompletionFromItemIdMap, onlyWithNextSteps: false, onlyParallel: true);
        RunAndRecordRunnableTestCaseItems(host, itemFromItemIdMap, itemCompletionFromItemIdMap, onlyWithNextSteps: false, onlyParallel: false);

        return GetTestResultsByTestId(itemCompletionFromItemIdMap);
    }

    private static IDictionary<string, IList<TestResult>> GetTestResultsByTestId(Dictionary<string, TaskCompletionSource<IList<TestResult>>> itemCompletionFromItemIdMap)
    {
        var results = itemCompletionFromItemIdMap
            .Select(x => x.Value.Task.Result)
            .SelectMany(x => x)
            .ToList();
        var resultsByTestId = InitResultsByTestCaseIdMap(results
            .Select(x => x.TestCase)
            .Distinct()
            .ToList());
        foreach (var result in results)
        {
            var testCaseId = result.TestCase.Id.ToString();
            resultsByTestId[testCaseId].Add(result);
        }

        return resultsByTestId;
    }

    private static void InitFromItemIdMaps(IEnumerable<RunnableTestCaseItem> items, out Dictionary<string, RunnableTestCaseItem> itemFromItemIdMap, out Dictionary<string, TaskCompletionSource<IList<TestResult>>> itemCompletionFromItemIdMap)
    {
        itemFromItemIdMap = new Dictionary<string, RunnableTestCaseItem>();
        itemCompletionFromItemIdMap = new Dictionary<string, TaskCompletionSource<IList<TestResult>>>();
        foreach (var item in items)
        {
            var itemId = item.Id;
            itemFromItemIdMap[itemId] = item;
            itemCompletionFromItemIdMap[itemId] = new TaskCompletionSource<IList<TestResult>>();
        }
    }

    private static void RunAndRecordRunnableTestCaseItems(IYamlTestFrameworkHost host, Dictionary<string, RunnableTestCaseItem> itemFromItemIdMap, Dictionary<string, TaskCompletionSource<IList<TestResult>>> itemCompletionFromItemIdMap, bool onlyWithNextSteps = false, bool onlyParallel = false)
    {
        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItems(): ENTER (onlyWithNextSteps: {onlyWithNextSteps}, onlyParallel: {onlyParallel})");

        var items = itemCompletionFromItemIdMap
            .Where(kvp => kvp.Value.Task.Status < TaskStatus.RanToCompletion)
            .Select(kvp => itemFromItemIdMap[kvp.Key])
            .ToList();

        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItems(): PRE-FILTER: item count: {items.Count}");

        if (onlyWithNextSteps)
        {
            items = items
                .Where(item => !string.IsNullOrEmpty(YamlTestProperties.Get(item.RunnableTest.Test, "nextTestCaseId")))
                .Where(item => string.IsNullOrEmpty(YamlTestProperties.Get(item.RunnableTest.Test, "afterTestCaseId")))
                .ToList();
        }

        if (onlyParallel)
        {
            items = items
                .Where(item => YamlTestProperties.Get(item.RunnableTest.Test, "parallelize") == "true")
                .ToList();
        }

        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItems(): POST-FILTER: item count: {items.Count}");

        if (onlyParallel)
        {
            RunAndRecordRunnableTestCaseItemsInParallel(host, itemFromItemIdMap, itemCompletionFromItemIdMap, items);
        }
        else
        {
            RunAndRecordRunnableTestCaseItemsSequentially(host, itemFromItemIdMap, itemCompletionFromItemIdMap, items);
        }

        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItems(): EXIT");
    }

    private static void RunAndRecordRunnableTestCaseItemsSequentially(IYamlTestFrameworkHost host, Dictionary<string, RunnableTestCaseItem> itemFromItemIdMap, Dictionary<string, TaskCompletionSource<IList<TestResult>>> itemCompletionFromItemIdMap, List<RunnableTestCaseItem> items)
    {
        foreach (var item in items)
        {
            var id = item.Id;
            RunAndRecordRunnableTestCaseItemsStepByStep(host, itemFromItemIdMap, itemCompletionFromItemIdMap, id);
        }
    }

    private static void RunAndRecordRunnableTestCaseItemsInParallel(IYamlTestFrameworkHost host, Dictionary<string, RunnableTestCaseItem> itemFromItemIdMap, Dictionary<string, TaskCompletionSource<IList<TestResult>>> itemCompletionFromItemIdMap, List<RunnableTestCaseItem> items)
    {
        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsInParallel() ==> Running {items.Count} tests in parallel");

        foreach (var item in items)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                RunAndRecordRunnableTestCaseItemsStepByStep(host, itemFromItemIdMap, itemCompletionFromItemIdMap, item.Id);
            });
        }

        var parallelCompletionTasks = itemCompletionFromItemIdMap
            .Where(kvp => items.Any(item => item.Id == kvp.Key))
            .Select(kvp => kvp.Value.Task);
        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsInParallel() ==> Waiting for {parallelCompletionTasks.Count()} parallel tests to complete");
        
        Task.WaitAll(parallelCompletionTasks.ToArray());
        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsInParallel() ==> All parallel tests complete");
    }

    private static void RunAndRecordRunnableTestCaseItemsStepByStep(IYamlTestFrameworkHost host, Dictionary<string, RunnableTestCaseItem> itemFromItemIdMap, Dictionary<string, TaskCompletionSource<IList<TestResult>>> itemCompletionFromItemIdMap, string firstStepItemId)
    {
        var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - STARTING with firstStepItemId: {firstStepItemId}");
        
        var firstItem = itemFromItemIdMap[firstStepItemId];
        ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - About to execute FIRST item: {firstStepItemId}, TestCase: {firstItem.RunnableTest.Test.DisplayName}");
        
        var firstTestResults = RunAndRecordRunnableTestCaseItem(firstItem, host);
        var firstTestOutcome = TestResultHelpers.TestOutcomeFromResults(firstTestResults);
        
        ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - FIRST item completed: {firstStepItemId}, TestCase: {firstItem.RunnableTest.Test.DisplayName}, Outcome: {firstTestOutcome}");

        // defer setting completion until all steps are complete

        var checkItem = firstItem;
        while (true)
        {
            var nextItem = GetNextRunnableTestCaseItem(itemFromItemIdMap, checkItem);
            if (nextItem == null)
            {
                ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - No next runnable item for {checkItem.RunnableTest.Test.DisplayName}");
                break;
            }
            var nextItemId = nextItem!.Id;
            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - Found NEXT item: {nextItemId}, TestCase: {nextItem.RunnableTest.Test.DisplayName}");
            
            var itemCompletion = itemCompletionFromItemIdMap.ContainsKey(nextItemId) ? itemCompletionFromItemIdMap[nextItemId] : null;
            if (itemCompletion == null)
            {
                ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - nextItemId '{nextItemId}' completion not found for test '{checkItem.RunnableTest.Test.DisplayName}'");
                break;
            }

            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - About to execute NEXT item: {nextItemId}, TestCase: {nextItem.RunnableTest.Test.DisplayName}");
            var itemResults = RunAndRecordRunnableTestCaseItem(nextItem, host);
            var itemOutcome = TestResultHelpers.TestOutcomeFromResults(itemResults);
            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - NEXT item completed: {nextItemId}, TestCase: {nextItem.RunnableTest.Test.DisplayName}, Outcome: {itemOutcome}");
            
            // Defensive programming: check if task is already completed
            if (!itemCompletion.Task.IsCompleted)
            {
                itemCompletion.SetResult(itemResults);
            }
            else
            {
                ConsoleHelpers.WriteWarningLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - Task already completed for {nextItem.RunnableTest.Test.DisplayName}, skipping SetResult");
            }

            checkItem = nextItem;
        }

        // now that all steps are complete, set the completion outcome
        var firstStepCompletion = itemCompletionFromItemIdMap[firstStepItemId];
        if (!firstStepCompletion.Task.IsCompleted)
        {
            firstStepCompletion.SetResult(firstTestResults);
            ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - Setting completion for FIRST item: {firstItem.RunnableTest.Test.DisplayName}, Outcome: {firstTestOutcome}");
        }
        else
        {
            ConsoleHelpers.WriteWarningLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - First step task already completed for {firstItem.RunnableTest.Test.DisplayName}, skipping SetResult");
        }
        
        ConsoleHelpers.WriteDebugLine($"YamlTestFramework.RunAndRecordRunnableTestCaseItemsStepByStep() - Thread: {threadId} - COMPLETED step-by-step execution chain starting with: {firstStepItemId}");
    }

    private static RunnableTestCaseItem? GetNextRunnableTestCaseItem(Dictionary<string, RunnableTestCaseItem> itemFromItemIdMap, RunnableTestCaseItem current)
    {
        var test = current.RunnableTest.Test;
        var nextTestCaseId = YamlTestProperties.Get(test, "nextTestCaseId");
        if (string.IsNullOrEmpty(nextTestCaseId))
        {
            TestLogger.Log($"YamlTestFramework.GetNextRunnableTestCaseItem() ==> No nextTestCaseId for {test.Id}");
            return null;
        }

        var matrixId = current.MatrixId;
        if (string.IsNullOrEmpty(matrixId))
        {
            TestLogger.Log($"YamlTestFramework.GetNextRunnableTestCaseItem() ==> matrixId not found for {test.Id}");
            return null;
        }

        var nextItemId = RunnableTestCaseItem.ItemIdFromIds(nextTestCaseId, matrixId);
        if (!itemFromItemIdMap.ContainsKey(nextItemId))
        {
            TestLogger.Log($"YamlTestFramework.GetNextRunnableTestCaseItem() ==> nextItemId '{nextItemId}' not found for {test.Id}");
            return null;
        }

        return itemFromItemIdMap[nextItemId];
    }

    private static bool IsTrait(Trait trait, string check)
    {
        return trait.Name == check || trait.Value == check;
    }

    private static List<List<RunnableTestCaseItem>> GetPriorityGroups(IEnumerable<RunnableTestCaseItem> items)
    {
        TestLogger.Log($"YamlTestFramework.GetPriorityGroups(): items count: {items.Count()}");

        var before = items.Where(item => item.RunnableTest.Test.Traits.Count(x => IsTrait(x, "before")) > 0);
        var after = items.Where(item => item.RunnableTest.Test.Traits.Count(x => IsTrait(x, "after")) > 0);
        var middle = items.Where(item => !before.Contains(item) && !after.Contains(item));

        var itemsList = new List<List<RunnableTestCaseItem>>();
        itemsList.Add(before.ToList());
        itemsList.Add(middle.ToList());
        itemsList.Add(after.ToList());

        return itemsList;
    }

    private static IList<TestResult> RunAndRecordRunnableTestCaseItem(RunnableTestCaseItem item, IYamlTestFrameworkHost host)
    {
        TestLogger.Log($"YamlTestFramework.RunAndRecordRunnableTestCaseItem({item.RunnableTest.Test.DisplayName})");
        return item.RunAndRecord(host);
    }

    #endregion

    #region constants
    public const string YamlFileExtension = ".yaml";
    public const string FakeExecutor = "executor://cycodt/cli/TestFramework/v1";
    public readonly static string YamlDefaultTagsFileName = $"cycodt-tags.yaml";
    public const string YamlTestsConfigDirectoryName = "tests";
    public const string YamlTestsConfigFileName = "cycodt-config.yaml";
    public const string DefaultTimeout = "600000";
    #endregion
}
