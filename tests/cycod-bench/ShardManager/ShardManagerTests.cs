using CycodBench.Configuration;
using CycodBench.Logging;
using CycodBench.Models;
using CycodBench.ShardManager;
using Moq;
using Xunit;

namespace CycodBench.Tests.ShardManager;

public class ShardManagerTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<ILogger> _mockLogger;
    
    public ShardManagerTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger>();
    }
    
    [Fact]
    public void GetProblemsForCurrentShard_WithShardCount1_ReturnsAllProblems()
    {
        // Arrange
        _mockConfig.Setup(c => c.ShardCount).Returns(1);
        _mockConfig.Setup(c => c.ShardId).Returns(0);
        
        var shardManager = new CycodBench.ShardManager.ShardManager(_mockConfig.Object, _mockLogger.Object);
        
        var problems = new List<SwebenchProblem>
        {
            new SwebenchProblem { Id = "problem1" },
            new SwebenchProblem { Id = "problem2" },
            new SwebenchProblem { Id = "problem3" }
        };
        
        // Act
        var result = shardManager.GetProblemsForCurrentShard(problems);
        
        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(problems[0], result);
        Assert.Contains(problems[1], result);
        Assert.Contains(problems[2], result);
    }
    
    [Fact]
    public void GetProblemsForShard_WithShardCount2_DistributesProblemsEvenly()
    {
        // Arrange
        _mockConfig.Setup(c => c.ShardCount).Returns(2);
        
        var shardManager = new CycodBench.ShardManager.ShardManager(_mockConfig.Object, _mockLogger.Object);
        
        var problems = new List<SwebenchProblem>
        {
            new SwebenchProblem { Id = "problem1" },
            new SwebenchProblem { Id = "problem2" },
            new SwebenchProblem { Id = "problem3" },
            new SwebenchProblem { Id = "problem4" }
        };
        
        // Act
        var shard0Problems = shardManager.GetProblemsForShard(problems, 0);
        var shard1Problems = shardManager.GetProblemsForShard(problems, 1);
        
        // Assert
        Assert.NotEmpty(shard0Problems);
        Assert.NotEmpty(shard1Problems);
        
        // Make sure no problem is in both shards
        foreach (var problem in shard0Problems)
        {
            Assert.DoesNotContain(problem, shard1Problems);
        }
        
        // Make sure all problems are accounted for
        Assert.Equal(problems.Count, shard0Problems.Count() + shard1Problems.Count());
    }
    
    [Fact]
    public void CalculateShardId_ReturnsSameShardForSameProblem()
    {
        // Arrange
        _mockConfig.Setup(c => c.ShardCount).Returns(10);
        
        var shardManager = new CycodBench.ShardManager.ShardManager(_mockConfig.Object, _mockLogger.Object);
        
        var problem = new SwebenchProblem { Id = "problem1" };
        
        // Act
        var shardId1 = shardManager.CalculateShardId(problem);
        var shardId2 = shardManager.CalculateShardId(problem);
        
        // Assert
        Assert.Equal(shardId1, shardId2);
    }
    
    [Fact]
    public void IsInCurrentShard_WithMatchingShardId_ReturnsTrue()
    {
        // Arrange
        _mockConfig.Setup(c => c.ShardCount).Returns(10);
        _mockConfig.Setup(c => c.ShardId).Returns(5);
        
        var shardManager = new CycodBench.ShardManager.ShardManager(_mockConfig.Object, _mockLogger.Object);
        
        var problem = new SwebenchProblem { Id = "test-problem" };
        
        // Calculate which shard this problem would be in
        int expectedShardId = shardManager.CalculateShardId(problem);
        
        // Reconfigure the mock to return this shard ID
        _mockConfig.Setup(c => c.ShardId).Returns(expectedShardId);
        
        // Act
        bool result = shardManager.IsInCurrentShard(problem);
        
        // Assert
        Assert.True(result);
    }
}