using CycodBench.DatasetManager;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Commands;

/// <summary>
/// Handler for the download command.
/// </summary>
public static class DownloadCommand
{
    /// <summary>
    /// Handles the download command.
    /// </summary>
    public static async Task<int> HandleCommand(string[] args, IServiceProvider services)
    {
        try
        {
            var logger = services.GetRequiredService<Logging.ILogger>();
            var datasetManager = services.GetRequiredService<IDatasetManager>();
            
            bool force = false;
            string? cachePath = null;
            string datasetType = "verified";
            
            // Parse arguments
            // Find the first non-option argument (dataset type)
            for (int i = 1; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--"))
                {
                    datasetType = args[i];
                    break;
                }
            }
            
            // Parse options
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--force")
                {
                    force = true;
                }
                else if (args[i] == "--cache-path" && i + 1 < args.Length)
                {
                    cachePath = args[i + 1];
                    i++;
                }
            }
            
            // Validate dataset type
            if (datasetType != "verified" && datasetType != "full" && datasetType != "lite")
            {
                logger.Error($"Unknown dataset type: {datasetType}. Must be one of: verified, full, lite");
                return 1;
            }
            
            logger.Info($"Downloading {datasetType} dataset...");
            
            if (cachePath != null)
            {
                // Update the cache path in configuration if provided
                // This would require a way to update configuration at runtime
            }
            
            bool downloadSuccess = await datasetManager.DownloadDatasetAsync(datasetType, force);
            if (!downloadSuccess)
            {
                logger.Error($"Failed to download {datasetType} dataset.");
                return 1;
            }
            
            logger.Info($"{datasetType} dataset downloaded successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to download dataset: {ex.Message}");
            return 1;
        }
    }
}
