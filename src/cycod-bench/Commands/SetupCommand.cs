using CycodBench.EvaluationToolsManager;
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
            
            bool force = false;
            string? toolsPath = null;
            
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
            }
            
            logger.Info("Setting up SWE-bench evaluation tools...");
            
            if (toolsPath != null)
            {
                // Update the tools path in configuration if provided
                // This would require a way to update configuration at runtime
            }
            
            bool setupSuccess = await evalToolsManager.SetupToolsAsync(force);
            if (!setupSuccess)
            {
                logger.Error("Failed to set up evaluation tools.");
                return 1;
            }
            
            logger.Info("Evaluation tools setup completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to set up evaluation tools: {ex.Message}");
            return 1;
        }
    }
}
