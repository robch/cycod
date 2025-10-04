# Unified Shell and Process Management System: Design Decisions Addendum

This document serves as an addendum to the main plan, addressing critical design decisions that should be made before implementation begins.

## 1. Return Type Structure

**Decision needed**: Design the result objects for each function type with appropriate properties before implementation begins.

**Recommendation**: Implement strongly-typed result objects instead of string returns for all functions.

```csharp
// Base result type for common properties
public abstract class ExecutionResultBase 
{
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

// For RunShellCommand and ExecuteInShell
public class ShellCommandResult : ExecutionResultBase
{
    public string Stdout { get; set; }
    public string Stderr { get; set; }
    public int ExitCode { get; set; }
    public bool TimedOut { get; set; }
    public string ShellName { get; set; }  // For auto-promoted shells
    public bool WasPromoted { get; set; }  // Indicates if command was auto-promoted
}

// For GetShellOrProcessOutput
public class OutputResult : ExecutionResultBase
{
    public string Stdout { get; set; }
    public string Stderr { get; set; }
    public string Combined { get; set; }
    public bool WasCleared { get; set; }
    public bool PatternMatched { get; set; }
    public string MatchedPattern { get; set; }
}

// For CreateNamedShell and StartNamedProcess
public class ResourceCreationResult : ExecutionResultBase
{
    public string ResourceName { get; set; }
    public string ResourceType { get; set; }  // "Shell" or "Process"
}

// For TerminateShellOrProcess
public class TerminationResult : ExecutionResultBase
{
    public bool WasRunning { get; set; }
    public bool WasForced { get; set; }
    public int? ExitCode { get; set; }
}
```

**Rationale**: 
1. Strongly-typed results make the API self-documenting and provide clear expectations
2. Return objects allow for rich information without parsing strings
3. The inheritance hierarchy provides consistency while allowing specialized properties
4. This approach enables future extensions without breaking changes
5. IDEs can provide intellisense for available properties
6. Unit testing becomes more precise with property-specific assertions

For CLI use cases, these objects can be automatically serialized to JSON with formatting for display to users.

## 2. Auto-Promoted Shell Lifecycle

**Decision needed**: Define the lifecycle policy for auto-promoted shells, including naming conventions and cleanup rules.

**Recommendation**:

1. **Naming convention**: Auto-promoted shells should follow a consistent naming pattern: `auto-{shellType}-{timestamp}-{shortId}` (e.g., `auto-bash-20230912T153042-7a3b2c`)

2. **Metadata**: Auto-promoted shells should include metadata about their origin:
   - Original command that created them
   - Timestamp of creation
   - Expected timeout that was exceeded

3. **Lifecycle policy**:
   - Auto-promoted shells should have a default inactivity timeout (30 minutes)
   - After the inactivity period with no interactions, they should be automatically terminated
   - The inactivity timeout should be configurable system-wide and per-shell
   - ListShellsAndProcesses should include creation time and inactivity timeout information

4. **User control**:
   - Users should be able to "adopt" an auto-promoted shell by renaming it
   - Adopted shells would no longer be subject to auto-cleanup
   - Example: `RenameShell("auto-bash-20230912T153042-7a3b2c", "my-build-shell")`

**Rationale**:
1. Predictable naming helps users identify auto-promoted shells
2. Including timestamp and short ID ensures uniqueness
3. Automatic cleanup prevents resource leaks from forgotten shells
4. The ability to "adopt" shells gives users control over lifecycle when needed
5. Metadata helps diagnose why a command was promoted to a shell

## 3. Concurrency and Safety Model

**Decision needed**: Define the concurrency model for shell access, including whether to implement locking, queuing, or rejection of concurrent commands.

**Recommendation**:

1. **One active command per shell**: Only one command should be allowed to execute in a shell at a time

2. **Locking mechanism**:
   - Implement a per-shell lock for command execution
   - Attempts to execute when a command is running should fail fast with a "ShellBusyException"
   - Include the currently running command details in the exception

3. **Status checking**:
   - Add an `IsShellBusy(shellName)` function to check status before attempting execution
   - Return busy status and current command information

4. **Timeout option**:
   - Add an optional wait parameter to ExecuteInShell:
   ```csharp
   ExecuteInShell(string shellName, string command, int timeoutMs = 60000, int? waitForShellMs = null)
   ```
   - If waitForShellMs is provided, the call will wait up to that time for the shell to become available

5. **Concurrent process output reading**:
   - Allow multiple concurrent readers of output (GetShellOrProcessOutput)
   - Use appropriate synchronization for output buffer access

