using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CommandLineImageOptionTests
{
    private string _testDirectory = string.Empty;
    private string _testImagePath = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        // Create a test directory and a dummy image file
        _testDirectory = Path.Combine(Path.GetTempPath(), "CycoDCommandLineTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _testImagePath = Path.Combine(_testDirectory, "test-image.png");
        File.WriteAllText(_testImagePath, "dummy png content");
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
    public void CommandLineOptions_ImageOption_ParsesSingleImage()
    {
        // Arrange
        var args = new[] { "--image", "test.png", "--input", "What's in this image?" };
        
        // Act
        var parsed = CycoDevCommandLineOptions.Parse(args, out var options, out var ex);
        
        // Assert
        Assert.IsTrue(parsed, $"Should parse successfully. Error: {ex?.Message}");
        Assert.IsNull(ex, "Should not have parsing errors");
        Assert.IsNotNull(options, "Options should not be null");
        
        var chatCommand = options.Commands.FirstOrDefault() as ChatCommand;
        Assert.IsNotNull(chatCommand, "Should have a ChatCommand");
        Assert.AreEqual(1, chatCommand.ImagePatterns.Count, "Should have one image pattern");
        Assert.AreEqual("test.png", chatCommand.ImagePatterns[0], "Should parse image pattern correctly");
        
        Assert.AreEqual(1, chatCommand.InputInstructions.Count, "Should have one input instruction");
        Assert.AreEqual("What's in this image?", chatCommand.InputInstructions[0], "Should parse input correctly");
    }

    [TestMethod]
    public void CommandLineOptions_ImageOption_ParsesMultipleImages()
    {
        // Arrange
        var args = new[] { "--image", "*.png", "images/*.jpg", "--input", "Analyze these images" };
        
        // Act
        var parsed = CycoDevCommandLineOptions.Parse(args, out var options, out var ex);
        
        // Assert
        Assert.IsTrue(parsed, $"Should parse successfully. Error: {ex?.Message}");
        Assert.IsNull(ex, "Should not have parsing errors");
        
        var chatCommand = options.Commands.FirstOrDefault() as ChatCommand;
        Assert.IsNotNull(chatCommand, "Should have a ChatCommand");
        Assert.AreEqual(2, chatCommand.ImagePatterns.Count, "Should have two image patterns");
        Assert.AreEqual("*.png", chatCommand.ImagePatterns[0], "Should parse first image pattern correctly");
        Assert.AreEqual("images/*.jpg", chatCommand.ImagePatterns[1], "Should parse second image pattern correctly");
    }

    [TestMethod]
    public void CommandLineOptions_ImageOptionOnly_ParsesWithoutInput()
    {
        // Arrange
        var args = new[] { "--image", "screenshot.png" };
        
        // Act
        var parsed = CycoDevCommandLineOptions.Parse(args, out var options, out var ex);
        
        // Assert
        Assert.IsTrue(parsed, $"Should parse successfully. Error: {ex?.Message}");
        Assert.IsNull(ex, "Should not have parsing errors");
        
        var chatCommand = options.Commands.FirstOrDefault() as ChatCommand;
        Assert.IsNotNull(chatCommand, "Should have a ChatCommand");
        Assert.AreEqual(1, chatCommand.ImagePatterns.Count, "Should have one image pattern");
        Assert.AreEqual("screenshot.png", chatCommand.ImagePatterns[0], "Should parse image pattern correctly");
        Assert.AreEqual(0, chatCommand.InputInstructions.Count, "Should have no input instructions");
    }

    [TestMethod]
    public void CommandLineOptions_MultipleImageOptions_CombinesPatterns()
    {
        // Arrange
        var args = new[] { "--image", "photo1.png", "--image", "photo2.jpg", "*.gif" };
        
        // Act
        var parsed = CycoDevCommandLineOptions.Parse(args, out var options, out var ex);
        
        // Assert
        Assert.IsTrue(parsed, $"Should parse successfully. Error: {ex?.Message}");
        Assert.IsNull(ex, "Should not have parsing errors");
        
        var chatCommand = options.Commands.FirstOrDefault() as ChatCommand;
        Assert.IsNotNull(chatCommand, "Should have a ChatCommand");
        Assert.AreEqual(3, chatCommand.ImagePatterns.Count, "Should have three image patterns from multiple --image options");
        Assert.AreEqual("photo1.png", chatCommand.ImagePatterns[0], "Should parse first image pattern correctly");
        Assert.AreEqual("photo2.jpg", chatCommand.ImagePatterns[1], "Should parse second image pattern correctly");
        Assert.AreEqual("*.gif", chatCommand.ImagePatterns[2], "Should parse third image pattern correctly");
    }

    [TestMethod]
    public void CommandLineOptions_ImageWithQuestionAlias_ParsesCorrectly()
    {
        // Arrange
        var args = new[] { "--image", "chart.png", "--question", "What does this chart show?" };
        
        // Act
        var parsed = CycoDevCommandLineOptions.Parse(args, out var options, out var ex);
        
        // Assert
        Assert.IsTrue(parsed, $"Should parse successfully. Error: {ex?.Message}");
        Assert.IsNull(ex, "Should not have parsing errors");
        
        var chatCommand = options.Commands.FirstOrDefault() as ChatCommand;
        Assert.IsNotNull(chatCommand, "Should have a ChatCommand");
        Assert.AreEqual(1, chatCommand.ImagePatterns.Count, "Should have one image pattern");
        Assert.AreEqual("chart.png", chatCommand.ImagePatterns[0], "Should parse image pattern correctly");
        
        Assert.AreEqual(1, chatCommand.InputInstructions.Count, "Should have one input instruction");
        Assert.AreEqual("What does this chart show?", chatCommand.InputInstructions[0], "Should parse question correctly");
    }
}