using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

abstract class TestBaseCommand : Command
{
    public TestBaseCommand()
    {
        Globs = new List<string>();
        ExcludeGlobs = new List<string>();
        ExcludeFileNamePatternList = new List<Regex>();
        Tests = new List<string>();
        Contains = new List<string>();
        Remove = new List<string>();
        IncludeOptionalCategories = new List<string>();
    }

    public List<string> Globs;
    public List<string> ExcludeGlobs;
    public List<Regex> ExcludeFileNamePatternList;

    public List<string> Tests { get; set; }
    public List<string> Contains { get; set; }
    public List<string> Remove { get; set; }

    public List<string> IncludeOptionalCategories { get; set; }

    public override bool IsEmpty()
    {
        return false;
    }

    override public Command Validate()
    {
        var ignoreFile = FileHelpers.FindFileSearchParents(".cycodtignore");
        if (ignoreFile != null)
        {
            FileHelpers.ReadIgnoreFile(ignoreFile, out var excludeGlobs, out var excludeFileNamePatternList);
            ExcludeGlobs.AddRange(excludeGlobs);
            ExcludeFileNamePatternList.AddRange(excludeFileNamePatternList);
        }

        return this;
    }

    protected IList<TestCase> FindAndFilterTests()
    {
        var files = FindTestFiles();
        var filters = GetTestFilters();

        var atLeastOneFileSpecified = files.Any();
        var tests = atLeastOneFileSpecified
            ? files.SelectMany(file => GetTestsFromFile(file))
            : Array.Empty<TestCase>();

        var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
        var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();

        return filtered;
    }

    protected List<FileInfo> FindTestFiles()
    {
        if (Globs.Count == 0)
        {
            var directory = YamlTestConfigHelpers.GetTestDirectory();
            var globPattern = PathHelpers.Combine(directory.FullName, "**", "*.yaml")!;
            Globs.Add(globPattern);
        }

        var files = FileHelpers
            .FindMatchingFiles(Globs, ExcludeGlobs, ExcludeFileNamePatternList)
            .Select(x => new FileInfo(x))
            .ToList();

        return files;
    }

    protected void AddFindFiles(List<FileInfo> filesAsList, string pattern)
    {
        var found = FindFiles(pattern);
        if (found.Count() == 0)
        {
            ConsoleHelpers.WriteWarningLine($"WARNING: No files found: {pattern}");
        }
        filesAsList.AddRange(found);
    }

    protected static IList<FileInfo> FindFiles(string pattern)
    {
        ConsoleHelpers.WriteDebugLine($"Finding files with pattern: {pattern}");
        var files = FileHelpers.FindFiles(Directory.GetCurrentDirectory(), pattern);
        return files.Select(x => new FileInfo(x)).ToList();
    }

    protected List<string> GetTestFilters()
    {
        var filters = new List<string>();
        
        filters.AddRange(Tests);
        foreach (var item in Contains)
        {
            filters.Add($"+{item}");
        }
        
        foreach (var item in Remove)
        {
            filters.Add($"-{item}");
        }

        return filters;
    }

    private IEnumerable<TestCase> FilterOptionalTests(IEnumerable<TestCase> tests, List<string> includeOptionalCategories)
    {
        var allTests = tests.ToList();
        
        // If we're including all optional tests, just return everything
        var includeAllOptional = includeOptionalCategories.Count == 1 && string.IsNullOrEmpty(includeOptionalCategories[0]);
        if (includeAllOptional) return allTests;

        // Determine which tests will be excluded
        var excludeAllOptional = includeOptionalCategories.Count == 0;
        var excludedTests = allTests
            .Where(test => HasOptionalTrait(test) && 
                          (excludeAllOptional || !HasMatchingOptionalCategory(test, includeOptionalCategories)))
            .ToList();

        if (excludedTests.Count > 0)
        {
            // Repair the test chain by updating nextTestCaseId and afterTestCaseId properties
            RepairTestChain(allTests, excludedTests);
        }

        // Return the filtered tests (without excluded ones)
        return allTests.Except(excludedTests);
    }

