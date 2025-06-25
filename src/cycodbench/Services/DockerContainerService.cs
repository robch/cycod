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
                var containerImage = image ?? "ubuntu:latest";
                
                // Generate container name if not specified
                var containerName = name ?? $"cycodbench-{Guid.NewGuid().ToString().Substring(0, 8)}";
                
                // Build docker run command
                var arguments = new StringBuilder("docker run -d");
                
                // Add resource limits if specified
                if (!string.IsNullOrEmpty(memoryLimit))
                {
                    arguments.Append($" --memory={memoryLimit}");
                }
                
                if (!string.IsNullOrEmpty(cpuLimit))
                {
                    arguments.Append($" --cpus={cpuLimit}");
                }
                
                // Add workspace mount if specified
                if (!string.IsNullOrEmpty(workspacePath))
                {
                    var hostPath = Path.GetFullPath(workspacePath);
                    arguments.Append($" -v \"{hostPath}:/workspace\"");
                }
                
                // Add container name
                arguments.Append($" --name {containerName}");
                
                // Add image
                arguments.Append($" {containerImage}");
                
                // Add command to keep container running
                arguments.Append(" tail -f /dev/null");
                
                // Execute docker run command
                var output = await ExecuteCommandAsync(arguments.ToString());
                var containerId = output.Trim();
                
                Console.WriteLine($"Created container: {containerId}");
                
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
                var command = $"docker cp \"{sourcePath}\" {containerId}:{destinationPath}";
                await ExecuteCommandAsync(command);
                return true;
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
                var command = $"docker cp {containerId}:{sourcePath} \"{destinationPath}\"";
                await ExecuteCommandAsync(command);
                return true;
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
                var dockerCommand = new StringBuilder($"docker exec");
                
                if (!string.IsNullOrEmpty(workDir))
                {
                    dockerCommand.Append($" -w {workDir}");
                }
                
                dockerCommand.Append($" {containerId} /bin/bash -c \"{command.Replace("\"", "\\\"")}\"");
                
                return await ExecuteCommandAsync(dockerCommand.ToString(), timeout);
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
                var output = await ExecuteCommandAsync("docker ps -q");
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
                var containerIds = await ListContainersAsync();
                var containerInfos = new List<ContainerInfo>();
                
                if (containerIds.Count == 0)
                {
                    return containerInfos;
                }
                
                // Get container details in JSON format
                var format = "--format \"{{json .}}\"";
                var output = await ExecuteCommandAsync($"docker ps {format}");
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    try
                    {
                        var containerInfo = JsonSerializer.Deserialize<ContainerInfo>(line);
                        if (containerInfo != null)
                        {
                            containerInfos.Add(containerInfo);
                        }
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
        public async Task<bool> StopContainerAsync(string containerId, bool remove = true, int timeout = 10)
        {
            try
            {
                // Stop the container
                await ExecuteCommandAsync($"docker stop --time={timeout} {containerId}");
                
                // Remove the container if requested
                if (remove)
                {
                    await ExecuteCommandAsync($"docker rm {containerId}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to stop container: {ex.Message}", ex);
            }
        }
        
        private async Task<string> ExecuteCommandAsync(string command, int timeout = 60)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
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
            
            if (await Task.Run(() => process.WaitForExit(timeout * 1000)))
            {
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Command failed with exit code {process.ExitCode}: {errorBuilder}");
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
                    // Ignore process kill errors
                }
                
                throw new TimeoutException($"Command timed out after {timeout} seconds: {command}");
            }
        }
        
        private async Task SetupEvaluationToolsAsync(string containerId)
        {
            // Basic setup for Ubuntu-based containers
            var commands = new[]
            {
                "apt-get update",
                "apt-get install -y git python3 python3-pip",
                "pip3 install pytest"
            };
            
            foreach (var command in commands)
            {
                await ExecuteCommandAsync(containerId, command, timeout: 300);
            }
        }
        
        private async Task SetupAgentAsync(string containerId)
        {
            // Copy agent files and install dependencies
            // This is a placeholder - actual agent setup would depend on the agent implementation
            await ExecuteCommandAsync(containerId, "mkdir -p /agent", timeout: 10);
        }
    }
}