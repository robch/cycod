using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Interface for result operations.
    /// </summary>
    public interface IResultService
    {
        /// <summary>
        /// Load results from a file.
        /// </summary>
        /// <param name="filePath">Path to the results file</param>
        /// <returns>The loaded results</returns>
        Task<ResultCollection> LoadResultsAsync(string filePath);
        
        /// <summary>
        /// Save results to a file.
        /// </summary>
        /// <param name="results">Results to save</param>
        /// <param name="filePath">Path to save the results file</param>
        /// <returns>The path to the saved file</returns>
        Task<string> SaveResultsAsync(ResultCollection results, string filePath);
        
        /// <summary>
        /// Filter results.
        /// </summary>
        /// <param name="results">Results to filter</param>
        /// <param name="problemId">Problem ID to filter by (optional)</param>
        /// <param name="repository">Repository name to filter by (optional)</param>
        /// <param name="containsText">Text to search for in result details (optional)</param>
        /// <param name="status">Status to filter by (optional)</param>
        /// <returns>A new collection containing only the filtered results</returns>
        Task<ResultCollection> FilterResultsAsync(
            ResultCollection results,
            string? problemId = null,
            string? repository = null, 
            string? containsText = null,
            string? status = null);
        
        /// <summary>
        /// Merge multiple result collections into one.
        /// </summary>
        /// <param name="resultCollections">Result collections to merge</param>
        /// <returns>The merged result collection</returns>
        Task<ResultCollection> MergeResultsAsync(params ResultCollection[] resultCollections);
        
        /// <summary>
        /// Generate a report from results.
        /// </summary>
        /// <param name="results">Results to generate report from</param>
        /// <param name="verbose">Whether to include verbose details in the report</param>
        /// <returns>The generated report as markdown</returns>
        Task<string> GenerateReportAsync(ResultCollection results, bool verbose = false);
    }
}