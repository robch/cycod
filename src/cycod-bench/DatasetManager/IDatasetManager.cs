using CycodBench.Models;

namespace CycodBench.DatasetManager;

/// <summary>
/// Interface for managing the SWE-bench dataset.
/// </summary>
public interface IDatasetManager
{
    /// <summary>
    /// Downloads the SWE-bench dataset.
    /// </summary>
    /// <param name="datasetType">The type of dataset to download (verified, full, or lite).</param>
    /// <param name="forceDownload">Whether to force download even if the dataset exists locally.</param>
    Task<bool> DownloadDatasetAsync(string datasetType = "verified", bool forceDownload = false);

    /// <summary>
    /// Gets the problems from the dataset.
    /// </summary>
    /// <param name="datasetType">The type of dataset to use.</param>
    /// <returns>A list of SWE-bench problems.</returns>
    Task<List<SwebenchProblem>> GetProblemsAsync(string datasetType = "verified");

    /// <summary>
    /// Gets a specific problem by ID.
    /// </summary>
    /// <param name="problemId">The problem ID.</param>
    /// <param name="datasetType">The type of dataset to use.</param>
    /// <returns>The SWE-bench problem, or null if not found.</returns>
    Task<SwebenchProblem?> GetProblemByIdAsync(string problemId, string datasetType = "verified");
    
    /// <summary>
    /// Gets the path to the dataset directory.
    /// </summary>
    /// <param name="datasetType">The type of dataset.</param>
    /// <returns>The path to the dataset directory.</returns>
    string GetDatasetPath(string datasetType = "verified");
    
    /// <summary>
    /// Gets the Hugging Face repository name for the dataset.
    /// </summary>
    /// <param name="datasetType">The type of dataset.</param>
    /// <returns>The repository name.</returns>
    string GetDatasetRepositoryName(string datasetType);
    
    /// <summary>
    /// Checks if the dataset exists locally.
    /// </summary>
    /// <param name="datasetType">The type of dataset.</param>
    /// <returns>True if the dataset exists locally, otherwise false.</returns>
    bool IsDatasetCached(string datasetType = "verified");
}