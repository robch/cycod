using CycodBench.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Tests.Logging;

public class LoggerTests
{
    [Fact]
    public void Debug_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        var factoryMock = new Mock<ILoggerFactory>();
        factoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var logger = new Logger(factoryMock.Object, "TestCategory");

        // Act
        logger.Debug("Test message");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test message")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Info_LogsCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        var factoryMock = new Mock<ILoggerFactory>();
        factoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var logger = new Logger(factoryMock.Object, "TestCategory");

        // Act
        logger.Info("Test {0}", "info");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test info")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Error_WithException_LogsExceptionAndMessage()
    {
        // Arrange
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        var factoryMock = new Mock<ILoggerFactory>();
        factoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var logger = new Logger(factoryMock.Object, "TestCategory");
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.Error(exception, "Error occurred");

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Logger_WritesToFile_WhenFilePathProvided()
    {
        // Arrange
        var logPath = Path.GetTempFileName();
        
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        var factoryMock = new Mock<ILoggerFactory>();
        factoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var logger = new Logger(factoryMock.Object, "TestCategory", logPath);

        // Act
        logger.Info("Test file logging");

        // Assert
        Assert.True(File.Exists(logPath));
        var fileContent = File.ReadAllText(logPath);
        Assert.Contains("Test file logging", fileContent);
        Assert.Contains("[Information]", fileContent);
        
        // Clean up
        try { File.Delete(logPath); } catch { }
    }

    [Fact]
    public void BeginScope_ReturnsDisposableScope()
    {
        // Arrange
        var disposableMock = new Mock<IDisposable>();
        
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        loggerMock.Setup(l => l.BeginScope(It.IsAny<object>())).Returns(disposableMock.Object);
        
        var factoryMock = new Mock<ILoggerFactory>();
        factoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var logger = new Logger(factoryMock.Object, "TestCategory");

        // Act
        var scope = logger.BeginScope("Test scope");

        // Assert
        Assert.NotNull(scope);
    }
}