# cycodmd Run Command - Layer 6: Display Control

## Purpose

Layer 6 (Display Control) for the Run command determines **how script execution results are presented** to the user. Unlike other cycodmd commands, Run has minimal display control options since it simply executes scripts and formats the output in a fixed markdown structure.

## Command Line Options

### Display Control Options

Run command has **NO command-specific display control options**. The output format is fixed and not user-configurable.

### Inherited Options (from CycoDmdCommand)

The only display-related option available is:

#### `--save-output`
**Purpose**: Save command output to file instead of/in addition to console display.

**Type**: Optional string argument (file path template)

**Default**: None (output to console only)

**Example**:
```bash
cycodmd run "echo Hello" --save-output output.md
```

**Source**: [CycoDmdCommandLineOptions.cs:427-433](cycodmd-run-layer-6-proof.md#save-output-parsing)

**Note**: This is a Layer 7 (Output Persistence) option but affects display timing (output is delayed if saving).

---

### Global Options (from CommandLineOptions)

These inherited options affect all commands:

#### `--verbose`
**Purpose**: Enable verbose output.

**Source**: CommandLineOptions.cs:346-349

---

#### `--quiet`
**Purpose**: Suppress console output.

**Source**: CommandLineOptions.cs:350-353

---

#### `--debug`
**Purpose**: Enable debug output.

**Source**: CommandLineOptions.cs:341-345

---

## Automatic Display Behaviors

### Fixed Markdown Format

Run command output is **always** formatted as markdown with a fixed structure:

**For single-line scripts**:
```markdown
## `{script}`

Output:
```
{output}
```

Exit code: {exitCode}
```

**For multi-line scripts**:
```markdown
## Run

Run:
```
{script}
```

Output:
```
{output}
```

Exit code: {exitCode}
```

**Implementation**: Program.cs:450-464

**Display Control**: None - format is hard-coded and not user-configurable.

---

### Exit Code Display

The exit code is displayed **only if non-zero** (errors):

```
Exit code: {exitCode}
```

**Rationale**: Success (exit code 0) is implicit; errors are explicit.

**Implementation**: Program.cs:456

---

### Error Formatting

If script execution fails, errors are formatted as:

```markdown
## Error executing command: {command}

{error message}

{stack trace}
```

**Implementation**: Program.cs:466-469

---

### Console Output

Script results are output to console unless:
- `--save-output` is used
- AI instructions are present via `--instructions`

**Implementation**: Program.cs:375

---

## Data Flow

### Input to Layer 6

From previous layers:
- **Script**: Shell script to execute (Layer 1)
- **Shell type**: Cmd, Bash, PowerShell, or Default (Layer 1)
- **Execution result**: stdout, stderr, exit code (Layer 9 - Actions)

From command properties:
- None - Run has no display control properties

### Layer 6 Processing

1. **Execute Script**:
   - Run script using selected shell
   - Capture output (stdout + stderr merged)
   - Capture exit code

2. **Format Output**:
   - Determine single-line vs. multi-line script
   - Create markdown header
   - Add script display (if multi-line)
   - Add output section with code blocks
   - Add exit code (if non-zero)

3. **Output**:
   - Return formatted markdown string
   - Output to console if not delaying

### Output from Layer 6

To output stages:
- **Formatted markdown**: Fixed-format string with script results
- **Console output**: Via `ConsoleHelpers.WriteLineIfNotEmpty`

To Layer 7 (Output Persistence):
- **Formatted content**: Optionally saved via `--save-output`

---

## Integration Points

### Layer 1 → Layer 6

Layer 1 (Target Selection) provides:
- Script content
- Shell type (cmd, bash, powershell, default)

These are used to execute the script; results are then formatted by Layer 6.

### Layer 6 → Layer 7

Layer 6 provides:
- **Formatted output string**: With script results in fixed markdown format

Layer 7 (Output Persistence) uses:
- `SaveOutput` for file persistence (if specified)

### Layer 6 → Console

Direct output via:
- `ConsoleHelpers.WriteLineIfNotEmpty` (Program.cs:375)

---

## Why So Simple?

### Design Rationale

Run command has minimal display control because:

1. **Purpose**: Execute arbitrary shell commands
2. **Output**: Raw script output is the primary value
3. **Format**: Markdown wrapping is sufficient for most use cases
4. **Simplicity**: Users want to see what the script printed, not fancy formatting

### What's Not Needed

- ❌ Line numbers (script output is arbitrary)
- ❌ Highlighting (no pattern matching)
- ❌ Context expansion (no search/filtering)
- ❌ Files-only mode (only one "file" - the script)
- ❌ HTML stripping (scripts don't output HTML typically)

### What's Fixed

- ✅ Markdown wrapping (always enabled)
- ✅ Exit code display (if non-zero)
- ✅ Error formatting (standardized)
- ✅ Code block formatting (automatic backtick selection)

---

## Examples

### Example 1: Simple Command
```bash
cycodmd run "echo Hello, World!"
```

Output:
```markdown
## `echo Hello, World!`

Output:
```
Hello, World!
```
```

---

### Example 2: Multi-Line Script
```bash
cycodmd run --bash "
echo Starting...
ls -la
echo Done
"
```

Output:
```markdown
## Run

Run:
```
echo Starting...
ls -la
echo Done
```

Output:
```
Starting...
total 48
drwxr-xr-x  12 user  staff   384 Jan 15 10:30 .
drwxr-xr-x   8 user  staff   256 Jan 15 09:00 ..
...
Done
```
```

---

### Example 3: Command with Error
```bash
cycodmd run "exit 1"
```

Output:
```markdown
## `exit 1`

Output:
```
```

Exit code: 1
```

---

### Example 4: Failed Execution
```bash
cycodmd run "nonexistent-command"
```

Output:
```markdown
## Error executing command: nonexistent-command

Command not found: nonexistent-command

[Stack trace if available]
```

---

## Implementation Details

### Display Control Properties

Run command has **NO display control properties**:

```csharp
class RunCommand : CycoDmdCommand
{
    public string ScriptToRun { get; set; }
    public ScriptType Type { get; set; }
}
```

**Properties**: Only script content and shell type, no display options.

---

### Parsing Implementation

Run command has **NO display-specific parsing** in `TryParseRunCommandOptions` (lines 56-98).

Only script content parsing:
- `--script`, `--cmd`, `--bash`, `--powershell`

---

### Execution Implementation

In `Program.HandleRunCommand`:
- Lines 366-381: Simple wrapper around content formatting
- No display properties extracted (none exist)
- Direct call to `GetCheckSaveRunCommandContentAsync`

---

### Formatting Implementation

In `Program.GetFormattedRunCommandContentAsync`:
- Lines 433-470: Fixed markdown formatting
- Line 450: Detect single-line vs. multi-line
- Line 451: Create header
- Line 453: Get required backtick count for code blocks
- Lines 454-462: Build formatted string
- Line 456: Conditional exit code display (only if non-zero)

**Key Logic**:
```csharp
var isMultiLine = script.Contains("\n");
var header = isMultiLine ? "## Run\n\n" : $"## `{script}`\n\n";
var backticks = new string('`', MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(output));
var scriptPart = isMultiLine ? $"Run:\n{backticks}\n{script.TrimEnd()}\n{backticks}\n\n" : string.Empty;
var outputPart = $"Output:\n{backticks}\n{output.TrimEnd()}\n{backticks}\n\n";
var exitCodePart = exitCode != 0 ? $"Exit code: {exitCode}\n\n" : string.Empty;
```

---

### Backtick Escaping

**Purpose**: Handle output that contains backticks.

**Implementation**: `MarkdownHelpers.GetCodeBlockBacktickCharCountRequired(output)`

**Logic**:
- Count consecutive backticks in output
- Use one more backtick than the maximum found
- Ensures code blocks are properly escaped

**Example**:
- Output contains "```" (3 backticks)
- Use "````" (4 backticks) for code block fence

---

## Special Behaviors

### Single-Line vs. Multi-Line Detection

**Detection**: `script.Contains("\n")`

**Single-Line**:
- Script shown in header: `` ## `{script}` ``
- Script not repeated in body

**Multi-Line**:
- Generic header: `## Run`
- Script shown in "Run:" section

**Rationale**: Single-line commands are obvious from header; multi-line scripts need to be displayed.

---

### Exit Code Conditional Display

**Logic**: `exitCode != 0 ? $"Exit code: {exitCode}\n\n" : string.Empty`

**Display Exit Code**: Only if non-zero (error)

**Rationale**:
- Success (exit code 0) is the expected/normal case
- Failures should be explicit
- Reduces noise in successful output

---

### Merged Output

**Implementation**: `result.MergedOutput` (Program.cs:447)

**Behavior**: stdout and stderr are merged into single output stream.

**Rationale**:
- Simplifies display (no separate sections)
- Preserves temporal order of output
- Most scripts don't distinguish stdout vs. stderr semantically

---

## Comparison with Other Commands

### Files Command

Files command has extensive Layer 6 options:
- ✅ Line numbers
- ✅ Highlighting
- ✅ Files-only mode
- ✅ Markdown wrapping control

### WebSearch/WebGet Commands

Web commands have moderate Layer 6 options:
- ✅ HTML stripping
- ✅ Browser selection
- ✅ Interactive mode

### Run Command

Run command has minimal Layer 6 options:
- ❌ No command-specific display options
- ✅ Fixed markdown format
- ✅ Automatic formatting only

**Why the difference?**

| Command | Primary Function | Display Needs |
|---------|------------------|---------------|
| Files | Search and filter content | Line-level control, highlighting |
| Web | Retrieve and display pages | Format conversion (HTML→text/markdown) |
| Run | Execute scripts | Show raw output with minimal wrapping |

Run command's simplicity is intentional - users want to see raw script output, not fancy formatting.

---

## See Also

- [Layer 1: Target Selection](cycodmd-run-layer-1.md) - Script specification and shell selection
- [Layer 7: Output Persistence](cycodmd-run-layer-7.md) - Saving script output
- [Layer 9: Actions on Results](cycodmd-run-layer-9.md) - Script execution
- [Source Code Evidence](cycodmd-run-layer-6-proof.md) - Detailed line-by-line proof
