# Program.cs

This class serves as the entry point for the CycodBench application and handles command-line interface interactions.

## Responsibilities

- Parse and validate command-line arguments
- Set up the application environment
- Create and configure services
- Route to the appropriate command handler based on user input
- Display help and version information
- Handle global exceptions and logging

## Public Interface

```csharp
public class Program
{
    // Entry point
    public static int Main(string[] args)
    {
        // Create and run the root command with subcommands
    }
    
    // Configure services for dependency injection
    private static IServiceProvider ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        
        // Register services with appropriate lifetimes
        // Configure options from configuration
        // Set up logging
        
        return services.BuildServiceProvider();
    }
    
    // Command handlers - each using dependency injection
    private static async Task<int> HandleRunCommand(RunCommandOptions options, IServiceProvider serviceProvider)
    {
        // Get required services from container
        var benchmarkRunner = serviceProvider.GetRequiredService<IBenchmarkRunner>();
        
        // Execute the command using injected services
        return await benchmarkRunner.RunAsync(...);
    }
}
```

## Command Structure

The application will use a hierarchical command structure with the following main commands:

1. `run` - Run the benchmark on a shard of problems
2. `merge` - Merge results from multiple shards
3. `ensemble` - Select the best solutions from merged results
4. `validate` - Validate the configuration and environment
5. `setup` - Install and configure SWE-bench evaluation tools
6. `download` - Download and cache the SWE-bench dataset

## Command-Line Arguments

### Global Options
- `--log-level` - Set the logging level (Debug, Info, Warning, Error)
- `--config` - Path to configuration file

### Setup Command
- `--force` - Force reinstallation of evaluation tools
- `--tools-path` - Path to install evaluation tools (default: ~/.cycod-bench/tools)

### Download Command
- `--force` - Force redownload of the dataset
- `--cache-path` - Path to cache the dataset (default: ~/.cycod-bench/dataset)

### Run Command
- `--shard-count` - Total number of shards (default: 1)
- `--shard-id` - Current shard ID (0-indexed, default: 0)
- `--candidates` - Number of candidate solutions per problem (default: 8)
- `--parallelism` - Number of parallel processes (default: 8)
- `--problems` - Number of problems to run (default: all)
- `--workspace` - Path to workspace directory
- `--agent-path` - Path to cycod agent executable
- `--output` - Path to output file
- `--force-download` - Force download of the dataset even if cached

### Merge Command
- `--input` - Paths to shard result files to merge
- `--output` - Path to output merged file

### Ensemble Command
- `--input` - Path to merged result file
- `--output` - Path to output ensembled result file

## Implementation Overview

The Program class will:

1. Use System.CommandLine library to parse and handle command-line arguments
2. Configure a dependency injection container using Microsoft.Extensions.DependencyInjection
3. Register all services with appropriate lifetimes:
   ```csharp
   services.AddSingleton<IConfiguration>(configuration);
   services.AddSingleton<ILogger, Logger>();
   services.AddSingleton<IDatasetManager, DatasetManager>();
   services.AddSingleton<IEvaluationToolsManager, EvaluationToolsManager>();
   services.AddScoped<IShardManager, ShardManager>();
   services.AddScoped<IBenchmarkRunner, BenchmarkRunner>();
   services.AddScoped<IResultManager, ResultManager>();
   services.AddTransient<IProblemProcessor, ProblemProcessor>();
   services.AddTransient<IAgentExecutor, AgentExecutor>();
   services.AddTransient<IDockerManager, DockerManager>();
   services.AddTransient<IEvaluationService, EvaluationService>();
   services.AddTransient<IEnsemblerService, EnsemblerService>();
   ```
4. Configure logging based on command-line arguments
5. Route to the appropriate command handler
6. Catch and handle global exceptions
7. Return appropriate exit codes

For each command, it will:
1. Parse and validate specific command-line arguments
2. Create and configure the required services
3. Execute the command logic
4. Format and display the results

The actual implementation of each command will be delegated to specialized service classes like BenchmarkRunner, ShardManager, and EnsemblerService.

## Error Handling

The Program class will implement global error handling to catch any unhandled exceptions, log the error details, and return an appropriate exit code. It will provide the user with friendly error messages and suggestions for fixing common issues.