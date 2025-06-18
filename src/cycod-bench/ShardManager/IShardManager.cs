using CycodBench.Models;

namespace CycodBench.ShardManager;

/// <summary>
/// Interface for managing the sharding of SWE-bench problems.
/// </summary>
public interface IShardManager
{
    /// <summary>
    /// Gets the total number of shards.
    /// </summary>
    int ShardCount { get; }

    /// <summary>
    /// Gets the current shard ID.
    /// </summary>
    int ShardId { get; }

    /// <summary>
    /// Gets the problems assigned to the current shard.
    /// </summary>
    /// <param name="allProblems">The complete list of problems.</param>
    /// <returns>A filtered list of problems assigned to the current shard.</returns>
    IEnumerable<SwebenchProblem> GetProblemsForCurrentShard(IEnumerable<SwebenchProblem> allProblems);

    /// <summary>
    /// Gets the problems assigned to a specific shard.
    /// </summary>
    /// <param name="allProblems">The complete list of problems.</param>
    /// <param name="shardId">The shard ID to get problems for.</param>
    /// <returns>A filtered list of problems assigned to the specified shard.</returns>
    IEnumerable<SwebenchProblem> GetProblemsForShard(IEnumerable<SwebenchProblem> allProblems, int shardId);

    /// <summary>
    /// Determines if a problem belongs to the current shard.
    /// </summary>
    /// <param name="problem">The problem to check.</param>
    /// <returns>True if the problem belongs to the current shard, false otherwise.</returns>
    bool IsInCurrentShard(SwebenchProblem problem);

    /// <summary>
    /// Saves the current shard state to disk.
    /// </summary>
    /// <param name="problems">The problems assigned to the current shard.</param>
    /// <param name="outputPath">The path to save the shard state to.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveShardStateAsync(IEnumerable<SwebenchProblem> problems, string outputPath);

    /// <summary>
    /// Loads the shard state from disk.
    /// </summary>
    /// <param name="inputPath">The path to load the shard state from.</param>
    /// <returns>A task resulting in the loaded problems for the current shard.</returns>
    Task<IEnumerable<SwebenchProblem>> LoadShardStateAsync(string inputPath);

    /// <summary>
    /// Calculates the shard ID for a specific problem.
    /// </summary>
    /// <param name="problem">The problem to calculate the shard ID for.</param>
    /// <returns>The shard ID for the problem.</returns>
    int CalculateShardId(SwebenchProblem problem);
}