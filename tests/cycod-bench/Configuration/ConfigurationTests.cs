using CycodBench.Configuration;
using CycodBench.Logging;
using Moq;
using Xunit;
using System.IO.Abstractions.TestingHelpers;

namespace Tests.Configuration;

public class ConfigurationTests
{
    private readonly Mock<CycodBench.Logging.ILogger> _loggerMock;
    private readonly MockFileSystem _fileSystem;

    public ConfigurationTests()
    {
        _loggerMock = new Mock<CycodBench.Logging.ILogger>();
        _fileSystem = new MockFileSystem();
    }

    [Fact]
    public void GetString_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var args = new[] { "--test=value" };
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.GetString("test");

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void GetString_WithDefaultValue_ReturnsDefaultWhenKeyNotFound()
    {
        // Arrange
        var args = Array.Empty<string>();
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.GetString("nonexistent", "default");

        // Assert
        Assert.Equal("default", result);
    }

    [Fact]
    public void GetInt_WithValidValue_ReturnsIntValue()
    {
        // Arrange
        var args = new[] { "--number=42" };
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.GetInt("number", 0);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetInt_WithInvalidValue_ReturnsDefaultValue()
    {
        // Arrange
        var args = new[] { "--number=invalid" };
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.GetInt("number", 99);

        // Assert
        Assert.Equal(99, result);
    }

    [Fact]
    public void GetBool_WithValidValue_ReturnsBoolValue()
    {
        // Arrange
        var args = new[] { "--feature=true" };
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.GetBool("feature", false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetSection_ReturnsConfigurationForSection()
    {
        // Arrange
        var args = new[] { "--section__key=value" };
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var section = config.GetSection("section");
        var result = section.GetString("key");

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void Exists_ReturnsTrueForExistingKey()
    {
        // Arrange
        var args = new[] { "--test=value" };
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.Exists("test");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_ReturnsFalseForNonExistingKey()
    {
        // Arrange
        var args = Array.Empty<string>();
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);

        // Act
        var result = config.Exists("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Configuration_LoadsFromJsonFile()
    {
        // Arrange
        string configJson = @"{
            ""TestSetting"": ""TestValue"",
            ""Section"": {
                ""NestedSetting"": ""NestedValue""
            }
        }";
        
        string configPath = Path.GetTempFileName();
        File.WriteAllText(configPath, configJson);
        
        var args = Array.Empty<string>();
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args, configPath, null);
        
        // Act
        var result = config.GetString("TestSetting");
        var nestedResult = config.GetSection("Section").GetString("NestedSetting");

        // Assert
        Assert.Equal("TestValue", result);
        Assert.Equal("NestedValue", nestedResult);
        
        // Clean up
        try { File.Delete(configPath); } catch { }
    }
    
    [Fact]
    public void Configuration_HandlesDefaultValues()
    {
        // Arrange
        var args = Array.Empty<string>();
        var config = new CycodBench.Configuration.Configuration(_loggerMock.Object, args);
        
        // Act
        var dockerImage = config.GetString("Docker:Image", "");
        var shardCount = config.GetInt("ShardCount", 0);
        
        // Assert
        Assert.Equal("swebench/base:latest", dockerImage);
        Assert.Equal(1, shardCount);
    }
}