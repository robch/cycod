---
hide:
- toc
icon: material/console
---

--8<-- "snippets/ai-generated.md"

# Run Commands with CYCODMD

The `cycodmd run` command allows you to execute shell commands or scripts and convert their output to markdown. This powerful feature makes it easy to include the results of commands in your markdown documentation, perfect for tutorials, technical documentation, or sharing command output with others.

## Basic Syntax

``` bash title="Run a single command"
cycodmd run "COMMAND"
```

``` bash title="Run multiple commands"
cycodmd run "COMMAND1" "COMMAND2" "COMMAND3"
```

## Specifying the Shell

CYCODMD can run commands using different shells:

``` bash title="Using Command Prompt"
cycodmd run --cmd "COMMAND"
```

``` bash title="Using PowerShell"
cycodmd run --powershell "COMMAND"
```

``` bash title="Using Bash"
cycodmd run --bash "COMMAND"
```

By default, CYCODMD will use:
- `cmd` on Windows
- `bash` on Linux/macOS

## Adding AI Processing

You can apply AI processing to command output using the `--instructions` parameter:

``` bash title="Process command output with AI"
cycodmd run "ls -la" --instructions "Format the directory listing as a table with sizes in human-readable format"
```

## Examples

### Simple Command Execution

Get a basic directory listing with markdown formatting:

``` bash title="Directory listing (Windows)"
cycodmd run "dir"
```

``` bash title="Directory listing (Linux/macOS)"
cycodmd run "ls -la"
```

### Process Information

List running processes and format as a markdown table:

``` bash title="Format process list as table"
cycodmd run --powershell "Get-Process" --instructions "Create a table of the top 10 processes by CPU usage"
```

### System Information

Generate a system information report:

``` bash title="Create system info report"
cycodmd run --powershell "systeminfo" --instructions "Summarize the key system information in bullet points"
```

### Running Multiple Commands

Execute multiple commands and combine their output:

``` bash title="Combine multiple commands"
cycodmd run "echo Current directory:" "pwd" "echo Files:" "ls -la" --instructions "Format as a readable report"
```

### AI-Enhanced Logs

Process log files with AI-enhanced formatting:

``` bash title="Process log files with AI"
cycodmd run "cat /var/log/syslog | tail -n 50" --instructions "Identify errors and warnings, highlight them, and explain what might be causing them"
```

### Using Script Files

Execute a script file and process its output:

``` bash title="Process script output"
cycodmd run "python analysis_script.py" --instructions "Format the results as a technical report with charts described in markdown"
```

### Saving Output

Save the processed output to a markdown file:

``` bash title="Save output to file"
cycodmd run "npm list --depth=0" --instructions "Create a dependency report" --save-output "dependencies-report.md"
```

### Creating Reusable Commands

Save commonly used commands as aliases:

``` bash title="Create command alias"
cycodmd run --powershell "Get-Service" --instructions "List running services in a table" --save-alias services-report
```

``` bash title="Use command alias"
cycodmd --services-report
```

## Advanced Usage

### Multi-Step AI Processing

Apply multiple processing steps to your command output:

``` bash title="Multi-step processing"
cycodmd run "docker ps" --instructions @step1-instructions.md @step2-instructions.md
```

Where `step1-instructions.md` might contain "Extract container names and images" and `step2-instructions.md` could have "Format as a table with status information highlighted".

### Shell Environment Variables

Use environment variables in your commands:

``` bash title="Using environment variables"
cycodmd run "echo Current user: $USER" "echo Home directory: $HOME" --bash
```

### Timeout Control

Set a timeout for long-running commands:

``` bash title="Set command timeout"
cycodmd run --bash "find / -name '*.log' -type f" --timeout 30000
```

## Common Use Cases

- Generating documentation from command-line tools
- Creating reports from system information
- Processing and formatting log files
- Documenting build processes and their outputs
- Creating tutorials with command examples and their results
- Generating reports from data analysis scripts

## Best Practices

1. **Be Careful with Sensitive Commands**: Avoid commands that could expose sensitive information
2. **Consider Timeouts**: Set appropriate timeouts for long-running commands
3. **Test First**: For complex commands, test them in a terminal before using with CYCODMD
4. **Use Clear Instructions**: When using AI processing, be specific about how you want the output formatted
5. **Save Common Commands**: Use aliases to save frequently used command patterns

## Related Commands

- `cycodmd run --help` - Display help for the run command
- `cycodmd web search` - Search the web and convert results to markdown
- `cycodmd web get` - Retrieve web content and convert to markdown

## See Also

- [CYCODMD File Conversion](file-conversion.md)
- [CYCODMD Web Search](web-features.md)
- [CYCODMD AI Processing](ai-processing.md)