using CycodBench.DatasetManager;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Commands;

/// <summary>
/// Handler for the list command.
/// </summary>
public static class ListCommand
{
    /// <summary>
    /// Handles the list command.
    /// </summary>
    public static async Task<int> HandleCommand(string[] args, IServiceProvider services)
    {
        try
        {
            var logger = services.GetRequiredService<Logging.ILogger>();
            var datasetManager = services.GetRequiredService<IDatasetManager>();
            
            string datasetType = "verified"; // Default
            string? filter = null;
            
            // Parse arguments - first non-option argument is the dataset type
            for (int i = 1; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-") && i == 1)
                {
                    datasetType = args[i];
                }
                else if ((args[i] == "--filter" || args[i] == "-f") && i + 1 < args.Length)
                {
                    filter = args[i + 1];
                    i++;
                }
            }
            
            // Validate dataset type
            if (datasetType != "verified" && datasetType != "full" && datasetType != "lite")
            {
                logger.Error($"Unknown dataset type: {datasetType}. Must be one of: verified, full, lite");
                return 1;
            }
            
            // Check if dataset is downloaded
            if (!datasetManager.IsDatasetCached(datasetType))
            {
                logger.Error($"Dataset '{datasetType}' not found. Please download it first using the 'download {datasetType}' command.");
                return 1;
            }
            
            // Get problems
            var problems = await datasetManager.GetProblemsAsync(datasetType);
            
            // Filter problems if filter is specified
            if (!string.IsNullOrEmpty(filter))
            {
                problems = problems.Where(p => 
                    p.Repository.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                    p.ProblemStatement.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            
            if (problems.Count == 0)
            {
                logger.Info($"No problems found in the {datasetType} dataset{(filter != null ? $" matching filter '{filter}'" : "")}.");
                return 0;
            }
            
            logger.Info($"Found {problems.Count} problems in the {datasetType} dataset{(filter != null ? $" matching filter '{filter}'" : "")}:");
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
            
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to list problems: {ex.Message}");
            return 1;
        }
    }
}
