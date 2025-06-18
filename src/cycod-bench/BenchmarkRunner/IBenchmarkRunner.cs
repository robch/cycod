using CycodBench.Models;

namespace CycodBench.BenchmarkRunner;

/// <summary>
/// Interface for the benchmark runner that orchestrates the execution of SWE-bench problems.
/// </summary>
public interface IBenchmarkRunner
{
    /// <summary>
    /// Runs the benchmark with the specified parameters.
    /// </summary>
    /// <param name="shardCount">Total number of shards to split the dataset into.</param>
    /// <param name="shardId">The ID of the current shard (0 to ShardCount-1).</param>
    /// <param name="candidateCount">Number of candidate solutions to generate per problem.</param>
    /// <param name="parallelism">Level of parallelism for processing problems.</param>
    /// <param name="problemLimit">Optional limit on the number of problems to process.</param>
    /// <param name="cancellationToken">Cancellation token for stopping the benchmark run.</param>
    /// <returns>A BenchmarkResult containing the results of the benchmark run.</returns>
    Task<BenchmarkResult> RunAsync(
        int shardCount,
        int shardId,
        int candidateCount,
        int parallelism,
        int? problemLimit = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the problems assigned to the specified shard.
    /// </summary>
    /// <param name="shardId">The ID of the shard (0 to ShardCount-1).</param>
    /// <param name="shardCount">Total number of shards.</param>
    /// <param name="limit">Optional limit on the number of problems to return.</param>
    /// <returns>A list of SwebenchProblem instances assigned to the shard.</returns>
    List<SwebenchProblem> GetShardProblems(
        int shardId, 
        int shardCount, 
        int? limit = null);
    
    /// <summary>
    /// Calculates performance metrics from a benchmark result.
    /// </summary>
    /// <param name="result">The benchmark result to analyze.</param>
    /// <returns>A BenchmarkMetrics instance containing calculated metrics.</returns>
    BenchmarkMetrics CalculateMetrics(BenchmarkResult result);
}