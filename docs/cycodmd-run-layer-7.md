# cycodmd Run - Layer 7: Output Persistence

**[← Back to Run Overview](cycodmd-filtering-pipeline-catalog-README.md#4-run-script)** | **[Proof →](cycodmd-run-layer-7-proof.md)**

## Purpose

Layer 7 (Output Persistence) for the Run command controls **where and how script execution output is saved** to files. The Run command has minimal Layer 7-specific options and primarily relies on shared options.

## Command-Line Options

### `--save-output [file]`

**Type**: Shared option (all commands)  
**Default**: `output.md`  
**Purpose**: Save the script's output (stdout/stderr) to a file

**Behavior**:
- Captures all output from script execution
- Saves to specified file in markdown format
- Includes both stdout and stderr
- Applied after script completes

**Examples**:
```bash
# Save script output to default output.md
cycodmd run "ls -la" --save-output

# Save to custom file
cycodmd run "ps aux | grep node" --save-output processes.md

# Organized output
cycodmd run --bash "df -h" --save-output "reports/disk-usage-$(date +%Y%m%d).md"
```

**Source**: `CycoDmdCommandLineOptions.cs` line 427-432 (shared option parsing)

---

### `--save-chat-history [file]`

**Type**: Shared option (all commands with AI processing)  
**Default**: `chat-history-{time}.jsonl`  
**Purpose**: Save AI interaction history if script output is processed with AI instructions

**Behavior**:
- Only meaningful when combined with `--instructions`
- Saves AI analysis of script output
- Useful for debugging AI interpretation of command output

**Examples**:
```bash
# Analyze script output with AI and save history
cycodmd run "docker ps" \
  --instructions "Identify problematic containers" \
  --save-chat-history docker-analysis.jsonl

# System diagnostics with AI
cycodmd run --bash "systemctl status" \
  --instructions "Check for failed services" \
  --save-output status-report.md \
  --save-chat-history ai-diagnostics.jsonl
```

**Source**: `CycoDmdCommandLineOptions.cs` line 434-440 (shared option parsing)

---

## Run Command-Specific Behavior

### No Per-Script Output

Unlike FindFiles (`--save-file-output`) or WebCommands (`--save-page-output`), Run command does NOT have per-item output because:
- Run executes a single script (or set of commands as one script)
- All output is from one execution context
- No multiple "items" to save separately

### Script as Input, Output as Result

- **Layer 1 (Target Selection)**: The script/command to run
- **Layer 7 (Output Persistence)**: Where to save the script's output

**Example Flow**:
```bash
cycodmd run "npm test" --save-output test-results.md
#           ^^^^^^^^^^                ^^^^^^^^^^^^^^
#           Input (what to run)       Output (where to save result)
```

---

## Option Interactions

### Combining with AI Processing

```bash
# Execute script, analyze output with AI, save both
cycodmd run "git log --oneline -20" \
  --instructions "Summarize recent changes and identify patterns" \
  --save-output git-analysis.md \
  --save-chat-history git-ai.jsonl
```

**Result**:
- `git-analysis.md`: AI-analyzed summary of git log
- `git-ai.jsonl`: AI interaction history

---

### Multiple Commands (Concatenated)

**Via Positional Args**:
```bash
cycodmd run "echo 'Part 1'" "echo 'Part 2'" --save-output combined.md
```

**Via Script Options**:
```bash
cycodmd run --bash "echo 'Part 1'; echo 'Part 2'" --save-output combined.md
```

Both concatenate commands into a single script; output is saved as one file.

---

## Common Patterns

### System Diagnostics
```bash
cycodmd run --bash "
  echo '=== Disk Usage ==='
  df -h
  echo ''
  echo '=== Memory Usage ==='
  free -h
  echo ''
  echo '=== Top Processes ==='
  ps aux --sort=-%mem | head -10
" --save-output system-report.md
```

### Test Suite Execution with AI Analysis
```bash
cycodmd run "npm test" \
  --save-output test-results.md \
  --instructions "Identify failing tests and suggest fixes" \
  --save-chat-history test-analysis.jsonl
```

### Batch Command Execution
```bash
cycodmd run \
  --bash "git status" \
  --bash "git log -5 --oneline" \
  --bash "git diff --stat" \
  --save-output git-status-full.md
```

---

## Data Flow

### Input to Layer 7
From **Layer 1-6**:
- Script execution output (stdout)
- Script error output (stderr)
- Exit code (success/failure)
- Formatted output (markdown)

From **Layer 8** (if AI used):
- AI analysis of script output
- Chat history

### Processing in Layer 7
1. **Collect script output** (stdout + stderr)
2. **Format as markdown** (if not already)
3. **Apply AI processing** (if `--instructions` used)
4. **Write output file** (`--save-output`)
5. **Write chat history** (`--save-chat-history`, if AI used)

### Output from Layer 7
- File containing script output
- Optional: AI chat history file
- Status messages (if not `--quiet`)

---

## Integration with Other Layers

### Layer 9 (Actions) Implicit

For Run command, **Layer 9 (Actions)** is implicit:
- **Action**: Execute the script
- **Layer 7**: Save the result

Unlike FindFiles (`--replace-with --execute`), Run command's primary action IS execution.

---

## Limitations

### No Per-Command Output Splitting

**Not Supported**:
```bash
# This does NOT create separate output files per command
cycodmd run "cmd1" "cmd2" "cmd3" --save-output ???
```

**Workaround**: Run separate `cycodmd run` invocations:
```bash
cycodmd run "cmd1" --save-output output1.md
cycodmd run "cmd2" --save-output output2.md
cycodmd run "cmd3" --save-output output3.md
```

### No Template Variables

Run command output files do NOT support template variables like `{fileBase}` because there's no "file" context.

**Example** (literal path used):
```bash
cycodmd run "ls" --save-output "{date}-listing.md"
# Creates file literally named: {date}-listing.md (NOT expanded)
```

**Workaround**: Use shell expansion:
```bash
cycodmd run "ls" --save-output "$(date +%Y%m%d)-listing.md"
```

---

## Related Options

### Script Type Selection (Layer 1)
- `--bash`, `--cmd`, `--powershell`, `--script`: Affect what runs, not where output goes
- Layer 7 saves output regardless of script type

### Global Options
- `--quiet`: Suppresses status messages about file creation
- `--verbose`: Shows detailed output writing progress
- `--working-dir`: Changes base directory for script execution and relative output paths

---

## Examples

### Example 1: Simple Command Output
```bash
cycodmd run "ls -la /var/log" --save-output log-files.md
```

**Result**: `log-files.md` contains directory listing in markdown format.

---

### Example 2: Multi-Command Script
```bash
cycodmd run --bash "
  echo '# System Report'
  echo '## Disk Usage'
  df -h
  echo ''
  echo '## Memory'
  free -h
" --save-output system-report.md
```

**Result**: `system-report.md` with formatted system information.

---

### Example 3: AI-Analyzed Diagnostics
```bash
cycodmd run --bash "docker ps -a" \
  --instructions "Identify stopped containers and suggest cleanup actions" \
  --save-output docker-status.md \
  --save-chat-history docker-ai.jsonl
```

**Result**:
- `docker-status.md`: AI-analyzed container status
- `docker-ai.jsonl`: AI interaction log

---

### Example 4: Test Execution with Analysis
```bash
cycodmd run "pytest --verbose" \
  --save-output test-results.md \
  --instructions "Summarize test failures and suggest fixes" \
  --save-chat-history test-analysis.jsonl
```

**Result**:
- `test-results.md`: Test output + AI analysis
- `test-analysis.jsonl`: AI reasoning log

---

## See Also

- **[Layer 1: Target Selection](cycodmd-run-layer-1.md)** - Script/command to execute
- **[Layer 8: AI Processing](cycodmd-run-layer-8.md)** - AI analysis of script output
- **[Layer 9: Actions on Results](cycodmd-run-layer-9.md)** - Script execution (implicit action)
- **[Proof Document](cycodmd-run-layer-7-proof.md)** - Source code evidence

---

**[← Back to Run Overview](cycodmd-filtering-pipeline-catalog-README.md#4-run-script)** | **[Proof →](cycodmd-run-layer-7-proof.md)**