**Rationale**:
1. Allowing only one command per shell matches user mental model and prevents confusing output interleaving
2. Fast failure by default prevents deadlocks and makes concurrency issues immediately visible
3. The status checking function allows implementing retry or queuing at the application level if desired
4. The wait parameter provides flexibility for workflows that need sequential execution
5. Allowing concurrent output reading enables monitoring while commands are running

## 4. Output Buffer Management

**Decision needed**: Design a buffer management strategy that handles large outputs without memory issues.

**Recommendation**:

1. **Bounded in-memory buffers**:
   - Set a default maximum buffer size (e.g., 10MB for stdout, 10MB for stderr)
   - Make buffer size configurable per shell/process
   - When buffer size is exceeded, implement a circular buffer that discards oldest content

2. **File-backed option for large outputs**:
   - Add an option to use file-backed storage for shells/processes expected to produce large output
   ```csharp
   CreateNamedShell(..., OutputStorageMode storageMode = OutputStorageMode.Memory)
   StartNamedProcess(..., OutputStorageMode storageMode = OutputStorageMode.Memory)
   ```
   - Enum values: `Memory`, `File`, `Hybrid`

3. **Incremental output retrieval**:
   - Add options to retrieve output incrementally:
   ```csharp
   GetShellOrProcessOutput(..., long offset = 0, int? maxBytes = null)
   ```
   - Return current offset with output for subsequent calls

4. **Output truncation notification**:
   - Include metadata in output results indicating if content was truncated
   - Provide the total bytes processed and bytes retained

5. **Streaming option for real-time monitoring**:
   - Consider adding a callback-based API for streaming output as it's produced:
   ```csharp
   SubscribeToOutput(string name, Action<string, OutputType> callback)
   ```

**Rationale**:
1. Bounded buffers prevent memory issues with high-volume output
2. File-backed storage enables handling very large outputs without memory pressure
3. Incremental retrieval allows efficient access to specific parts of output
4. Truncation notification helps users understand when data was discarded
5. The streaming option enables real-time monitoring without polling

## 5. Error Handling and Categorization

**Decision needed**: Establish a consistent error handling strategy with well-defined categories and reporting formats.

**Recommendation**:

1. **Error categorization**: Define a comprehensive set of error types:
   ```csharp
   public enum ExecutionErrorType
   {
       // Resource errors
       ResourceNotFound,        // Shell or process doesn't exist
       ResourceAlreadyExists,   // When creating with a name that's taken
       ResourceBusy,            // When resource is in use
       
       // Execution errors
       CommandFailed,           // Command execution failed (non-zero exit)
       CommandTimeout,          // Command exceeded timeout
       CommandCancelled,        // Command was cancelled by user
       
       // Input/output errors
       InputRejected,           // Process not accepting input
       OutputBufferFull,        // Output buffer limit reached
       OutputParsingError,      // Error in pattern matching
       
       // System errors
       InsufficientResources,   // System resources not available
       AccessDenied,            // Permission issue
       SystemError,             // Unexpected system error
       
       // Validation errors
       InvalidArgument,         // Invalid parameter
       InvalidOperation,        // Operation not valid in current state
   }
   ```

2. **Standardized error properties**:
   - All errors should include:
     - Error type (from enum)
     - Error code (for programmatic handling)
     - Human-readable message
     - Context information relevant to the error
     - Suggestion for resolution when applicable

3. **Exception hierarchy**:
   ```csharp
   public class ShellExecutionException : Exception
   {
       public ExecutionErrorType ErrorType { get; }
       public string ErrorCode { get; }
       public string Context { get; }
       public string Suggestion { get; }
   }
   
   // Derived exceptions for specific categories
   public class ResourceNotFoundException : ShellExecutionException { }
   public class ResourceBusyException : ShellExecutionException { }
   // etc.
   ```

4. **Error reporting in results**:
   - All result objects should include error information when Success is false
   - The ErrorMessage should be human-readable
   - Include an ErrorType property mapping to the enum

**Rationale**:
1. Categorized errors enable programmatic handling of different error types
2. Standardized properties make errors consistent and predictable
3. Context and suggestions help users resolve issues
4. The exception hierarchy allows catching specific error types
5. Error reporting in results enables error handling without try/catch blocks

## 6. Process Resource Management

**Decision needed**: Define resource management policies, including whether to implement resource limits and monitoring.

**Recommendation**:

1. **Optional resource limits**:
   ```csharp
   public class ResourceLimits
   {
       public long? MaxMemoryBytes { get; set; }
       public int? MaxCpuPercent { get; set; }
       public TimeSpan? MaxExecutionTime { get; set; }
       public long? MaxOutputBytes { get; set; }
   }
   
   StartNamedProcess(..., ResourceLimits resourceLimits = null)
   ```

