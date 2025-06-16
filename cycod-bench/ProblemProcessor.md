# ProblemProcessor.cs

This class handles the processing of individual SWE-bench problems, managing the generation of multiple candidate solutions.

## Responsibilities

- Setup problem-specific workspace
- Coordinate the generation of multiple candidate solutions
- Manage Docker containers for each solution attempt
- Execute the agent for each candidate solution
- Collect and organize results from agent executions
- Handle timeouts and resource cleanup
- Implement retries for failed attempts

## Public Interface

```csharp
public interface IProblemProcessor
{
    Task<ProblemResult> ProcessProblemAsync(
        SwebenchProblem problem,
        int candidateCount,
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    Task<CandidateSolution> ProcessCandidateSolutionAsync(
        SwebenchProblem problem,
        int candidateIndex,
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    EvaluationResult EvaluateCandidate(
        SwebenchProblem problem,
        CandidateSolution candidateSolution,
        DirectoryInfo workspaceDir);
}
```

## Implementation

```csharp
public class ProblemProcessor
{
    // Constructor
    public ProblemProcessor(
        IDockerManager dockerManager,
        IAgentExecutor agentExecutor, 
        IEvaluationService evaluationService,
        ILogger logger,
        ProcessorConfiguration config);
    
    // Main processing method
    public async Task<ProblemResult> ProcessProblemAsync(
        SwebenchProblem problem,
        int candidateCount,
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    // Process a single candidate solution
    public async Task<CandidateSolution> ProcessCandidateSolutionAsync(
        SwebenchProblem problem,
        int candidateIndex,
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    // Get evaluation results
    public EvaluationResult EvaluateCandidate(
        SwebenchProblem problem,
        CandidateSolution candidateSolution,
        DirectoryInfo workspaceDir);
}
```

## Implementation Overview

The ProblemProcessor class will:

1. **Setup for each problem**:
   - Create a problem-specific directory structure
   - Prepare for parallel processing of candidate solutions
   - Initialize tracking and logging for the problem

2. **Process each candidate solution in parallel (controlled by parallelism setting)**:
   - Request a Docker container for the problem from DockerManager
   - Setup the container with necessary files and environment
   - Execute the agent inside the container via AgentExecutor
   - Capture agent outputs (stdout, stderr, logs)
   - Extract the diff produced by the agent
   - Evaluate the solution via EvaluationService
   - Cleanup container resources

3. **Collect and organize results**:
   - Gather all candidate solutions and their evaluation results
   - Collect metrics (execution time, memory usage, etc.)
   - Create a comprehensive problem result object

## Workspace Structure

For each problem, the ProblemProcessor will create a directory structure like:
```
workspace/
  └── problem-id/
      ├── candidate-0/
      │   ├── logs/
      │   │   ├── agent.log
      │   │   └── container.log
      │   ├── diff.patch
      │   └── evaluation.json
      ├── candidate-1/
      │   └── ...
      └── ...
```

## Error Handling and Retries

The ProblemProcessor will:
- Implement a configurable retry policy for transient failures
- Handle Docker-specific errors (container creation failures, etc.)
- Implement timeouts for agent execution
- Gracefully handle agent crashes or failures
- Ensure resources are cleaned up even when errors occur

## Performance Considerations

The ProblemProcessor will:
- Use a semaphore to control parallel execution of candidates
- Monitor resource usage and adjust execution accordingly
- Implement efficient logging that doesn't impact performance
- Use async/await pattern for I/O-bound operations

## Metrics Collection

The ProblemProcessor will collect detailed metrics for each candidate solution:
- Total execution time
- Agent execution time
- Evaluation time
- Peak memory usage
- CPU utilization
- Success/failure status
- Error counts and types

These metrics will help in analyzing agent performance and identifying bottlenecks.