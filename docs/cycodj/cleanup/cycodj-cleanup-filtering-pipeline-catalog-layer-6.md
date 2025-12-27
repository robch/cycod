# cycodj cleanup - Layer 6: Display Control

## Overview

Layer 6 (Display Control) in the `cleanup` command controls **how cleanup operations are presented**, including:
- Operation progress display
- File list formatting
- Color-coded status messages
- Dry-run vs. execution mode display

## CLI Options

Layer 6 in the `cleanup` command has **no user-configurable CLI options** for display control. Display is built into the command logic.

## Implementation Summary

1. **Color-Coded Output**:
   - Cyan: Section headers
   - Yellow: Warnings and files to remove
   - Green: Success messages and kept files
   - Red: Error warnings
   - Dark Gray: File details

2. **Automatic Display Features**:
   - Progress messages with `overrideQuiet: true` (always shown)
   - File size display in KB
   - Duplicate group visualization
   - Keep vs. remove differentiation

3. **Dry-Run Display**:
   - Shows what would be deleted
   - Yellow warning about dry-run mode
   - Instructions to use `--execute`

4. **Execution Display**:
   - Confirmation prompt (requires "DELETE")
   - Per-file deletion status
   - Summary with total size freed

## See Also

- [Layer 6 Proof Document](./cycodj-cleanup-filtering-pipeline-catalog-layer-6-proof.md)
- [Cleanup Command Overview](../cycodj-filtering-pipeline-catalog-README.md#cleanup-command)
