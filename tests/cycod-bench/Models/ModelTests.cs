using System;
using System.Collections.Generic;
using System.IO;
using CycodBench.Models;
using Xunit;

namespace Tests.Models;

public class ModelTests : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), "CycodBenchTests", Guid.NewGuid().ToString());

    public ModelTests()
    {
        Directory.CreateDirectory(_testDir);
    }

    [Fact]
    public void SwebenchProblem_SerializationWorks()
    {
        // Arrange
        var problem = new SwebenchProblem
        {
            Id = "test-problem-123",
            Repository = "owner/repo",
            BaseCommit = "base123",
            HeadCommit = "head456",
            ProblemStatement = "Fix the bug",
            IssueUrl = "https://github.com/owner/repo/issues/123",
            PullRequestUrl = "https://github.com/owner/repo/pull/456",
            AdditionalContext = "Some context",
            DockerImage = "swebench/test-image:latest",
            TestCommand = "npm test",
            TestFiles = new List<string> { "test1.js", "test2.js" }
        };

        // Act
        var json = problem.ToJson();
        var deserialized = json.FromJson<SwebenchProblem>();

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("test-problem-123", deserialized!.Id);
        Assert.Equal("owner/repo", deserialized.Repository);
        Assert.Equal("base123", deserialized.BaseCommit);
        Assert.Equal("head456", deserialized.HeadCommit);
        Assert.Equal("Fix the bug", deserialized.ProblemStatement);
        Assert.Equal(2, deserialized.TestFiles.Count);
        
        // Check computed properties
        Assert.StartsWith("test-pro", deserialized.ShortId);
        Assert.Equal("owner", deserialized.Owner);
        Assert.Equal("repo", deserialized.Repo);
    }

    [Fact]
    public void CandidateSolution_SerializationWorks()
    {
        // Arrange
        var solution = new CandidateSolution
        {
            Id = "solution-123",
            ProblemId = "problem-123",
            CandidateIndex = 1,
            Diff = "--- a/file.js\n+++ b/file.js\n@@ -1,1 +1,1 @@\n-old\n+new",
            ExecutionTimeMs = 5000,
            AgentLogs = "Logs go here",
            AgentVersion = "1.0.0",
            WorkspacePath = "/tmp/workspace",
            Timestamp = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        // Act
        var json = solution.ToJson();
        var deserialized = json.FromJson<CandidateSolution>();

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("solution-123", deserialized!.Id);
        Assert.Equal("problem-123", deserialized.ProblemId);
        Assert.Equal(1, deserialized.CandidateIndex);
        Assert.Equal(5000, deserialized.ExecutionTimeMs);
        
        // Check computed properties
        Assert.NotEmpty(deserialized.ModifiedFiles);
        Assert.Contains("file.js", deserialized.ModifiedFiles);
    }
    
    [Fact]
    public void EvaluationResult_SerializationWorks()
    {
        // Arrange
        var evaluation = new EvaluationResult
        {
            Id = "eval-123",
            SolutionId = "solution-123",
            ProblemId = "problem-123",
            Applied = true,
            Passed = true,
            BuildExitCode = 0,
            TestExitCode = 0,
            BuildOutput = "Build succeeded",
            TestOutput = "Tests passed",
            EvaluationTimeMs = 2000,
            Timestamp = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero),
            Metadata = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }
        };

        // Act
        var json = evaluation.ToJson();
        var deserialized = json.FromJson<EvaluationResult>();

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("eval-123", deserialized!.Id);
        Assert.Equal("solution-123", deserialized.SolutionId);
        Assert.True(deserialized.Applied);
        Assert.True(deserialized.Passed);
        Assert.Equal(0, deserialized.BuildExitCode);
        Assert.Equal(2, deserialized.Metadata.Count);
        Assert.Equal("value1", deserialized.Metadata["key1"]);
    }

    [Fact]
    public void JsonlSerialization_Works()
    {
        // Arrange
        var solution1 = new CandidateSolution
        {
            Id = "solution-1",
            ProblemId = "problem-1",
            CandidateIndex = 1
        };
        
        var solution2 = new CandidateSolution
        {
            Id = "solution-2",
            ProblemId = "problem-1",
            CandidateIndex = 2
        };
        
        var filePath = Path.Combine(_testDir, "test.jsonl");
        
        // Act
        solution1.AppendToJsonlFile(filePath);
        solution2.AppendToJsonlFile(filePath);
        
        var deserialized = filePath.ReadJsonlFile<CandidateSolution>();

        // Assert
        Assert.Equal(2, deserialized.Count);
        Assert.Equal("solution-1", deserialized[0].Id);
        Assert.Equal("solution-2", deserialized[1].Id);
    }
    
    [Fact]
    public void BenchmarkResult_SerializationWorks()
    {
        // Arrange
        var result = new BenchmarkResult
        {
            Id = "benchmark-123",
            StartTime = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2023, 1, 1, 13, 0, 0, TimeSpan.Zero),
            TotalElapsedMs = 3600000,
            Config = new BenchmarkConfig
            {
                ShardCount = 4,
                ShardId = 1,
                CandidatesPerProblem = 8
            },
            SuccessfulCount = 5,
            TotalCount = 10,
            AgentVersion = "1.0.0",
            RunnerVersion = "1.0.0",
            Results = new Dictionary<string, EnsembleResult>
            {
                ["problem-1"] = new EnsembleResult
                {
                    ProblemId = "problem-1",
                    SelectedSolutionId = "solution-1"
                }
            }
        };
        
        // Act
        var json = result.ToJson();
        var deserialized = json.FromJson<BenchmarkResult>();
        
        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("benchmark-123", deserialized!.Id);
        Assert.Equal(5, deserialized.SuccessfulCount);
        Assert.Equal(10, deserialized.TotalCount);
        Assert.Equal(50.0, deserialized.SuccessRate);
        Assert.Single(deserialized.Results);
        Assert.Equal("problem-1", deserialized.Results["problem-1"].ProblemId);
        Assert.Equal(4, deserialized.Config.ShardCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }
}