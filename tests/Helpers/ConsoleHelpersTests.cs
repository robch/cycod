using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

[TestClass]
public class ConsoleHelpersTests
{
    private StringWriter? _stringWriter;
    private TextWriter? _originalOutput;

    [TestInitialize]
    public void Setup()
    {
        // Redirect Console.Out
        _originalOutput = Console.Out;
        _stringWriter = new StringWriter();
        Console.SetOut(_stringWriter);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Restore Console.Out
        Console.SetOut(_originalOutput!);
        _stringWriter!.Dispose();
    }

    #region Configuration Tests

    [TestMethod]
    public void Configure_SetsProperties_PropertiesAreCorrectlySet()
    {
        // Arrange & Act
        ConsoleHelpers.Configure(debug: true, verbose: true, quiet: false);
        
        // Assert
        Assert.IsTrue(ConsoleHelpers.IsDebug());
        Assert.IsTrue(ConsoleHelpers.IsVerbose());
        Assert.IsFalse(ConsoleHelpers.IsQuiet());
    }

    [TestMethod]
    public void ConfigureDebug_SetsDebugProperty_DebugIsCorrectlySet()
    {
        // Arrange & Act
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: false);
        ConsoleHelpers.ConfigureDebug(true);
        
        // Assert
        Assert.IsTrue(ConsoleHelpers.IsDebug());
    }

    #endregion

    #region Output Tests

    [TestMethod]
    public void WriteLine_NormalMode_WritesToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: false);
        
        // Act
        ConsoleHelpers.WriteLine("Test message");
        
        // Assert
        Assert.AreEqual("Test message" + Environment.NewLine, _stringWriter!.ToString());
    }

    [TestMethod]
    public void WriteLine_QuietMode_DoesNotWriteToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: true);
        
        // Act
        ConsoleHelpers.WriteLine("Test message");
        
        // Assert
        Assert.AreEqual("", _stringWriter!.ToString());
    }

    [TestMethod]
    public void WriteLine_OverrideQuiet_WritesToConsoleEvenInQuietMode()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: true);
        
        // Act
        ConsoleHelpers.WriteLine("Test message", overrideQuiet: true);
        
        // Assert
        Assert.AreEqual("Test message" + Environment.NewLine, _stringWriter!.ToString());
    }

    [TestMethod]
    public void Write_NormalMode_WritesToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: false);
        
        // Act
        ConsoleHelpers.Write("Test message");
        
        // Assert
        Assert.AreEqual("Test message", _stringWriter!.ToString());
    }

    [TestMethod]
    public void Write_QuietMode_DoesNotWriteToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: true);
        
        // Act
        ConsoleHelpers.Write("Test message");
        
        // Assert
        Assert.AreEqual("", _stringWriter!.ToString());
    }

    #endregion

    #region Error and Warning Tests

    [TestMethod]
    public void WriteErrorLine_AlwaysWritesToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: true);
        
        // Act
        ConsoleHelpers.WriteErrorLine("Error message");
        
        // Assert
        Assert.AreEqual("Error message" + Environment.NewLine, _stringWriter!.ToString());
    }

    [TestMethod]
    public void WriteWarningLine_AlwaysWritesToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: true);
        
        // Act
        ConsoleHelpers.WriteWarningLine("Warning message");
        
        // Assert
        Assert.AreEqual("Warning message" + Environment.NewLine, _stringWriter!.ToString());
    }

    [TestMethod]
    public void WriteDebugLine_InDebugMode_WritesToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: true, verbose: false, quiet: false);
        
        // Act
        ConsoleHelpers.WriteDebugLine("Debug message");
        
        // Assert
        Assert.IsTrue(_stringWriter!.ToString().Contains("Debug message"));
    }

    [TestMethod]
    public void WriteDebugLine_NotInDebugMode_DoesNotWriteToConsole()
    {
        // Arrange
        ConsoleHelpers.Configure(debug: false, verbose: false, quiet: false);
        
        // Act
        ConsoleHelpers.WriteDebugLine("Debug message");
        
        // Assert
        Assert.AreEqual("", _stringWriter!.ToString());
    }

    #endregion

    #region Utility Method Tests

    [TestMethod]
    public void IsStandardInputReference_WithValidReferences_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(ConsoleHelpers.IsStandardInputReference("-"));
        Assert.IsTrue(ConsoleHelpers.IsStandardInputReference("stdin"));
        Assert.IsTrue(ConsoleHelpers.IsStandardInputReference("STDIN"));
        Assert.IsTrue(ConsoleHelpers.IsStandardInputReference("STDIN:"));
    }

    [TestMethod]
    public void IsStandardInputReference_WithInvalidReferences_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(ConsoleHelpers.IsStandardInputReference("file.txt"));
        Assert.IsFalse(ConsoleHelpers.IsStandardInputReference("stdi"));
        Assert.IsFalse(ConsoleHelpers.IsStandardInputReference("STDINN"));
    }

    #endregion
}