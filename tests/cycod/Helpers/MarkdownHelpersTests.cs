using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MarkdownHelpersTests
{
    #region GetCodeBlockBacktickCharCountRequired Tests

    [TestMethod]
    public void GetCodeBlockBacktickCharCountRequired_NoBackticks_ReturnsMinimumThree()
    {
        // Arrange
        var content = "This is a code block with no backticks";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        
        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void GetCodeBlockBacktickCharCountRequired_SingleBacktick_ReturnsMinimumThree()
    {
        // Arrange
        var content = "This is a code block with a single `backtick`";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        
        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void GetCodeBlockBacktickCharCountRequired_ThreeBackticks_ReturnsFour()
    {
        // Arrange
        var content = "This is a code block with three ```backticks";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        
        // Assert
        Assert.AreEqual(4, result);
    }

    [TestMethod]
    public void GetCodeBlockBacktickCharCountRequired_FiveBackticks_ReturnsSix()
    {
        // Arrange
        var content = "This is a code block with five `````backticks";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        
        // Assert
        Assert.AreEqual(6, result);
    }

    [TestMethod]
    public void GetCodeBlockBacktickCharCountRequired_NullContent_ReturnsMinimumThree()
    {
        // Arrange
        string? content = null;
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        
        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void GetCodeBlockBacktickCharCountRequired_EmptyContent_ReturnsMinimumThree()
    {
        // Arrange
        var content = "";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(content);
        
        // Assert
        Assert.AreEqual(3, result);
    }

    #endregion

    #region GetCodeBlockBackticks Tests

    [TestMethod]
    public void GetCodeBlockBackticks_NoBackticks_ReturnsThreeBackticks()
    {
        // Arrange
        var content = "Code without backticks";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBackticks(content);
        
        // Assert
        Assert.AreEqual("```", result);
    }

    [TestMethod]
    public void GetCodeBlockBackticks_ThreeBackticks_ReturnsFourBackticks()
    {
        // Arrange
        var content = "Code with ```three backticks";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBackticks(content);
        
        // Assert
        Assert.AreEqual("````", result);
    }

    [TestMethod]
    public void GetCodeBlockBackticks_NullContent_ReturnsThreeBackticks()
    {
        // Arrange
        string? content = null;
        
        // Act
        var result = MarkdownHelpers.GetCodeBlockBackticks(content);
        
        // Assert
        Assert.AreEqual("```", result);
    }

    #endregion

    #region GetCodeBlock Tests

    [TestMethod]
    public void GetCodeBlock_BasicContent_ReturnsFormattedBlock()
    {
        // Arrange
        var content = "console.log('hello');";

        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "js");

        // Assert
        var expected = "```js\nconsole.log('hello');\n```";
        AssertNormalizedLFsAreEqual(result, expected);
    }

    [TestMethod]
    public void GetCodeBlock_WithLeadingLF_AddsNewLineBeforeBlock()
    {
        // Arrange
        var content = "print('hello')";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "python", leadingLF: true);
        
        // Assert
        var expected = "\n```python\nprint('hello')\n```";
        AssertNormalizedLFsAreEqual(expected, result);
    }

    [TestMethod]
    public void GetCodeBlock_WithTrailingLF_AddsNewLineAfterBlock()
    {
        // Arrange
        var content = "print('hello')";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "python", trailingLF: true);
        
        // Assert
        var expected = "```python\nprint('hello')\n```\n";
        AssertNormalizedLFsAreEqual(expected, result);
    }

    [TestMethod]
    public void GetCodeBlock_WithLeadingAndTrailingLF_AddsNewLinesAroundBlock()
    {
        // Arrange
        var content = "print('hello')";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "python", leadingLF: true, trailingLF: true);
        
        // Assert
        var expected = "\n```python\nprint('hello')\n```\n";
        AssertNormalizedLFsAreEqual(expected, result);
    }

    [TestMethod]
    public void GetCodeBlock_WithBackticksInContent_EscapesProperly()
    {
        // Arrange
        var content = "const code = `template string`;";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "js");
        
        // Assert
        var expected = "```js\nconst code = `template string`;\n```";
        AssertNormalizedLFsAreEqual(expected, result);
    }

    [TestMethod]
    public void GetCodeBlock_WithMoreBackticksInContent_IncreasesWrapperBackticks()
    {
        // Arrange
        var content = "const code = ```triple backticks```;";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "js");
        
        // Assert
        var expected = "````js\nconst code = ```triple backticks```;\n````";
        AssertNormalizedLFsAreEqual(expected, result);
    }

    [TestMethod]
    public void GetCodeBlock_NullContent_ReturnsEmptyString()
    {
        // Arrange
        string? content = null;
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "js");
        
        // Assert
        AssertNormalizedLFsAreEqual("", result);
    }

    [TestMethod]
    public void GetCodeBlock_EmptyContent_ReturnsEmptyString()
    {
        // Arrange
        var content = "";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content, "js");
        
        // Assert
        AssertNormalizedLFsAreEqual("", result);
    }

    [TestMethod]
    public void GetCodeBlock_NoLanguageSpecified_ReturnsUnspecifiedCodeBlock()
    {
        // Arrange
        var content = "Some code";
        
        // Act
        var result = MarkdownHelpers.GetCodeBlock(content);
        
        // Assert
        var expected = "```\nSome code\n```";
        AssertNormalizedLFsAreEqual(expected, result);
    }

    #endregion

    #region Helper Methods
 
    private static void AssertNormalizedLFsAreEqual(string actual, string expected)
    {
        actual = actual.Replace("\r\n", "\n").Replace("\r", "\n");
        expected = expected.Replace("\r\n", "\n").Replace("\r", "\n");
        var maxLength = Math.Max(expected.Length, actual.Length);
        for (int i = 0; i < maxLength; i++)
        {
            if (i >= expected.Length) Assert.Fail($"Actual string is longer than expected at index {i}. actual[i]: 0x{(int)actual[i]:X}\n{Environment.StackTrace}");
            if (i >= actual.Length) Assert.Fail($"Expected string is longer than actual at index {i}. expected[i]: 0x{(int)expected[i]:X}\n{Environment.StackTrace}");

            Assert.AreEqual((int)expected[i], (int)actual[i], $"Characters at index {i} do not match. Expected: (0x{(int)expected[i]:X}), Actual: (0x{(int)actual[i]:X})\n{Environment.StackTrace}");
        }
    }

    #endregion
}