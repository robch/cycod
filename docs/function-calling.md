# Function Calling

One of the most powerful features of CycoD is the ability for the AI assistant to call functions that can interact with your system. This document explains how function calling works in CycoD and the available functions.

## Available Functions

CycoD provides three main categories of functions that the AI assistant can use:

### Shell Command Functions

These functions allow the AI to execute commands in different shell environments:

| Function | Description |
|----------|-------------|
| `RunBashCommandAsync` | Run commands in a bash shell with persistent session |
| `RunCmdCommandAsync` | Run commands in a cmd shell (Windows) with persistent session |
| `RunPowershellCommandAsync` | Run commands in a PowerShell shell with persistent session |

Key features of shell command functions:

- Each function maintains a persistent session, so state (like environment variables) is maintained across multiple commands
- Sessions are automatically reset if a command times out (default timeout: 60 seconds)
- You can specify a custom timeout in milliseconds for long-running commands
- Error codes are captured and returned with output
- UTF-8 encoding is automatically set where possible

Example usage with timeout:
```
RunBashCommandAsync("find / -name '*.log'", 120000)  // 2 minute timeout
```

### File Operation Functions

These functions allow the AI to interact with the file system:

| Function | Description |
|----------|-------------|
| `ListFiles` | List files and directories in a specified path (up to 2 levels deep) |
| `ViewFile` | View the contents of a file, optionally with line numbers and line range |
| `CreateFile` | Create a new file with specified content |
| `StrReplace` | Replace text in a file (only replaces unique occurrences) |
| `Insert` | Insert text at a specific line in a file |
| `UndoEdit` | Revert the last edit made to a file |

Key features of file operation functions:

- `ViewFile` supports viewing specific line ranges and adding line numbers
- `StrReplace` has built-in safety to only replace unique occurrences of text
- All file edits are tracked to support the `UndoEdit` function
- `ListFiles` handles relative and absolute paths and displays directories as such

### Time and Date Functions

Simple utility functions for getting current time information:

| Function | Description |
|----------|-------------|
| `GetCurrentDate` | Returns the current date in YYYY-MM-DD format |
| `GetCurrentTime` | Returns the current time in HH:MM:SS format |

## How Function Calling Works

When the AI suggests using a function, the following process occurs:

1. The AI analyzes your request and determines a function should be called
2. CycoD interprets the function call and executes it with the provided parameters
3. The function result is sent back to the AI as a tool message
4. The AI continues its response, taking into account the function's output

Function calls are displayed in the terminal with function name, arguments, and results, so you can see what operations are being performed on your behalf.

## Example Usage

When using CycoD interactively, you can ask the AI to perform tasks that use these functions:

### Shell Command Example

User: 
```
Can you show me the current directory structure and then create a Python script that lists all files in the current directory?
```

The AI might use `RunBashCommandAsync` to run `ls -la` and then use `CreateFile` to create a Python script.

### File Operation Example

User:
```
Can you help me fix the syntax error in my JavaScript file at ./src/app.js?
```

The AI might use `ViewFile` to examine the code, identify errors, and then use `StrReplace` to fix them.

## Security Considerations

Since these functions can execute commands and modify files on your system, consider the following security precautions:

1. Be careful about the prompts you provide to the AI
2. Review suggested changes before confirming them
3. Don't use CycoD with elevated privileges unless necessary
4. Be cautious when allowing the AI to modify important files
5. Use the `--debug` flag to see detailed information about all function calls
6. Remember that persistent shell sessions remain active between commands

## Function Parameters

Each function has specific parameters it accepts:

### Shell Command Parameters

- `command`: The command to execute (required)
- `timeoutMs`: Timeout in milliseconds (optional, default: 60000)

### File Operation Parameters

**ListFiles**:
- `path`: Absolute or relative path to directory (required)

**ViewFile**:
- `path`: Absolute or relative path to file (required)
- `startLine`: Optional starting line number (1-indexed) 
- `endLine`: Optional ending line number (-1 for all remaining lines)
- `lineNumbers`: Boolean to control showing line numbers (default: false)

**CreateFile**:
- `path`: Absolute or relative path for new file (required)
- `fileText`: Content to write to the file (required)

**StrReplace**:
- `path`: Absolute or relative path to file (required)
- `oldStr`: Text to find and replace (required, must be unique)
- `newStr`: Replacement text (required)

**Insert**:
- `path`: Absolute or relative path to file (required)
- `insertLine`: Line number after which to insert (required, 0 = start of file)
- `newStr`: Text to insert (required)

**UndoEdit**:
- `path`: Absolute or relative path to file (required)