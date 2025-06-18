using CycodBench.Models;

namespace CycodBench.ResultManager;

/// <summary>
/// Interface for managing benchmark results.
/// </summary>
public interface IResultManager
{
    /// <summary>
    /// Saves a candidate solution to the results storage.
    /// </summary>
    /// <param name="solution">The candidate solution to save.</param>
    /// <param name="outputPath">Optional path to save the result to. If not provided, uses the default path.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveCandidateSolutionAsync(CandidateSolution solution, string? outputPath = null);
    
    /// <summary>
    /// Gets all candidate solutions for a specific problem.
    /// </summary>
    /// <param name="problemId">The ID of the problem to get solutions for.</param>
    /// <param name="inputPath">Optional path to load results from. If not provided, uses the default path.</param>
    /// <returns>A list of candidate solutions for the problem.</returns>
    Task<IEnumerable<CandidateSolution>> GetCandidateSolutionsAsync(string problemId, string? inputPath = null);
    
    /// <summary>
    /// Gets all candidate solutions from a specific results file.
    /// </summary>
    /// <param name="inputPath">The path to load results from.</param>
    /// <returns>A list of candidate solutions.</returns>
    Task<IEnumerable<CandidateSolution>> GetAllCandidateSolutionsAsync(string inputPath);
    
    /// <summary>
    /// Merges results from multiple shards into a single results file.
    /// </summary>
    /// <param name="inputPaths">The paths to the shard results files.</param>
    /// <param name="outputPath">The path to save the merged results to.</param>
    /// <returns>A task representing the asynchronous merge operation.</returns>
    Task<int> MergeResultsAsync(IEnumerable<string> inputPaths, string outputPath);
    
    /// <summary>
    /// Generates a final benchmark report from a results file.
    /// </summary>
    /// <param name="inputPath">The path to the results file.</param>
    /// <param name="outputPath">The path to save the report to.</param>
    /// <param name="config">The benchmark configuration used for the run.</param>
    /// <returns>A task resulting in the benchmark result.</returns>
    Task<BenchmarkResult> GenerateReportAsync(string inputPath, string outputPath, BenchmarkConfig config);
    
    /// <summary>
    /// Gets the default output directory for results.
    /// </summary>
    /// <returns>The path to the default output directory.</returns>
    string GetDefaultResultsDirectory();
    
    /// <summary>
    /// Gets the path to the results file for a specific shard.
    /// </summary>
    /// <param name="shardId">The shard ID.</param>
    /// <returns>The path to the shard results file.</returns>
    string GetShardResultsPath(int shardId);
    
    /// <summary>
    /// Saves a benchmark result to the specified output path.
    /// </summary>
    /// <param name="result">The benchmark result to save.</param>
    /// <param name="outputPath">The path to save the result to.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveBenchmarkResultAsync(BenchmarkResult result, string outputPath);
}