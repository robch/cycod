using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CycodBench.Services
{
    /// <summary>
    /// Docker implementation of the container service.
    /// </summary>
    public class DockerContainerService : IContainerService
    {
        private static readonly Random _random = new();
        private static readonly SemaphoreSlim _dockerSemaphore = new(4); // Limit concurrent Docker operations
        
        /// <inheritdoc />
        public async Task<string> InitContainerAsync(
            string? problemId = null, 
            string? name = null, 
            string? image = null, 
            string? memoryLimit = null, 
            string? cpuLimit = null, 
            string? workspacePath = null, 
            bool setupTools = false, 
            bool setupAgent = false)
        {
            try
            {
                // Determine image to use
                var containerImage = image;
                if (string.IsNullOrEmpty(containerImage) && !string.IsNullOrEmpty(problemId))
                {
                    // Get the SWEbench image for this problem
                    containerImage = GetProblemImageName(problemId);
                    
                    // Pull the image if necessary
                    await PullImageAsync(containerImage);
                }
                else if (string.IsNullOrEmpty(containerImage))
                {
                    // Fallback only if no problem ID and no image specified
                    containerImage = "ubuntu:latest";
                }
                
                // Generate container name if not specified
                var randomSuffix = GenerateRandomString(8);
                var containerName = name ?? (problemId != null ? 
                    $"cycodbench-{problemId}-{randomSuffix}" : 
                    $"cycodbench-{randomSuffix}");
                
                // Build docker run command with a StringBuilder
                var arguments = new StringBuilder("run");
                
                // Add resource limits if specified
                if (!string.IsNullOrEmpty(memoryLimit))
                {
                    arguments.Append($" --memory={memoryLimit}");
                }
                
                if (!string.IsNullOrEmpty(cpuLimit))
                {
                    arguments.Append($" --cpus={cpuLimit}");
                }

                // Run in detached mode
                arguments.Append(" -d");
                
                // Add workspace mount if specified
                if (!string.IsNullOrEmpty(workspacePath))
                {
                    var hostPath = Path.GetFullPath(workspacePath);
                    arguments.Append($" -v \"{hostPath}:/workspace\"");
                }

                // Add testbed volume for problem code
                arguments.Append(" -v /testbed");
                
                // Add container name
                arguments.Append($" --name {containerName}");
                
                // Add image
                arguments.Append($" {containerImage}");
                
                // Add command to keep container running with proper git config
                arguments.Append(" bash -c \"git config --global user.email a && git config --global user.name a && " +
                                "git config --global --add safe.directory /testbed && tail -f /dev/null\"");
                
                // Execute docker run command
                var output = await RunDockerCommandAsync(arguments.ToString());
                var containerId = output.Trim();
                
                Console.WriteLine($"Created container: {containerId}");
                
                // Create standard directories in the container
                if (!string.IsNullOrEmpty(workspacePath))
                {
                    await EnsureWorkspaceDirectoriesAsync(containerId);
                }
                
                // Setup tools if requested
                if (setupTools)
                {
                    await SetupEvaluationToolsAsync(containerId);
                }
                
                // Setup agent if requested
                if (setupAgent)
                {
                    await SetupAgentAsync(containerId);
                }
                
                return containerId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize container: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> CopyToContainerAsync(string containerId, string sourcePath, string destinationPath)
        {
            try
            {
                await _dockerSemaphore.WaitAsync();
                try
                {
                    var command = $"cp \"{sourcePath}\" {containerId}:\"{destinationPath}\"";
                    await RunDockerCommandAsync(command);
                    return true;
                }
                finally
                {
                    _dockerSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to copy to container: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> CopyFromContainerAsync(string containerId, string sourcePath, string destinationPath)
        {
            try
            {
                await _dockerSemaphore.WaitAsync();
                try
                {
                    var command = $"cp {containerId}:\"{sourcePath}\" \"{destinationPath}\"";
                    await RunDockerCommandAsync(command);
                    return true;
                }
                finally
                {
                    _dockerSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to copy from container: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<string> ExecuteCommandAsync(string containerId, string command, string? workDir = null, int timeout = 60)
        {
            try
            {
                var dockerCommand = new StringBuilder("exec");
                
                if (!string.IsNullOrEmpty(workDir))
                {
                    dockerCommand.Append($" -w \"{workDir}\"");
                }
                
                dockerCommand.Append($" {containerId} /bin/bash -c \"{command.Replace("\"", "\\\"")}\"");
                
                return await RunDockerCommandAsync(dockerCommand.ToString(), timeout * 1000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute command in container: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<List<string>> ListContainersAsync()
        {
            try
            {
                var output = await RunDockerCommandAsync("ps -q");
                return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to list containers: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<List<ContainerInfo>> GetContainerDetailsAsync()
        {
            try
            {
                // Get container details in JSON format
                var format = "--format \"{{json .}}\"";
                var output = await RunDockerCommandAsync($"ps -a {format}");
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                var containerInfos = new List<ContainerInfo>();
                foreach (var line in lines)
                {
                    try
                    {
                        // Process each container JSON
                        var jsonElement = JsonDocument.Parse(line).RootElement;
                        
                        var containerInfo = new ContainerInfo
                        {
                            Id = GetJsonPropertyValue(jsonElement, "ID", "ContainerId"),
                            Name = GetJsonPropertyValue(jsonElement, "Names", "Name"),
                            Status = GetJsonPropertyValue(jsonElement, "Status"),
                            Image = GetJsonPropertyValue(jsonElement, "Image"),
                            Ports = GetJsonPropertyValue(jsonElement, "Ports"),
                            CreatedAt = GetJsonPropertyValue(jsonElement, "CreatedAt")
                        };
                        
                        containerInfos.Add(containerInfo);
                    }
                    catch (JsonException)
                    {
                        // Skip invalid JSON
                    }
                }
                
                return containerInfos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get container details: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> StopContainerAsync(string containerId, bool remove = true, int timeout = 60)
        {
            try
            {
                await _dockerSemaphore.WaitAsync();
                try
                {
                    // Check if container exists
                    var checkCommand1 = $"ps -a --filter \"id={containerId}\" --format \"{{{{.ID}}}}\"";
                    var checkCommand2 = $"ps -a --filter \"name={containerId}\" --format \"{{{{.ID}}}}\"";
                    var containerIds1 = await RunDockerCommandAsync(checkCommand1);
                    var containerIds2 = await RunDockerCommandAsync(checkCommand2);
                    var ok1 = !string.IsNullOrWhiteSpace(containerIds1);
                    var ok2 = !string.IsNullOrWhiteSpace(containerIds2);

                    var notFound = !ok1 && !ok2;
                    if (notFound) return false;
                    
                    // Stop the container
                    await RunDockerCommandAsync($"stop {containerId}", timeout * 1000);
                    
                    // Remove the container if requested
                    if (remove)
                    {
                        await RunDockerCommandAsync($"rm {containerId}", timeout * 1000);
                    }
                    
                    return true;
                }
                finally
                {
                    _dockerSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to stop container: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the image name for a SWE-bench problem.
        /// </summary>
        /// <param name="problemId">The ID of the problem.</param>
        /// <returns>The Docker image name for the problem.</returns>
        private string GetProblemImageName(string problemId)
        {
            // Convert double underscores to _1776_ as per swebench convention
            string issueKey = problemId.Replace("__", "_1776_");
            return $"swebench/sweb.eval.x86_64.{issueKey}:latest";
        }
        
        /// <summary>
        /// Pulls a Docker image.
        /// </summary>
        /// <param name="imageName">The name of the image to pull.</param>
        private async Task PullImageAsync(string imageName)
        {
            Console.WriteLine($"Pulling Docker image: {imageName}");
            
            // Use semaphore to limit concurrent pulls
            await _dockerSemaphore.WaitAsync();
            try
            {
                await RunDockerCommandAsync($"pull {imageName}");
            }
            finally
            {
                _dockerSemaphore.Release();
            }
        }
        
        /// <summary>
        /// Ensures that the standard workspace directories exist in the container.
        /// </summary>
        /// <param name="containerId">The container ID.</param>
        private async Task EnsureWorkspaceDirectoriesAsync(string containerId)
        {
            var command = "mkdir -p /workspace/input /workspace/output /workspace/bin";
            await ExecuteCommandAsync(containerId, command);
        }
        
        /// <summary>
        /// Sets up evaluation tools in the container.
        /// </summary>
        /// <param name="containerId">The container ID.</param>
        private async Task SetupEvaluationToolsAsync(string containerId)
        {
            // For SWEbench images, most tools are already installed
            // Just make sure directories have the right permissions
            var commands = new[]
            {
                "chmod -R 755 /workspace",
                "chmod -R 755 /testbed || true"
            };
            
            foreach (var command in commands)
            {
                await ExecuteCommandAsync(containerId, command, timeout: 30);
            }
        }
        
        /// <summary>
        /// Sets up the agent in the container.
        /// </summary>
        /// <param name="containerId">The container ID.</param>
        private async Task SetupAgentAsync(string containerId)
        {
            try
            {
                ConsoleHelpers.WriteLine("Setting up agent in container...");

                var localDotCycodPath = PathHelpers.Combine(Directory.GetCurrentDirectory(), ".cycod");
                var localAgentPath = PathHelpers.Combine(Directory.GetCurrentDirectory(), "cycod");

                await ExecuteCommandAsync(containerId, "mkdir -p /testbed/.cycod", timeout: 10);
                await CopyToContainerAsync(containerId, localDotCycodPath!, "/testbed/.cycod");

                await ExecuteCommandAsync(containerId, "mkdir -p /workspace/bin", timeout: 10);
                await CopyToContainerAsync(containerId, localAgentPath!, "/workspace/bin/cycod");

                await ExecuteCommandAsync(containerId, "chmod +x /workspace/bin/cycod", timeout: 10);

                ConsoleHelpers.WriteLine("Setting up agent in container... Done!");
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine($"Error setting up agent: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Execute a Docker command and return the output.
        /// </summary>
        /// <param name="arguments">The arguments for the Docker command.</param>
        /// <param name="timeoutMs">Timeout in milliseconds.</param>
        /// <returns>The command output.</returns>
        private async Task<string> RunDockerCommandAsync(string arguments, int timeoutMs = 60000)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Console.WriteLine($"Running Docker command: docker {arguments}");
            
            using var process = new Process { StartInfo = processStartInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    outputBuilder.AppendLine(args.Data);
                }
            };
            
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    errorBuilder.AppendLine(args.Data);
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            if (await Task.Run(() => process.WaitForExit(timeoutMs)))
            {
                if (process.ExitCode != 0)
                {
                    string errorOutput = errorBuilder.ToString().Trim();
                    
                    // Some docker commands like 'docker stop' may return non-zero when container doesn't exist
                    if (errorOutput.Contains("No such container") || errorOutput.Contains("not found"))
                    {
                        return string.Empty;
                    }
                    
                    throw new Exception($"Docker command failed with exit code {process.ExitCode}: {errorOutput}");
                }
                
                return outputBuilder.ToString().Trim();
            }
            else
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    // Ignore errors when trying to kill the process
                }
                
                throw new TimeoutException($"Docker command timed out after {timeoutMs / 1000} seconds: docker {arguments}");
            }
        }

        /// <summary>
        /// Gets a JSON property value from a JsonElement with fallback property names.
        /// </summary>
        /// <param name="element">The JsonElement to extract from.</param>
        /// <param name="propertyNames">One or more property names to try.</param>
        /// <returns>The property value or empty string if not found.</returns>
        private string GetJsonPropertyValue(JsonElement element, params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                if (element.TryGetProperty(propName, out var prop))
                {
                    return prop.ToString();
                }
            }
            return string.Empty;
        }
        
        /// <summary>
        /// Generates a random alphanumeric string.
        /// </summary>
        /// <param name="length">The length of the string to generate.</param>
        /// <returns>A random string.</returns>
        private string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
        
        /// <summary>
        /// Run an evaluation in the container.
        /// </summary>
        /// <param name="containerId">The container ID.</param>
        /// <param name="predictionPath">Path to the predictions file in the container.</param>
        /// <param name="outputPath">Path to the output directory in the container.</param>
        /// <param name="numWorkers">Number of worker processes.</param>
        /// <returns>The evaluation output.</returns>
        public async Task<string> RunEvaluationAsync(string containerId, string predictionPath, string outputPath, int numWorkers = 1)
        {
            var command = $"python -m swebench.harness.run_evaluation " +
                          $"--predictions_path {predictionPath} " +
                          $"--report_dir {outputPath} " +
                          $"--max_workers {numWorkers} " +
                          $"--cache_level instance " +
                          $"--namespace swebench " +
                          $"--instance_image_tag latest";
            
            return await ExecuteCommandAsync(containerId, command, workDir: "/workspace", timeout: 300);
        }
    }
}