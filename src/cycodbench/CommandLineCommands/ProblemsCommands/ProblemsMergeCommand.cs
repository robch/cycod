using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to merge multiple problem datasets into one.
/// </summary>
public class ProblemsMergeCommand : ProblemsCommand
{
    /// <summary>
    /// List of dataset paths to merge
    /// </summary>
    public List<string> DatasetPaths { get; set; } = new List<string>();

    public override string GetCommandName()
    {
        return "problems merge";
    }
    
    public override bool IsEmpty()
    {
        return DatasetPaths.Count == 0;
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(OutputPath))
        {
            OutputPath = "merged-problems.json";
        }
        
        return this;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Merging problem datasets: {string.Join(", ", DatasetPaths)}");
        
        try
        {
            // Get the dataset service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var datasetService = serviceProvider.GetRequiredService<IDatasetService>();
            
            // Load all datasets
            var datasetsToMerge = new List<ProblemDataset>();
            
            foreach (var path in DatasetPaths)
            {
                try
                {
                    var dataset = await datasetService.LoadDatasetAsync(path);
                    Console.WriteLine($"Loaded {dataset.Problems.Count} problems from {path} dataset");
                    datasetsToMerge.Add(dataset);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error loading dataset {path}: {ex.Message}");
                }
            }
            
            if (datasetsToMerge.Count == 0)
            {
                Console.Error.WriteLine("No valid datasets found to merge.");
                return false;
            }
            
            // Merge datasets
            var mergedDataset = await datasetService.MergeDatasetsAsync(datasetsToMerge.ToArray());
            
            Console.WriteLine($"Merged datasets contain {mergedDataset.Problems.Count} problems");
            
            // Save merged dataset
            await datasetService.SaveDatasetAsync(mergedDataset, OutputPath!);
            
            Console.WriteLine($"Merged dataset saved to {OutputPath}");
            
            return mergedDataset;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error merging datasets: {ex.Message}");
            return false;
        }
    }
}