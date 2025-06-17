using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.Logging;
using Moq;
using Xunit;

namespace Tests.DockerManager;

public class DockerManagerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IDockerManager _dockerManager;

    public DockerManagerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Configure mocks
        _configurationMock.Setup(c => c.GetString("container_memory_limit", It.IsAny<string>()))
            .Returns("8g");
        _configurationMock.Setup(c => c.GetDouble("container_cpu_limit", It.IsAny<double>()))
            .Returns(4.0);
        
        _dockerManager = new CycodBench.DockerManager.DockerManager(_loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public void GetProblemImageName_ReturnsCorrectImageName()
    {
        // Arrange
        string problemId = "django__django-12345";
        
        // Act
        string imageName = _dockerManager.GetProblemImageName(problemId);
        
        // Assert
        Assert.Equal("swebench/sweb.eval.x86_64.django_1776_django-12345:latest", imageName);
        
        // Test with a problem ID that has no double underscore
        problemId = "postgresql-14950";
        imageName = _dockerManager.GetProblemImageName(problemId);
        Assert.Equal("swebench/sweb.eval.x86_64.postgresql-14950:latest", imageName);
    }
    
    [Fact(Skip = "Requires Docker to be installed")]
    public async Task IsDockerAvailableAsync_WithDockerInstalled_ReturnsTrue()
    {
        // Act
        bool isAvailable = await _dockerManager.IsDockerAvailableAsync();
        
        // Assert
        Assert.True(isAvailable);
    }
    
    // Additional tests could be added for other methods
    // but they would require mocking the Process class or similar approach,
    // which is out of scope for this implementation.
}