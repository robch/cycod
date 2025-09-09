# Unified Shell and Process Management System

This document outlines a new unified system for managing shell commands and background processes with a consistent interface.

## RunShellCommand

**Description**: Executes a command in a shell with adaptive timeout handling. If the command completes within the expected timeout, returns the result directly. If it exceeds the timeout, automatically converts to a managed shell that can be interacted with later.

- Simplifies running one-off commands
- Automatically handles long-running commands without requiring special setup
- Provides stdout, stderr, and exit code information
- Controls environment through working directory and environment variables

```csharp
[Description("Executes a command in a shell with adaptive timeout handling. If the command completes within the expected timeout, returns the result directly. If it exceeds the timeout, automatically converts to a managed shell that can be interacted with later.")]
public async Task<string> RunShellCommand(
    [Description("The command to execute")] string command,
    [Description("Shell type to use: bash, cmd, powershell")] string shellType = "bash",
    [Description("Expected timeout in milliseconds")] int expectedTimeout = 60000,
    [Description("Working directory for the command")] string workingDir = null,
    [Description("Environment variables as JSON object")] string environmentVariables = null)
```

**Example**:

```
// Simple command that completes quickly
result = RunShellCommand("ls -la", shellType="bash", expectedTimeout=5000)
// Output: 
// total 56
// drwxr-xr-x 8 user group  4096 Aug 19 14:02 .
// drwxr-xr-x 4 user group  4096 Aug 19 13:50 ..
// -rw-r--r-- 1 user group   831 Aug 19 14:02 README.md
// ...

// Command that exceeds the timeout
result = RunShellCommand("npm install", expectedTimeout=5000)
// Output:
// {
//   "status": "stillRunning",
//   "shellName": "auto-bash-123",
//   "outputSoFar": "added 42 packages, and audited 123 packages in 3s\n...",
//   "errorSoFar": "",
//   "runningTime": 5000
// }

// Now can interact with the long-running shell using other functions
WaitForShellOrProcessOutput("auto-bash-123", "added [0-9]+ packages")
```

## CreateNamedShell

**Description**: Creates a named shell that persists across commands. Useful for workflows requiring multiple related commands that share state, environment variables, or working directory.

- Creates a persistent shell with a specific name for later reference
- Supports multiple shells of the same type
- Controls initial working directory and environment variables
- Returns a shell ID for use with other functions

```csharp
[Description("Creates a named shell that persists across commands. Useful for workflows requiring multiple related commands that share state, environment variables, or working directory.")]
public async Task<string> CreateNamedShell(
    [Description("Shell type: bash, cmd, powershell")] string shellType = "bash",
    [Description("Optional custom name for the shell")] string shellName = null,
    [Description("Working directory for the shell")] string workingDirectory = null,
    [Description("Environment variables as JSON object")] string environmentVariables = null)
```

**Example**:

```
// Create a named bash shell for a Node.js project
shellId = CreateNamedShell(
    shellType="bash", 
    shellName="node-project", 
    workingDirectory="/projects/my-app",
    environmentVariables="{\"NODE_ENV\": \"development\", \"DEBUG\": \"app:*\"}"
)
// Output: Shell created with ID: node-project

// Now use the shell for multiple commands
ExecuteInShell(shellId, "npm install")
ExecuteInShell(shellId, "npm run build")
ExecuteInShell(shellId, "npm test")
```

## StartNamedProcess

**Description**: Starts a background process that runs independently. Ideal for servers, watchers, or other long-running processes that need to operate in the background while you perform other tasks.

- Launches a process with customizable arguments
- Assigns a unique ID for later management
- Controls working directory and environment variables
- Captures stdout and stderr for later retrieval
- Non-blocking - returns immediately while process runs in background

