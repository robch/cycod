using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CycodBench.Models;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to evaluate solutions of a problem.
/// </summary>
public class SolutionsEvalCommand : SolutionsCommand
{
    /// <summary>
    /// Path to solutions file
    /// </summary>
    public string? SolutionsFilePath { get; set; }
    
    /// <summary>
    /// The container provider to use (docker, aca, aws)
    /// </summary>
    public string ContainerProvider { get; set; } = "docker";
    
    /// <summary>
    /// Specific container ID to use
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// Timeout for solution evaluation in seconds
    /// </summary>
    public int Timeout { get; set; } = 600;

    public override string GetCommandName()
    {
        return "solutions eval";
    }
    
    public override bool IsEmpty()
    {
        return string.IsNullOrEmpty(SolutionsFilePath);
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(SolutionsFilePath))
        {
            throw new CommandLineException("Solutions file path must be specified for 'solutions eval'");
        }
        
        return this;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Evaluating solutions from file: {SolutionsFilePath}");
        
        try
        {
            // Get the services
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var datasetService = serviceProvider.GetRequiredService<IDatasetService>();
            var solutionService = serviceProvider.GetRequiredService<ISolutionService>();
            var resultService = serviceProvider.GetRequiredService<IResultService>();
            
            // Load solutions
            if (!File.Exists(SolutionsFilePath))
            {
                throw new FileNotFoundException($"Solutions file not found: {SolutionsFilePath}");
            }
            
            var solutions = await solutionService.LoadSolutionsAsync(SolutionsFilePath);
            
            // Filter solutions if specific problem ID is provided
            if (!string.IsNullOrEmpty(ProblemId))
            {
                solutions = await solutionService.FilterSolutionsAsync(solutions, problemId: ProblemId);
                
                if (solutions.Solutions.Count == 0)
                {
                    Console.WriteLine($"No solutions found for problem ID: {ProblemId}");
                    return false;
                }
            }
            
            Console.WriteLine($"Evaluating {solutions.Solutions.Count} solutions");
            
            // Load the problems dataset (infer from the first solution)
            string datasetName = solutions.Solutions.FirstOrDefault()?.ProblemDataset ?? "verified";
            ProblemDataset dataset;
            
            try
            {
                if (File.Exists(datasetName))
                {
                    dataset = await datasetService.LoadDatasetAsync(datasetName);
                }
                else
                {
                    var datasetFile = await datasetService.DownloadDatasetAsync(datasetName);
                    dataset = await datasetService.LoadDatasetAsync(datasetFile);
                }
                
                Console.WriteLine($"Loaded {dataset.Problems.Count} problems from dataset");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading dataset: {ex.Message}");
                Console.Error.WriteLine($"Using empty dataset - evaluation may not work correctly");
                dataset = new ProblemDataset();
            }
            
            // Evaluate solutions
            var results = new ResultCollection();
            
            foreach (var solution in solutions.Solutions)
            {
                try
                {
                    // Find the corresponding problem
                    var problem = dataset.Problems.FirstOrDefault(p => p.Id == solution.ProblemId);
                    if (problem == null)
                    {
                        Console.WriteLine($"Warning: Problem {solution.ProblemId} not found in dataset. Skipping evaluation.");
                        
                        // Create a skipped result
                        var skippedResult = new Result
                        {
                            ProblemId = solution.ProblemId,
                            ProblemDataset = solution.ProblemDataset,
                            ProblemIdx = solution.ProblemIdx,
                            Id = solution.Id,
                            AgentPatch = solution.AgentPatch,
                            AgentOutput = solution.AgentOutput,
                            EvalStatus = "skipped",
                            EvalOutput = "Problem not found in dataset"
                        };
                        
                        results.Results.Add(skippedResult);
                        continue;
                    }
                    
                    Console.WriteLine($"Evaluating solution for {solution.ProblemId}...");
                    
                    var result = await solutionService.EvaluateSolutionAsync(
                        solution, 
                        problem, 
                        ContainerId,
                        Timeout);
                    
                    results.Results.Add(result);
                    
                    Console.WriteLine($"Evaluation complete: {result.EvalStatus}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error evaluating solution for {solution.ProblemId}: {ex.Message}");
                    
                    // Create a failed result
                    var failedResult = new Result
                    {
                        ProblemId = solution.ProblemId,
                        ProblemDataset = solution.ProblemDataset,
                        ProblemIdx = solution.ProblemIdx,
                        Id = solution.Id,
                        AgentPatch = solution.AgentPatch,
                        AgentOutput = solution.AgentOutput,
                        EvalStatus = "failed",
                        EvalOutput = $"Evaluation error: {ex.Message}\n{ex.StackTrace}"
                    };
                    
                    results.Results.Add(failedResult);
                }
            }
            
            // Save results
            await resultService.SaveResultsAsync(results, OutputPath ?? "results.json");
            
            Console.WriteLine($"Evaluation complete. Results saved to {OutputPath ?? "results.json"}");
            
            // Display summary
            int passedCount = results.Results.Count(r => r.EvalStatus == "passed");
            int failedCount = results.Results.Count(r => r.EvalStatus == "failed");
            int skippedCount = results.Results.Count(r => r.EvalStatus == "skipped");
            
            Console.WriteLine("Evaluation Results Summary:");
            Console.WriteLine($"- Total solutions: {results.Results.Count}");
            Console.WriteLine($"- Passed: {passedCount}");
            Console.WriteLine($"- Failed: {failedCount}");
            Console.WriteLine($"- Skipped: {skippedCount}");
            
            return results;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error evaluating solutions: {ex.Message}");
            return false;
        }
    }
}