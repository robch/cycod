using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CycodBench.Configuration;
using CycodBench.DatasetManager;
using CycodBench.DockerManager;
using CycodBench.EvaluationToolsManager;
using CycodBench.Logging;
using CycodBench.AgentExecutor;
using CycodBench.EvaluationService;
using CycodBench.EnsemblerService;
using CycodBench.ShardManager;
using CycodBench.ProblemProcessor;
using CycodBench.ResultManager;
using CycodBench.Models;
using CycodBench.Commands;
using CycodBench.BenchmarkRunner;

namespace CycodBench;

/// <summary>
/// Main entry point for the CycodBench application.
/// </summary>
public class Program
{
    /// <summary>
    /// Entry point of the application.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Setup dependency injection
            var serviceProvider = ConfigureServices(args);
            var logger = serviceProvider.GetRequiredService<CycodBench.Logging.ILogger>();
            
            logger.Info("CycodBench: SWE-bench Benchmark Runner for cycod agent");

            if (args.Length == 0)
            {
                PrintUsage(logger);
                return 0;
            }

            var command = args[0].ToLower();
            
            // Process commands
            return command switch
            {
                "setup" => await SetupCommand.HandleCommand(args, serviceProvider),
                "download" => await DownloadCommand.HandleCommand(args, serviceProvider),
                "run" => await RunCommand.HandleCommand(args, serviceProvider),
                "merge" => await MergeCommand.HandleCommand(args, serviceProvider),
                "ensemble" => await EnsembleCommand.HandleCommand(args, serviceProvider),
                "list" => await ListCommand.HandleCommand(args, serviceProvider),
                "help" or "--help" or "-h" => PrintHelp(args, logger),
                "version" or "--version" or "-v" => PrintVersion(logger),
                _ => PrintUnknownCommand(command, logger)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }
    
    private static int PrintUsage(CycodBench.Logging.ILogger logger)
    {
        logger.Info("Usage:");
        logger.Info("  CycodBench setup [options] - Install SWE-bench evaluation tools");
        logger.Info("  CycodBench download [verified|full|lite] [options] - Download the SWE-bench dataset");
        logger.Info("  CycodBench list [verified|full|lite] [options] - List problems in the dataset");
        logger.Info("  CycodBench run [options] - Run the benchmark");
        logger.Info("  CycodBench merge --input <files> --output <file> - Merge results from multiple shards");
        logger.Info("  CycodBench ensemble --input <file> --output <file> - Run ensemble on merged results");
        logger.Info("  CycodBench help [command] - Show help for a command");
        logger.Info("  CycodBench version - Show version information");
        return 0;
    }
    
    private static int PrintHelp(string[] args, CycodBench.Logging.ILogger logger)
    {
        if (args.Length > 1)
        {
            var helpCommand = args[1].ToLower();
            switch (helpCommand)
            {
                case "setup":
                    logger.Info("CycodBench setup - Install SWE-bench evaluation tools");
                    logger.Info("Options:");
                    logger.Info("  --force - Force reinstallation of evaluation tools");
                    logger.Info("  --tools-path <path> - Path to install evaluation tools");
                    break;
                case "download":
                    logger.Info("CycodBench download [verified|full|lite] - Download the SWE-bench dataset");
                    logger.Info("Arguments:");
                    logger.Info("  dataset-type - Dataset type to download (verified, full, or lite)");
                    logger.Info("Options:");
                    logger.Info("  --force - Force redownload of the dataset");
                    logger.Info("  --cache-path <path> - Path to cache the dataset");
                    break;
                case "run":
                    logger.Info("CycodBench run - Run the benchmark");
                    logger.Info("Options:");
                    logger.Info("  --shard-count <count> - Total number of shards to split the dataset into");
                    logger.Info("  --shard-id <id> - The ID of the current shard (0 to ShardCount-1)");
                    logger.Info("  --candidates <count> - Number of candidate solutions to generate per problem");
                    logger.Info("  --parallelism <count> - Number of problems to process in parallel");
                    logger.Info("  --problems <count> - Maximum number of problems to process (default: all)");
                    logger.Info("  --dataset <type> - Dataset type to use (verified, full, or lite)");
                    break;
                case "merge":
                    logger.Info("CycodBench merge - Merge results from multiple shards");
                    logger.Info("Options:");
                    logger.Info("  --input <files> - Paths to shard result files to merge");
                    logger.Info("  --output <file> - Path to output merged file");
                    break;
                case "ensemble":
                    logger.Info("CycodBench ensemble - Run ensemble on merged results");
                    logger.Info("Options:");
                    logger.Info("  --input <file> - Path to merged result file");
                    logger.Info("  --output <file> - Path to output ensembled result file");
                    break;
                case "list":
                    logger.Info("CycodBench list [verified|full|lite] - List problems in the dataset");
                    logger.Info("Arguments:");
                    logger.Info("  dataset-type - Dataset type to list (verified, full, or lite)");
                    logger.Info("Options:");
                    logger.Info("  --filter <filter> - Filter problems by repository or content");
                    break;
                default:
                    return PrintUsage(logger);
            }
            return 0;
        }
        
        return PrintUsage(logger);
    }
    