```csharp
[Description("Starts a background process that runs independently. Ideal for servers, watchers, or other long-running processes that need to operate in the background while you perform other tasks.")]
public async Task<string> StartNamedProcess(
    [Description("The executable to run")] string executablePath,
    [Description("Arguments to pass to the process")] string processArguments = "",
    [Description("Optional custom name for the process")] string processName = null,
    [Description("Working directory for the process")] string workingDirectory = null,
    [Description("Environment variables as JSON object")] string environmentVariables = null)
```

**Example**:

```
// Start a development server in the background
serverName = StartNamedProcess(
    executablePath="node", 
    processArguments="server.js --port=3000", 
    processName="dev-server",
    workingDirectory="/projects/api",
    environmentVariables="{\"NODE_ENV\": \"development\"}"
)
// Output: Process started with ID: dev-server

// Wait for the server to be ready
WaitForShellOrProcessOutput(serverName, "Server listening on port 3000")

// Later, check server output
output = GetShellOrProcessOutput(serverName)

// When finished, stop the server
TerminateShellOrProcess(serverName)
```

## ExecuteInShell

**Description**: Executes a command in an existing named shell. Use this to run commands in shells created with CreateNamedShell or shells that were automatically created when a RunShellCommand exceeded its timeout.

- Runs commands in existing shell sessions
- Maintains state between commands (working directory, environment variables, etc.)
- Sets timeouts for command execution
- Returns command output and exit code

```csharp
[Description("Executes a command in an existing named shell. Use this to run commands in shells created with CreateNamedShell or shells that were automatically created when a RunShellCommand exceeded its timeout.")]
public async Task<string> ExecuteInShell(
    [Description("Shell name from CreateNamedShell or auto-created shell")] string shellName,
    [Description("Command to execute in the shell")] string command,
    [Description("Timeout in milliseconds")] int timeoutMs = 60000)
```

**Example**:

```
// Execute a command in an existing shell
result = ExecuteInShell("node-project", "git status")
// Output:
// On branch main
// Your branch is up to date with 'origin/main'.
// nothing to commit, working tree clean

// Execute with a custom timeout
result = ExecuteInShell("node-project", "npm run build", timeoutMs=180000)
```

## GetShellOrProcessOutput

**Description**: Gets output from a shell or process. Provides options to retrieve specific output types, clear buffers, and wait for new output or patterns.

- Retrieves stdout, stderr, or combined output
- Option to clear output buffers after retrieval
- Can wait for new output for a specified time
- Can wait for output matching a specific pattern
- Works with both shells and processes

```csharp
[Description("Gets output from a shell or process. Provides options to retrieve specific output types, clear buffers, and wait for new output or patterns.")]
public async Task<string> GetShellOrProcessOutput(
    [Description("Shell or process name")] string name,
    [Description("Output type: stdout, stderr, or all")] string outputType = "all",
    [Description("Whether to clear the output buffer after retrieval")] bool clearBuffer = false,
    [Description("Time to wait for new output in milliseconds (0 = don't wait)")] int waitTimeMs = 0,
    [Description("Pattern to wait for in the output (null = don't wait for pattern)")] string waitPattern = null)
```

**Example**:

```
// Get all output from a process without clearing buffer
output = GetShellOrProcessOutput("dev-server")

// Get only stderr and clear the buffer
errors = GetShellOrProcessOutput("dev-server", outputType="stderr", clearBuffer=true)

// Wait up to 5 seconds for new output
newOutput = GetShellOrProcessOutput("dev-server", waitTimeMs=5000)

// Wait for a specific pattern to appear
readyOutput = GetShellOrProcessOutput("dev-server", waitPattern="Ready to accept connections")
```

## SendInputToShellOrProcess

**Description**: Sends input to a shell or process. Useful for interactive applications that require user input during execution.

- Sends text to stdin of a shell or process
- Enables interaction with prompts and interactive programs
- Works with both shells and processes
- Automatically appends newline if not provided

```csharp
[Description("Sends input to a shell or process. Useful for interactive applications that require user input during execution.")]
public async Task<string> SendInputToShellOrProcess(
    [Description("Shell or process name")] string name,
    [Description("Text to send as input")] string inputText)
```

