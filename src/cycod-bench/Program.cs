using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CycodBench.Configuration;
using CycodBench.DatasetManager;
using CycodBench.DockerManager;
using CycodBench.EvaluationToolsManager;
using CycodBench.Logging;
using System.CommandLine;

namespace CycodBench;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Setup dependency injection
        var serviceProvider = ConfigureServices(args);
        var logger = serviceProvider.GetRequiredService<CycodBench.Logging.ILogger>();
        var configuration = serviceProvider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
        
        logger.Info("CycodBench: SWE-bench Benchmark Runner for cycod agent");

        // Simple implementation for now until we can fully implement System.CommandLine
        if (args.Length == 0)
        {
            logger.Info("Usage:");
            logger.Info("  CycodBench setup - Install SWE-bench evaluation tools");
            logger.Info("  CycodBench download [verified|full|lite] - Download the SWE-bench dataset");
            logger.Info("  CycodBench list [verified|full|lite] - List problems in the dataset");
            logger.Info("  CycodBench run [options] - Run the benchmark");
            logger.Info("  CycodBench merge --input <files> --output <file> - Merge results from multiple shards");
            logger.Info("  CycodBench ensemble --input <file> --output <file> - Run ensemble on merged results");
            return 0;
        }

        var command = args[0].ToLower();
        
        try
        {
            switch (command)
            {
                case "setup":
                    logger.Info("Setting up evaluation tools...");
                    var evaluationToolsManager = serviceProvider.GetRequiredService<IEvaluationToolsManager>();
                    bool setupSuccess = await evaluationToolsManager.SetupToolsAsync();
                    if (!setupSuccess)
                    {
                        logger.Error("Failed to set up evaluation tools.");
                        return 1;
                    }
                    logger.Info("Evaluation tools setup completed successfully.");
                    break;
                case "download":
                    logger.Info("Downloading dataset...");
                    var datasetManager = serviceProvider.GetRequiredService<IDatasetManager>();
                    // Parse dataset type from args
                    string datasetType = "verified"; // Default to verified
                    if (args.Length > 1)
                    {
                        datasetType = args[1].ToLower();
                        if (datasetType != "verified" && datasetType != "full" && datasetType != "lite")
                        {
                            logger.Error($"Unknown dataset type: {datasetType}. Must be one of: verified, full, lite");
                            return 1;
                        }
                    }
                    bool downloadSuccess = await datasetManager.DownloadDatasetAsync(datasetType);
                    if (!downloadSuccess)
                    {
                        logger.Error($"Failed to download {datasetType} dataset.");
                        return 1;
                    }
                    logger.Info($"{datasetType} dataset downloaded successfully.");
                    break;
                case "run":
                    logger.Info("Running benchmark...");
                    // Run implementation will be added later
                    break;
                case "merge":
                    logger.Info("Merging results...");
                    // Merge implementation will be added later
                    break;
                case "list":
                    logger.Info("Listing problems in the dataset...");
                    var listDatasetManager = serviceProvider.GetRequiredService<IDatasetManager>();
                    
                    // Parse dataset type from args if provided
                    string listDatasetType = "verified"; // Default to verified
                    if (args.Length > 1)
                    {
                        listDatasetType = args[1].ToLower();
                        if (listDatasetType != "verified" && listDatasetType != "full" && listDatasetType != "lite")
                        {
                            logger.Error($"Unknown dataset type: {listDatasetType}. Must be one of: verified, full, lite");
                            return 1;
                        }
                    }
                    
                    // Check if dataset is downloaded
                    if (!listDatasetManager.IsDatasetCached(listDatasetType))
                    {
                        logger.Error($"Dataset '{listDatasetType}' not found. Please download it first using the 'download {listDatasetType}' command.");
                        return 1;
                    }
                    
                    // List problems
                    try
                    {
                        var problems = await listDatasetManager.GetProblemsAsync(listDatasetType);
                        
                        if (problems.Count == 0)
                        {
                            logger.Info($"No problems found in the {listDatasetType} dataset.");
                            return 0;
                        }
                        
                        logger.Info($"Found {problems.Count} problems in the {listDatasetType} dataset:");
                        logger.Info("------------------------------------------------------------");
                        
                        foreach (var problem in problems)
                        {
                            // Format each problem for display
                            logger.Info($"ID: {problem.Id}");
                            logger.Info($"Repository: {problem.Repository}");
                            string shortDesc = problem.ProblemStatement.Length > 100 
                                ? problem.ProblemStatement.Substring(0, 100) + "..." 
                                : problem.ProblemStatement;
                            logger.Info($"Problem: {shortDesc}");
                            logger.Info("------------------------------------------------------------");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Failed to list problems: {ex.Message}");
                        return 1;
                    }
                    break;
                case "ensemble":
                    logger.Info("Running ensemble...");
                    // Ensemble implementation will be added later
                    break;
                default:
                    logger.Error($"Unknown command: {command}");
                    return 1;
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while executing the command");
            return 1;
        }

        return 0;
    }

    private static ServiceProvider ConfigureServices(string[] args)
    {
        var services = new ServiceCollection();

        // Add Microsoft.Extensions.Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add Logger (singleton)
        services.AddSingleton<CycodBench.Logging.ILogger>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new CycodBench.Logging.Logger(loggerFactory, "CycodBench", "logs/cycodbench.log");
        });

        // Add Configuration (singleton)
        services.AddSingleton<CycodBench.Configuration.IConfiguration>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            return new CycodBench.Configuration.Configuration(logger, args);
        });

        // Add Phase 2 Infrastructure services
        
        // Add DockerManager (singleton)
        services.AddSingleton<IDockerManager>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            return new DockerManager.DockerManager(logger, config);
        });
        
        // Add DatasetManager (singleton)
        services.AddSingleton<IDatasetManager>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            return new DatasetManager.DatasetManager(logger, config);
        });
        
        // Add EvaluationToolsManager (singleton)
        services.AddSingleton<IEvaluationToolsManager>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var dockerManager = provider.GetRequiredService<IDockerManager>();
            return new EvaluationToolsManager.EvaluationToolsManager(logger, config, dockerManager);
        });

        // Add other services as they are implemented in later phases

        return services.BuildServiceProvider();
    }
}