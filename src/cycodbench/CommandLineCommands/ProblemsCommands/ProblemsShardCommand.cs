using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to filter problems into 'shards'.
/// </summary>
public class ProblemsShardCommand : ProblemsCommand
{
    /// <summary>
    /// Dataset path or name
    /// </summary>
    public string? DatasetPath { get; set; }
    
    /// <summary>
    /// Shard index (if using sharding)
    /// </summary>
    public int ShardIndex { get; set; } = 0;
    
    /// <summary>
    /// Total number of shards (if using sharding)
    /// </summary>
    public int TotalShards { get; set; } = 1;
    
    /// <summary>
    /// Number of solution candidates to generate per problem
    /// </summary>
    public int CandidatesPerProblem { get; set; } = 1;

    public override string GetCommandName()
    {
        return "problems shard";
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
        Console.WriteLine($"Sharding problems from {DatasetPath} dataset...");
        
        if (!string.IsNullOrEmpty(ProblemId))
        {
            Console.WriteLine($"Filtering by problem ID: {ProblemId}");
        }
        
        if (!string.IsNullOrEmpty(Repository))
        {
            Console.WriteLine($"Filtering by repository: {Repository}");
        }
        
        if (!string.IsNullOrEmpty(ContainsPattern))
        {
            Console.WriteLine($"Filtering by contains: {ContainsPattern}");
        }
        
        if (TotalShards > 1)
        {
            if (ShardIndex > 0)
            {
                Console.WriteLine($"Processing shard: {ShardIndex}/{TotalShards}");
            }
            else
            {
                Console.WriteLine($"Creating {TotalShards} shard(s)");
            }
        }
        
        Console.WriteLine($"Candidates per problem: {CandidatesPerProblem}");
        
        if (MaxItems > 0)
        {
            Console.WriteLine($"Maximum problems to select: {MaxItems}");
        }
        
        try
        {
            // Get the dataset service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var datasetService = serviceProvider.GetRequiredService<IDatasetService>();
            
            // Load the dataset
            var dataset = await datasetService.LoadDatasetAsync(DatasetPath!);
            Console.WriteLine($"Loaded {dataset.Problems.Count} problems from {DatasetPath} dataset");
            
            // Apply filters
            var filteredDataset = await datasetService.FilterProblemsAsync(
                dataset,
                problemId: ProblemId,
                repository: Repository,
                containsText: ContainsPattern,
                maxProblems: MaxItems > 0 ? MaxItems : null
            );
            
            Console.WriteLine($"Found {filteredDataset.Problems.Count} problems after filtering");
            
            // Process shards
            List<(ProblemDataset Dataset, string OutputPath)> outputDatasets = new List<(ProblemDataset, string)>();
            
            if (TotalShards > 1 || CandidatesPerProblem > 1)
            {
                if (ShardIndex > 0)
                {
                    // Process only the specified shard
                    if (ShardIndex > TotalShards)
                    {
                        Console.Error.WriteLine($"Shard index {ShardIndex} is greater than total shards {TotalShards}");
                        return false;
                    }
                    
                    // Using 0-based index internally
                    var shardIndexZeroBased = ShardIndex - 1;
                    // Let the dataset service handle candidate expansion
                    var shardDataset = await datasetService.GetShardAsync(filteredDataset, shardIndexZeroBased, TotalShards, CandidatesPerProblem);
                    
                    string outputPath = OutputPath ?? GetDefaultOutputPath(DatasetPath, ShardIndex, TotalShards);
                    outputDatasets.Add((shardDataset, outputPath));
                    
                    Console.WriteLine($"Created shard {ShardIndex}/{TotalShards} with {shardDataset.Problems.Count} problems");
                }
                else
                {
                    // Create all shards
                    // Let the dataset service handle candidate expansion
                    var shards = await datasetService.CreateShardsAsync(filteredDataset, TotalShards, CandidatesPerProblem);
                    
                    for (int i = 0; i < shards.Length; i++)
                    {
                        // Using 1-based index for output
                        string outputPath = OutputPath;
                        if (string.IsNullOrEmpty(outputPath))
                        {
                            outputPath = GetDefaultOutputPath(DatasetPath, i + 1, TotalShards);
                        }
                        else if (shards.Length > 1)
                        {
                            // Add shard number to output path if multiple shards
                            string directory = Path.GetDirectoryName(outputPath) ?? string.Empty;
                            string fileName = Path.GetFileNameWithoutExtension(outputPath);
                            string extension = Path.GetExtension(outputPath);
                            outputPath = Path.Combine(directory, $"{fileName}-shard{i+1}{extension}");
                        }
                        
                        outputDatasets.Add((shards[i], outputPath));
                        Console.WriteLine($"Created shard {i+1}/{TotalShards} with {shards[i].Problems.Count} problems");
                    }
                }
            }
            else
            {
                // No sharding, just output the filtered dataset
                string outputPath = OutputPath ?? GetDefaultOutputPath(DatasetPath);
                outputDatasets.Add((filteredDataset, outputPath));
            }
            
            // Save all output datasets
            List<string> savedFiles = new List<string>();
            foreach (var (datasetToSave, outputPath) in outputDatasets)
            {
                var savedFilePath = await datasetService.SaveDatasetAsync(datasetToSave, outputPath);
                savedFiles.Add(savedFilePath);
                Console.WriteLine($"Saved {datasetToSave.Problems.Count} problems to {savedFilePath}");
            }
            
            return string.Join(Environment.NewLine, savedFiles);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sharding problems: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine($"Inner error: {ex.InnerException.Message}");
            }
            return false;
        }
    }
    
    /// <summary>
    /// Helper to generate a default output path for sharded datasets
    /// </summary>
    private string GetDefaultOutputPath(string inputPath, int? shardIndex = null, int? totalShards = null)
    {
        // Remove path and file extension
        string baseName = Path.GetFileNameWithoutExtension(inputPath);
        
        // If it's one of the standard datasets, use that name
        if (inputPath == "verified" || inputPath == "full" || inputPath == "lite")
        {
            baseName = inputPath;
        }
        
        // Handle filtering
        List<string> filters = new List<string>();
        
        if (!string.IsNullOrEmpty(ProblemId))
        {
            filters.Add($"id-{ProblemId.Replace("/", "-")}");
        }
        
        if (!string.IsNullOrEmpty(Repository))
        {
            filters.Add($"repo-{Repository.Replace("/", "-")}");
        }
        
        string filterSuffix = filters.Count > 0 ? $"-{string.Join("-", filters)}" : "";
        
        // Handle sharding
        string shardSuffix = "";
        if (shardIndex.HasValue && totalShards.HasValue && totalShards.Value > 1)
        {
            shardSuffix = $"-shard{shardIndex.Value}-of-{totalShards.Value}";
        }
        
        return $"{baseName}{filterSuffix}{shardSuffix}.json";
    }
}