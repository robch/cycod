# cycodmd Run - Layer 5: Context Expansion

**[← Back to Main Catalog](cycodmd-filtering-pipeline-catalog-README.md#run)**

## Purpose

Layer 5 controls how content is expanded to show context around matches. This layer is responsible for showing additional lines before and after matching content.

## Implementation Status

⚠️ **NOT IMPLEMENTED** / **NOT APPLICABLE** for Run command.

The Run command executes shell scripts and returns their **complete output** as-is. It:
1. Accepts a script to execute
2. Runs the script in the specified shell (bash, cmd, PowerShell, or default)
3. Returns the entire output (stdout and stderr)

There is **no filtering or context expansion** in the Run command.

## Why Layer 5 Doesn't Apply

The Run command's purpose is to **execute scripts**, not to search or filter content. Its data model is:
```
Script Text → Shell Execution → Complete Output
```

There is no concept of:
- "Matching lines" within output
- Context lines before/after matches
- Line-level filtering
- Output truncation or expansion

## Design Philosophy

The Run command is designed as a **pass-through executor**:
- Input: Script text
- Process: Execute in shell
- Output: Complete, unmodified output

Any filtering or context expansion would alter the script's output, which could:
- Break scripts that parse their own output
- Hide errors or warnings
- Make debugging harder

## Related Functionality

While Layer 5 (Context Expansion) is not implemented, the Run command supports:

### Shell Selection
- **`--bash`**: Execute script in bash shell
- **`--cmd`**: Execute script in Windows cmd
- **`--powershell`**: Execute script in PowerShell
- **Default**: Platform-specific (cmd on Windows, bash on Linux/Mac)

### AI Processing (Layer 8)
- **`--instructions`**: AI can analyze script output
- **`--save-chat-history`**: Save AI interaction for debugging

### Output Persistence (Layer 7)
- **`--save-output`**: Save script output to file

## Comparison with File Search

| Feature | File Search | Run Command |
|---------|-------------|-------------|
| Purpose | Search and filter files | Execute scripts |
| Line-level filtering | ✅ `--line-contains` | ❌ Not applicable |
| Context expansion | ✅ `--lines`, `--lines-before`, `--lines-after` | ❌ Not applicable |
| Line numbers | ✅ `--line-numbers` | ❌ Not applicable |
| Content removal | ✅ `--remove-all-lines` | ❌ Not applicable |
| Output modification | ✅ Filtering, expansion, formatting | ❌ Complete output only |

## Alternative: Post-Process Run Output

If you need line-level filtering or context expansion on script output:

### Option 1: Save and Process
```bash
# 1. Run script and save output
cycodmd run --bash "your-script.sh" --save-output output.txt

# 2. Process output with File Search
cycodmd output.txt --line-contains "ERROR" --lines 3
```

### Option 2: Pipe Through File Search
```bash
# On Unix-like systems
cycodmd run --bash "your-script.sh" | cycodmd --line-contains "ERROR" --lines 3
```

### Option 3: AI-Based Filtering
```bash
# Use AI to analyze and filter output
cycodmd run --bash "your-script.sh" \
  --instructions "Show only lines containing errors or warnings, with 2 lines of context"
```

## Future Possibilities

If line-level filtering were added to the Run command (controversial), it could support:
- `--lines N`: Show N lines around lines matching a pattern
- `--line-contains <pattern>`: Filter output to matching lines
- `--highlight-matches`: Highlight matching content in output

However, this would fundamentally change the Run command's purpose from "execute and return complete output" to "execute, filter, and return modified output", which may not be desirable.

## Use Cases Where Layer 5 Isn't Needed

The Run command is designed for scenarios where you want **complete output**:

1. **Debugging**: Need full context of errors
2. **Log generation**: Want complete logs
3. **Script orchestration**: Scripts may parse their own output
4. **CI/CD pipelines**: Need exact, unmodified output
5. **Data processing**: Scripts output structured data (JSON, CSV) that shouldn't be filtered

---

**[→ See Proof Documentation](cycodmd-run-layer-5-proof.md)** | **[← Back to Main Catalog](cycodmd-filtering-pipeline-catalog-README.md#run)**
