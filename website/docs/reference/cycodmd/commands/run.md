---
hide:
  - toc
icon: material/console
---

--8<-- "snippets/ai-generated.md"

# cycodmd run

??? question "Why use the run command?"

    The `run` command executes shell commands and converts their output to markdown. This is useful for:
    
    - Including command output in documentation
    - Processing system information with AI
    - Creating tutorials with real command examples
    - Analyzing logs and command output intelligently

## Basic Usage

``` bash title="Run a simple command"
cycodmd run "echo Hello, World!"
```

``` bash title="Specify which shell to use"
cycodmd run --cmd "dir"                        # Windows Command Prompt
cycodmd run --powershell "Get-Process"         # PowerShell
cycodmd run --bash "ls -la | grep .md"         # Bash shell
```

``` bash title="Run multiple commands"
cycodmd run "echo Step 1" "echo Step 2" "echo Step 3"
```

## AI Processing

``` bash title="Format command output with AI"
cycodmd run "systeminfo" --instructions "Create a summary table of the system information"
```

``` bash title="Process script output"
cycodmd run "python analyze_data.py" --instructions "Create a report with key findings"
```

``` bash title="Multi-step AI instructions"
cycodmd run "netstat -an" --instructions @step1-extract.txt @step2-format.txt
```

## Saving Output

``` bash title="Save to a file"
cycodmd run "dir /s *.cs" --instructions "Create a file listing summary" --save-output "reports/file-listing.md"
```

``` bash title="Create an alias for future use"
cycodmd run --powershell "Get-Process" --instructions "Format as table" --save-alias processes

# Then use it later
cycodmd --processes
```

## Options

### Shell Options

| Option | Description |
|--------|-------------|
| `--cmd [COMMAND]` | Run command using CMD shell (default on Windows) |
| `--bash [COMMAND]` | Run command using Bash shell (default on Linux/macOS) |
| `--powershell [COMMAND]` | Run command using PowerShell shell |
| `--script [COMMAND]` | Alias for the default shell based on your OS |

### AI Processing Options

| Option | Description |
|--------|-------------|
| `--instructions "TEXT"` | Apply AI processing to the command output |

### Output Options

| Option | Description |
|--------|-------------|
| `--save-output [FILE]` | Save command output to specified file |
| `--save-alias ALIAS` | Save current options as an alias |

## Advanced Examples

``` bash title="Complex command pipeline"
cycodmd run --bash "find . -type f -name '*.js' | xargs wc -l | sort -nr" --instructions "Create a report of JavaScript file sizes"
```

``` bash title="System documentation"
cycodmd run "systeminfo" "ipconfig /all" "netstat -an" --instructions "Create a comprehensive system report"
```

``` bash title="Environment analysis"
cycodmd run --powershell "Get-ChildItem env:" --instructions "Create a table of environment variables by purpose"
```

## Best Practices

- **Quote Complex Commands**: Always quote commands that contain special characters
- **Use Appropriate Shell**: Select the shell that best supports your command syntax
- **Test Commands First**: Run complex commands directly in a shell before using with cycodmd
- **Be Specific with Instructions**: Provide clear AI instructions for better results

## Related Commands

- [cycodmd](../index.md) - Convert files to markdown
- [cycodmd web search](web-search.md) - Search the web and create markdown
- [cycodmd web get](web-get.md) - Convert web page content to markdown