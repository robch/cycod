using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[TestClass]
public class LineHelpersTests
{
    #region IsLineMatch Tests

    [TestMethod]
    public void IsLineMatch_EmptyPatterns_ReturnsTrue()
    {
        // Arrange
        var line = "test line";
        var includePatterns = new List<Regex>();
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLineMatch_SingleIncludePattern_MatchesCorrectly()
    {
        // Arrange
        var line = "hello world";
        var includePatterns = new List<Regex> { new Regex("world") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLineMatch_SingleIncludePattern_NoMatch_ReturnsFalse()
    {
        // Arrange
        var line = "hello world";
        var includePatterns = new List<Regex> { new Regex("goodbye") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsLineMatch_MultipleIncludePatterns_AllMatch_ReturnsTrue()
    {
        // Arrange
        var line = "hello world test";
        var includePatterns = new List<Regex>
        {
            new Regex("hello"),
            new Regex("world"),
            new Regex("test")
        };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLineMatch_MultipleIncludePatterns_OneDoesntMatch_ReturnsFalse()
    {
        // Arrange
        var line = "hello world test";
        var includePatterns = new List<Regex>
        {
            new Regex("hello"),
            new Regex("goodbye"), // This one doesn't match
            new Regex("test")
        };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsLineMatch_SingleExcludePattern_Matches_ReturnsFalse()
    {
        // Arrange
        var line = "hello world";
        var includePatterns = new List<Regex>();
        var excludePatterns = new List<Regex> { new Regex("world") };

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsLineMatch_SingleExcludePattern_NoMatch_ReturnsTrue()
    {
        // Arrange
        var line = "hello world";
        var includePatterns = new List<Regex>();
        var excludePatterns = new List<Regex> { new Regex("goodbye") };

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLineMatch_IncludeAndExclude_BothMatch_ReturnsFalse()
    {
        // Arrange
        var line = "hello world test";
        var includePatterns = new List<Regex> { new Regex("hello") };
        var excludePatterns = new List<Regex> { new Regex("test") };

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsFalse(result, "Exclude should take precedence");
    }

    [TestMethod]
    public void IsLineMatch_IncludeAndExclude_OnlyIncludeMatches_ReturnsTrue()
    {
        // Arrange
        var line = "hello world";
        var includePatterns = new List<Regex> { new Regex("hello") };
        var excludePatterns = new List<Regex> { new Regex("test") };

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLineMatch_EmptyLine_NoPatterns_ReturnsTrue()
    {
        // Arrange
        var line = "";
        var includePatterns = new List<Regex>();
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsLineMatch_CaseInsensitivePattern_MatchesCorrectly()
    {
        // Arrange
        var line = "Hello World";
        var includePatterns = new List<Regex> { new Regex("hello", RegexOptions.IgnoreCase) };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.IsLineMatch(line, includePatterns, excludePatterns);

        // Assert
        Assert.IsTrue(result);
    }

    #endregion

    #region AddLineNumbers Tests

    [TestMethod]
    public void AddLineNumbers_EmptyString_ReturnsEmptyWithLineNumber()
    {
        // Arrange
        var content = "";

        // Act
        var result = LineHelpers.AddLineNumbers(content);

        // Assert
        Assert.AreEqual("1: ", result);
    }

    [TestMethod]
    public void AddLineNumbers_SingleLine_AddsLineNumber()
    {
        // Arrange
        var content = "hello world";

        // Act
        var result = LineHelpers.AddLineNumbers(content);

        // Assert
        Assert.AreEqual("1: hello world", result);
    }

    [TestMethod]
    public void AddLineNumbers_MultipleLines_AddsLineNumbersToAll()
    {
        // Arrange
        var content = "line one\nline two\nline three";

        // Act
        var result = LineHelpers.AddLineNumbers(content);

        // Assert
        var expected = "1: line one\n2: line two\n3: line three";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void AddLineNumbers_ManyLines_AddsCorrectNumbers()
    {
        // Arrange
        var lines = new List<string>();
        for (int i = 1; i <= 100; i++)
        {
            lines.Add($"line {i}");
        }
        var content = string.Join("\n", lines);

        // Act
        var result = LineHelpers.AddLineNumbers(content);

        // Assert
        var resultLines = result.Split('\n');
        Assert.AreEqual(100, resultLines.Length);
        Assert.IsTrue(resultLines[0].StartsWith("1: "));
        Assert.IsTrue(resultLines[99].StartsWith("100: "));
    }

    #endregion

    #region FilterAndExpandContext Tests - Basic Filtering

    [TestMethod]
    public void FilterAndExpandContext_NoMatches_ReturnsNull()
    {
        // Arrange
        var content = "line one\nline two\nline three";
        var includePatterns = new List<Regex> { new Regex("notfound") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FilterAndExpandContext_SingleMatch_NoContext_ReturnsMatch()
    {
        // Arrange
        var content = "line one\nline two\nline three";
        var includePatterns = new List<Regex> { new Regex("two") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual("line two", result);
    }

    [TestMethod]
    public void FilterAndExpandContext_MultipleMatches_NoContext_ReturnsAllMatches()
    {
        // Arrange
        var content = "line one\nline two\nline three\nline four";
        var includePatterns = new List<Regex> { new Regex("line") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        var expected = "line one\nline two\nline three\nline four";
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region FilterAndExpandContext Tests - Context Expansion

    [TestMethod]
    public void FilterAndExpandContext_ContextBefore_IncludesPreviousLines()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4\nline 5";
        var includePatterns = new List<Regex> { new Regex("line 3") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 2, 0, false, excludePatterns, "```", false);

        // Assert
        var expected = "line 1\nline 2\nline 3";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void FilterAndExpandContext_ContextAfter_IncludesFollowingLines()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4\nline 5";
        var includePatterns = new List<Regex> { new Regex("line 3") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 2, false, excludePatterns, "```", false);

        // Assert
        var expected = "line 3\nline 4\nline 5";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void FilterAndExpandContext_ContextBeforeAndAfter_IncludesBoth()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4\nline 5";
        var includePatterns = new List<Regex> { new Regex("line 3") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, false, excludePatterns, "```", false);

        // Assert
        var expected = "line 2\nline 3\nline 4";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void FilterAndExpandContext_ContextAtStart_DoesntGoNegative()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3";
        var includePatterns = new List<Regex> { new Regex("line 1") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 5, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual("line 1", result);
    }

    [TestMethod]
    public void FilterAndExpandContext_ContextAtEnd_DoesntGoBeyond()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3";
        var includePatterns = new List<Regex> { new Regex("line 3") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 5, false, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual("line 3", result);
    }

    #endregion

    #region FilterAndExpandContext Tests - Line Numbers

    [TestMethod]
    public void FilterAndExpandContext_WithLineNumbers_AddsNumbers()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3";
        var includePatterns = new List<Regex> { new Regex("line 2") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, true, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual("  2: line 2", result);
    }

    [TestMethod]
    public void FilterAndExpandContext_WithLineNumbersAndContext_NumbersCorrectly()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4\nline 5";
        var includePatterns = new List<Regex> { new Regex("line 3") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, true, excludePatterns, "```", false);

        // Assert
        var expected = "  2: line 2\n  3: line 3\n  4: line 4";
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region FilterAndExpandContext Tests - Highlighting

    [TestMethod]
    public void FilterAndExpandContext_WithHighlighting_MarksMatchingLines()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4";
        var includePatterns = new List<Regex> { new Regex("line 2") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, false, excludePatterns, "```", true);

        // Assert
        var lines = result.Split('\n');
        Assert.AreEqual(3, lines.Length);
        Assert.IsFalse(lines[0].StartsWith("*"), "Context line should not be marked");
        Assert.IsTrue(lines[1].StartsWith("* "), "Matching line should be marked with *");
        Assert.IsFalse(lines[2].StartsWith("*"), "Context line should not be marked");
    }

    [TestMethod]
    public void FilterAndExpandContext_WithHighlightingAndLineNumbers_MarksCorrectly()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4";
        var includePatterns = new List<Regex> { new Regex("line 2") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, true, excludePatterns, "```", true);

        // Assert
        var lines = result.Split('\n');
        Assert.AreEqual(3, lines.Length);
        Assert.IsTrue(lines[0].StartsWith("  1:"), "Context line should have space prefix");
        Assert.IsTrue(lines[1].StartsWith("* 2:"), "Matching line should have * prefix");
        Assert.IsTrue(lines[2].StartsWith("  3:"), "Context line should have space prefix");
    }

    #endregion

    #region FilterAndExpandContext Tests - Line Breaks

    [TestMethod]
    public void FilterAndExpandContext_NonContiguousMatches_InsertsBreaks()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4\nline 5\nline 6";
        var includePatterns = new List<Regex> { new Regex("line [26]") }; // Matches 2 and 6
        var excludePatterns = new List<Regex>();

        // Act - Need context expansion for breaks to occur
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, false, excludePatterns, "```", false);

        // Assert
        Assert.IsTrue(result!.Contains("```\n\n```"), "Should contain separator for line break");
    }

    [TestMethod]
    public void FilterAndExpandContext_ContiguousMatches_NoBreaks()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4";
        var includePatterns = new List<Regex> { new Regex("line [23]") }; // Matches 2 and 3
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.IsFalse(result.Contains("```\n\n```"), "Should not contain separator for contiguous lines");
    }

    #endregion

    #region FilterAndExpandContext Tests - Exclude Patterns in Context

    [TestMethod]
    public void FilterAndExpandContext_ExcludePatternInContext_SkipsContextLine()
    {
        // Arrange
        var content = "good 1\nbad line\ngood 2\nMATCH\ngood 3\nbad line\ngood 4";
        var includePatterns = new List<Regex> { new Regex("MATCH") };
        var excludePatterns = new List<Regex> { new Regex("bad") };

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 2, 2, false, excludePatterns, "```", false);

        // Assert
        // Should include: good 2, MATCH, good 3
        // Should exclude: bad line (both before and after)
        var lines = result.Split('\n');
        Assert.IsTrue(lines.Contains("good 2"));
        Assert.IsTrue(lines.Contains("MATCH"));
        Assert.IsTrue(lines.Contains("good 3"));
        Assert.IsFalse(result.Contains("bad line"));
    }

    #endregion

    #region FilterAndExpandContext Tests - Edge Cases

    [TestMethod]
    public void FilterAndExpandContext_EmptyContent_ReturnsNull()
    {
        // Arrange
        var content = "";
        var includePatterns = new List<Regex> { new Regex("test") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FilterAndExpandContext_SingleLineContent_HandlesCorrectly()
    {
        // Arrange
        var content = "single line match";
        var includePatterns = new List<Regex> { new Regex("match") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual("single line match", result);
    }

    [TestMethod]
    public void FilterAndExpandContext_AllLinesMatch_ReturnsAll()
    {
        // Arrange
        var content = "test 1\ntest 2\ntest 3";
        var includePatterns = new List<Regex> { new Regex("test") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public void FilterAndExpandContext_OverlappingContext_MergesCorrectly()
    {
        // Arrange
        var content = "line 1\nline 2\nline 3\nline 4\nline 5";
        var includePatterns = new List<Regex> { new Regex("line [24]") }; // Lines 2 and 4
        var excludePatterns = new List<Regex>();

        // Act - With context of 1 before/after, should get all lines 1-5
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, false, excludePatterns, "```", false);

        // Assert
        Assert.AreEqual(content, result, "Overlapping context should merge to include all lines");
    }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public void Scenario_FindingCodePatterns_WorksCorrectly()
    {
        // Arrange - Simulate finding method calls in code
        var content = @"using System;

public class Test
{
    public void Method1()
    {
        Console.WriteLine(""test"");
    }

    public void Method2()
    {
        var x = 10;
    }
}";
        var includePatterns = new List<Regex> { new Regex(@"Console\.WriteLine") };
        var excludePatterns = new List<Regex>();

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 1, 1, true, excludePatterns, "```", false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("Console.WriteLine"));
        Assert.IsTrue(result.Contains("6:") || result.Contains("7:")); // Line numbers present
    }

    [TestMethod]
    public void Scenario_FilteringLogFiles_ExcludesDebugLines()
    {
        // Arrange - Simulate filtering log file
        var content = @"INFO: Application started
DEBUG: Loading config
ERROR: Connection failed
DEBUG: Retrying connection
INFO: Application running";
        
        var includePatterns = new List<Regex> { new Regex(".*") }; // All lines
        var excludePatterns = new List<Regex> { new Regex("DEBUG") };

        // Act
        var result = LineHelpers.FilterAndExpandContext(
            content, includePatterns, 0, 0, false, excludePatterns, "```", false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("INFO: Application started"));
        Assert.IsTrue(result.Contains("ERROR: Connection failed"));
        Assert.IsTrue(result.Contains("INFO: Application running"));
        Assert.IsFalse(result.Contains("DEBUG"));
    }

    #endregion
}
