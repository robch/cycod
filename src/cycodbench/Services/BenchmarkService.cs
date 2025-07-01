using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Implementation of the benchmark service for the integrated workflow.
    /// </summary>
    public class BenchmarkService : IBenchmarkService
    {
        private readonly IDatasetService _datasetService;
        private readonly ISolutionService _solutionService;
        private readonly IResultService _resultService;
        private readonly Func<string, IContainerService> _containerServiceFactory;
        
        public BenchmarkService(
            IDatasetService datasetService,
            ISolutionService solutionService,
            IResultService resultService,
            Func<string, IContainerService> containerServiceFactory)
        {
            _datasetService = datasetService;
            _solutionService = solutionService;
            _resultService = resultService;
            _containerServiceFactory = containerServiceFactory;
        }
        
        /// <inheritdoc />
        public async Task<ResultCollection> RunBenchmarkAsync(
            string datasetName,
            string? problemId = null,
            int candidatesCount = 1,
            string? shardSpec = null,
            int parallelCount = 1,
            int timeout = 3600,
            string? containerId = null,
            string outputPath = "results.json")
        {
            Console.WriteLine($"Running benchmark on {datasetName} dataset...");
            
            // Load or download the dataset
            var dataset = await _datasetService.LoadDatasetAsync(datasetName);
            Console.WriteLine($"Loaded {dataset.Problems.Count} problems from {datasetName} dataset");
            
            // Filter problems if a specific problem ID is provided
            if (!string.IsNullOrEmpty(problemId))
            {
                dataset = await _datasetService.FilterProblemsAsync(dataset, problemId: problemId);
                Console.WriteLine($"Filtered to {dataset.Problems.Count} problems matching ID: {problemId}");
            }
            
            // Apply sharding if specified
            if (!string.IsNullOrEmpty(shardSpec))
            {
                var parts = shardSpec.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[0], out int shardIndex) && int.TryParse(parts[1], out int totalShards))
                {
                    dataset = await _datasetService.GetShardAsync(dataset, shardIndex - 1, totalShards);
                    Console.WriteLine($"Using shard {shardIndex}/{totalShards} with {dataset.Problems.Count} problems");
                }
                else
                {
                    throw new ArgumentException($"Invalid shard specification: {shardSpec}. Should be in format 'index/total'.");
                }
            }
            
            // Solve problems
            Console.WriteLine($"Solving {dataset.Problems.Count} problems with {candidatesCount} candidates per problem...");
            
            var solutions = new SolutionCollection();
            
            // Set up parallelism
            var options = new ParallelOptions { MaxDegreeOfParallelism = parallelCount };
            
            await Parallel.ForEachAsync(dataset.Problems, options, async (problem, cancellationToken) =>
            {
                for (int i = 0; i < candidatesCount; i++)
                {
                    try
                    {
                        Console.WriteLine($"Solving problem {problem.Id} (candidate {i + 1}/{candidatesCount})...");
                        
                        var solution = await _solutionService.SolveProblemAsync(problem, containerId, timeout);
                        solution.ProblemDataset = datasetName;
                        solution.ProblemIdx = i;
                        
                        lock (solutions)
                        {
                            solutions.Solutions.Add(solution);
                        }
                        
                        Console.WriteLine($"Generated solution for {problem.Id} (candidate {i + 1}/{candidatesCount})");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to solve problem {problem.Id} (candidate {i + 1}/{candidatesCount}): {ex.Message}");
                    }
                }
            });
            
            // Pick the best solutions if multiple candidates
            SolutionCollection selectedSolutions = solutions;
            
            if (candidatesCount > 1)
            {
                Console.WriteLine("Picking best solutions...");
                selectedSolutions = await _solutionService.PickBestSolutionsAsync(solutions);
                Console.WriteLine($"Selected {selectedSolutions.Solutions.Count} best solutions");
            }
            
            // Evaluate solutions
            Console.WriteLine("Evaluating solutions...");
            
            var results = new ResultCollection();
            
            // We use a sequential approach for evaluation to avoid container conflicts
            foreach (var solution in selectedSolutions.Solutions)
            {
                try
                {
                    // Find the corresponding problem
                    var problem = dataset.Problems.FirstOrDefault(p => p.Id == solution.ProblemId);
                    if (problem == null)
                    {
                        Console.WriteLine($"Warning: Problem {solution.ProblemId} not found in dataset.");
                        continue;
                    }
                    
                    Console.WriteLine($"Evaluating solution for {solution.ProblemId}...");
                    
                    var result = await _solutionService.EvaluateSolutionAsync(solution, problem, containerId, timeout / 3);
                    results.Results.Add(result);
                    
                    Console.WriteLine($"Evaluated solution for {solution.ProblemId}: {result.EvalStatus}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to evaluate solution for {solution.ProblemId}: {ex.Message}");
                }
            }
            
            // Save results
            Console.WriteLine($"Saving {results.Results.Count} results to {outputPath}...");
            await _resultService.SaveResultsAsync(results, outputPath);
            
            // Generate summary
            int passedCount = results.Results.Count(r => r.EvalStatus == "passed");
            int failedCount = results.Results.Count(r => r.EvalStatus == "failed");
            int skippedCount = results.Results.Count(r => r.EvalStatus == "skipped");
            
            Console.WriteLine("Benchmark Results Summary:");
            Console.WriteLine($"- Total problems: {results.Results.Count}");
            Console.WriteLine($"- Passed: {passedCount} ({(double)passedCount / results.Results.Count:P1})");
            Console.WriteLine($"- Failed: {failedCount}");
            Console.WriteLine($"- Skipped: {skippedCount}");
            
            return results;
        }
    }
}