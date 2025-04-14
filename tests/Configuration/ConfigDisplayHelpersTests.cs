using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ConfigDisplayHelpersTests
{
    private StringWriter? _stringWriter;
    private TextWriter? _originalOutput;

    [TestInitialize]
    public void Setup()
    {
        // Redirect console output
        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Restore console output
        Console.SetOut(_originalOutput!);
        _stringWriter?.Dispose();
    }

    [TestMethod]
    public void DisplayConfigValue_ShowLocationTrue_DisplaysLocationInfo()
    {
        // Arrange - Create ConfigValue with known source values
        var configValue = new ConfigValue("test-value", ConfigSource.LocalConfig);
        
        // Act
        ConfigDisplayHelpers.DisplayConfigValue("test.key", configValue, showLocation: true);
        
        // Assert
        var output = _stringWriter!.ToString();
        Assert.IsTrue(output.Contains("test.key: test-value"));
    }
    
    [TestMethod]
    public void DisplayConfigValue_CommandLineSource_DisplaysCorrectLocation()
    {
        // Arrange
        var configValue = new ConfigValue("cli-value", ConfigSource.CommandLine);
        
        // Act
        ConfigDisplayHelpers.DisplayConfigValue("test.key", configValue, showLocation: true);
        
        // Assert
        var output = _stringWriter!.ToString();
        Assert.IsTrue(output.Contains("LOCATION: Command line (specified)"));
        Assert.IsTrue(output.Contains("test.key: cli-value"));
    }
    
    [TestMethod]
    public void DisplayConfigValue_EnvironmentVariableSource_DisplaysCorrectLocation()
    {
        // Arrange
        var configValue = new ConfigValue("env-value", ConfigSource.EnvironmentVariable);
        
        // Act
        ConfigDisplayHelpers.DisplayConfigValue("test.key", configValue, showLocation: true);
        
        // Assert
        var output = _stringWriter!.ToString();
        Assert.IsTrue(output.Contains("LOCATION: Environment variable (specified)"));
        Assert.IsTrue(output.Contains("test.key: env-value"));
    }
    
    [TestMethod]
    public void DisplayConfigValue_ShowLocationFalse_DoesNotDisplayLocationInfo()
    {
        // Arrange
        var configValue = new ConfigValue("test-value", ConfigSource.LocalConfig);
        
        // Act
        ConfigDisplayHelpers.DisplayConfigValue("test.key", configValue, showLocation: false);
        
        // Assert
        var output = _stringWriter!.ToString();
        Assert.IsFalse(output.Contains("LOCATION:"));
        Assert.IsTrue(output.Contains("test.key: test-value"));
    }
}