**Example**:

```
// Start an interactive Python session
pythonId = StartNamedProcess("python", "-i")

// Send Python commands
SendInputToShellOrProcess(pythonId, "import math")
SendInputToShellOrProcess(pythonId, "print(math.pi)")

// Get the output
output = GetShellOrProcessOutput(pythonId)
// Output: 3.141592653589793

// Interact with a shell-based utility that prompts for input
ExecuteInShell("bash-session", "read -p 'Enter your name: ' name && echo \"Hello, $name!\"")
SendInputToShellOrProcess("bash-session", "Alice")
result = GetShellOrProcessOutput("bash-session")
// Output: Enter your name: Alice
// Hello, Alice!
```

## WaitForShellOrProcessExit

**Description**: Waits for a shell or process to exit. Useful for ensuring that a process has completed before proceeding with subsequent operations.

- Blocks until the shell/process exits or timeout occurs
- Returns exit status information
- Can specify maximum wait time
- Works with both shells and processes

```csharp
[Description("Waits for a shell or process to exit. Useful for ensuring that a process has completed before proceeding with subsequent operations.")]
public async Task<string> WaitForShellOrProcessExit(
    [Description("Shell or process name")] string name,
    [Description("Timeout in milliseconds, -1 for indefinite")] int timeoutMs = -1)
```

**Example**:

```
// Start a build process
buildId = StartNamedProcess("npm", "run build", "project-build")

// Wait for it to complete with a 5-minute timeout
result = WaitForShellOrProcessExit(buildId, timeoutMs=300000)
// Output: Process project-build exited with code 0

// Wait indefinitely for a process to exit
result = WaitForShellOrProcessExit("long-task")
```

## WaitForShellOrProcessOutput

**Description**: Waits for output matching a pattern from a shell or process. Useful for detecting when a specific condition or state has been reached in a running application.

- Waits for output matching a regular expression pattern
- Can specify maximum wait time
- Returns the matched output
- Works with both shells and processes
- Non-destructive - doesn't clear output buffers

```csharp
[Description("Waits for output matching a pattern from a shell or process. Useful for detecting when a specific condition or state has been reached in a running application.")]
public async Task<string> WaitForShellOrProcessOutput(
    [Description("Shell or process name")] string name,
    [Description("Regular expression pattern to wait for")] string pattern,
    [Description("Timeout in milliseconds, -1 for indefinite")] int timeoutMs = -1)
```

**Example**:

```
// Start a web server
serverName = StartNamedProcess(executablePath="node", processArguments="server.js")

// Wait for the server to be ready
result = WaitForShellOrProcessOutput(serverName, "Server listening on port [0-9]+")
// Output: Pattern matched: "Server listening on port 3000"

// Wait for a build to reach a certain percentage
WaitForShellOrProcessOutput("webpack-build", "\\[([0-9]{1,3})%\\]", timeoutMs=60000)

// Wait for an error condition
errorResult = WaitForShellOrProcessOutput("test-run", "Error:|Exception:", timeoutMs=30000)
```

## TerminateShellOrProcess

**Description**: Terminates a shell or process. Use this to clean up resources when they're no longer needed or to stop processes that have become unresponsive.

- Terminates shells or processes by ID
- Option for graceful termination or force kill
- Returns termination status
- Frees system resources

```csharp
[Description("Terminates a shell or process. Use this to clean up resources when they're no longer needed or to stop processes that have become unresponsive.")]
public async Task<string> TerminateShellOrProcess(
    [Description("Shell or process name")] string name,
    [Description("Whether to force kill if graceful termination fails")] bool force = false)
```

**Example**:

```
// Gracefully terminate a server
result = TerminateShellOrProcess("dev-server")
// Output: Process dev-server was successfully terminated

// Force kill an unresponsive process
result = TerminateShellOrProcess("frozen-app", force=true)
// Output: Process frozen-app was forcefully terminated

// Terminate a shell session
TerminateShellOrProcess("bash-session")
```

