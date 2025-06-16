# ShardManager.cs

This class is responsible for handling the distribution of SWE-bench problems across multiple shards for distributed processing.

## Responsibilities

- Determine which problems should be processed in the current shard
- Calculate fair and balanced distribution of problems
- Support reproducible sharding (consistent assignments across runs)
- Handle edge cases (uneven distribution, empty shards, etc.)
- Provide utilities for working with sharded results

## Public Interface

```csharp
public interface IShardManager
{
    List<SwebenchProblem> GetShardProblems(
        List<SwebenchProblem> allProblems,
        int shardCount,
        int shardId);
        
    int CalculateTotalShardCount(int problemCount, int minProblemsPerShard);
    
    bool ValidateShardConfig(
        int problemCount,
        int shardCount,
        int shardId,
        out string errorMessage);
        
    Task<List<string>> ListShardResultFiles(
        string resultsDirectory,
        string pattern = "shard-*.jsonl",
        CancellationToken cancellationToken = default);
}
```

## Implementation

```csharp
public class ShardManager
{
    // Constructor
    public ShardManager(ILogger logger);
    
    // Core sharding functionality
    public List<SwebenchProblem> GetShardProblems(
        List<SwebenchProblem> allProblems,
        int shardCount,
        int shardId);
    
    // Utility methods for sharding
    public int CalculateTotalShardCount(int problemCount, int minProblemsPerShard);
    
    public bool ValidateShardConfig(
        int problemCount,
        int shardCount,
        int shardId,
        out string errorMessage);
        
    // Methods for working with sharded results
    public async Task<List<string>> ListShardResultFiles(
        string resultsDirectory,
        string pattern = "shard-*.jsonl",
        CancellationToken cancellationToken = default);
}
```

## Implementation Overview

The ShardManager class will:

1. **Calculate shard assignments**:
   - Use a deterministic algorithm to assign problems to shards
   - Ensure an even distribution of problems across shards when possible
   - Handle cases where the number of problems doesn't divide evenly by shard count

2. **Filter problems for the current shard**:
   - Given all problems, filter to only those that should be processed in the current shard
   - Support zero-indexed shard IDs (0 to shardCount-1)
   - Validate that the shard ID is valid before processing

3. **Provide utility methods**:
   - Calculate optimal shard counts based on problem count and desired problems per shard
   - Validate shard configurations before execution
   - Support listing and gathering results from completed shards

## Sharding Algorithm

The ShardManager will implement a sharding algorithm that:
- Ensures deterministic assignment of problems to shards across different runs
- Distributes problems evenly across shards when possible
- Works with both ordered and unordered problem lists
- Handles edge cases like single problem, single shard, etc.

By default, the algorithm will:
1. Sort problems by ID to ensure consistency
2. Assign problems to shards in round-robin fashion or by ranges based on configuration
3. For the current shard, filter the problem list to include only assigned problems

## Error Handling

The ShardManager will:
- Validate shard configuration parameters before processing
- Detect and report invalid shard IDs (out of range)
- Handle empty problem lists gracefully
- Provide detailed error messages for invalid configurations

## Future Extensibility

The ShardManager design allows for:
- Alternative sharding strategies (e.g., by problem difficulty, by repository)
- Dynamic resharding if a shard fails or needs to be reprocessed
- Support for partial shards (e.g., processing only a subset of a shard)
- Integration with distributed computing frameworks