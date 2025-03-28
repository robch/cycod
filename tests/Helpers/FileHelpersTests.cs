namespace Helpers;

using System.Text.RegularExpressions;

[TestClass]
public class FileHelpersTests
{
    #region GetFileNameFromTemplate Tests

    [TestMethod]
    public void GetFileNameFromTemplate_NullTemplate_ReturnsNull()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        string? template = null;

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FullFileNamePlaceholder_ReturnsFileName()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "{fileName}";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual(fileName, result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FileNamePlaceholder_ReturnsFileName()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "{filename}";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual(fileName, result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FilePathPlaceholder_ReturnsDirectoryName()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "{filePath}";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("C:\\path\\to", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FileBasePlaceholder_ReturnsFileNameWithoutExtension()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "{fileBase}";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("file", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FileExtPlaceholder_ReturnsExtensionWithoutDot()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "{fileExt}";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("txt", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_ComplexTemplate_ReturnsCorrectlyFormattedString()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "backup-{fileBase}-{timeStamp}.{fileExt}";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.IsNotNull(result);
        
        // The result should start with "backup-file-" and end with ".txt"
        StringAssert.StartsWith(result, "backup-file-");
        StringAssert.EndsWith(result, ".txt");
        
        // The timestamp should be 14 digits (yyyyMMddHHmmss)
        var match = Regex.Match(result!, @"backup-file-(\d{14})\.txt");
        Assert.IsTrue(match.Success, "Timestamp format is incorrect");
    }

    [TestMethod]
    public void GetFileNameFromTemplate_TemplateWithTrailingSlashes_TrimsSlashes()
    {
        // Arrange
        var fileName = "C:\\path\\to\\file.txt";
        var template = "{fileBase}/////";

        // Act
        var result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("file", result);
    }

    #endregion

    #region Utility Method Tests

    [TestMethod]
    public void FileExists_NullFileName_ReturnsFalse()
    {
        // Act
        var result = FileHelpers.FileExists(null);
        
        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void FileExists_EmptyFileName_ReturnsFalse()
    {
        // Act
        var result = FileHelpers.FileExists("");
        
        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void FileExists_StdinReference_ReturnsTrue()
    {
        // Act
        var result = FileHelpers.FileExists("-");
        
        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ExpandAtFileValue_NoAtPrefix_ReturnsOriginalValue()
    {
        // Arrange
        var value = "just a string";
        
        // Act
        var result = FileHelpers.ExpandAtFileValue(value);
        
        // Assert
        Assert.AreEqual(value, result);
    }

    #endregion
}