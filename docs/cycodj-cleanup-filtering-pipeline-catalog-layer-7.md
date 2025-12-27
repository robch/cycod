# cycodj cleanup - Layer 7: Output Persistence

[← Back to cleanup command](cycodj-cleanup-filtering-pipeline-catalog-README.md) | [→ Proof Document](cycodj-cleanup-filtering-pipeline-catalog-layer-7-proof.md)

## Overview

**Layer 7: Output Persistence** is **NOT IMPLEMENTED** in the `cleanup` command.

## Why Not Implemented

The `cleanup` command is fundamentally different from other cycodj commands:

1. **Purpose**: It performs **actions** (deleting files), not just displaying information
2. **Output Type**: It produces **interactive console output** with confirmations and progress
3. **Safety Requirement**: Users need to see immediate feedback during dangerous operations (file deletion)
4. **Design Choice**: Output is intentionally kept on console for safety and interactivity

## Current Behavior

The `cleanup` command:
- Always outputs to console (using `overrideQuiet: true`)
- Does NOT support `--save-output` option
- Does NOT inherit the standard `SaveOutput` property usage
- Does NOT follow the standard execution pattern

## Alternative: Redirect Console Output

Users who want to save cleanup output can use shell redirection:

```bash
# Capture output using shell redirection
cycodj cleanup --find-duplicates > cleanup-report.txt

# Capture both stdout and stderr
cycodj cleanup --find-empty 2>&1 | tee cleanup-log.txt
```

## Execution Pattern

The `cleanup` command has a unique pattern:

```csharp
public override async Task<int> ExecuteAsync()
{
    ConsoleHelpers.WriteLine("## Chat History Cleanup", ConsoleColor.Cyan, overrideQuiet: true);
    // ... direct console output throughout ...
    // No call to SaveOutputIfRequested()
    // No use of SaveOutput property
    return 0;
}
```

## Command-Line Options

The cleanup command does NOT support:
- ❌ `--save-output` 

## Why This Design Makes Sense

1. **Interactive Confirmation**: The command prompts for user confirmation before deleting (`Type 'DELETE' to confirm:`). This requires direct console interaction.

2. **Real-time Progress**: During file deletion, the command shows immediate feedback for each file. Users need to see this in real-time.

3. **Safety First**: For destructive operations, immediate console visibility is more important than file output.

4. **Dry Run Default**: The command defaults to dry-run mode, showing what WOULD be deleted without actually deleting. This is inherently console-oriented.

## Source Code Evidence

See the [proof document](cycodj-cleanup-filtering-pipeline-catalog-layer-7-proof.md) for:
- Source code showing NO Layer 7 implementation
- Direct console output usage
- Interactive confirmation logic

## Related Layers

- [Layer 9: Actions on Results](cycodj-cleanup-filtering-pipeline-catalog-layer-9.md) - Where cleanup's unique functionality lives
