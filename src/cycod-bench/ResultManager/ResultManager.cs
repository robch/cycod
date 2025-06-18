using CycodBench.Configuration;
using CycodBench.Logging;
using CycodBench.Models;

namespace CycodBench.ResultManager;

/// <summary>
/// Implementation of the IResultManager interface for managing benchmark results.
/// </summary>
public class ResultManager : IResultManager
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultManager"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public ResultManager(IConfiguration config, ILogger logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SaveCandidateSolutionAsync(CandidateSolution solution, string? outputPath = null)
    {
        if (solution == null)
        {
            throw new ArgumentNullException(nameof(solution));
        }

        string resultPath = outputPath ?? GetShardResultsPath(_config.ShardId);
        
        try
        {
            var directory = Path.GetDirectoryName(resultPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Append the solution as a line in the JSONL file
            _logger.Debug($"Saving candidate solution {solution.Id} to {resultPath}");
            solution.AppendToJsonlFile(resultPath);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save candidate solution: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CandidateSolution>> GetCandidateSolutionsAsync(string problemId, string? inputPath = null)
    {
        if (string.IsNullOrEmpty(problemId))
        {
            throw new ArgumentException("Problem ID cannot be null or empty", nameof(problemId));
        }

        string resultPath = inputPath ?? GetShardResultsPath(_config.ShardId);
        
        if (!File.Exists(resultPath))
        {
            _logger.Warning($"Results file not found at {resultPath}");
            return Enumerable.Empty<CandidateSolution>();
        }

        try
        {
            _logger.Debug($"Loading candidate solutions for problem {problemId} from {resultPath}");
            var allSolutions = await GetAllCandidateSolutionsAsync(resultPath);
            return allSolutions.Where(s => s.ProblemId == problemId).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get candidate solutions: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CandidateSolution>> GetAllCandidateSolutionsAsync(string inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            throw new ArgumentException("Input path cannot be null or empty", nameof(inputPath));
        }

        if (!File.Exists(inputPath))
        {
            _logger.Warning($"Results file not found at {inputPath}");
            return Enumerable.Empty<CandidateSolution>();
        }

        try
        {
            _logger.Debug($"Loading all candidate solutions from {inputPath}");
            return inputPath.ReadJsonlFile<CandidateSolution>();
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to get all candidate solutions: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> MergeResultsAsync(IEnumerable<string> inputPaths, string outputPath)
    {
        if (inputPaths == null)
        {
            throw new ArgumentNullException(nameof(inputPaths));
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));
        }

        var pathsList = inputPaths.ToList();
        if (!pathsList.Any())
        {
            _logger.Warning("No input files provided for merging");
            return 0;
        }

        try
        {
            // Delete output file if it already exists
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _logger.Info($"Merging {pathsList.Count} result files to {outputPath}");
            int totalSolutions = 0;

            // Process one file at a time to avoid memory issues with large datasets
            foreach (var inputPath in pathsList)
            {
                if (!File.Exists(inputPath))
                {
                    _logger.Warning($"Results file not found at {inputPath}, skipping");
                    continue;
                }

                _logger.Debug($"Processing {inputPath}...");
                var solutions = await GetAllCandidateSolutionsAsync(inputPath);
                var solutionsList = solutions.ToList();
                
                // Append solutions to the output file
                foreach (var solution in solutionsList)
                {
                    solution.AppendToJsonlFile(outputPath);
                    totalSolutions++;
                }
                
                _logger.Debug($"Added {solutionsList.Count} solutions from {inputPath}");
            }

            _logger.Info($"Successfully merged {totalSolutions} solutions to {outputPath}");
            return totalSolutions;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to merge results: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BenchmarkResult> GenerateReportAsync(string inputPath, string outputPath, BenchmarkConfig config)
    {
        if (string.IsNullOrEmpty(inputPath))
        {
            throw new ArgumentException("Input path cannot be null or empty", nameof(inputPath));
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));
        }

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        try
        {
            _logger.Info($"Generating benchmark report from {inputPath}");
            
            // Load all candidate solutions
            var solutions = await GetAllCandidateSolutionsAsync(inputPath);
            var solutionsList = solutions.ToList();
            
            if (!solutionsList.Any())
            {
                _logger.Warning("No candidate solutions found for generating report");
                return new BenchmarkResult
                {
                    Config = config,
                    StartTime = DateTimeOffset.UtcNow,
                    EndTime = DateTimeOffset.UtcNow,
                };
            }

            // Group solutions by problem ID
            var solutionsByProblem = solutionsList
                .GroupBy(s => s.ProblemId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Create ensemble results for each problem
            var ensembleResults = new Dictionary<string, EnsembleResult>();
            int successfulCount = 0;

            foreach (var (problemId, problemSolutions) in solutionsByProblem)
            {
                // Find the best solution (in a real implementation, use the EnsemblerService)
                var bestSolution = problemSolutions
                    .Where(s => s.EvaluationResult != null && s.EvaluationResult.Passed)
                    .OrderBy(s => s.ExecutionTimeMs)
                    .FirstOrDefault();

                var ensembleResult = new EnsembleResult
                {
                    ProblemId = problemId,
                    CandidateSolutionIds = problemSolutions.Select(s => s.Id).ToList(),
                    SelectedSolutionId = bestSolution?.Id ?? string.Empty,
                    SelectionReason = bestSolution != null ? "Passed tests with fastest execution time" : "No successful solutions",
                    SelectedSolution = bestSolution,
                };

                ensembleResults.Add(problemId, ensembleResult);

                // Count successful problems (those with at least one passing test)
                if (bestSolution != null)
                {
                    successfulCount++;
                }
            }

            // Calculate timestamps and duration
            var timestamps = solutionsList
                .Select(s => s.Timestamp)
                .OrderBy(t => t)
                .ToList();

            var startTime = timestamps.FirstOrDefault();
            var endTime = timestamps.LastOrDefault();
            var totalElapsedMs = (long)(endTime - startTime).TotalMilliseconds;

            // Create the benchmark result
            var benchmarkResult = new BenchmarkResult
            {
                Config = config,
                StartTime = startTime,
                EndTime = endTime,
                TotalElapsedMs = totalElapsedMs,
                Results = ensembleResults,
                SuccessfulCount = successfulCount,
                TotalCount = solutionsByProblem.Count,
                AgentVersion = solutionsList.FirstOrDefault()?.AgentVersion ?? string.Empty,
                RunnerVersion = GetType().Assembly.GetName().Version?.ToString() ?? "0.0.0",
                Metadata = new Dictionary<string, string>
                {
                    { "GeneratedAt", DateTimeOffset.UtcNow.ToString("o") },
                    { "InputPath", inputPath },
                    { "TotalSolutions", solutionsList.Count.ToString() },
                }
            };

            // Save the benchmark result
            _logger.Debug($"Saving benchmark result to {outputPath} (directory exists: {Directory.Exists(Path.GetDirectoryName(outputPath))})");
            await SaveBenchmarkResultAsync(benchmarkResult, outputPath);
            
            // Verify the file was created
            if (!File.Exists(outputPath))
            {
                _logger.Warning($"File was not found after saving: {outputPath}");
                
                // Try direct method as a fallback
                try
                {
                    FileHelpers.WriteAllText(outputPath, benchmarkResult.ToJson());
                    _logger.Debug($"Fallback file write completed, file exists: {File.Exists(outputPath)}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Fallback save failed: {ex.Message}");
                }
            }
            
            _logger.Info($"Successfully generated benchmark report at {outputPath}");
            _logger.Info($"Success rate: {benchmarkResult.SuccessRate:F2}% ({benchmarkResult.SuccessfulCount}/{benchmarkResult.TotalCount})");

            return benchmarkResult;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to generate report: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public string GetDefaultResultsDirectory()
    {
        var resultDir = _config.ResultsDirectory;
        if (string.IsNullOrEmpty(resultDir))
        {
            resultDir = Path.Combine(AppContext.BaseDirectory, "results");
        }
        
        if (!Directory.Exists(resultDir))
        {
            Directory.CreateDirectory(resultDir);
        }
        
        return resultDir;
    }

    /// <inheritdoc />
    public string GetShardResultsPath(int shardId)
    {
        var resultDir = GetDefaultResultsDirectory();
        return Path.Combine(resultDir, $"shard-{shardId}.jsonl");
    }
    
    /// <inheritdoc />
    public async Task SaveBenchmarkResultAsync(BenchmarkResult result, string outputPath)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));
        }

        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save the result as a JSON file
            _logger.Debug($"Saving benchmark result to {outputPath}");
            var json = result.ToJson();
            FileHelpers.WriteAllText(outputPath, json);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save benchmark result: {ex.Message}");
            throw;
        }
    }
}