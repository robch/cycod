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
            string? setupAgentFromPath = null,
            bool setupTools = false)
        {
            try
            {
                // Validate/create container name and local workspace path
                var containerImage = await EnsureImageName(image, problemId);
                var containerName = EnsureContainerName(name, problemId);
                workspacePath = EnsureLocalWorkspacePath(workspacePath, containerName);

                // Build docker run command with a StringBuilder
                var arguments = new StringBuilder("run");

                var addMemory = !string.IsNullOrEmpty(memoryLimit);
                if (addMemory) arguments.Append($" --memory={memoryLimit}");

                var addCpus = !string.IsNullOrEmpty(cpuLimit);
                if (addCpus) arguments.Append($" --cpus={cpuLimit}");

                arguments.Append(" -d");
                arguments.Append($" -v \"{Path.GetFullPath(workspacePath)}:/workspace\"");
                arguments.Append(" -v /testbed");
                arguments.Append($" --name {containerName}");
                arguments.Append($" {containerImage}");
                arguments.Append(" bash -c \"git config --global user.email a && git config --global user.name a && " +
                                "git config --global --add safe.directory /testbed && tail -f /dev/null\"");

                // Execute docker run command to create the container
                var output = await RunDockerCommandAsync(arguments.ToString());
                var containerId = output.Trim();
                Console.WriteLine($"Created container: {containerId}");

                // Ensure the container workspace paths exist and set up tools/agent if requested
                await EnsureContainerWorkspacePaths(containerId);
                if (setupTools) await SetupEvaluationToolsAsync(containerId);

                var setupAgent = !string.IsNullOrEmpty(setupAgentFromPath);
                if (setupAgent) await SetupAgentAsync(containerId, setupAgentFromPath!);

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
        /// Ensures the local workspace path exists or creates a new one based on the container name.
        /// </summary>
        /// <param name="workspacePath">The specified workspace path, or null to create a new one.</param>
        /// <param name="containerName">The name of the container to base the new workspace path on.</param>
        /// <returns>The full path to the workspace directory.</returns>
        private string EnsureLocalWorkspacePath(string? workspacePath, string containerName)
        {
            // If workspacePath is specified, ensure it exists
            var specified = !string.IsNullOrEmpty(workspacePath);
            var needsCreation = specified && !Directory.Exists(workspacePath);
            if (needsCreation) DirectoryHelpers.EnsureDirectoryExists(workspacePath!);

            var ok = specified && Directory.Exists(workspacePath);
            if (ok) return Path.GetFullPath(workspacePath!);

            // If not specified, create a new workspace path based on container name
            var shortId = containerName.Length > 32 ? containerName[..32] : containerName;
            string workspaceId = $"{shortId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            workspacePath = PathHelpers.Combine(Directory.GetCurrentDirectory(), "workspaces", workspaceId);

            needsCreation = !string.IsNullOrEmpty(workspacePath) && !Directory.Exists(workspacePath);
            if (needsCreation) DirectoryHelpers.EnsureDirectoryExists(workspacePath!);

            ok = !string.IsNullOrEmpty(workspacePath) && Directory.Exists(workspacePath);
            if (ok) return Path.GetFullPath(workspacePath!);

            // Fallback to system temp path if workspacePath is still invalid
            return Path.GetTempPath();
        }

        /// <summary>
        /// Ensures a valid container name is provided.
        /// </summary>
        /// <param name="containerName">The specified container name, or null to generate a new one.</param>
        /// <param name="problemId">The ID of the problem, used to generate a container name if none is specified.</param>
        /// <returns></returns>
        private string EnsureContainerName(string? containerName, string? problemId)
        {
            var randomSuffix = GenerateRandomString(8);
            containerName ??= (problemId != null ?
                $"cycodbench-{problemId}-{randomSuffix}" :
                $"cycodbench-{randomSuffix}");
            return containerName;
        }

        /// <summary>
        /// Ensures a valid Docker image is specified or pulls the appropriate image for the problem.
        /// </summary>
        /// <param name="image">The specified Docker image, or null to use the default image.</param>
        /// <param name="problemId">The ID of the problem, used to determine the image if none is specified.</param>
        /// <returns>The Docker image name to use.</returns>
        private async Task<string?> EnsureImageName(string? image, string? problemId)
        {

            // Determine image to use
            var containerImage = image;
            if (string.IsNullOrEmpty(containerImage) && !string.IsNullOrEmpty(problemId))
            {
                containerImage = GetProblemImageName(problemId);
                await PullImageAsync(containerImage);
            }
            else if (string.IsNullOrEmpty(containerImage))
            {
                containerImage = "ubuntu:latest";
            }

            return containerImage;
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
        private async Task EnsureContainerWorkspacePaths(string containerId)
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
        /// <param name="localAgentPath">The local path to the agent files.</param>
        private async Task SetupAgentAsync(string containerId, string localAgentPath)
        {
            try
            {
                ConsoleHelpers.WriteLine("Setting up agent in container...");

                var localDotCycodPath = PathHelpers.Combine(Directory.GetCurrentDirectory(), ".cycod");
                await CopyToContainerAsync(containerId, localDotCycodPath!, "/testbed/.cycod");

                await ExecuteCommandAsync(containerId, "mkdir -p /workspace/bin", timeout: 10);
                await CopyToContainerAsync(containerId, $"{localAgentPath}/linux/cycod/cycod", "/workspace/bin/cycod");
                await ExecuteCommandAsync(containerId, "chmod +x /workspace/bin/cycod", timeout: 10);

                var cycoDmdLocalFilePath = PathHelpers.Combine(localAgentPath, "linux/cycodmd/cycodmd");
                var copyCycoDmd = FileHelpers.FileExists(cycoDmdLocalFilePath);
                if (copyCycoDmd) 
                {
                    await CopyToContainerAsync(containerId, cycoDmdLocalFilePath!, "/workspace/bin/cycodmd");
                    await ExecuteCommandAsync(containerId, "chmod +x /workspace/bin/cycodmd", timeout: 10);
                }

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
                    ConsoleHelpers.WriteDebugLine(args.Data);
                    outputBuilder.AppendLine(args.Data);
                }
            };
            
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    ConsoleHelpers.WriteDebugLine(args.Data);
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