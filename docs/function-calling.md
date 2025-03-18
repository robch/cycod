# Function Calling

One of the most powerful features of ChatX is the ability for the AI assistant to call functions that can interact with your system. This document explains how function calling works in ChatX and the available functions.

## Available Functions

ChatX provides three main categories of functions that the AI assistant can use:

### Shell Command Functions

These functions allow the AI to execute commands in different shell environments:

| Function | Description |
|----------|-------------|
| `RunBashCommandAsync` | Run commands in a bash shell |
| `RunCmdCommandAsync` | Run commands in a cmd shell (Windows) |
| `RunPowershellCommandAsync` | Run commands in a PowerShell shell |

Each function maintains a persistent session, so state (like environment variables) is maintained across multiple commands.

### File Operation Functions

These functions allow the AI to interact with the file system:

| Function | Description |
|----------|-------------|
| `ListFiles` | List files and directories in a specified path |
| `ViewFile` | View the contents of a file, optionally with line numbers |
| `CreateFile` | Create a new file with specified content |
| `StrReplace` | Replace text in a file |
| `Insert` | Insert text at a specific line in a file |
| `UndoEdit` | Revert the last edit made to a file |

### Time and Date Functions

Simple utility functions for getting current time information:

| Function | Description |
|----------|-------------|
| `GetCurrentDate` | Returns the current date |
| `GetCurrentTime` | Returns the current time |

## Example Usage

When using ChatX interactively, you can ask the AI to perform tasks that use these functions:

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
3. Don't use ChatX with elevated privileges unless necessary
4. Be cautious when allowing the AI to modify important files