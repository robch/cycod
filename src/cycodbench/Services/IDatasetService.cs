using System.Threading.Tasks;
using CycodBench.Models;

namespace CycodBench.Services
{
    /// <summary>
    /// Interface for dataset operations.
    /// </summary>
    public interface IDatasetService
    {
        /// <summary>
        /// Download a dataset from the repository.
        /// </summary>
        /// <param name="datasetName">Dataset name ('verified', 'full', or 'lite')</param>
        /// <param name="outputPath">Path to save the dataset file</param>
        /// <param name="force">Whether to force redownload if the dataset already exists</param>
        /// <returns>The path to the downloaded dataset file</returns>
        Task<string> DownloadDatasetAsync(string datasetName, string? outputPath = null, bool force = false);
        
        /// <summary>
        /// Load a dataset from a file.
        /// </summary>
        /// <param name="filePath">Path to the dataset file</param>
        /// <returns>The loaded dataset</returns>
        Task<ProblemDataset> LoadDatasetAsync(string filePath);
        
        /// <summary>
        /// Save a dataset to a file.
        /// </summary>
        /// <param name="dataset">Dataset to save</param>
        /// <param name="filePath">Path to save the dataset file</param>
        /// <returns>The path to the saved file</returns>
        Task<string> SaveDatasetAsync(ProblemDataset dataset, string filePath);
        
        /// <summary>
        /// Filter problems in a dataset.
        /// </summary>
        /// <param name="dataset">Dataset to filter</param>
        /// <param name="problemId">Problem ID to filter by (optional)</param>
        /// <param name="repository">Repository name to filter by (optional)</param>
        /// <param name="containsText">Text to search for in problem details (optional)</param>
        /// <param name="maxProblems">Maximum number of problems to include (optional)</param>
        /// <returns>A new dataset containing only the filtered problems</returns>
        Task<ProblemDataset> FilterProblemsAsync(
            ProblemDataset dataset,
            string? problemId = null,
            string? repository = null, 
            string? containsText = null,
            int? maxProblems = null);
        
        /// <summary>
        /// Merge multiple datasets into one.
        /// </summary>
        /// <param name="datasets">Datasets to merge</param>
        /// <returns>The merged dataset</returns>
        Task<ProblemDataset> MergeDatasetsAsync(params ProblemDataset[] datasets);
        
        /// <summary>
        /// Create shards from a dataset.
        /// </summary>
        /// <param name="dataset">Dataset to shard</param>
        /// <param name="totalShards">Total number of shards</param>
        /// <returns>Array of sharded datasets</returns>
        Task<ProblemDataset[]> CreateShardsAsync(ProblemDataset dataset, int totalShards);
        
        /// <summary>
        /// Get a specific shard from a dataset.
        /// </summary>
        /// <param name="dataset">Dataset to get shard from</param>
        /// <param name="shardIndex">Shard index (0-based)</param>
        /// <param name="totalShards">Total number of shards</param>
        /// <returns>The specified shard</returns>
        Task<ProblemDataset> GetShardAsync(ProblemDataset dataset, int shardIndex, int totalShards);
    }
}