using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using CycodBench.Configuration;
using CycodBench.Logging;
using CycodBench.Models;

namespace CycodBench.DockerManager;

/// <summary>
/// Implementation of the Docker container management for benchmark problems.
/// </summary>
public class DockerManager : IDockerManager
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private static readonly Random _random = new();
    private static readonly SemaphoreSlim _dockerSemaphore = new(4); // Limit concurrent Docker operations
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DockerManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration instance.</param>
    public DockerManager(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    /// <inheritdoc/>
    public string GetProblemImageName(string problemId)
    {
        // Following the same pattern as Augment Code's implementation
        // Convert double underscores to _1776_ as per their convention
        string issueKey = problemId.Replace("__", "_1776_");
        return $"swebench/sweb.eval.x86_64.{issueKey}:latest";
    }

    /// <inheritdoc/>
    public async Task<bool> IsDockerAvailableAsync()
    {
        try
        {
            string output = await RunDockerCommandAsync("info");
            return !string.IsNullOrEmpty(output);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Docker is not available: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task PullImageAsync(string imageName)
    {
        _logger.Info($"Pulling Docker image: {imageName}");
        
        // Use semaphore to limit concurrent pulls
        await _dockerSemaphore.WaitAsync();
        try
        {
            await RunDockerCommandAsync($"pull {imageName}");
            _logger.Info($"Successfully pulled Docker image: {imageName}");
        }
        finally
        {
            _dockerSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<string> StartContainerAsync(string problemId, string workspacePath)
    {
        // Clean up any existing container with the same name
        await StopContainerAsync($"sweb.cycodbench.{problemId}");
        
        // Get the image name for the problem
        string imageName = GetProblemImageName(problemId);
        
        // Pull the image if necessary
        await PullImageAsync(imageName);
        
        _logger.Info($"Starting container for problem: {problemId}");
        
        // Generate unique container name
        string randomSuffix = GenerateRandomString(8);
        string containerName = $"sweb.cycodbench.{problemId}_{randomSuffix}";
        
        // Get resource limits from configuration
        string memoryLimit = _configuration.GetString("container_memory_limit", "8g");
        double cpuLimit = _configuration.GetDouble("container_cpu_limit", 4.0);
        
        // Construct the docker run command
        string runCommand = $"run --name {containerName} " +
                           $"--memory={memoryLimit} " +
                           $"--cpus={cpuLimit} " +
                           "-d " + // Detached mode
                           "--volume /testbed " + 
                           $"{imageName} " +
                           "bash -c 'git config --global user.email a && git config --global user.name a && " +
                           "git config --global --add safe.directory /testbed && " + 
                           "git commit --allow-empty -am cycodbench && sleep 7200'";
        
        await _dockerSemaphore.WaitAsync();
        try
        {
            string output = await RunDockerCommandAsync(runCommand);
            string containerId = output.Trim();
            _logger.Info($"Started container with ID: {containerId}");
            
            // Wait a moment for the container to fully start
            await Task.Delay(5000);
            
            // Create symlink from workspace to container volume
            string volumePath = await GetVolumePathAsync(containerId, "/testbed");
            
            // Make sure the workspace directory exists
            Directory.CreateDirectory(workspacePath);
            
            // Create a symlink or directory reference
            string problemWorkspacePath = Path.Combine(workspacePath, problemId);
            if (File.Exists(problemWorkspacePath))
            {
                File.Delete(problemWorkspacePath);
            }
            
            // On Windows, we can't create symlinks easily, so we'll store the path
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                File.WriteAllText(problemWorkspacePath, volumePath);
            }
            else
            {
                // Unix-based systems can use symlinks
                if (Directory.Exists(problemWorkspacePath))
                {
                    Directory.Delete(problemWorkspacePath);
                }
                CreateSymlink(volumePath, problemWorkspacePath);
            }
            
            return containerId;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to start container for problem {problemId}");
            throw;
        }
        finally
        {
            _dockerSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task StopContainerAsync(string containerId, string removeImage = "")
    {
        _logger.Info($"Stopping container: {containerId}");
        
        try
        {
            // Check if container exists
            string checkCommand = $"ps -a --filter \"name={containerId}\" --format \"{{{{.ID}}}}\"";
            string containerIds = await RunDockerCommandAsync(checkCommand);
            
            if (string.IsNullOrWhiteSpace(containerIds))
            {
                _logger.Info($"Container {containerId} not found, nothing to stop");
                return;
            }
            
            // Stop the container
            await RunDockerCommandAsync($"stop {containerId}");
            _logger.Info($"Stopped container: {containerId}");
            
            // Remove the container
            await RunDockerCommandAsync($"rm {containerId}");
            _logger.Info($"Removed container: {containerId}");
            
            // Remove the image if specified
            if (!string.IsNullOrWhiteSpace(removeImage))
            {
                await Task.Delay(5000); // Small delay to ensure container removal is complete
                await RunDockerCommandAsync($"rmi -f {removeImage}");
                _logger.Info($"Removed image: {removeImage}");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Error stopping container {containerId}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<string> ExecuteCommandAsync(string containerId, string command, int timeoutMs = 30000)
    {
        _logger.Debug($"Executing command in container {containerId}: {command}");
        string execCommand = $"exec {containerId} {command}";
        return await RunDockerCommandAsync(execCommand, timeoutMs);
    }

    /// <inheritdoc/>
    public async Task CopyToContainerAsync(string containerId, string sourcePath, string destinationPath)
    {
        _logger.Debug($"Copying to container {containerId}: {sourcePath} -> {destinationPath}");
        await RunDockerCommandAsync($"cp \"{sourcePath}\" {containerId}:\"{destinationPath}\"");
    }

    /// <inheritdoc/>
    public async Task CopyFromContainerAsync(string containerId, string sourcePath, string destinationPath)
    {
        _logger.Debug($"Copying from container {containerId}: {sourcePath} -> {destinationPath}");
        await RunDockerCommandAsync($"cp {containerId}:\"{sourcePath}\" \"{destinationPath}\"");
    }

    /// <inheritdoc/>
    public async Task<string> GetVolumePathAsync(string containerId, string volumeName)
    {
        // Get container inspect information in JSON format
        string inspectOutput = await RunDockerCommandAsync($"inspect {containerId}");
        
        // Use regex to extract the volume path (simpler than parsing the full JSON in this case)
        string pattern = $@"""Source""\s*:\s*""([^""]+)""";
        Match match = Regex.Match(inspectOutput, pattern);
        
        if (match.Success)
        {
            string volumePath = match.Groups[1].Value;
            _logger.Debug($"Found volume path for container {containerId}: {volumePath}");
            return volumePath;
        }
        
        throw new InvalidOperationException($"Could not find volume path for container {containerId}");
    }

    /// <summary>
    /// Runs a Docker command asynchronously.
    /// </summary>
    /// <param name="arguments">The arguments to pass to docker.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>The command output.</returns>
    private async Task<string> RunDockerCommandAsync(string arguments, int timeoutMs = 60000)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) => 
        {
            if (e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for the process with timeout
        if (await Task.Run(() => process.WaitForExit(timeoutMs)))
        {
            if (process.ExitCode != 0)
            {
                string errorOutput = errorBuilder.ToString();
                _logger.Warning($"Docker command failed with exit code {process.ExitCode}: {errorOutput}");
                if (errorOutput.Contains("No such container") || errorOutput.Contains("not found"))
                {
                    return string.Empty;
                }
                
                throw new Exception($"Docker command failed with exit code {process.ExitCode}: {errorOutput}");
            }

            return outputBuilder.ToString();
        }
        else
        {
            try
            {
                // Process didn't exit within timeout, kill it
                process.Kill();
            }
            catch
            {
                // Ignore errors when trying to kill the process
            }

            throw new TimeoutException($"Docker command timed out after {timeoutMs}ms: docker {arguments}");
        }
    }
    
    /// <summary>
    /// Creates a symbolic link.
    /// </summary>
    /// <param name="target">The target path.</param>
    /// <param name="link">The link path.</param>
    private void CreateSymlink(string target, string link)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ln",
                Arguments = $"-sf \"{target}\" \"{link}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new Exception($"Failed to create symlink: {error}");
        }
    }
    
    /// <summary>
    /// Generates a random alphanumeric string.
    /// </summary>
    /// <param name="length">The length of the string.</param>
    /// <returns>A random string.</returns>
    private static string GenerateRandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}