    private static int PrintVersion(CycodBench.Logging.ILogger logger)
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0";
        logger.Info($"CycodBench version {version}");
        return 0;
    }
    
    private static int PrintUnknownCommand(string command, CycodBench.Logging.ILogger logger)
    {
        logger.Error($"Unknown command: {command}");
        PrintUsage(logger);
        return 1;
    }

    /// <summary>
    /// Configures the dependency injection services.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Service provider</returns>
    private static ServiceProvider ConfigureServices(string[] args)
    {
        var services = new ServiceCollection();

        // Add Microsoft.Extensions.Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add Logger (singleton)
        services.AddSingleton<CycodBench.Logging.ILogger>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new CycodBench.Logging.Logger(loggerFactory, "CycodBench", "cycodbench.log");
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

        // Add Phase 3 Service Layer services
        
        // Add AgentExecutor (singleton)
        services.AddSingleton<IAgentExecutor>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var dockerManager = provider.GetRequiredService<IDockerManager>();
            return new AgentExecutor.AgentExecutor(logger, config, dockerManager);
        });
        
        // Add EvaluationService (singleton)
        services.AddSingleton<IEvaluationService>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var dockerManager = provider.GetRequiredService<IDockerManager>();
            var evalToolsManager = provider.GetRequiredService<IEvaluationToolsManager>();

            return new EvaluationService.EvaluationService(logger, config, dockerManager, evalToolsManager);
        });
        
        // Add EnsemblerService (singleton)
        services.AddSingleton<IEnsemblerService>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var agentExecutor = provider.GetRequiredService<IAgentExecutor>();
            return new EnsemblerService.EnsemblerService(logger, config, agentExecutor);
        });
        
        // Add Phase 4 Orchestration Layer services
        
        // Add ShardManager (singleton)
        services.AddSingleton<ShardManager.IShardManager>(provider =>
        {
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            return new ShardManager.ShardManager(config, logger);
        });
        
        // Add ProblemProcessor (singleton)
        services.AddSingleton<ProblemProcessor.IProblemProcessor>(provider =>
        {
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var dockerManager = provider.GetRequiredService<IDockerManager>();
            var agentExecutor = provider.GetRequiredService<IAgentExecutor>();
            var evaluationService = provider.GetRequiredService<IEvaluationService>();
            return new ProblemProcessor.ProblemProcessor(agentExecutor, dockerManager, evaluationService, config, logger);
        });
        
        // Add ResultManager (singleton)
        services.AddSingleton<ResultManager.IResultManager>(provider =>
        {
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            return new ResultManager.ResultManager(config, logger);
        });
        
        // Add Phase 5 Application Layer services
        
        // Add BenchmarkRunner (singleton)
        services.AddSingleton<IBenchmarkRunner>(provider =>
        {
            var datasetManager = provider.GetRequiredService<IDatasetManager>();
            var shardManager = provider.GetRequiredService<IShardManager>();
            var problemProcessor = provider.GetRequiredService<IProblemProcessor>();
            var resultManager = provider.GetRequiredService<IResultManager>();
            var logger = provider.GetRequiredService<CycodBench.Logging.ILogger>();
            var config = provider.GetRequiredService<CycodBench.Configuration.IConfiguration>();
            return new BenchmarkRunner.BenchmarkRunner(
                datasetManager, 
                shardManager, 
                problemProcessor, 
                resultManager, 
                logger, 
                config);
        });

        return services.BuildServiceProvider();
    }
}