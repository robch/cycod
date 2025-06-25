using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to merge multiple evaluation results files.
/// </summary>
public class ResultsMergeCommand : ResultsCommand
{
    public List<string> ResultFilePaths { get; set; } = new List<string>();
    // OutputPath is already defined in the ResultsCommand base class

    public override string GetCommandName()
    {
        return "results merge";
    }
    public override bool IsEmpty()
    {
        return ResultFilePaths.Count == 0;
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(OutputPath))
        {
            OutputPath = "merged-results.json";
        }
        
        return this;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Merging results files: {string.Join(", ", ResultFilePaths)}");
        
        try
        {
            // Get the result service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var resultService = serviceProvider.GetRequiredService<IResultService>();
            
            // Load all result files
            var resultsToMerge = new List<ResultCollection>();
            
            foreach (var path in ResultFilePaths)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        Console.Error.WriteLine($"Results file not found: {path}");
                        continue;
                    }
                    
                    var results = await resultService.LoadResultsAsync(path);
                    resultsToMerge.Add(results);
                    Console.WriteLine($"Loaded {results.Results.Count} results from {path}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error loading results file {path}: {ex.Message}");
                }
            }
            
            if (resultsToMerge.Count == 0)
            {
                Console.Error.WriteLine("No valid results files found to merge.");
                return false;
            }
            
            // Merge results
            var mergedResults = await resultService.MergeResultsAsync(resultsToMerge.ToArray());
            
            Console.WriteLine($"Merged results contain {mergedResults.Results.Count} entries");
            
            // Save merged results
            await resultService.SaveResultsAsync(mergedResults, OutputPath ?? "merged-results.json");
            
            Console.WriteLine($"Merged results saved to {OutputPath ?? "merged-results.json"}");
            
            return mergedResults;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error merging results: {ex.Message}");
            return false;
        }
    }
}