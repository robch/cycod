# cycodj branches - Layer 7: Output Persistence

[← Back to branches command](cycodj-branches-filtering-pipeline-catalog-README.md) | [→ Proof Document](cycodj-branches-filtering-pipeline-catalog-layer-7-proof.md)

## Overview

**Layer 7: Output Persistence** controls where and how the output of the `branches` command is saved. By default, output goes to the console (stdout), but users can optionally save it to a file.

## Implementation

The `branches` command implements output persistence identically to other cycodj commands through shared base class functionality.

### Command-Line Options

| Option | Argument | Description |
|--------|----------|-------------|
| `--save-output` | `<file>` | Save command output to the specified file instead of printing to console |

## File Output Details

### File Content
The saved output includes:
- Conversation tree structure with visual branching
- Branch relationships (parent-child)
- Timestamps and titles for each conversation
- Tool call counts
- Statistics (if requested)
- Verbose information (if requested)

## Example Usage

```bash
# Save branch tree to file
cycodj branches --save-output tree.md

# Save with statistics
cycodj branches --stats --save-output tree-stats.md

# Save filtered by time
cycodj branches --today --save-output today-branches.md

# With AI analysis
cycodj branches --instructions "Identify main conversation threads" --save-output threads.md
```

## Source Code References

See the [proof document](cycodj-branches-filtering-pipeline-catalog-layer-7-proof.md) for:
- Source code with line numbers (ExecuteAsync at lines 19-36)
- Output generation (GenerateBranchesOutput at lines 38-172)
- Identical save logic implementation

## Related Layers

- [Layer 6: Display Control](cycodj-branches-filtering-pipeline-catalog-layer-6.md)
- [Layer 8: AI Processing](cycodj-branches-filtering-pipeline-catalog-layer-8.md)
