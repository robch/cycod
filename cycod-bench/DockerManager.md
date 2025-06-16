# DockerManager.cs

This class is responsible for managing Docker containers used to run the agent on SWE-bench problems.

## Responsibilities

- Create and manage Docker containers for problem execution
- Pull required Docker images
- Mount volumes and set up container environments
- Execute commands inside containers
- Copy files to and from containers
- Monitor container resource usage
- Handle container cleanup and resource management
- Implement parallel container management

## Public Interface

```csharp
public interface IDockerManager
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    
    Task<DockerContainer> CreateContainerForProblemAsync(
        string problemId, 
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    Task<CommandResult> ExecuteCommandAsync(
        DockerContainer container,
        string command,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
    
    Task CopyFileToContainerAsync(
        DockerContainer container,
        string sourcePath,
        string containerPath,
        CancellationToken cancellationToken = default);
    
    Task CopyDirectoryToContainerAsync(
        DockerContainer container,
        DirectoryInfo sourceDir,
        string containerPath,
        CancellationToken cancellationToken = default);
    
    Task CopyFileFromContainerAsync(
        DockerContainer container,
        string containerPath,
        string destinationPath,
        CancellationToken cancellationToken = default);
    
    Task CleanupContainerAsync(
        DockerContainer container,
        bool force = false,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsDockerAvailableAsync(CancellationToken cancellationToken = default);
}
```

## Implementation

```csharp
public class DockerManager
{
    // Constructor
    public DockerManager(ILogger logger, DockerConfiguration config);
    
    // Initialize Docker environment
    public async Task InitializeAsync(CancellationToken cancellationToken = default);
    
    // Create container for a problem
    public async Task<DockerContainer> CreateContainerForProblemAsync(
        string problemId, 
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    // Execute command in container
    public async Task<CommandResult> ExecuteCommandAsync(
        DockerContainer container,
        string command,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
    
    // Copy file to container
    public async Task CopyFileToContainerAsync(
        DockerContainer container,
        string sourcePath,
        string containerPath,
        CancellationToken cancellationToken = default);
    
    // Copy directory to container
    public async Task CopyDirectoryToContainerAsync(
        DockerContainer container,
        DirectoryInfo sourceDir,
        string containerPath,
        CancellationToken cancellationToken = default);
    
    // Copy file from container
    public async Task CopyFileFromContainerAsync(
        DockerContainer container,
        string containerPath,
        string destinationPath,
        CancellationToken cancellationToken = default);
    
    // Stop and remove container
    public async Task CleanupContainerAsync(
        DockerContainer container,
        bool force = false,
        CancellationToken cancellationToken = default);
    
    // Check if Docker is available
    public Task<bool> IsDockerAvailableAsync(CancellationToken cancellationToken = default);
}
```

## Implementation Overview

The DockerManager class will:

1. **Manage Docker container lifecycle**:
   - Pull required SWE-bench Docker images if not already available
   - Create containers with appropriate settings for each problem
   - Start, stop, and remove containers as needed
   - Implement a container pool for efficiency (if configured)

2. **Set up container environments**:
   - Mount necessary volumes (workspace directory, agent directory)
   - Set required environment variables
   - Configure networking and security settings

3. **Execute commands inside containers**:
   - Provide a simple interface to run commands
   - Handle command timeouts and cancellation
   - Capture command output (stdout and stderr)
   - Support for interactive and non-interactive commands

4. **File operations**:
   - Copy agent executable and support files to container
   - Copy problem statement and other inputs to container
   - Copy solution outputs and logs from container

5. **Resource management**:
   - Track active containers to ensure cleanup
   - Monitor container resource usage (CPU, memory)
   - Implement resource limits based on configuration
   - Handle parallel container execution

## Docker Interaction Methods

The DockerManager will interact with Docker through:
- Docker CLI commands via Process.Start
- Docker REST API via HttpClient
- Docker .NET SDK (if available)

The choice will depend on the requirements and available libraries.

## Error Handling

The DockerManager will:
- Detect common Docker errors (missing images, insufficient permissions)
- Implement proper error propagation with descriptive messages
- Handle container timeouts and resource exhaustion
- Ensure cleanup even when errors occur
- Support automatic retries for transient failures

## Container Environment Setup

For each SWE-bench problem, the DockerManager will:
- Use the appropriate base image
- Mount the workspace directory
- Set up a shared volume for file exchange
- Configure environment variables required by the problem
- Set appropriate resource limits

## Security Considerations

The DockerManager will implement:
- Secure container configuration
- Proper handling of mounted volumes
- Limited container capabilities
- Resource quotas to prevent DoS
- Isolation between different problem containers