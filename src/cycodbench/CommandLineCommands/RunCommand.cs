using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to run the complete benchmark workflow.
/// </summary>
public class RunCommand : CycodBenchCommand
{
    /// <summary>
    /// Dataset path or name
    /// </summary>
    public string? DatasetPath { get; set; } = "verified";
    
    /// <summary>
    /// Problem ID to process
    /// </summary>
    public string? ProblemId { get; set; }
    
    /// <summary>
    /// The container provider to use (docker, aca, aws)
    /// </summary>
    public string ContainerProvider { get; set; } = "docker";
    
    /// <summary>
    /// Specific container ID to use
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// Number of solution candidates to generate per problem
    /// </summary>
    public int CandidatesPerProblem { get; set; } = 1;
    
    /// <summary>
    /// Shard index (if using sharding)
    /// </summary>
    public int ShardIndex { get; set; } = 0;
    
    /// <summary>
    /// Total number of shards (if using sharding)
    /// </summary>
    public int TotalShards { get; set; } = 0;
    
    /// <summary>
    /// Number of problems to process in parallel
    /// </summary>
    public int ParallelCount { get; set; } = 1;
    
    /// <summary>
    /// Timeout for agent execution in seconds
    /// </summary>
    public int Timeout { get; set; } = 3600;
    
    /// <summary>
    /// Path to save results file
    /// </summary>
    public string? OutputPath { get; set; } = "results.json";

    public override string GetCommandName()
    {
        return "run";
    }
    
    public override bool IsEmpty()
    {
        // This command doesn't require any specific arguments to run
        // It can operate with default parameters
        return false;
    }
    
    public override Command Validate()
    {
        // Perform any validation on command parameters
        if (string.IsNullOrEmpty(DatasetPath))
        {
            DatasetPath = "verified";
        }
        
        if (CandidatesPerProblem <= 0)
        {
            CandidatesPerProblem = 1;
        }
        
        if (ParallelCount <= 0)
        {
            ParallelCount = 1;
        }
        
        if (Timeout <= 0)
        {
            Timeout = 3600;
        }
        
        return this;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Running benchmark on {DatasetPath} dataset...");
        
        try
        {
            // Get the benchmark service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var benchmarkService = serviceProvider.GetRequiredService<IBenchmarkService>();
            
            string? shardSpec = null;
            if (TotalShards > 0)
            {
                shardSpec = $"{ShardIndex}/{TotalShards}";
            }
            
            // Run the benchmark
            ResultCollection results = await benchmarkService.RunBenchmarkAsync(
                datasetName: DatasetPath!,
                problemId: ProblemId,
                candidatesCount: CandidatesPerProblem,
                shardSpec: shardSpec,
                parallelCount: ParallelCount,
                timeout: Timeout,
                containerId: ContainerId,
                outputPath: OutputPath!);
            
            Console.WriteLine($"Benchmark completed. Results saved to {OutputPath}");
            return results;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error running benchmark: {ex.Message}");
            return false;
        }
    }
}