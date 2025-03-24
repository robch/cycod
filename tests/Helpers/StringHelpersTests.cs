namespace Helpers;

[TestClass]
public class StringHelpersTests
{
    #region ReplaceOnce Tests

    [TestMethod]
    public void ReplaceOnce_ExactMatchOnce_ReplacesSuccessfully()
    {
        // Arrange
        string content = "This is a test string with a specific word.";
        string oldStr = "specific";
        string newStr = "replaced";

        // Act
        string? result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This is a test string with a replaced word.", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void ReplaceOnce_ExactMatchMultipleTimes_ReturnsNull()
    {
        // Arrange
        string content = "This test string has two test words.";
        string oldStr = "test";
        string newStr = "replaced";

        // Act
        string? result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound);
    }

    [TestMethod]
    public void ReplaceOnce_NoMatch_ReturnsNull()
    {
        // Arrange
        string content = "This is a test string.";
        string oldStr = "nonexistent";
        string newStr = "replaced";

        // Act
        string? result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, countFound);
    }

    [TestMethod]
    public void ReplaceOnce_ExactMatchFailsFuzzySucceeds_ReplacesSuccessfully()
    {
        // Arrange
        string content = "This is a test string\r\nwith a specific  \r\nword.";
        string oldStr = "specific\nword";
        string newStr = "replaced text";

        // Act
        string? result = StringHelpers.ReplaceOnce(content, oldStr, newStr, out int countFound);

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
        string content = "Replace this word in this sentence.";
        string oldStr = "this word";
        string newStr = "these words";

        // Act
        string? result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Replace these words in this sentence.", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_MatchMultipleTimes_ReturnsNull()
    {
        // Arrange
        string content = "This text has 'match' and another 'match' in it.";
        string oldStr = "match";
        string newStr = "replacement";

        // Act
        string? result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_NoMatch_ReturnsNull()
    {
        // Arrange
        string content = "This text has no matches.";
        string oldStr = "nonexistent";
        string newStr = "replacement";

        // Act
        string? result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_CaseSensitiveMatch_ReplacesCorrectly()
    {
        // Arrange
        string content = "This text has Case sensitivity.";
        string oldStr = "Case";
        string newStr = "proper case";

        // Act
        string? result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This text has proper case sensitivity.", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void ExactlyReplaceOnce_EmptyString_ReturnsNull()
    {
        // Arrange
        string content = "Some content";
        string oldStr = "";
        string newStr = "replacement";

        // Act
        string? result = StringHelpers.ExactlyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound); // Empty string is found multiple times
    }

    #endregion

    #region FuzzyReplaceOnce Tests

    [TestMethod]
    public void FuzzyReplaceOnce_WhitespaceVariation_ReplacesSuccessfully()
    {
        // Arrange
        string content = "This is line one\r\n  line two with spaces  \r\nline three";
        string oldStr = "line two with spaces\nline three";
        string newStr = "replaced text";

        // Act
        string? result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("This is line one\r\n  replaced text", result); // Preserves leading whitespace
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_MultiLineWithTrailingWhitespace_ReplacesSuccessfully()
    {
        // Arrange
        string content = "Line 1\r\nLine 2  \r\nLine 3";
        string oldStr = "Line 2\nLine 3";
        string newStr = "New Content";

        // Act
        string? result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Line 1\r\nNew Content", result);
        Assert.AreEqual(1, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_MultipleFuzzyMatches_ReturnsNull()
    {
        // Arrange
        string content = "Line 1  \r\nLine 2\r\nLine 3\r\nLine 1  \r\nLine 2\r\nLine 3";
        string oldStr = "Line 1\nLine 2";
        string newStr = "New Content";

        // Act
        string? result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(2, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_NoFuzzyMatch_ReturnsNull()
    {
        // Arrange
        string content = "Line 1\r\nLine 2\r\nLine 3";
        string oldStr = "Line X\nLine Y";
        string newStr = "New Content";

        // Act
        string? result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(0, countFound);
    }

    [TestMethod]
    public void FuzzyReplaceOnce_SingleLineWithDifferentTrailingWhitespace_ReplacesSuccessfully()
    {
        // Arrange
        string content = "This line has trailing spaces    \r\nNext line";
        string oldStr = "This line has trailing spaces";
        string newStr = "Replaced line";

        // Act
        string? result = StringHelpers.FuzzyReplaceOnce(content, oldStr, newStr, out int countFound);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Replaced line    \r\nNext line", result); // Preserves trailing whitespace
        Assert.AreEqual(1, countFound);
    }

    #endregion
}