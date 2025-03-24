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
        string fileName = "C:\\path\\to\\file.txt";
        string? template = null;

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_EmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FullFileNamePlaceholder_ReturnsFileName()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "{fileName}";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual(fileName, result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FileNamePlaceholder_ReturnsFileName()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "{filename}";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual(fileName, result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FilePathPlaceholder_ReturnsDirectoryName()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "{filePath}";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("C:\\path\\to", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FileBasePlaceholder_ReturnsFileNameWithoutExtension()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "{fileBase}";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("file", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_FileExtPlaceholder_ReturnsExtensionWithoutDot()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "{fileExt}";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("txt", result);
    }

    [TestMethod]
    public void GetFileNameFromTemplate_ComplexTemplate_ReturnsCorrectlyFormattedString()
    {
        // Arrange
        string fileName = "C:\\path\\to\\file.txt";
        string template = "backup-{fileBase}-{timeStamp}.{fileExt}";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

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
        string fileName = "C:\\path\\to\\file.txt";
        string template = "{fileBase}/////";

        // Act
        string? result = FileHelpers.GetFileNameFromTemplate(fileName, template);

        // Assert
        Assert.AreEqual("file", result);
    }

    #endregion

    #region MakeRelativePath Tests

    [TestMethod]
    public void MakeRelativePath_PathInsideCurrentDirectory_ReturnsRelativePath()
    {
        // This test needs to be aware of the current directory
        string currentDir = Directory.GetCurrentDirectory();
        string fullPath = Path.Combine(currentDir, "subfolder", "file.txt");
        
        // Act
        string result = FileHelpers.MakeRelativePath(fullPath);
        
        // Assert
        Assert.AreEqual(Path.Combine("subfolder", "file.txt"), result);
    }

    #endregion

    #region Utility Method Tests

    [TestMethod]
    public void FileExists_NullFileName_ReturnsFalse()
    {
        // Act
        bool result = FileHelpers.FileExists(null);
        
        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void FileExists_EmptyFileName_ReturnsFalse()
    {
        // Act
        bool result = FileHelpers.FileExists("");
        
        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void FileExists_StdinReference_ReturnsTrue()
    {
        // Act
        bool result = FileHelpers.FileExists("-");
        
        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ExpandAtFileValue_NoAtPrefix_ReturnsOriginalValue()
    {
        // Arrange
        string value = "just a string";
        
        // Act
        string result = FileHelpers.ExpandAtFileValue(value);
        
        // Assert
        Assert.AreEqual(value, result);
    }

    #endregion
}