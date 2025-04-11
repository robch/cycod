namespace Helpers;

[TestClass]
public class StringHelpersTests
{
    #region ReplaceOnce Tests

    [TestMethod]
    public void ReplaceOnce_ExactMatchOnce_ReplacesSuccessfully()
    {
        // Arrange
        var content = "This is a test string with a specific word.";
        var oldStr = "specific";
        var newStr = "replaced";

        // Act
        var result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This is a test string with a replaced word.", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void ReplaceOnce_ExactMatchMultipleTimes_ReturnsNull()
    {
        // Arrange
        var content = "This test string has two test words.";
        var oldStr = "test";
        var newStr = "replaced";

        // Act
        var result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound);
    }

    [TestMethod]
    public void ReplaceOnce_NoMatch_ReturnsNull()
    {
        // Arrange
        var content = "This is a test string.";
        var oldStr = "nonexistent";
        var newStr = "replaced";

        // Act
        var result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, countFound);
    }

    [TestMethod]
    public void ReplaceOnce_ExactMatchFailsFuzzySucceeds_ReplacesSuccessfully()
    {
        // Arrange
        var content = "This is a test string\r\nwith a specific  \r\nword.";
        var oldStr = "specific\nword";
        var newStr = "replaced text";

        // Act
        var result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This is a test string\r\nwith a replaced text.", result);
        Assert.AreEqual(1, countFound);
    }

    #endregion

    #region ExactlyReplaceOnce Tests

    [TestMethod]
    public void ExactlyReplaceOnce_MatchOnce_ReplacesSuccessfully()
    {
        // Arrange
        var content = "Replace this word in this sentence.";
        var oldStr = "this word";
        var newStr = "these words";

        // Act
        var result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Replace these words in this sentence.", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_MatchMultipleTimes_ReturnsNull()
    {
        // Arrange
        var content = "This text has 'match' and another 'match' in it.";
        var oldStr = "match";
        var newStr = "replacement";

        // Act
        var result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_NoMatch_ReturnsNull()
    {
        // Arrange
        var content = "This text has no matches.";
        var oldStr = "nonexistent";
        var newStr = "replacement";

        // Act
        var result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_CaseSensitiveMatch_ReplacesCorrectly()
    {
        // Arrange
        var content = "This text has Case sensitivity.";
        var oldStr = "Case";
        var newStr = "proper case";

        // Act
        var result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This text has proper case sensitivity.", result);
        Assert.AreEqual(1, countFound);
    }

    #endregion

    #region FuzzyReplaceOnce Tests

    [TestMethod]
    public void FuzzyReplaceOnce_WhitespaceVariation_ReplacesSuccessfully()
    {
        // Arrange
        var content = "This is line one\r\n  line two with spaces  \r\nline three";
        var oldStr = "line two with spaces\nline three";
        var newStr = "replaced text";

        // Act
        var result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This is line one\r\n  replaced text", result); // Preserves leading whitespace
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_MultiLineWithTrailingWhitespace_ReplacesSuccessfully()
    {
        // Arrange
        var content = "Line 1\r\nLine 2  \r\nLine 3";
        var oldStr = "Line 2\nLine 3";
        var newStr = "New Content";

        // Act
        var result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Line 1\r\nNew Content", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_MultipleFuzzyMatches_ReturnsNull()
    {
        // Arrange
        var content = "Line 1  \r\nLine 2\r\nLine 3\r\nLine 1  \r\nLine 2\r\nLine 3";
        var oldStr = "Line 1\nLine 2";
        var newStr = "New Content";

        // Act
        var result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_NoFuzzyMatch_ReturnsNull()
    {
        // Arrange
        var content = "Line 1\r\nLine 2\r\nLine 3";
        var oldStr = "Line X\nLine Y";
        var newStr = "New Content";

        // Act
        var result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_SingleLineWithDifferentTrailingWhitespace_ReplacesSuccessfully()
    {
        // Arrange
        var content = "This line has trailing spaces    \r\nNext line";
        var oldStr = "This line has trailing spaces";
        var newStr = "Replaced line";

        // Act
        var result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out var countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Replaced line    \r\nNext line", result); // Preserves trailing whitespace
        Assert.AreEqual(1, countFound);
    }

    #endregion
}