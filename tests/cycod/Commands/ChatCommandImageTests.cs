using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ChatCommandImageTests
{
    private string _testDirectory = string.Empty;
    private string _testImagePath = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        // Create a test directory and a dummy image file
        _testDirectory = Path.Combine(Path.GetTempPath(), "CycoDImageTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _testImagePath = Path.Combine(_testDirectory, "test-image.png");
        
        // Create a minimal PNG file (1x1 transparent pixel)
        var pngBytes = new byte[] 
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, // IHDR chunk length
            0x49, 0x48, 0x44, 0x52, // IHDR chunk type
            0x00, 0x00, 0x00, 0x01, // Width: 1
            0x00, 0x00, 0x00, 0x01, // Height: 1
            0x08, 0x06, 0x00, 0x00, 0x00, // Bit depth, color type, compression, filter, interlace
            0x1F, 0x15, 0xC4, 0x89, // CRC
            0x00, 0x00, 0x00, 0x0A, // IDAT chunk length
            0x49, 0x44, 0x41, 0x54, // IDAT chunk type
            0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01, // Compressed image data
            0x0D, 0x0A, 0x2D, 0xB4, // CRC
            0x00, 0x00, 0x00, 0x00, // IEND chunk length
            0x49, 0x45, 0x4E, 0x44, // IEND chunk type
            0xAE, 0x42, 0x60, 0x82  // CRC
        };
        
        File.WriteAllBytes(_testImagePath, pngBytes);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [TestMethod]
    public void ChatCommand_ImagePatterns_PropertyExists()
    {
        // Arrange & Act
        var command = new ChatCommand();
        
        // Assert
        Assert.IsNotNull(command.ImagePatterns, "ImagePatterns property should be initialized");
        Assert.AreEqual(0, command.ImagePatterns.Count, "ImagePatterns should start empty");
    }

    [TestMethod]
    public void ChatCommand_Clone_CopiesImagePatterns()
    {
        // Arrange
        var originalCommand = new ChatCommand();
        originalCommand.ImagePatterns.Add("*.png");
        originalCommand.ImagePatterns.Add("images/*.jpg");
        
        // Act
        var clonedCommand = originalCommand.Clone();
        
        // Assert
        Assert.AreEqual(2, clonedCommand.ImagePatterns.Count, "Cloned command should have same number of image patterns");
        Assert.AreEqual("*.png", clonedCommand.ImagePatterns[0], "First image pattern should be copied");
        Assert.AreEqual("images/*.jpg", clonedCommand.ImagePatterns[1], "Second image pattern should be copied");
        
        // Verify deep copy (modifying one doesn't affect the other)
        originalCommand.ImagePatterns.Add("*.gif");
        Assert.AreEqual(2, clonedCommand.ImagePatterns.Count, "Cloned command should not be affected by changes to original");
    }

    [TestMethod]
    public void SlashImageCommandHandler_IsCommand_RecognizesImageCommand()
    {
        // Arrange
        var handler = new SlashImageCommandHandler();
        
        // Act & Assert
        Assert.IsTrue(handler.IsCommand("/image test.png"), "Should recognize /image command");
        Assert.IsTrue(handler.IsCommand("/image"), "Should recognize /image command without arguments");
        Assert.IsFalse(handler.IsCommand("/files test.png"), "Should not recognize other commands");
        Assert.IsFalse(handler.IsCommand("image test.png"), "Should not recognize without slash prefix");
        Assert.IsFalse(handler.IsCommand(""), "Should not recognize empty string");
    }

    [TestMethod]
    public void SlashImageCommandHandler_HandleCommand_ParsesArguments()
    {
        // Arrange
        var handler = new SlashImageCommandHandler();
        
        // Act
        var result1 = handler.HandleCommand("/image test.png");
        var result2 = handler.HandleCommand("/image *.jpg images/*.png");
        var result3 = handler.HandleCommand("/image");
        
        // Assert
        Assert.AreEqual(1, result1.Count, "Should parse single argument");
        Assert.AreEqual("test.png", result1[0], "Should extract correct argument");
        
        Assert.AreEqual(2, result2.Count, "Should parse multiple arguments");
        Assert.AreEqual("*.jpg", result2[0], "Should extract first argument");
        Assert.AreEqual("images/*.png", result2[1], "Should extract second argument");
        
        Assert.AreEqual(0, result3.Count, "Should handle no arguments");
    }

    [TestMethod]
    public void SlashImageCommandHandler_GetImageAddedMessage_ProperMessages()
    {
        // Arrange
        var handler = new SlashImageCommandHandler();
        
        // Act & Assert
        var emptyResult = handler.GetImageAddedMessage(new List<string>());
        Assert.AreEqual("No image patterns provided.", emptyResult, "Should handle empty patterns");
        
        // Test with actual image file
        var patterns = new List<string> { _testImagePath };
        var result = handler.GetImageAddedMessage(patterns);
        Assert.IsTrue(result.Contains("Added image file"), "Should indicate image was added");
        Assert.IsTrue(result.Contains("test-image.png"), "Should include filename");
        
        // Test with non-existent pattern
        var nonExistentPatterns = new List<string> { "nonexistent/*.png" };
        var nonExistentResult = handler.GetImageAddedMessage(nonExistentPatterns);
        Assert.IsTrue(nonExistentResult.Contains("No image files found"), "Should indicate no files found");
    }
}