# cycodmd Run Command - Layer 2: Container Filter

## Purpose

**Layer 2 (Container Filter) does NOT apply to the Run command.**

The Run command executes shell scripts/commands directly. There are no "containers" (files, URLs, etc.) to filter. The script content itself is the execution target, not a container to be filtered.

## Command

**Command**: `cycodmd run [script...]`  
**Command Class**: `RunCommand`

## Layer 2 Status: N/A (Not Applicable)

### Why Layer 2 Doesn't Apply

**Container Filter** is designed to:
- Filter which files to include/exclude (FindFilesCommand)
- Filter which URLs to fetch (WebSearchCommand, WebGetCommand)

**Run Command** is different:
- No file discovery phase
- No URL fetching phase
- Script content is directly executed, not searched or filtered

### What Happens Instead

The Run command flow is much simpler:

```
Layer 1: Script Selection
    ↓
Script content collected from:
    - Positional arguments
    - --script, --bash, --cmd, --powershell options
    ↓
Layer 9: Action (Execute Script)
    ↓
Output returned
```

**Layers 2-8** are either N/A or minimal for Run command.

---

## No Options for Layer 2

The Run command has **no options** that implement Layer 2 (Container Filter) functionality.

### Available Run Options (All Other Layers)

**Layer 1 (Target Selection)**:
- Positional args: Script content
- `--script`: Script content
- `--bash`: Bash script
- `--cmd`: CMD script
- `--powershell`: PowerShell script

**Layer 7 (Output Persistence)**:
- `--save-output`: Save script output (shared option)
- `--save-chat-history`: Save AI interaction (shared option)

**Layer 8 (AI Processing)**:
- `--instructions`: AI processing of output (shared option)
- `--built-in-functions`: Enable AI functions (shared option)

**Layer 9 (Actions)**:
- Script execution itself

---

## Conceptual Comparison

| Command | Layer 2 Purpose | Layer 2 Options |
|---------|----------------|-----------------|
| **FindFilesCommand** | Filter which files to process | `--file-contains`, `--file-not-contains`, `--contains` |
| **WebSearchCommand** | Filter which URLs to fetch | `--exclude` |
| **WebGetCommand** | Filter which URLs to fetch | `--exclude` |
| **RunCommand** | N/A - No containers to filter | (none) |

---

## What if You Want to Filter Script Output?

If you want to filter the OUTPUT of a script (not the script itself), you can:

### Option 1: Pipe to File Search

```bash
# Run script and save output
cycodmd run "ls -la" --save-output script-output.txt

# Filter the output
cycodmd script-output.txt --line-contains "\.cs$"
```

### Option 2: Use Shell Filtering

```bash
# Filter within the script itself
cycodmd run --bash "ls -la | grep '.cs$'"
```

### Option 3: AI Processing

```bash
# Run script and process output with AI
cycodmd run "git log --oneline" \
  --instructions "Summarize recent commits, ignore merge commits"
```

---

## Examples (Showing No Layer 2)

```bash
# Basic script execution (no filtering)
cycodmd run "echo Hello World"

# Bash script (no filtering)
cycodmd run --bash "ls -la"

# PowerShell script (no filtering)
cycodmd run --powershell "Get-ChildItem"

# Multi-line script (no filtering)
cycodmd run --bash "
  echo 'Starting...'
  ls -la
  echo 'Done'
"

# With AI processing of output (Layer 8, not Layer 2)
cycodmd run "git status" \
  --instructions "Summarize the repository state"
```

**Note**: None of these examples use Layer 2 options because Layer 2 doesn't exist for Run command.

---

## See Also

- [Layer 2 Proof Document](cycodmd-run-layer-2-proof.md) - Evidence that Layer 2 is N/A
- [Layer 1: Target Selection](cycodmd-run-layer-1.md) - Script specification
- [Layer 9: Actions](cycodmd-run-layer-9.md) - Script execution
- [FindFiles Layer 2](cycodmd-files-layer-2.md) - Example of actual Layer 2 filtering
