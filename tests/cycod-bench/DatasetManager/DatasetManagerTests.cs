using CycodBench.Configuration;
using CycodBench.DatasetManager;
using CycodBench.Logging;
using Moq;
using Xunit;

namespace Tests.DatasetManager;

public class DatasetManagerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IDatasetManager _datasetManager;

    public DatasetManagerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Configure mocks
        _configurationMock.Setup(c => c.GetString("dataset_path", It.IsAny<string>()))
            .Returns(Path.Combine(Path.GetTempPath(), "CycodBenchTest", "datasets"));
        
        _datasetManager = new CycodBench.DatasetManager.DatasetManager(_loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public void GetDatasetRepositoryName_ReturnsCorrectRepository()
    {
        // Act & Assert
        Assert.Equal("princeton-nlp/SWE-bench_Verified", _datasetManager.GetDatasetRepositoryName("verified"));
        Assert.Equal("princeton-nlp/SWE-bench", _datasetManager.GetDatasetRepositoryName("full"));
        Assert.Equal("princeton-nlp/SWE-bench_Lite", _datasetManager.GetDatasetRepositoryName("lite"));
    }
    
    [Fact]
    public void GetDatasetRepositoryName_WithInvalidDatasetType_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _datasetManager.GetDatasetRepositoryName("invalid"));
        Assert.Contains("Unknown dataset type", exception.Message);
    }
    
    [Fact]
    public void GetDatasetPath_ReturnsCorrectPath()
    {
        // Arrange
        string expectedBasePath = Path.Combine(Path.GetTempPath(), "CycodBenchTest", "datasets");
        
        // Act
        string verifiedPath = _datasetManager.GetDatasetPath("verified");
        string fullPath = _datasetManager.GetDatasetPath("full");
        string litePath = _datasetManager.GetDatasetPath("lite");
        
        // Assert
        Assert.Equal(Path.Combine(expectedBasePath, "verified"), verifiedPath);
        Assert.Equal(Path.Combine(expectedBasePath, "full"), fullPath);
        Assert.Equal(Path.Combine(expectedBasePath, "lite"), litePath);
    }
    
    [Fact]
    public void IsDatasetCached_WithNonExistentPath_ReturnsFalse()
    {
        // Act
        bool isCached = _datasetManager.IsDatasetCached("verified");
        
        // Assert
        Assert.False(isCached);
    }
    
    // Integration tests for downloading and loading problems would require
    // actual HTTP requests and file operations, which are out of scope for unit tests
    // Additional tests could be added for these methods if we mock the HTTP client
}