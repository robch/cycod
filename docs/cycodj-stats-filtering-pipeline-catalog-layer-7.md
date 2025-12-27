# cycodj stats - Layer 7: Output Persistence

[← Back to stats command](cycodj-stats-filtering-pipeline-catalog-README.md) | [→ Proof Document](cycodj-stats-filtering-pipeline-catalog-layer-7-proof.md)

## Overview

**Layer 7: Output Persistence** controls where and how the output of the `stats` command is saved. By default, output goes to the console (stdout), but users can optionally save it to a file.

## Implementation

The `stats` command implements output persistence identically to other cycodj commands through shared base class functionality.

### Command-Line Options

| Option | Argument | Description |
|--------|----------|-------------|
| `--save-output` | `<file>` | Save command output to the specified file instead of printing to console |

## File Output Details

### File Content
The saved output includes:
- Overall statistics (conversation counts, message counts by role)
- Average messages per conversation
- Longest conversation details
- Activity by date (optional with `--no-dates` to exclude)
- Tool usage statistics (optional with `--show-tools`)

## Example Usage

```bash
# Save statistics to file
cycodj stats --save-output stats.md

# Save with tool usage
cycodj stats --show-tools --save-output detailed-stats.md

# Save filtered by time
cycodj stats --last 30 --save-output last-30-days.md

# With AI analysis
cycodj stats --instructions "Identify trends and patterns" --save-output analysis.md
```

## Source Code References

See the [proof document](cycodj-stats-filtering-pipeline-catalog-layer-7-proof.md) for:
- Source code with line numbers (ExecuteAsync at lines 15-32)
- Output generation (GenerateStatsOutput at lines 34-216)
- Statistics formatting and aggregation

## Related Layers

- [Layer 6: Display Control](cycodj-stats-filtering-pipeline-catalog-layer-6.md)
- [Layer 8: AI Processing](cycodj-stats-filtering-pipeline-catalog-layer-8.md)
