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
    }

    public List<string> Files { get; set; }
    public List<string> Tests { get; set; }
    public List<string> Contains { get; set; }
    public List<string> Remove { get; set; }

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

        var filtered = YamlTestCaseFilter.FilterTestCases(tests, filters).ToList();

        if (tests.Count() == 0)
        {
            throw new Exception(!atLeastOneFileSpecified
                ? "No tests found"
                : files.Count() == 1
                    ? $"No tests found in {files.Count()} file"
                    : $"No tests found in {files.Count()} files");
        }
        
        if (filtered.Count() == 0)
        {
            Console.WriteLine(atLeastOneFileSpecified
                ? $"Found {tests.Count()} tests in {files.Count()} files\n"
                : $"Found {tests.Count()} tests\n");

            throw new Exception("No tests matching criteria.");
        }

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
}