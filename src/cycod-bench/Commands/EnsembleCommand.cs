using CycodBench.DatasetManager;
using CycodBench.EnsemblerService;
using CycodBench.ResultManager;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Commands;

/// <summary>
/// Handler for the ensemble command.
/// </summary>
public static class EnsembleCommand
{
    /// <summary>
    /// Handles the ensemble command.
    /// </summary>
    public static async Task<int> HandleCommand(string[] args, IServiceProvider services)
    {
        try
        {
            var logger = services.GetRequiredService<Logging.ILogger>();
            var resultManager = services.GetRequiredService<IResultManager>();
            var datasetManager = services.GetRequiredService<IDatasetManager>();
            var ensemblerService = services.GetRequiredService<IEnsemblerService>();
            var config = services.GetRequiredService<Configuration.IConfiguration>();
            
            string? inputPath = null;
            string? outputPath = null;
            
            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if ((args[i] == "--input" || args[i] == "-i") && i + 1 < args.Length)
                {
                    inputPath = args[i + 1];
                    i++;
                }
                else if ((args[i] == "--output" || args[i] == "-o") && i + 1 < args.Length)
                {
                    outputPath = args[i + 1];
                    i++;
                }
            }
            
            // Validate inputs
            if (string.IsNullOrEmpty(inputPath))
            {
                logger.Error("No input path specified. Use --input <file> to specify the merged results file.");
                return 1;
            }
            
            // Validate input file
            if (!File.Exists(inputPath))
            {
                logger.Error($"Input file not found: {inputPath}");
                return 1;
            }
            
            // Set default output path if not specified
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.Combine(resultManager.GetDefaultResultsDirectory(), "ensemble_results.json");
                logger.Info($"No output path specified. Using default: {outputPath}");
            }
            
            // Load the dataset to get the problems
            logger.Info("Loading dataset...");
            var allDatasetProblems = await datasetManager.GetProblemsAsync("verified");
            var problemsById = allDatasetProblems.ToDictionary(p => p.Id, p => p);
            
            // Load all candidate solutions
            logger.Info($"Loading candidate solutions from {inputPath}...");
            var allSolutions = await resultManager.GetAllCandidateSolutionsAsync(inputPath);
            var solutionsByProblemId = allSolutions.GroupBy(s => s.ProblemId)
                                                 .ToDictionary(g => g.Key, g => g.ToList());
            
            // Create a benchmark config for the report
            var benchmarkConfig = new Models.BenchmarkConfig
            {
                ShardCount = config.ShardCount,
                CandidatesPerProblem = config.CandidateCount,
                Parallelism = config.Parallelism,
                AgentPath = config.AgentPath,
                AgentTimeoutMs = config.AgentTimeoutMs,
                EvaluationTimeoutMs = config.EvaluationTimeoutMs,
                ResultsPath = inputPath,
                WorkspacePath = config.WorkspaceDirectory
            };
            
            // Generate the report
            logger.Info("Generating ensemble report...");
            var benchmarkResult = await resultManager.GenerateReportAsync(inputPath, outputPath, benchmarkConfig);
            
            logger.Info($"Ensemble completed successfully. Results saved to {outputPath}");
            logger.Info($"Success rate: {benchmarkResult.SuccessRate:F2}% ({benchmarkResult.SuccessfulCount}/{benchmarkResult.TotalCount})");
            
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to run ensemble: {ex.Message}");
            return 1;
        }
    }
}
