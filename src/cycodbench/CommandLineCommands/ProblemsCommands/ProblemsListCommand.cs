using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to list available problems.
/// </summary>
public class ProblemsListCommand : ProblemsCommand
{
    /// <summary>
    /// Dataset path or name
    /// </summary>
    public string? DatasetPath { get; set; }
    
    /// <summary>
    /// Show detailed information about each problem
    /// </summary>
    public bool Verbose { get; set; } = false;

    public override string GetCommandName()
    {
        return "problems list";
    }
    
    public override bool IsEmpty()
    {
        // This command doesn't require any specific arguments to run
        return false;
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(DatasetPath))
        {
            DatasetPath = "verified";
        }
        
        return this;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Listing problems from {DatasetPath} dataset...");
        
        try
        {
            // Get the dataset service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var datasetService = serviceProvider.GetRequiredService<IDatasetService>();
            
            // Load the dataset
            ProblemDataset dataset;
            string datasetFile;
            
            // Check if DatasetPath is one of our standard dataset names
            bool isStandardDataset = DatasetPath == "verified" || DatasetPath == "full" || DatasetPath == "lite";
            
            if (File.Exists(DatasetPath))
            {
                // Load from specified file directly
                datasetFile = DatasetPath;
                dataset = await datasetService.LoadDatasetAsync(datasetFile);
                Console.WriteLine($"Loaded {dataset.Problems.Count} problems from {DatasetPath}");
            }
            else if (isStandardDataset)
            {
                // Try to download standard dataset if needed
                try
                {
                    datasetFile = await datasetService.DownloadDatasetAsync(DatasetPath!, null, false);
                    dataset = await datasetService.LoadDatasetAsync(datasetFile);
                    Console.WriteLine($"Loaded {dataset.Problems.Count} problems from {DatasetPath} dataset");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error loading dataset: {ex.Message}");
                    Console.Error.WriteLine("Please provide a valid dataset name (verified, full, lite) or a path to a dataset file.");
                    return false;
                }
            }
            else
            {
                // Neither a file nor a standard dataset name
                Console.Error.WriteLine($"Dataset file not found: {DatasetPath}");
                Console.Error.WriteLine("Please provide a valid dataset name (verified, full, lite) or a path to an existing dataset file.");
                return false;
            }
            
            // Apply filters
            var filteredDataset = await datasetService.FilterProblemsAsync(
                dataset,
                problemId: ProblemId,
                repository: Repository,
                containsText: ContainsPattern,
                maxProblems: MaxItems > 0 ? MaxItems : null
            );
            
            Console.WriteLine($"Found {filteredDataset.Problems.Count} problems after filtering");
            
            // Display problems
            for (int i = 0; i < filteredDataset.Problems.Count; i++)
            {
                var problem = filteredDataset.Problems[i];
                
                if (Verbose)
                {
                    Console.WriteLine($"{i+1}. {problem.Id} ({problem.Repo})");
                    Console.WriteLine($"   Base commit: {problem.BaseCommit}");
                    Console.WriteLine($"   Problem statement:");
                    Console.WriteLine($"   {problem.ProblemStatement.Replace("\n", "\n   ").Substring(0, Math.Min(100, problem.ProblemStatement.Length))}...");
                    Console.WriteLine($"   Tests to fix: {problem.FailToPass.Count}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"{i+1}. {problem.Id} ({problem.Repo})");
                }
            }
            
            // Save filtered dataset if output path specified
            if (!string.IsNullOrEmpty(OutputPath))
            {
                await datasetService.SaveDatasetAsync(filteredDataset, OutputPath);
                Console.WriteLine($"Saved {filteredDataset.Problems.Count} problems to {OutputPath}");
            }
            
            return filteredDataset;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error listing problems: {ex.Message}");
            return false;
        }
    }
}