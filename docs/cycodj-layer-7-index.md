# cycodj Layer 7 Documentation Index

Quick navigation for Layer 7 (Output Persistence) documentation across all cycodj commands.

## Documents Created

| Command | Catalog | Proof |
|---------|---------|-------|
| **list** | [Catalog](cycodj-list-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodj-list-filtering-pipeline-catalog-layer-7-proof.md) |
| **show** | [Catalog](cycodj-show-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodj-show-filtering-pipeline-catalog-layer-7-proof.md) |
| **search** | [Catalog](cycodj-search-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodj-search-filtering-pipeline-catalog-layer-7-proof.md) |
| **branches** | [Catalog](cycodj-branches-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodj-branches-filtering-pipeline-catalog-layer-7-proof.md) |
| **stats** | [Catalog](cycodj-stats-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodj-stats-filtering-pipeline-catalog-layer-7-proof.md) |
| **cleanup** | [Catalog](cycodj-cleanup-filtering-pipeline-catalog-layer-7.md) | [Proof](cycodj-cleanup-filtering-pipeline-catalog-layer-7-proof.md) |

## Quick Reference

### Shared Implementation (5 commands)
- list, show, search, branches, stats all use identical Layer 7 implementation
- **Recommended starting point**: [list proof document](cycodj-list-filtering-pipeline-catalog-layer-7-proof.md) for comprehensive details

### Not Implemented (1 command)
- cleanup deliberately does NOT implement Layer 7
- See [cleanup catalog](cycodj-cleanup-filtering-pipeline-catalog-layer-7.md) for explanation

## Key Source Files

| File | Purpose | Key Lines |
|------|---------|-----------|
| `CycoDjCommandLineOptions.cs` | Parses `--save-output` | 171-180 |
| `CycoDjCommand.cs` | Base class implementation | 17, 58-75 |
| `ListCommand.cs` | Example execution pattern | 25-42 |
| `ShowCommand.cs` | Example execution pattern | 18-35 |
| `SearchCommand.cs` | Example execution pattern | 23-40 |
| `BranchesCommand.cs` | Example execution pattern | 19-36 |
| `StatsCommand.cs` | Example execution pattern | 15-32 |
| `CleanupCommand.cs` | Non-standard (no Layer 7) | 18-119 |

## Related Documentation

- [Main cycodj Catalog README](cycodj-filtering-pipeline-catalog-README.md)
- [Overall CLI Filtering Patterns Catalog](CLI-Filtering-Patterns-Catalog.md)
- [Creation Summary](cycodj-layer-7-creation-summary.md)
