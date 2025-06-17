using CycodBench.Configuration;
using CycodBench.DockerManager;
using CycodBench.EvaluationToolsManager;
using CycodBench.Logging;
using CycodBench.Models;
using Moq;
using Xunit;

namespace Tests.EvaluationToolsManager;

public class EvaluationToolsManagerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IDockerManager> _dockerManagerMock;
    private readonly IEvaluationToolsManager _toolsManager;

    public EvaluationToolsManagerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _configurationMock = new Mock<IConfiguration>();
        _dockerManagerMock = new Mock<IDockerManager>();
        
        // Configure mocks
        _configurationMock.Setup(c => c.GetString("eval_tools_path", It.IsAny<string>()))
            .Returns(Path.Combine(Path.GetTempPath(), "CycodBenchTest", "eval_tools"));
        
        _dockerManagerMock.Setup(dm => dm.IsDockerAvailableAsync())
            .ReturnsAsync(true);
        
        _toolsManager = new CycodBench.EvaluationToolsManager.EvaluationToolsManager(
            _loggerMock.Object,
            _configurationMock.Object,
            _dockerManagerMock.Object);
    }

    [Fact]
    public void GetToolsPath_ReturnsCorrectPath()
    {
        // Arrange
        string expectedPath = Path.Combine(Path.GetTempPath(), "CycodBenchTest", "eval_tools");
        
        // Act
        string toolsPath = _toolsManager.GetToolsPath();
        
        // Assert
        Assert.Equal(expectedPath, toolsPath);
    }
    
    [Fact]
    public async Task AreToolsSetupAsync_WithMissingTools_ReturnsFalse()
    {
        // Act
        bool areToolsSetup = await _toolsManager.AreToolsSetupAsync();
        
        // Assert
        Assert.False(areToolsSetup);
    }
    
    [Fact]
    public async Task CreatePredictionsFileAsync_CreatesCorrectFile()
    {
        // Arrange
        var problem = new SwebenchProblem
        {
            Id = "test-123",
            BaseCommit = "abcdef12345",
            Repository = "org/repo"
        };
        
        var candidate = new CandidateSolution
        {
            Id = "candidate-1",
            ProblemId = "test-123",
            Diff = "diff --git a/file.txt b/file.txt\nindex 123..456 789\n--- a/file.txt\n+++ b/file.txt\n@@ -1 +1 @@\n-old line\n+new line"
        };
        
        string outputDirectory = Path.Combine(Path.GetTempPath(), "CycodBenchTest", "predictions");
        Directory.CreateDirectory(outputDirectory);
        
        try
        {
            // Act
            string predictionFilePath = await _toolsManager.CreatePredictionsFileAsync(problem, candidate, outputDirectory);
            
            // Assert
            Assert.True(File.Exists(predictionFilePath));
            string content = await File.ReadAllTextAsync(predictionFilePath);
            
            // Check that the file contains the expected JSON
            Assert.Contains("\"instance_id\": \"test-123\"", content);
            Assert.Contains("\"commit_hash\": \"abcdef12345\"", content);
            Assert.Contains("\"repo_path\": \"org/repo\"", content);
            Assert.Contains("\"diff\":", content);
        }
        finally
        {
            // Clean up
            Directory.Delete(outputDirectory, true);
        }
    }
    
    // Testing the actual evaluation logic would require integration tests
    // with Python and Docker, which are out of scope for these unit tests
}