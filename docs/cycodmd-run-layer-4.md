# cycodmd RunCommand - Layer 4: CONTENT REMOVAL

[🔙 Back to RunCommand](cycodmd-run-catalog-README.md) | [🔍 View Proof](cycodmd-run-layer-4-proof.md)

## Purpose

Layer 4 implements **active content removal** - the ability to explicitly remove lines from display even if they would otherwise be included by earlier filtering layers.

## Implementation Status

❌ **Not Implemented**

RunCommand does not implement Layer 4 (CONTENT REMOVAL). It executes scripts and captures their output but provides no options for filtering or removing specific lines from that output.

## Command-Line Options

### No Layer 4 Options Available

RunCommand does not provide command-line options for removing specific lines or patterns from script output.

## Rationale

RunCommand focuses on:
1. **Layer 1**: Executing scripts (bash, cmd, PowerShell)
2. **Layer 6**: Displaying output
3. **Layer 8**: AI processing of output (via `--instructions`)

Content removal (Layer 4) is not implemented because:
- Script output is typically shown verbatim
- Users can pipe output to other commands for filtering
- AI instructions (Layer 8) can be used to summarize/filter output
- RunCommand is designed for simple script execution, not complex filtering

## Workarounds

If you need to remove specific content from script output, you can:

### 1. Use AI Instructions (Layer 8)
```bash
cycodmd run --bash "your-script.sh" --instructions "Show only errors, exclude warnings"
```

### 2. Filter in the Script Itself
```bash
cycodmd run --bash "your-script.sh | grep -v 'DEBUG'"
```

### 3. Pipe to FindFilesCommand
```bash
cycodmd run --bash "your-script.sh" --save-output output.md
cycodmd output.md --remove-all-lines "DEBUG|INFO"
```

### 4. Use Shell Filtering
```bash
# Bash
cycodmd run --bash "command 2>&1 | grep -v 'noise'"

# PowerShell
cycodmd run --powershell "command | Where-Object { $_ -notmatch 'noise' }"
```

## Comparison with FindFilesCommand

| Feature | FindFilesCommand | RunCommand |
|---------|------------------|------------|
| Layer 4 Option | `--remove-all-lines` | ❌ Not available |
| Purpose | Remove lines from files | N/A |
| Regex Patterns | ✅ Multiple patterns | ❌ Not supported |
| Context Awareness | ✅ Excludes from context | N/A |

## Future Enhancement Possibility

Layer 4 could potentially be added to RunCommand with options like:
- `--remove-output-lines <patterns>`: Remove lines from script output
- `--filter-stderr`: Filter stderr separately from stdout
- `--remove-empty-lines`: Remove blank lines from output

However, these are **not currently implemented**.

## Source Code Evidence

See [Layer 4 Proof](cycodmd-run-layer-4-proof.md) for detailed evidence showing:
- RunCommand class has no content removal properties
- Only has `ScriptToRun` and `Type` properties
- No `--remove-*` options in command-line parser
- No line-level filtering in script execution code

## Related Layers

- [Layer 1: TARGET SELECTION](cycodmd-run-layer-1.md) - What script to run
- [Layer 2: CONTAINER FILTER](cycodmd-run-layer-2.md) - Not applicable
- [Layer 3: CONTENT FILTER](cycodmd-run-layer-3.md) - What output to capture
- **Layer 4: CONTENT REMOVAL** ← You are here (Not Implemented)
- [Layer 5: CONTEXT EXPANSION](cycodmd-run-layer-5.md) - Not applicable
- [Layer 6: DISPLAY CONTROL](cycodmd-run-layer-6.md) - How to format output
- [Layer 7: OUTPUT PERSISTENCE](cycodmd-run-layer-7.md) - Where to save results
- [Layer 8: AI PROCESSING](cycodmd-run-layer-8.md) - AI-assisted analysis
- [Layer 9: ACTIONS ON RESULTS](cycodmd-run-layer-9.md) - Script execution itself

---

[🔙 Back to RunCommand](cycodmd-run-catalog-README.md) | [🔍 View Proof](cycodmd-run-layer-4-proof.md)
