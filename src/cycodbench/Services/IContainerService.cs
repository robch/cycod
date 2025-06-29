using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycodBench.Services
{
    /// <summary>
    /// Interface for container operations.
    /// </summary>
    public interface IContainerService
    {
        /// <summary>
        /// Initialize a container.
        /// </summary>
        /// <param name="problemId">Problem ID to initialize container for (optional)</param>
        /// <param name="name">Custom name for the container (optional)</param>
        /// <param name="image">Container image to use (optional)</param>
        /// <param name="memoryLimit">Memory limit (e.g. "8g") (optional)</param>
        /// <param name="cpuLimit">CPU limit (e.g. 4) (optional)</param>
        /// <param name="workspacePath">Host path to mount as workspace (optional)</param>
        /// <param name="setupTools">Whether to set up evaluation tools in the container</param>
        /// <param name="setupAgent">Whether to set up the agent in the container</param>
        /// <returns>The ID of the created container</returns>
        Task<string> InitContainerAsync(
            string? problemId = null,
            string? name = null,
            string? image = null,
            string? memoryLimit = null,
            string? cpuLimit = null,
            string? workspacePath = null,
            bool setupTools = false,
            bool setupAgent = false);
        
        /// <summary>
        /// Copy files to a container.
        /// </summary>
        /// <param name="containerId">ID of the container</param>
        /// <param name="sourcePath">Source path on the host</param>
        /// <param name="destinationPath">Destination path in the container</param>
        /// <returns>Whether the copy was successful</returns>
        Task<bool> CopyToContainerAsync(string containerId, string sourcePath, string destinationPath);
        
        /// <summary>
        /// Copy files from a container.
        /// </summary>
        /// <param name="containerId">ID of the container</param>
        /// <param name="sourcePath">Source path in the container</param>
        /// <param name="destinationPath">Destination path on the host</param>
        /// <returns>Whether the copy was successful</returns>
        Task<bool> CopyFromContainerAsync(string containerId, string sourcePath, string destinationPath);
        
        /// <summary>
        /// Execute a command in a container.
        /// </summary>
        /// <param name="containerId">ID of the container</param>
        /// <param name="command">Command to execute</param>
        /// <param name="workDir">Working directory in the container</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <returns>Command output</returns>
        Task<string> ExecuteCommandAsync(string containerId, string command, string? workDir = null, int timeout = 60);
        
        /// <summary>
        /// List running containers.
        /// </summary>
        /// <returns>List of container IDs</returns>
        Task<List<string>> ListContainersAsync();
        
        /// <summary>
        /// Get detailed information about containers.
        /// </summary>
        /// <returns>Container information</returns>
        Task<List<ContainerInfo>> GetContainerDetailsAsync();
        
        /// <summary>
        /// Stop and optionally remove a container.
        /// </summary>
        /// <param name="containerId">ID of the container</param>
        /// <param name="remove">Whether to remove the container after stopping</param>
        /// <param name="timeout">Timeout in seconds</param>
        /// <returns>Whether the stop was successful</returns>
        Task<bool> StopContainerAsync(string containerId, bool remove = true, int timeout = 60);
    }

    /// <summary>
    /// Container information.
    /// </summary>
    public class ContainerInfo
    {
        /// <summary>
        /// ID of the container.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the container.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Status of the container.
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Image used by the container.
        /// </summary>
        public string Image { get; set; } = string.Empty;
        
        /// <summary>
        /// Port mappings.
        /// </summary>
        public string Ports { get; set; } = string.Empty;
        
        /// <summary>
        /// When the container was created.
        /// </summary>
        public string CreatedAt { get; set; } = string.Empty;
    }
}