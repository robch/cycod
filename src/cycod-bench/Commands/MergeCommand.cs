using CycodBench.ResultManager;
using Microsoft.Extensions.DependencyInjection;

namespace CycodBench.Commands;

/// <summary>
/// Handler for the merge command.
/// </summary>
public static class MergeCommand
{
    /// <summary>
    /// Handles the merge command.
    /// </summary>
    public static async Task<int> HandleCommand(string[] args, IServiceProvider services)
    {
        try
        {
            var logger = services.GetRequiredService<Logging.ILogger>();
            var resultManager = services.GetRequiredService<IResultManager>();
            
            List<string> inputPaths = new List<string>();
            string? outputPath = null;
            
            // Parse arguments
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--input" || args[i] == "-i")
                {
                    // Collect all paths until the next option
                    i++;
                    while (i < args.Length && !args[i].StartsWith("-"))
                    {
                        inputPaths.Add(args[i]);
                        i++;
                    }
                    i--; // Adjust for the loop increment
                }
                else if ((args[i] == "--output" || args[i] == "-o") && i + 1 < args.Length)
                {
                    outputPath = args[i + 1];
                    i++;
                }
            }
            
            // Validate input paths
            if (inputPaths.Count == 0)
            {
                logger.Error("No input paths specified. Use --input <file1> <file2> ... to specify input files.");
                return 1;
            }
            
            // Check if each input file exists
            foreach (var path in inputPaths)
            {
                if (!File.Exists(path))
                {
                    logger.Error($"Input file not found: {path}");
                    return 1;
                }
            }
            
            // Set default output path if not specified
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.Combine(resultManager.GetDefaultResultsDirectory(), "merged_results.jsonl");
                logger.Info($"No output path specified. Using default: {outputPath}");
            }
            
            // Merge results
            logger.Info($"Merging {inputPaths.Count} result files...");
            int totalSolutions = await resultManager.MergeResultsAsync(inputPaths, outputPath);
            
            logger.Info($"Successfully merged {totalSolutions} solutions to {outputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logging.ILogger>().Error($"Failed to merge results: {ex.Message}");
            return 1;
        }
    }
}
