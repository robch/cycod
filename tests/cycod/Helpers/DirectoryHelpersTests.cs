using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

[TestClass]
public class DirectoryHelpersTests
{
    private string? _testRootPath;
    private string? _originalCurrentDirectory;

    [TestInitialize]
    public void Setup()
    {
        // Save original directory to restore later
        _originalCurrentDirectory = Directory.GetCurrentDirectory();
        
        // Create a test directory structure
        _testRootPath = Path.Combine(Path.GetTempPath(), $"DirectoryHelpersTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRootPath!);
        Directory.SetCurrentDirectory(_testRootPath!);
        // Normalize path to the canonical resolved form to avoid /var vs /private/var differences on macOS
        _testRootPath = Directory.GetCurrentDirectory();

    }

    [TestCleanup]
    public void Cleanup()
    {
        // Restore original directory
        Directory.SetCurrentDirectory(_originalCurrentDirectory!);
        
        // Clean up test directories
        if (Directory.Exists(_testRootPath!))
        {
            try
            {
                Directory.Delete(_testRootPath!, true);
            }
            catch (IOException)
            {
                // Ignore cleanup errors
            }
        }
    }

    #region EnsureDirectoryExists Tests

    [TestMethod]
    public void EnsureDirectoryExists_NonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        var subDirPath = Path.Combine(_testRootPath!, "subdir1");
        Assert.IsFalse(Directory.Exists(subDirPath));
        
        // Act
        DirectoryHelpers.EnsureDirectoryExists(subDirPath);
        
        // Assert
        Assert.IsTrue(Directory.Exists(subDirPath));
    }

    [TestMethod]
    public void EnsureDirectoryExists_ExistingDirectory_ReturnsDirectoryPath()
    {
        // Arrange
        var subDirPath = Path.Combine(_testRootPath!, "subdir2");
        Directory.CreateDirectory(subDirPath);
        
        // Act
        var result = DirectoryHelpers.EnsureDirectoryExists(subDirPath);
        
        // Assert
        Assert.AreEqual(subDirPath, result);
        Assert.IsTrue(Directory.Exists(subDirPath));
    }

    [TestMethod]
    public void EnsureDirectoryExists_NestedNonExistentDirectory_CreatesFullPath()
    {
        // Arrange
        var nestedDirPath = Path.Combine(_testRootPath!, "level1", "level2", "level3");
        Assert.IsFalse(Directory.Exists(nestedDirPath));
        
        // Act
        DirectoryHelpers.EnsureDirectoryExists(nestedDirPath);
        
        // Assert
        Assert.IsTrue(Directory.Exists(nestedDirPath));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void EnsureDirectoryExists_InvalidPath_ThrowsException()
    {
        // Arrange
        var invalidPath = new string(Path.GetInvalidPathChars());
        
        // Act
        DirectoryHelpers.EnsureDirectoryExists(invalidPath);
        
        // Assert is handled by ExpectedException
    }

    #endregion

    #region EnsureDirectoryForFileExists Tests

    [TestMethod]
    public void EnsureDirectoryForFileExists_NonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        var subDirPath = Path.Combine(_testRootPath!, "filedir1");
        var filePath = Path.Combine(subDirPath, "file.txt");
        Assert.IsFalse(Directory.Exists(subDirPath));
        
        // Act
        DirectoryHelpers.EnsureDirectoryForFileExists(filePath);
        
        // Assert
        Assert.IsTrue(Directory.Exists(subDirPath));
    }

    [TestMethod]
    public void EnsureDirectoryForFileExists_ExistingDirectory_DoesNotThrow()
    {
        // Arrange
        var subDirPath = Path.Combine(_testRootPath!, "filedir2");
        Directory.CreateDirectory(subDirPath);
        var filePath = Path.Combine(subDirPath, "file.txt");
        
        // Act & Assert (no exception)
        DirectoryHelpers.EnsureDirectoryForFileExists(filePath);
    }

    #endregion

    #region FindDirectorySearchParents Tests

    [TestMethod]
    public void FindDirectorySearchParents_ExistingDirectory_ReturnsPath()
    {
        // Arrange
        var targetDir = Path.Combine(_testRootPath!, "target");
        Directory.CreateDirectory(targetDir);
        
        // Act
        var result = DirectoryHelpers.FindDirectorySearchParents(new[] { "target" }, createIfNotFound: false);
        
        // Assert
        Assert.AreEqual(targetDir, result);
    }

    [TestMethod]
    public void FindDirectorySearchParents_NonExistentDirectoryDontCreate_ReturnsNull()
    {
        // Arrange
        var nonExistentDir = "nonexistent";
        
        // Act
        var result = DirectoryHelpers.FindDirectorySearchParents(new[] { nonExistentDir }, createIfNotFound: false);
        
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FindDirectorySearchParents_NonExistentDirectoryCreate_CreatesAndReturnsPath()
    {
        // Arrange
        var nonExistentDir = "newlyCreated";
        var expectedPath = Path.Combine(_testRootPath!, nonExistentDir);
        
        // Act
        var result = DirectoryHelpers.FindDirectorySearchParents(new[] { nonExistentDir }, createIfNotFound: true);
        
        // Assert
        Assert.AreEqual(expectedPath, result);
        Assert.IsTrue(Directory.Exists(expectedPath));
    }

    [TestMethod]
    public void FindDirectorySearchParents_NestedParentSearch_FindsCorrectPath()
    {
        // Arrange
        var targetDir = Path.Combine(_testRootPath!, "searchTarget");
        Directory.CreateDirectory(targetDir);
        
        var nestedDir1 = Path.Combine(_testRootPath!, "level1");
        Directory.CreateDirectory(nestedDir1);
        var nestedDir2 = Path.Combine(nestedDir1, "level2");
        Directory.CreateDirectory(nestedDir2);
        
        // Navigate to nested directory
        Directory.SetCurrentDirectory(nestedDir2);
        
        // Act
        var result = DirectoryHelpers.FindDirectorySearchParents(new[] { "searchTarget" }, createIfNotFound: false);
        
        // Assert
        Assert.AreEqual(targetDir, result);
    }

    #endregion

    #region FindOrCreateDirectorySearchParents Tests

    [TestMethod]
    public void FindOrCreateDirectorySearchParents_ExistingDirectory_ReturnsPath()
    {
        // Arrange
        var targetDir = Path.Combine(_testRootPath!, "findOrCreateTarget");
        Directory.CreateDirectory(targetDir);
        
        // Act
        var result = DirectoryHelpers.FindOrCreateDirectorySearchParents("findOrCreateTarget");
        
        // Assert
        Assert.AreEqual(targetDir, result);
    }

    [TestMethod]
    public void FindOrCreateDirectorySearchParents_NonExistentDirectory_CreatesAndReturnsPath()
    {
        // Arrange
        var nonExistentDir = "findOrCreateNew";
        var expectedPath = Path.Combine(_testRootPath!, nonExistentDir);
        
        // Act
        var result = DirectoryHelpers.FindOrCreateDirectorySearchParents(nonExistentDir);
        
        // Assert
        Assert.AreEqual(expectedPath, result);
        Assert.IsTrue(Directory.Exists(expectedPath));
    }

    #endregion
}