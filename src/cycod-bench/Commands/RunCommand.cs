using CycodBench.BenchmarkRunner;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Commands;

/// <summary>
/// Handler for the run command.
/// </summary>
public static class RunCommand
{
    /// <summary>
    /// Handles the run command.
    /// </summary>
    public static async Task<int> HandleCommand(string[] args, IServiceProvider services)
    {
        try
        {
            var logger = services.GetRequiredService<Logging.ILogger>();
            var benchmarkRunner = services.GetRequiredService<IBenchmarkRunner>();
            var datasetManager = services.GetRequiredService<DatasetManager.IDatasetManager>();
            var configuration = services.GetRequiredService<Configuration.IConfiguration>();
            
            // Parse command arguments
            int shardCount = configuration.ShardCount;
            int shardId = configuration.ShardId;
            int candidates = configuration.CandidateCount;
            int parallelism = configuration.Parallelism;
            int? problemLimit = null;
            string datasetType = "verified";
            
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--shard-count" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedShardCount))
                {
                    shardCount = parsedShardCount;
                    i++;
                }
                else if (args[i] == "--shard-id" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedShardId))
                {
                    shardId = parsedShardId;
                    i++;
                }
                else if (args[i] == "--candidates" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedCandidates))
                {
                    candidates = parsedCandidates;
                    i++;
                }
                else if (args[i] == "--parallelism" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedParallelism))
                {
                    parallelism = parsedParallelism;
                    i++;
                }
                else if (args[i] == "--problems" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedProblems))
                {
                    problemLimit = parsedProblems;
                    i++;
                }
                else if (args[i] == "--dataset" && i + 1 < args.Length)
                {
                    datasetType = args[i + 1];
                    i++;
                }
            }
            
            logger.Info($"Running benchmark with configuration:");
            logger.Info($"  Shard: {shardId+1}/{shardCount}");
            logger.Info($"  Candidates per problem: {candidates}");
            logger.Info($"  Parallelism: {parallelism}");
            logger.Info($"  Problem limit: {(problemLimit.HasValue ? problemLimit.ToString() : "all")}");
            logger.Info($"  Dataset: {datasetType}");

            // Verify that dataset exists
            if (!datasetManager.IsDatasetCached(datasetType))
            {
                logger.Error($"Dataset '{datasetType}' not found. Please download it first using the 'download {datasetType}' command.");
                return 1;
            }

            // Run the benchmark
            var result = await benchmarkRunner.RunAsync(
                shardCount,
                shardId,
                candidates,
                parallelism,
                problemLimit);

            // Calculate and display metrics
            var metrics = benchmarkRunner.CalculateMetrics(result);
            
            logger.Info("Benchmark run completed successfully.");
            logger.Info($"Success rate: {metrics.SuccessRate:F2}% ({metrics.SuccessfulCount}/{metrics.TotalCount})");
            logger.Info($"Average time per problem: {metrics.AverageTimePerProblemMs / 1000:F1} seconds");
            
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to run benchmark: {ex.Message}");
            return 1;
        }
    }
}
