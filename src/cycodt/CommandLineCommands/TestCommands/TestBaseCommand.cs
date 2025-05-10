using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

abstract class TestBaseCommand : Command
{
    public TestBaseCommand()
    {
        Files = new List<string>();
        Tests = new List<string>();
        Contains = new List<string>();
        Remove = new List<string>();
        IncludeOptionalCategories = new List<string>();
    }

    public List<string> Files { get; set; }
    public List<string> Tests { get; set; }
    public List<string> Contains { get; set; }
    public List<string> Remove { get; set; }

    public List<string> IncludeOptionalCategories { get; set; }

    public override bool IsEmpty()
    {
        return false;
    }

    protected IList<TestCase> FindAndFilterTests()
    {
        var files = FindTestFiles();
        var filters = GetTestFilters();

        var atLeastOneFileSpecified = files.Any();
        var tests = atLeastOneFileSpecified
            ? files.SelectMany(file => YamlTestFramework.GetTestsFromYaml("cycodt", file))
            : YamlTestFramework.GetTestsFromDirectory("cycodt", new DirectoryInfo("."));

        var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
        var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();

        return filtered;
    }

    protected List<FileInfo> FindTestFiles()
    {
        var files = new List<FileInfo>();
        foreach (var pattern in Files)
        {
            AddFindFiles(files, pattern);
        }
        return files;
    }

    protected void AddFindFiles(List<FileInfo> filesAsList, string pattern)
    {
        var found = FindFiles(pattern);
        if (found.Count() == 0)
        {
            Console.WriteLine($"WARNING: No files found: {pattern}");
        }
        filesAsList.AddRange(found);
    }

    protected static IList<FileInfo> FindFiles(string pattern)
    {
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
        var excludeAllOptional = includeOptionalCategories.Count == 0;
        if (excludeAllOptional) return tests.Where(test => !HasOptionalTrait(test));

        var includeAllOptional = includeOptionalCategories.Count == 1 && string.IsNullOrEmpty(includeOptionalCategories[0]);
        if (includeAllOptional) return tests;

        return tests.Where(test => !HasOptionalTrait(test) || HasMatchingOptionalCategory(test, includeOptionalCategories));
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
}