## ListShellsAndProcesses

**Description**: Lists all running shells and processes. Helps you keep track of what's running and allows you to manage multiple concurrent operations.

- Shows all active shells and processes
- Displays IDs, types, and status information
- Includes start times and running status
- Helps identify resources that need cleanup

```csharp
[Description("Lists all running shells and processes. Helps you keep track of what's running and allows you to manage multiple concurrent operations.")]
public string ListShellsAndProcesses()
```

**Example**:

```
status = ListShellsAndProcesses()
// Output:
// Running shells and processes: 3/4
// 
// ID: node-project
// Type: Shell (bash)
// Started: 8/19/2023 2:14:23 PM
// Status: Running
// ----------------------------------------
// ID: dev-server
// Type: Process (node)
// Started: 8/19/2023 2:15:45 PM
// Status: Running
// ----------------------------------------
// ID: python-repl
// Type: Process (python)
// Started: 8/19/2023 2:18:12 PM
// Status: Running
// ----------------------------------------
// ID: completed-task
// Type: Process (npm)
// Started: 8/19/2023 2:10:05 PM
// Status: Terminated (exit code: 0)
// ----------------------------------------
```

## Usage Patterns

### Basic Command Execution

For simple commands that should complete quickly:

```
result = RunShellCommand("git status")
```

### Auto-Adapting to Long-Running Commands

For commands that might take longer than expected:

```
result = RunShellCommand("npm install", expectedTimeout=10000)

if (IsJsonObject(result) && HasProperty(result, "status") && GetProperty(result, "status") == "stillRunning")
{
    // Extract the shell name
    shellName = GetProperty(result, "shellName")
    
    // Wait for completion
    WaitForShellOrProcessOutput(shellName, "added [0-9]+ packages")
    
    // Get the final output
    finalOutput = GetShellOrProcessOutput(shellName)
}
```

### Multi-Step Workflows

For sequences of related commands:

```
// Create a persistent shell
shellName = CreateNamedShell("bash", "deployment", workingDirectory="/projects/app")

// Run a series of commands in the same shell
ExecuteInShell(shellName, "git pull")
ExecuteInShell(shellName, "npm ci")
buildResult = ExecuteInShell(shellName, "npm run build")

// Check if build succeeded
if (buildResult.Contains("Build successful"))
{
    ExecuteInShell(shellName, "npm run deploy")
}

// Clean up
TerminateShellOrProcess(shellName)
```

### Background Services

For running services in the background:

```
// Start services
dbName = StartNamedProcess(executablePath="docker", processArguments="compose up database", processName="db-service")
apiName = StartNamedProcess(executablePath="node", processArguments="api.js", processName="api-service")
webName = StartNamedProcess(executablePath="npm", processArguments="run dev", processName="web-service", workingDirectory="/projects/frontend")

// Wait for all services to be ready
WaitForShellOrProcessOutput(dbName, "database system is ready to accept connections")
WaitForShellOrProcessOutput(apiName, "API server started on port 3000")
WaitForShellOrProcessOutput(webName, "Compiled successfully")

// Run tests against the running services
testResult = RunShellCommand("npm run integration-tests")

// Shut down services
TerminateShellOrProcess(dbName)
TerminateShellOrProcess(apiName)
TerminateShellOrProcess(webName)
```

### Interactive Applications

For applications requiring user input:

```
// Start an interactive process
replName = StartNamedProcess(executablePath="python", processArguments="-i")

// Send commands and get responses
SendInputToShellOrProcess(replName, "import random")
SendInputToShellOrProcess(replName, "random.randint(1, 100)")
result = GetShellOrProcessOutput(replName)

// Exit the REPL
SendInputToShellOrProcess(replName, "exit()")
TerminateShellOrProcess(replName)
```