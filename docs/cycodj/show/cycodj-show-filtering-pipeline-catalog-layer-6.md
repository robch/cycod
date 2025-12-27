# cycodj show - Layer 6: Display Control

## Overview

Layer 6 (Display Control) in the `show` command controls **how a single conversation is presented** to the user, including:
- Tool call visibility
- Tool output verbosity
- Content length truncation
- Statistics display
- System message visibility

## CLI Options

### `--show-tool-calls`
Displays detailed information about tool calls made during the conversation.

Shows for each tool call:
- Tool call ID
- Function name

**Example:**
```bash
cycodj show abc123 --show-tool-calls
```

### `--show-tool-output`
Displays full tool output without truncation.

Without this option:
- Tool outputs are truncated to `MaxContentLength` (default: 500 characters)

With this option:
- Tool outputs are shown in full

**Example:**
```bash
cycodj show abc123 --show-tool-output
```

### `--max-content-length <N>`
Sets the maximum character length for content truncation.

Affects:
- Tool message content (unless `--show-tool-output` is enabled)
- Other message content (truncated at 3× this value)

**Default**: 500 characters

**Example:**
```bash
cycodj show abc123 --max-content-length 1000
```

### `--stats`
Enables conversation statistics at the end of the output.

Shows:
- Total message count and breakdown by role
- Percentages for each role
- Tool call count
- Branch information (if applicable)

**Example:**
```bash
cycodj show abc123 --stats
```

## Implementation Summary

The display control layer in `show` command:

1. **Tool Call Display**:
   - Controlled by `ShowToolCalls` boolean (default: false)
   - Shows tool call details after assistant messages
   - Lists tool call IDs and function names

2. **Tool Output Display**:
   - Controlled by `ShowToolOutput` boolean (default: false)
   - When false: Truncates tool outputs to `MaxContentLength`
   - When true: Shows full tool output
   - Shows "truncated X chars" message when truncated

3. **Content Length Control**:
   - Controlled by `MaxContentLength` integer (default: 500)
   - Tool messages: Truncated to `MaxContentLength` (unless `ShowToolOutput`)
   - Other messages: Truncated to `MaxContentLength * 3` (1500 chars default)
   - Displays remaining character count after truncation

4. **Statistics Display**:
   - Controlled by `ShowStats` boolean (default: false)
   - Shows message counts and percentages
   - Shows tool call count
   - Shows branch information if applicable

5. **System Message Visibility**:
   - System messages shown only when `--verbose` (global option)
   - Without verbose: Shows placeholder message
   - With verbose: Shows full system prompt

6. **Formatting**:
   - Header with title and conversation metadata
   - Message numbering
   - Role labels (uppercase)
   - Section separators (═ and ─ characters)
   - Tool call ID references for tool responses

## Layer Flow

```
Input: Single conversation (from Layer 1)
  ↓
Display header (title, timestamp, metadata)
  ↓
For each message:
  - Check if system message → hide unless verbose
  - Display message number and role
  - Apply content truncation based on:
    - Message role (tool vs. others)
    - ShowToolOutput flag
    - MaxContentLength value
  - If ShowToolCalls enabled:
    - Display tool call details
  - If tool response:
    - Show tool call ID reference
  ↓
Display footer
  ↓
If ShowStats enabled:
  - Generate statistics section
  ↓
Output: Formatted conversation display
```

## Related Layers

- **Layer 1 (Target Selection)**: Finds the specific conversation to show
- **Layer 6 (Display Control)**: Controls presentation details
- **Layer 7 (Output Persistence)**: Receives formatted output for saving

## See Also

- [Layer 6 Proof Document](./cycodj-show-filtering-pipeline-catalog-layer-6-proof.md) - Source code evidence
- [Show Command Overview](../cycodj-filtering-pipeline-catalog-README.md#show-command)