    private void RepairTestChain(List<TestCase> allTests, List<TestCase> excludedTests)
    {
        // Create a dictionary to quickly look up tests by ID
        var testsById = allTests.ToDictionary(test => test.Id.ToString());
        
        // For each excluded test
        foreach (var excludedTest in excludedTests)
        {
            string? prevTestId = YamlTestProperties.Get(excludedTest, "afterTestCaseId");
            string? nextTestId = YamlTestProperties.Get(excludedTest, "nextTestCaseId");
            
            // Skip if no connections to repair
            if (string.IsNullOrEmpty(prevTestId) && string.IsNullOrEmpty(nextTestId))
                continue;
                
            // Find previous and next non-excluded tests
            TestCase? prevTest = null;
            if (!string.IsNullOrEmpty(prevTestId) && testsById.TryGetValue(prevTestId, out var tempPrevTest))
            {
                // Only consider this previous test if it's not also being excluded
                if (!excludedTests.Contains(tempPrevTest))
                {
                    prevTest = tempPrevTest;
                }
                else
                {
                    // If the previous test is also excluded, walk backward until finding a non-excluded test
                    string? currentPrevId = prevTestId;
                    while (!string.IsNullOrEmpty(currentPrevId))
                    {
                        if (testsById.TryGetValue(currentPrevId, out var currentPrevTest) && 
                            !excludedTests.Contains(currentPrevTest))
                        {
                            prevTest = currentPrevTest;
                            break;
                        }
                        
                        // Move to the previous test in the chain
                        var prevPrevId = testsById.TryGetValue(currentPrevId, out var prevPrevTest) 
                            ? YamlTestProperties.Get(prevPrevTest, "afterTestCaseId") 
                            : null;
                            
                        if (string.IsNullOrEmpty(prevPrevId)) break;
                        currentPrevId = prevPrevId;
                    }
                }
            }
            
            TestCase? nextTest = null;
            if (!string.IsNullOrEmpty(nextTestId) && testsById.TryGetValue(nextTestId, out var tempNextTest))
            {
                // Only consider this next test if it's not also being excluded
                if (!excludedTests.Contains(tempNextTest))
                {
                    nextTest = tempNextTest;
                }
                else
                {
                    // If the next test is also excluded, walk forward until finding a non-excluded test
                    string? currentNextId = nextTestId;
                    while (!string.IsNullOrEmpty(currentNextId))
                    {
                        if (testsById.TryGetValue(currentNextId, out var currentNextTest) && 
                            !excludedTests.Contains(currentNextTest))
                        {
                            nextTest = currentNextTest;
                            break;
                        }
                        
                        // Move to the next test in the chain
                        var nextNextId = testsById.TryGetValue(currentNextId, out var nextNextTest) 
                            ? YamlTestProperties.Get(nextNextTest, "nextTestCaseId") 
                            : null;
                            
                        if (string.IsNullOrEmpty(nextNextId)) break;
                        currentNextId = nextNextId;
                    }
                }
            }
            
            // Update the connections to skip over excluded tests
            if (prevTest != null && nextTest != null)
            {
                // Connect previous test to next test
                YamlTestProperties.Set(prevTest, "nextTestCaseId", nextTest.Id.ToString());
                // Connect next test to previous test
                YamlTestProperties.Set(nextTest, "afterTestCaseId", prevTest.Id.ToString());
                
                TestLogger.Log($"Repaired test chain: {prevTest.DisplayName} -> {nextTest.DisplayName} (skipping excluded tests)");
            }
        }
    }

    private bool HasOptionalTrait(TestCase test)
    {
        return test.Traits.Any(t => t.Name == "optional");
    }

    private bool HasMatchingOptionalCategory(TestCase test, List<string> categories)
    {
        var optionalTraits = test.Traits.Where(t => t.Name == "optional").Select(t => t.Value);
        return optionalTraits.Any(value => categories.Contains(value));
    }
    
    private IEnumerable<TestCase> GetTestsFromFile(FileInfo file)
    {
        try
        {
            return YamlTestFramework.GetTestsFromYaml("cycodt", file);
        }
        catch (DuplicateTestNamesException ex)
        {
            ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}");
            ConsoleHelpers.WriteLine("\nTo fix this issue:", ConsoleColor.Yellow);
            ConsoleHelpers.WriteLine("1. Open the YAML file and ensure each test has a unique name", ConsoleColor.Yellow);
            ConsoleHelpers.WriteLine("2. For cleanup steps, consider names like 'Clean up ProgramName test log files'", ConsoleColor.Yellow);
            ConsoleHelpers.WriteLine("3. For similar tests, add context like 'Run cycodt with uppercase ProgramName'", ConsoleColor.Yellow);
            return new List<TestCase>();
        }
    }
}
