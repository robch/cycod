# Models

This document describes the core data models used throughout the CycodBench system. These models represent the fundamental data structures for problems, solutions, evaluation results, and other entities.

## Key Models

## SwebenchProblem

Represents a single SWE-bench problem to be solved.

```csharp
public class SwebenchProblem
{
    public string Id { get; set; }
    public string Repository { get; set; }
    public string ProblemStatement { get; set; }
    public string BaseImage { get; set; }
    public List<string> TestFiles { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

## CandidateSolution

Represents a single solution attempt for a problem.

```csharp
public class CandidateSolution
{
    public int Index { get; set; }
    public string Diff { get; set; }
    public string AgentLogs { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public Dictionary<string, double> ResourceMetrics { get; set; }
    public EvaluationResult Evaluation { get; set; }
}
```

## EvaluationResult

Contains the results of evaluating a solution against test cases.

```csharp
public class EvaluationResult
{
    public bool IsSuccess { get; set; }
    public List<TestCaseResult> TestCases { get; set; }
    public string BuildOutput { get; set; }
    public TimeSpan EvaluationTime { get; set; }
    public Dictionary<string, string> Diagnostics { get; set; }
}
```

## TestCaseResult

Represents the result of a single test case.

```csharp
public class TestCaseResult
{
    public string Name { get; set; }
    public TestType Type { get; set; } // FAIL_TO_PASS or PASS_TO_PASS
    public bool Passed { get; set; }
    public string Output { get; set; }
}
```

## ProblemResult

Aggregates all candidate solutions for a single problem.

```csharp
public class ProblemResult
{
    public SwebenchProblem Problem { get; set; }
    public List<CandidateSolution> CandidateSolutions { get; set; }
    public TimeSpan TotalProcessingTime { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

## BenchmarkResult

Represents the results of an entire benchmark run.

```csharp
public class BenchmarkResult
{
    public int ShardId { get; set; }
    public int ShardCount { get; set; }
    public List<ProblemResult> ProblemResults { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public DateTime ExecutedAt { get; set; }
    public BenchmarkMetrics Metrics { get; set; }
    public Dictionary<string, string> Configuration { get; set; }
}
```

## EnsembleResult

Contains the results of the ensembling process.

```csharp
public class EnsembleResult
{
    public List<ProblemEnsembleResult> ProblemResults { get; set; }
    public EnsembleMetrics Metrics { get; set; }
    public DateTime EnsembledAt { get; set; }
}
```

## ProblemEnsembleResult

Represents the ensemble result for a single problem.

```csharp
public class ProblemEnsembleResult
{
    public string ProblemId { get; set; }
    public string ProblemStatement { get; set; }
    public int SelectedSolutionIndex { get; set; }
    public string SelectedDiff { get; set; }
    public bool IsSuccess { get; set; }
    public string EnsemblerReasoning { get; set; }
}
```

## DockerContainer

Represents a Docker container.

```csharp
public class DockerContainer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public Dictionary<string, string> Volumes { get; set; }
    public Dictionary<string, string> Environment { get; set; }
    public DateTime CreatedAt { get; set; }
    public ContainerStatus Status { get; set; }
}
```

## CommandResult

Represents the result of executing a command.

```csharp
public class CommandResult
{
    public int ExitCode { get; set; }
    public string StdOut { get; set; }
    public string StdErr { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public bool TimedOut { get; set; }
}
```

## BenchmarkMetrics

Contains metrics for a benchmark run.

```csharp
public class BenchmarkMetrics
{
    public int TotalProblems { get; set; }
    public int SuccessfulProblems { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, int> ResultsByRepository { get; set; }
    public Dictionary<string, double> AverageExecutionTimes { get; set; }
    public Dictionary<string, double> ResourceUsageMetrics { get; set; }
}
```

## EnsembleMetrics

Contains metrics for the ensemble process.

```csharp
public class EnsembleMetrics
{
    public int TotalProblems { get; set; }
    public int SuccessfulProblems { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<int, int> SelectionDistribution { get; set; }
    public int ImprovementsFromEnsembling { get; set; }
}
```

## Configuration Models

The system also includes various configuration models:

```csharp
public class BenchmarkConfiguration { /* Configuration properties */ }
public class DockerConfiguration { /* Docker-specific settings */ }
public class AgentConfiguration { /* Agent execution settings */ }
public class EvaluationConfiguration { /* Evaluation settings */ }
public class EnsemblerConfiguration { /* Ensemble settings */ }
public class ResultConfiguration { /* Result management settings */ }
```

These models provide strong typing and structure for the various configuration options throughout the system.