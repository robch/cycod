# EvaluationToolsManager.cs

This class is responsible for managing the SWE-bench evaluation tools, including setup, configuration, and execution.

## Responsibilities

- Install and configure SWE-bench evaluation tools
- Manage the SWE-bench evaluation environment
- Execute evaluation commands against solution diffs
- Parse and interpret evaluation results
- Handle errors and retries for evaluation runs
- Cache evaluation tools to avoid repeated installations

## Public Interface

```csharp
public interface IEvaluationToolsManager
{
    Task<bool> InitializeAsync(
        bool forceReinstall = false,
        CancellationToken cancellationToken = default);
    
    bool AreEvaluationToolsInstalled();
    
    EvaluationToolsStatus GetStatus();
    
    Task<EvaluationResult> RunEvaluationAsync(
        string problemId,
        DirectoryInfo workspaceDir,
        string diffFilePath,
        int maxRetries = 5,
        CancellationToken cancellationToken = default);
    
    Task<string> GeneratePredictionsFileAsync(
        string problemId,
        string diffContent,
        DirectoryInfo outputDir,
        CancellationToken cancellationToken = default);
        
    Task CleanupAsync(CancellationToken cancellationToken = default);
}
```

## Implementation

```csharp
public class EvaluationToolsManager
{
    // Constructor
    public EvaluationToolsManager(
        ILogger logger,
        EvaluationToolsConfiguration config);
    
    // Initialize the evaluation tools
    public async Task<bool> InitializeAsync(
        bool forceReinstall = false,
        CancellationToken cancellationToken = default);
    
    // Check if evaluation tools are installed
    public bool AreEvaluationToolsInstalled();
    
    // Get evaluation tools status
    public EvaluationToolsStatus GetStatus();
    
    // Run evaluation on a solution
    public async Task<EvaluationResult> RunEvaluationAsync(
        string problemId,
        DirectoryInfo workspaceDir,
        string diffFilePath,
        int maxRetries = 5,
        CancellationToken cancellationToken = default);
    
    // Generate predictions file from diff
    public async Task<string> GeneratePredictionsFileAsync(
        string problemId,
        string diffContent,
        DirectoryInfo outputDir,
        CancellationToken cancellationToken = default);
        
    // Cleanup evaluation tools resources
    public async Task CleanupAsync(CancellationToken cancellationToken = default);
}
```

## Implementation Overview

The EvaluationToolsManager class will:

1. **Install SWE-bench evaluation tools**:
   - Clone the SWE-bench repository
   - Apply necessary patches
   - Create a dedicated environment for the tools
   - Install dependencies

2. **Manage the evaluation environment**:
   - Track installation status and version
   - Support reinstallation if needed
   - Handle environment variables and paths
   - Support different operating systems

3. **Execute evaluation commands**:
   - Generate predictions file from diff
   - Run SWE-bench evaluation harness
   - Implement retry logic for transient failures
   - Handle timeouts and resource cleanup

4. **Process evaluation results**:
   - Parse evaluation output files
   - Extract success/failure status
   - Collect detailed test results
   - Generate structured evaluation reports

## SWE-bench Tools Installation

The EvaluationToolsManager will implement a process similar to the `setup.sh` script:

1. Install prerequisite tools like Git
2. Clone the SWE-bench repository from GitHub
3. Check out a specific version/commit
4. Apply any necessary patches
5. Set up a dedicated environment for the evaluation tools
6. Install dependencies
7. Verify the installation

## Evaluation Execution Process

The evaluation process will:

1. Generate a predictions file in the required format:
   ```json
   [{"instance_id": "problem-id", "prediction": "diff content"}]
   ```

2. Execute the SWE-bench evaluation harness:
   ```
   dotnet run -- --command eval --predictions-path predictions.json --problem-id problem-id --workspace workspace_dir
   ```

3. Process the evaluation results:
   ```
   workspace_dir/augment-agent.{problem-id}.json
   ```

## Error Handling and Retries

The EvaluationToolsManager will:
- Detect common evaluation failures
- Implement exponential backoff for retries
- Clean up Docker containers between retry attempts
- Log detailed diagnostics for failures
- Handle different error types appropriately

## Cross-Platform Support

The EvaluationToolsManager will:
- Support Windows, Linux, and macOS platforms
- Adapt installation and execution commands for each platform
- Handle path differences between platforms
- Use cross-platform abstractions for process execution

## Performance Considerations

The class will:
- Cache downloaded tools to avoid repeated installations
- Use efficient process communication
- Implement parallel evaluation when appropriate
- Monitor resource usage during evaluation

## Integration with Docker

The EvaluationToolsManager will:
- Understand how SWE-bench uses Docker containers
- Manage container creation and cleanup
- Handle container naming and conflicts
- Support container resource limits and configuration