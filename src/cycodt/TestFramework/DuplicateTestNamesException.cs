using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

/// <summary>
/// Exception thrown when duplicate test names are detected in a YAML file.
/// </summary>
public class DuplicateTestNamesException : InvalidOperationException
{
    /// <summary>
    /// Information about a duplicate test name.
    /// </summary>
    public class DuplicateInfo
    {
        /// <summary>
        /// The duplicate test name.
        /// </summary>
        public string TestName { get; }
        
        /// <summary>
        /// The first test case with this name.
        /// </summary>
        public TestCase Original { get; }
        
        /// <summary>
        /// The duplicate test case with the same name.
        /// </summary>
        public TestCase Duplicate { get; }
        
        public DuplicateInfo(string testName, TestCase original, TestCase duplicate)
        {
            TestName = testName;
            Original = original;
            Duplicate = duplicate;
        }
    }

    /// <summary>
    /// List of all duplicate test names found.
    /// </summary>
    public IReadOnlyList<DuplicateInfo> Duplicates { get; }
    
    /// <summary>
    /// The YAML file path where duplicates were found.
    /// </summary>
    public string FilePath { get; }

    public DuplicateTestNamesException(string message, string filePath, IEnumerable<DuplicateInfo> duplicates) 
        : base(message)
    {
        FilePath = filePath;
        Duplicates = new List<DuplicateInfo>(duplicates).AsReadOnly();
    }
}