using CycodBench.Models;

namespace CycodBench.DockerManager;

/// <summary>
/// Interface for managing Docker containers for benchmark problems.
/// </summary>
public interface IDockerManager
{
    /// <summary>
    /// Starts a Docker container for the given problem.
    /// </summary>
    /// <param name="problemId">The ID of the problem.</param>
    /// <param name="workspacePath">The path to the workspace directory.</param>
    /// <returns>The container ID.</returns>
    Task<string> StartContainerAsync(string problemId, string workspacePath);

    /// <summary>
    /// Stops and removes a Docker container.
    /// </summary>
    /// <param name="containerId">The ID or name of the container to stop.</param>
    /// <param name="removeImage">Optional image to remove after container is stopped.</param>
    Task StopContainerAsync(string containerId, string removeImage = "");

    /// <summary>
    /// Executes a command in the specified Docker container.
    /// </summary>
    /// <param name="containerId">The ID or name of the container.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>The command output.</returns>
    Task<string> ExecuteCommandAsync(string containerId, string command, int timeoutMs = 30000);

    /// <summary>
    /// Copies a file or directory into the Docker container.
    /// </summary>
    /// <param name="containerId">The ID or name of the container.</param>
    /// <param name="sourcePath">The source path on the host system.</param>
    /// <param name="destinationPath">The destination path in the container.</param>
    Task CopyToContainerAsync(string containerId, string sourcePath, string destinationPath);

    /// <summary>
    /// Copies a file or directory from the Docker container to the host.
    /// </summary>
    /// <param name="containerId">The ID or name of the container.</param>
    /// <param name="sourcePath">The source path in the container.</param>
    /// <param name="destinationPath">The destination path on the host system.</param>
    Task CopyFromContainerAsync(string containerId, string sourcePath, string destinationPath);

    /// <summary>
    /// Gets the volume path for a container.
    /// </summary>
    /// <param name="containerId">The ID or name of the container.</param>
    /// <param name="volumeName">The name of the volume to find.</param>
    /// <returns>The path to the volume on the host system.</returns>
    Task<string> GetVolumePathAsync(string containerId, string volumeName);

    /// <summary>
    /// Checks if Docker is available and running.
    /// </summary>
    /// <returns>True if Docker is available and running, otherwise false.</returns>
    Task<bool> IsDockerAvailableAsync();
    
    /// <summary>
    /// Pulls a Docker image.
    /// </summary>
    /// <param name="imageName">The name of the image to pull.</param>
    Task PullImageAsync(string imageName);
    
    /// <summary>
    /// Gets the image name for a SWE-bench problem.
    /// </summary>
    /// <param name="problemId">The ID of the problem.</param>
    /// <returns>The Docker image name for the problem.</returns>
    string GetProblemImageName(string problemId);
}