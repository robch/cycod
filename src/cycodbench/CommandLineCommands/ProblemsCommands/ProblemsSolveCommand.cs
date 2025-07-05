using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to generate solutions for one or more problems.
/// </summary>
public class ProblemsSolveCommand : ProblemsCommand
{
    /// <summary>
    /// Dataset path or name
    /// </summary>
    public string? DatasetPath { get; set; } = "verified";
    
    /// <summary>
    /// Specific container ID to use
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// The container provider to use (docker, aca, aws)
    /// </summary>
    public string ContainerProvider { get; set; } = "docker";
    
    /// <summary>
    /// Number of problems to process in parallel
    /// </summary>
    public int ParallelCount { get; set; } = 1;
    
    /// <summary>
    /// Timeout for agent execution in seconds
    /// </summary>
    public int Timeout { get; set; } = 3600;

    public override string GetCommandName()
    {
        return "problems solve";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Solving problems from {DatasetPath} dataset...");
        
        if (!string.IsNullOrEmpty(ProblemId))
        {
            Console.WriteLine($"Solving specific problem: {ProblemId}");
        }
        
        if (!string.IsNullOrEmpty(ContainerId))
        {
            Console.WriteLine($"Using container: {ContainerId}");
        }
        
        if (MaxItems > 0)
        {
            Console.WriteLine($"Maximum problems to solve: {MaxItems}");
        }
        
        Console.WriteLine($"Parallel threads: {ParallelCount}");
        
        Console.WriteLine($"Using container provider: {ContainerProvider}");
        
        Console.WriteLine($"Timeout: {Timeout} seconds");
        
        if (!string.IsNullOrEmpty(OutputPath))
        {
            Console.WriteLine($"Output path: {OutputPath}");
        }
        
        try
        {
            // Get the required services
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var datasetService = serviceProvider.GetRequiredService<IDatasetService>();
            var solutionService = serviceProvider.GetRequiredService<ISolutionService>();
            
            // Load or download the dataset
            var dataset = await datasetService.LoadDatasetAsync(DatasetPath!);
            Console.WriteLine($"Loaded {dataset.Problems.Count} problems from {DatasetPath} dataset");
            
            // Filter problems if a specific problem ID is provided
            if (!string.IsNullOrEmpty(ProblemId))
            {
                dataset = await datasetService.FilterProblemsAsync(dataset, problemId: ProblemId);
                Console.WriteLine($"Filtered to {dataset.Problems.Count} problems matching ID: {ProblemId}");
            }
            
            // Filter by repository if specified
            if (!string.IsNullOrEmpty(Repository))
            {
                dataset = await datasetService.FilterProblemsAsync(dataset, repository: Repository);
                Console.WriteLine($"Filtered to {dataset.Problems.Count} problems from repository: {Repository}");
            }
            
            // Filter by text pattern if specified
            if (!string.IsNullOrEmpty(ContainsPattern))
            {
                dataset = await datasetService.FilterProblemsAsync(dataset, containsText: ContainsPattern);
                Console.WriteLine($"Filtered to {dataset.Problems.Count} problems containing: {ContainsPattern}");
            }
            
            // Limit the number of problems if MaxItems is set
            if (MaxItems > 0 && dataset.Problems.Count > MaxItems)
            {
                dataset.Problems = dataset.Problems.GetRange(0, MaxItems);
                Console.WriteLine($"Limited to {dataset.Problems.Count} problems");
            }
            
            // Solve problems
            Console.WriteLine($"Solving {dataset.Problems.Count} problems...");
            
            var solutions = new SolutionCollection();
            
            // Set up parallelism
            var options = new ParallelOptions { MaxDegreeOfParallelism = ParallelCount };
            
            await Parallel.ForEachAsync(dataset.Problems, options, async (problem, cancellationToken) =>
            {
                try
                {
                    Console.WriteLine($"Solving problem {problem.Id}...");
                    
                    var solution = await solutionService.SolveProblemAsync(problem, ContainerId, Timeout);
                    solution.ProblemDataset = DatasetPath;
                    
                    lock (solutions)
                    {
                        solutions.Solutions.Add(solution);
                    }
                    
                    Console.WriteLine($"Generated solution for {problem.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to solve problem {problem.Id}: {ex.Message}");
                }
            });
            
            // Save solutions to file
            string outputPath = OutputPath ?? "solutions.json";
            await solutionService.SaveSolutionsAsync(solutions, outputPath);
            
            Console.WriteLine($"Completed solving problems. Solutions saved to {outputPath}");
            
            return solutions;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error solving problems: {ex.Message}");
            return false;
        }
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
}