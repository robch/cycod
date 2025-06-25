using CycodBench.Configuration;
using CycodBench.EvaluationToolsManager;
using CycodBench.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Commands;

/// <summary>
/// Handler for the setup command.
/// </summary>
public static class SetupCommand
{
    /// <summary>
    /// Handles the setup command.
    /// </summary>
    public static async Task<int> HandleCommand(string[] args, IServiceProvider services)
    {
        try
        {
            var logger = services.GetRequiredService<Logging.ILogger>();
            var evalToolsManager = services.GetRequiredService<IEvaluationToolsManager>();
            var configuration = services.GetRequiredService<IConfiguration>();
            
            bool force = false;
            string? toolsPath = null;
            bool skipDockerCheck = false;
            
            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--force")
                {
                    force = true;
                }
                else if (args[i] == "--tools-path" && i + 1 < args.Length)
                {
                    toolsPath = args[i + 1];
                    i++;
                }
                else if (args[i] == "--skip-docker-check")
                {
                    skipDockerCheck = true;
                }
            }
            
            logger.Info("Setting up SWE-bench evaluation tools...");
            
            if (toolsPath != null)
            {
                // Update the tools path in configuration if provided
                logger.Info($"Using custom tools path: {toolsPath}");
                
                // Store this path in the configuration for this session
                // Note: This would ideally update a persistent configuration file as well
                configuration.SetValue("eval_tools_path", toolsPath);
            }
            
            // Check if Docker is available if we're not skipping the check
            if (!skipDockerCheck)
            {
                var dockerManager = services.GetRequiredService<DockerManager.IDockerManager>();
                if (!await dockerManager.IsDockerAvailableAsync())
                {
                    logger.Warning("Docker is not available or not running. Some SWE-bench evaluation features will not work.");
                    logger.Warning("If you want to continue without Docker, use --skip-docker-check");
                    
                    // Ask user if they want to continue
                    Console.WriteLine("Do you want to continue setup without Docker? (y/N)");
                    string? response = Console.ReadLine();
                    
                    if (string.IsNullOrWhiteSpace(response) || !response.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Setup canceled.");
                        return 1;
                    }
                }
                else
                {
                    logger.Info("Docker is available and will be used for SWE-bench evaluation.");
                }
            }
            
            // Set up the SWEBench evaluation tools
            bool setupSuccess = await evalToolsManager.SetupToolsAsync(force);
            if (!setupSuccess)
            {
                logger.Error("Failed to set up SWE-bench evaluation tools.");
                return 1;
            }
            
            // Check if the tools are properly set up
            bool toolsReady = await evalToolsManager.AreToolsSetupAsync();
            if (!toolsReady)
            {
                logger.Error("SWE-bench evaluation tools setup verification failed.");
                return 1;
            }
            
            logger.Info($"SWE-bench evaluation tools setup completed successfully at {evalToolsManager.GetToolsPath()}");
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to set up SWE-bench evaluation tools: {ex.Message}");
            return 1;
        }
    }
}
