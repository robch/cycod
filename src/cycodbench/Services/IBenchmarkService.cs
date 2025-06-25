using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Interface for the integrated benchmark workflow.
    /// </summary>
    public interface IBenchmarkService
    {
        /// <summary>
        /// Run the benchmark workflow for a dataset.
        /// </summary>
        /// <param name="datasetName">Dataset name or file path</param>
        /// <param name="problemId">Problem ID to run (optional)</param>
        /// <param name="candidatesCount">Candidates to generate per problem</param>
        /// <param name="shardSpec">Shard specification (e.g. "1/4")</param>
        /// <param name="parallelCount">Problems to process in parallel</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <param name="containerId">Container ID to use (optional)</param>
        /// <param name="outputPath">Path to save the results file</param>
        /// <returns>The evaluation results</returns>
        Task<ResultCollection> RunBenchmarkAsync(
            string datasetName,
            string? problemId = null,
            int candidatesCount = 1,
            string? shardSpec = null,
            int parallelCount = 1,
            int timeout = 3600,
            string? containerId = null,
            string outputPath = "results.json");
    }
}