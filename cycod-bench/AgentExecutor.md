# AgentExecutor.cs

This class is responsible for executing the standalone cycod agent inside Docker containers to solve SWE-bench problems.

## Responsibilities

- Copy the agent executable and any dependencies to containers
- Execute the agent with appropriate parameters
- Collect agent output (logs, diffs, artifacts)
- Monitor agent execution and handle timeouts
- Parse and extract diffs from agent output
- Manage agent-specific environment variables
- Handle agent execution errors

## Public Interface

```csharp
public interface IAgentExecutor
{
    Task<AgentExecutionResult> ExecuteAgentAsync(
        DockerContainer container,
        SwebenchProblem problem,
        DirectoryInfo workspaceDir,
        int candidateIndex,
        CancellationToken cancellationToken = default);
    
    Task SetupAgentInContainerAsync(
        DockerContainer container,
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    string ExtractDiffFromOutput(
        string agentOutput,
        string logFilePath);
}
```

## Implementation

```csharp
public class AgentExecutor
{
    // Constructor
    public AgentExecutor(
        IDockerManager dockerManager,
        ILogger logger,
        AgentConfiguration config);
    
    // Main execution method
    public async Task<AgentExecutionResult> ExecuteAgentAsync(
        DockerContainer container,
        SwebenchProblem problem,
        DirectoryInfo workspaceDir,
        int candidateIndex,
        CancellationToken cancellationToken = default);
    
    // Setup agent in container
    public async Task SetupAgentInContainerAsync(
        DockerContainer container,
        DirectoryInfo workspaceDir,
        CancellationToken cancellationToken = default);
    
    // Extract diff from agent output
    public string ExtractDiffFromOutput(
        string agentOutput,
        string logFilePath);
}
```

## Implementation Overview

The AgentExecutor class will:

1. **Setup agent environment**:
   - Copy the cycod agent executable to the container
   - Set up any required agent dependencies
   - Prepare the problem statement file
   - Configure agent environment variables

2. **Execute the agent**:
   - Build the agent command with appropriate arguments
   - Execute the command inside the container via DockerManager
   - Monitor execution and handle timeouts
   - Capture stdout, stderr, and exit code

3. **Process agent results**:
   - Extract the diff from agent output
   - Collect performance metrics
   - Save agent logs for analysis
   - Create a structured result object

## Agent Command Construction

The AgentExecutor will construct the agent command as:

```
cycod --input "{problem_statement_file}" --folder {workspace_path}
```

Where:
- `{problem_statement_file}` is the path to a file containing the SWE-bench problem statement
- `{workspace_path}` is the path to the problem's codebase inside the container

## Error Handling

The AgentExecutor will:
- Handle agent crashes and execution failures
- Implement proper timeout handling for long-running agents
- Provide meaningful error messages when agent execution fails
- Capture agent stderr for debugging purposes
- Detect common agent-specific errors

## Logging and Debugging

The AgentExecutor will:
- Save detailed agent logs in a standardized format
- Include metadata about the execution environment
- Capture timing information for different phases
- Include resource usage metrics when available
- Format logs for easy analysis

## Agent Output Processing

The agent output processing will:
- Parse the agent's output to extract the generated diff
- Handle different output formats gracefully
- Validate the diff format
- Extract any additional metadata provided by the agent

## Performance Considerations

The AgentExecutor will:
- Monitor agent resource usage
- Implement configurable timeouts
- Use asynchronous execution for better resource utilization
- Minimize overhead in agent communication
- Support cancellation for long-running agent processes

## Configuration Options

The AgentExecutor will support configuration options for:
- Agent executable path
- Maximum execution time
- Memory limits
- Environment variables
- Debug flags
- Output verbosity