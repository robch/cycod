using CycodBench.DatasetManager;
using CycodBench.Logging;
using CycodBench.Models;
using CycodBench.ProblemProcessor;
using CycodBench.ResultManager;
using CycodBench.ShardManager;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CycodBench.BenchmarkRunner;

/// <summary>
/// Implementation of the benchmark runner that orchestrates the execution of SWE-bench problems.
/// </summary>
public class BenchmarkRunner : IBenchmarkRunner
{
    private readonly IDatasetManager _datasetManager;
    private readonly IShardManager _shardManager;
    private readonly IProblemProcessor _problemProcessor;
    private readonly IResultManager _resultManager;
    private readonly ILogger _logger;
    private readonly Configuration.IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the BenchmarkRunner class.
    /// </summary>
    /// <param name="datasetManager">The dataset manager for loading SWE-bench problems.</param>
    /// <param name="shardManager">The shard manager for problem distribution.</param>
    /// <param name="problemProcessor">The problem processor for generating candidate solutions.</param>
    /// <param name="resultManager">The result manager for storing benchmark results.</param>
    /// <param name="logger">The logger for capturing benchmark execution information.</param>
    /// <param name="config">The configuration for the benchmark runner.</param>
    public BenchmarkRunner(
        IDatasetManager datasetManager,
        IShardManager shardManager,
        IProblemProcessor problemProcessor,
        IResultManager resultManager,
        ILogger logger,
        Configuration.IConfiguration config)
    {
        _datasetManager = datasetManager ?? throw new ArgumentNullException(nameof(datasetManager));
        _shardManager = shardManager ?? throw new ArgumentNullException(nameof(shardManager));
        _problemProcessor = problemProcessor ?? throw new ArgumentNullException(nameof(problemProcessor));
        _resultManager = resultManager ?? throw new ArgumentNullException(nameof(resultManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    public async Task<BenchmarkResult> RunAsync(
        int shardCount,
        int shardId,
        int candidateCount,
        int parallelism,
        int? problemLimit = null,
        CancellationToken cancellationToken = default)
    {
        // Create benchmark result object
        var benchmarkResult = new BenchmarkResult
        {
            Id = Guid.NewGuid().ToString(),
            StartTime = DateTimeOffset.UtcNow,
            Config = new BenchmarkConfig
            {
                ShardCount = shardCount,
                ShardId = shardId,
                CandidatesPerProblem = candidateCount,
                Parallelism = parallelism,
                AgentPath = _config.AgentPath,
                AgentTimeoutMs = _config.AgentTimeoutMs,
                EvaluationTimeoutMs = _config.EvaluationTimeoutMs,
                WorkspacePath = _config.WorkspaceDirectory,
                ResultsPath = _config.ResultsDirectory,
                ContainerMemoryLimit = "8g",
                ContainerCpuLimit = 4,
                KeepWorkspaces = _config.KeepWorkspaces
            },
            RunnerVersion = GetType().Assembly.GetName().Version?.ToString() ?? "0.0.0",
            AgentVersion = GetAgentVersion()
        };

        _logger.Info($"Starting benchmark run {benchmarkResult.Id}");
        _logger.Info($"Configuration: ShardCount={shardCount}, ShardId={shardId}, CandidateCount={candidateCount}, Parallelism={parallelism}");

        try
        {
            // Get dataset type from config (default to verified)
            string datasetType = _config.GetString("DatasetType", "verified");
            _logger.Info($"Loading problems from {datasetType} dataset...");

            // Check if dataset is downloaded
            if (!_datasetManager.IsDatasetCached(datasetType))
            {
                throw new InvalidOperationException($"Dataset '{datasetType}' not found. Please download it first using the 'download {datasetType}' command.");
            }

            // Load all problems from the dataset
            var allProblems = await _datasetManager.GetProblemsAsync(datasetType);
            _logger.Info($"Loaded {allProblems.Count} total problems from dataset");

            // Get problems for this shard
            var shardProblems = GetShardProblems(shardId, shardCount, problemLimit);
            _logger.Info($"Processing {shardProblems.Count} problems in shard {shardId} of {shardCount}");

            if (shardProblems.Count == 0)
            {
                _logger.Warning($"No problems were assigned to shard {shardId}. Check your shard configuration.");
                return benchmarkResult;
            }

            // Create a stopwatch to measure total execution time
            var stopwatch = Stopwatch.StartNew();

            // Process the problems in parallel
            var results = new ConcurrentDictionary<string, List<CandidateSolution>>();
            var semaphore = new SemaphoreSlim(parallelism);
            var tasks = new List<Task>();

            foreach (var problem in shardProblems)
            {
                // Throttle parallelism using a semaphore
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        _logger.Info($"Processing problem {problem.Id} ({problem.Repository})");
                        var solutions = await _problemProcessor.ProcessProblemAsync(problem, candidateCount);
                        
                        // Save each solution to disk
                        foreach (var solution in solutions)
                        {
                            await _resultManager.SaveCandidateSolutionAsync(solution);
                        }
                        
                        // Store the solutions in memory for the final result
                        results[problem.Id] = solutions.ToList();
                        _logger.Info($"Completed problem {problem.Id} with {solutions.Count()} candidate solutions");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to process problem {problem.Id}: {ex.Message}");
                        // Add an empty result for this problem to indicate failure
                        results[problem.Id] = new List<CandidateSolution>();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Update the benchmark result with information about the run
            stopwatch.Stop();
            benchmarkResult.EndTime = DateTimeOffset.UtcNow;
            benchmarkResult.TotalElapsedMs = stopwatch.ElapsedMilliseconds;
            benchmarkResult.TotalCount = shardProblems.Count;

            // Count successful solutions (any problem with at least one passing solution)
            int successfulProblemCount = results.Count(r => r.Value.Any(s => s.EvaluationResult != null && s.EvaluationResult.Passed));
            benchmarkResult.SuccessfulCount = successfulProblemCount;

            // Save the results to disk
            string outputPath = _resultManager.GetShardResultsPath(shardId);
            await _resultManager.SaveBenchmarkResultAsync(benchmarkResult, outputPath);

            _logger.Info($"Benchmark run completed in {benchmarkResult.TotalElapsedMs / 1000:F1} seconds");
            _logger.Info($"Success rate: {benchmarkResult.SuccessRate:F2}% ({benchmarkResult.SuccessfulCount}/{benchmarkResult.TotalCount})");
            _logger.Info($"Results saved to {outputPath}");

            return benchmarkResult;
        }
        catch (Exception ex)
        {
            _logger.Error($"Benchmark run failed: {ex.Message}");
            benchmarkResult.EndTime = DateTimeOffset.UtcNow;
            return benchmarkResult;
        }
    }

    /// <inheritdoc/>
    public List<SwebenchProblem> GetShardProblems(int shardId, int shardCount, int? limit = null)
    {
        // Get dataset type from config (default to verified)
        string datasetType = _config.GetString("DatasetType", "verified");
        
        // Load all problems from the dataset
        var allProblems = _datasetManager.GetProblemsAsync(datasetType).Result;
        
        // Use the shard manager to filter problems for this shard
        var shardProblems = _shardManager.GetProblemsForShard(allProblems, shardId).ToList();
        
        // Apply limit if specified
        if (limit.HasValue && limit.Value > 0 && limit.Value < shardProblems.Count)
        {
            shardProblems = shardProblems.Take(limit.Value).ToList();
        }
        
        return shardProblems;
    }

    /// <inheritdoc/>
    public BenchmarkMetrics CalculateMetrics(BenchmarkResult result)
    {
        var metrics = new BenchmarkMetrics
        {
            SuccessRate = result.SuccessRate,
            SuccessfulCount = result.SuccessfulCount,
            TotalCount = result.TotalCount
        };

        // Calculate average time per problem
        if (result.TotalCount > 0)
        {
            metrics.AverageTimePerProblemMs = (double)result.TotalElapsedMs / result.TotalCount;
        }

        // Initialize the histogram
        metrics.SolutionHistogram = new Dictionary<string, int>
        {
            { "passed", 0 },
            { "failed", 0 },
            { "error", 0 },
            { "timeout", 0 }
        };

        // TODO: Calculate additional metrics once we have per-candidate timing data
        // metrics.AverageAgentTimeMs
        // metrics.AverageEvaluationTimeMs
        // metrics.AverageCandidatesPerSuccess
        // metrics.AverageTestPasses
        // metrics.AverageTestFailures

        return metrics;
    }

    private string GetAgentVersion()
    {
        try
        {
            // Try to get the agent version by running cycod --version
            var startInfo = new ProcessStartInfo
            {
                FileName = _config.AgentPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                // Extract version from output
                if (!string.IsNullOrEmpty(output))
                {
                    return output.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Debug($"Failed to get agent version: {ex.Message}");
        }

        return "unknown";
    }
}