2. **Resource usage monitoring**:
   - Add a function to retrieve current resource usage:
   ```csharp
   GetResourceUsage(string name)
   ```
   - Return memory, CPU, execution time, and output size

3. **Limit enforcement**:
   - Monitor resource usage in the background
   - When limits are exceeded:
     - Generate a warning event first
     - After a grace period, terminate the process if still exceeding limits
     - Include detailed information about the limit violation in the termination result

4. **System-wide limits**:
   - Set a maximum number of concurrent shells and processes
   - Implement a simple scheduling mechanism that queues requests when system limits are reached
   - Add configuration options for these limits

5. **Resource usage events**:
   - Provide a way to subscribe to resource usage events:
   ```csharp
   SubscribeToResourceEvents(string name, Action<ResourceEvent> callback)
   ```
   - Events include: limit warnings, limit violations, high resource usage

**Rationale**:
1. Optional resource limits give users control over resource usage without enforcing them by default
2. Resource monitoring helps identify problematic processes
3. Grace period before termination allows processes to recover or complete critical operations
4. System-wide limits prevent resource exhaustion
5. Events enable custom handling of resource issues

## 7. Cross-Platform Behavior Specification

**Decision needed**: Document expected behavior across platforms and establish platform-specific defaults or limitations.

**Recommendation**:

1. **Default shell by platform**:
   - Windows: PowerShell
   - macOS: Bash
   - Linux: Bash

2. **Shell availability checks**:
   - Implement startup checks to verify configured shells are available
   - Provide clear error messages when shells are not found
   - Add a function to list available shells on the current system

3. **Path handling normalization**:
   - Automatically normalize paths in working directory parameters
   - Convert forward/backslashes appropriate to the platform
   - Resolve relative paths to absolute

4. **Environment variable handling**:
   - Normalize environment variable names (case-sensitive on Unix, case-insensitive on Windows)
   - Provide helper functions for common environment operations
   - Handle PATH merging appropriately for each platform

5. **Signal handling differences**:
   - Document platform-specific differences in process termination behavior
   - Implement appropriate termination strategies for each platform
   - Add platform-specific grace periods for termination

6. **Feature capability matrix**:
   - Create a clear document specifying which features work on which platforms
   - Note any platform-specific limitations or behaviors
   - Include workarounds for platform-specific issues

**Rationale**:
1. Default shells should match platform conventions for predictability
2. Availability checks prevent confusing runtime errors
3. Path normalization hides platform differences from users
4. Environment handling differences are a common source of cross-platform issues
5. Termination behavior varies significantly across platforms
6. A capability matrix sets clear expectations for cross-platform use

## 8. Integration with Existing Helpers

**Decision needed**: Identify relevant existing helper classes and determine how they'll integrate with the new system.

**Recommendation**:

1. **FileHelpers integration**:
   - Use FileHelpers for all file operations (reading/writing output files, checking paths)
   - Leverage path manipulation functions for working directory normalization
   - Use file existence and permission checks before operations

2. **PathHelpers integration**:
   - Use PathHelpers for all path normalization and resolution
   - Leverage platform-specific path handling
   - Use for converting between relative and absolute paths

3. **EnvironmentHelpers integration**:
   - Use EnvironmentHelpers for managing environment variables
   - Leverage platform-specific environment handling
   - Use for detecting system capabilities

4. **ProcessHelpers integration**:
   - Build on existing process management capabilities
   - Extend rather than replace where possible
   - Maintain consistent behavior with existing functions

5. **Color and display integration**:
   - Use existing color and formatting helpers for output display
   - Maintain consistent styling with the rest of the application
   - Leverage existing progress indicators where applicable

6. **Configuration system integration**:
   - Store shell and process defaults in the configuration system
   - Use the existing scopes (local, user, global)
   - Follow the established configuration patterns

7. **Common interfaces and patterns**:
   - Identify common interfaces and patterns in the codebase
   - Ensure new components follow these patterns
   - Use existing naming conventions and coding styles

**Rationale**:
1. Using existing helpers ensures consistency with the rest of the application
2. Building on established code reduces duplication and bugs
3. Following existing patterns makes the new system feel native to the codebase
4. Leveraging existing configuration system makes settings discoverable and manageable
5. Consistent interfaces make the system easier to learn for developers familiar with the codebase
6. Integration with existing display helpers provides a consistent user experience
7. Extending rather than replacing maximizes code reuse and minimizes risk