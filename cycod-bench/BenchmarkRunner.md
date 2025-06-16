# BenchmarkRunner.cs

This class serves as the main orchestrator for running benchmark tests on SWE-bench problems.

## Responsibilities

- Coordinate the overall benchmark execution flow
- Load problems from the SWE-bench dataset
- Distribute problems across shards
- Process problems in parallel
- Track and report progress
- Handle errors and retry logic
- Collect and store results

## Public Interface

```csharp
public interface IBenchmarkRunner
{
    Task<BenchmarkResult> RunAsync(
        int shardCount,
        int shardId,
        int candidateCount,
        int parallelism,
        int? problemLimit = null,
        CancellationToken cancellationToken = default);
        
    List<SwebenchProblem> GetShardProblems(
        int shardId, 
        int shardCount, 
        int? limit = null);
        
    BenchmarkMetrics CalculateMetrics(BenchmarkResult result);
}
```

## Implementation

```csharp
public class BenchmarkRunner
{
    // Constructor
    public BenchmarkRunner(
        IDatasetManager datasetManager,
        IShardManager shardManager,
        IProblemProcessor problemProcessor,
        IResultManager resultManager,
        ILogger logger,
        BenchmarkConfiguration config);
    
    // Main execution method
    public async Task<BenchmarkResult> RunAsync(
        int shardCount,
        int shardId,
        int candidateCount,
        int parallelism,
        int? problemLimit = null,
        CancellationToken cancellationToken = default);
        
    // Get assigned problems for this shard
    public List<SwebenchProblem> GetShardProblems(
        int shardId, 
        int shardCount, 
        int? limit = null);
        
    // Calculate benchmark metrics
    public BenchmarkMetrics CalculateMetrics(BenchmarkResult result);
}
```

## Implementation Overview

The BenchmarkRunner class will:

1. **Initialize the benchmark run**:
   - Request problems from the DatasetManager
   - Use the ShardManager to determine which problems to process in the current shard
   - Set up the workspace directory structure
   - Initialize progress tracking

2. **Process problems in parallel**:
   - Use a semaphore to control the maximum parallelism
   - Create a thread-safe queue of problems to process
   - Create worker tasks to process problems from the queue
   - Implement a task tracking system to monitor progress and handle failures

3. **For each problem**:
   - Delegate to the ProblemProcessor to handle generating candidate solutions
   - Capture all results, logs, and metrics
   - Handle any exceptions that occur during processing

4. **Collect and store results**:
   - Delegate to ResultManager to store the results for each problem
   - Generate summary statistics and metrics
   - Create a shard-specific results file

5. **Clean up resources**:
   - Ensure all Docker containers are properly stopped and removed
   - Clean up temporary files
   - Release any system resources

## Progress Tracking

The BenchmarkRunner will implement a progress tracking system that:
- Displays a real-time console progress bar
- Reports estimated time remaining
- Shows current processing status for each problem
- Logs detailed information about each step

## Error Handling

The BenchmarkRunner will implement robust error handling that:
- Catches and logs exceptions for each problem
- Implements automatic retry logic for transient failures
- Provides detailed diagnostics for failed problems
- Ensures that failures in one problem don't affect others
- Creates a separate log of failed problems for later analysis

## Performance Considerations

The BenchmarkRunner will:
- Use asynchronous I/O operations wherever possible
- Implement cancellation support for graceful shutdown
- Monitor system resource usage and adjust parallelism if needed
- Batch operations where appropriate to reduce overhead