using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Interface for solution operations.
    /// </summary>
    public interface ISolutionService
    {
        /// <summary>
        /// Generate a solution for a problem.
        /// </summary>
        /// <param name="problem">Problem to solve</param>
        /// <param name="containerId">Container ID to use (optional)</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <returns>The generated solution</returns>
        Task<Solution> SolveProblemAsync(Problem problem, string? containerId = null, int timeout = 3600);
        
        /// <summary>
        /// Load solutions from a file.
        /// </summary>
        /// <param name="filePath">Path to the solutions file</param>
        /// <returns>The loaded solutions</returns>
        Task<SolutionCollection> LoadSolutionsAsync(string filePath);
        
        /// <summary>
        /// Save solutions to a file.
        /// </summary>
        /// <param name="solutions">Solutions to save</param>
        /// <param name="filePath">Path to save the solutions file</param>
        /// <returns>The path to the saved file</returns>
        Task<string> SaveSolutionsAsync(SolutionCollection solutions, string filePath);
        
        /// <summary>
        /// Filter solutions.
        /// </summary>
        /// <param name="solutions">Solutions to filter</param>
        /// <param name="problemId">Problem ID to filter by (optional)</param>
        /// <param name="repository">Repository name to filter by (optional)</param>
        /// <param name="containsText">Text to search for in solution details (optional)</param>
        /// <param name="maxSolutions">Maximum number of solutions to include (optional)</param>
        /// <returns>A new collection containing only the filtered solutions</returns>
        Task<SolutionCollection> FilterSolutionsAsync(
            SolutionCollection solutions,
            string? problemId = null,
            string? repository = null, 
            string? containsText = null,
            int? maxSolutions = null);
        
        /// <summary>
        /// Merge multiple solutions collections into one.
        /// </summary>
        /// <param name="solutionCollections">Solution collections to merge</param>
        /// <returns>The merged solution collection</returns>
        Task<SolutionCollection> MergeSolutionsAsync(params SolutionCollection[] solutionCollections);
        
        /// <summary>
        /// Pick the best solution for each problem from multiple candidates.
        /// </summary>
        /// <param name="solutions">Solutions to pick from</param>
        /// <returns>A new collection containing the best solution for each problem</returns>
        Task<SolutionCollection> PickBestSolutionsAsync(SolutionCollection solutions);
        
        /// <summary>
        /// Evaluate a solution.
        /// </summary>
        /// <param name="solution">Solution to evaluate</param>
        /// <param name="problem">Problem to evaluate against</param>
        /// <param name="containerId">Container ID to use (optional)</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <returns>The evaluation result</returns>
        Task<Result> EvaluateSolutionAsync(Solution solution, Problem problem, string? containerId = null, int timeout = 600);
    }
}