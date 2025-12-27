# cycodt `list` Command - Layer 9: Actions on Results

## Layer Status: ❌ NOT IMPLEMENTED

## Purpose

Layer 9 defines what **actions** are performed on the filtered and displayed results. This includes operations like:
- Executing the results (running tests, applying changes)
- Transforming the results (formatting, conversion)
- Validating the results (checking expectations)
- Modifying external state (creating files, cloning repositories)

## Implementation in `list` Command

The `list` command **does NOT implement Layer 9**. It is purely a read-only operation that:
1. Finds test files (Layer 1)
2. Filters test files (Layer 2)
3. Filters test cases within files (Layer 3)
4. Removes optional/excluded tests (Layer 4)
5. Displays the test names (Layer 6)

**No actions are performed on the results** - the command only lists test names and counts.

## CLI Options

**No options control Layer 9** for the `list` command.

## Data Flow

```
Test Cases (filtered)
    ↓
Display to console
    ↓
[END - No further actions]
```

## Source Code Evidence

See [Layer 9 Proof Document](cycodt-list-filtering-pipeline-catalog-layer-9-proof.md) for detailed source code analysis.

### Key Finding

The `list` command's `ExecuteList()` method (lines 13-57 in `TestListCommand.cs`) performs only display operations:

```csharp
private int ExecuteList()
{
    // ... find and filter tests ...
    
    // Display tests
    if (ConsoleHelpers.IsVerbose())
    {
        // Grouped display
    }
    else
    {
        // Simple display
    }
    
    ConsoleHelpers.WriteLine(tests.Count() == 1
        ? $"\nFound {tests.Count()} test..."
        : $"\nFound {tests.Count()} tests...");
    
    return 0;  // No actions performed
}
```

**No test execution, no file modification, no external actions.**

## Comparison with Other Commands

| Command | Layer 9 Actions |
|---------|----------------|
| `list` | ❌ None - read-only display |
| `run` | ✅ **Executes tests**, generates reports |
| `expect check` | ✅ **Validates expectations**, returns exit code |
| `expect format` | ✅ **Transforms input** to regex patterns |

## Potential Future Enhancements

While not currently implemented, the `list` command could potentially support:

1. **Test metadata operations**:
   - `--mark-as-broken`: Mark tests as broken/optional
   - `--tag-tests`: Add tags to tests

2. **Test file operations**:
   - `--move-tests`: Move tests between files
   - `--merge-tests`: Combine test files

3. **Test generation**:
   - `--generate-matrix`: Generate test matrix from templates
   - `--expand-foreach`: Expand foreach tests

4. **Export operations**:
   - `--export-json`: Export test metadata
   - `--export-csv`: Export test list

**None of these are currently implemented.**

## Related Layers

- **Layer 1** ([Target Selection](cycodt-list-filtering-pipeline-catalog-layer-1.md)): Determines which test files to process
- **Layer 2** ([Container Filter](cycodt-list-filtering-pipeline-catalog-layer-2.md)): Filters which test files to include
- **Layer 3** ([Content Filter](cycodt-list-filtering-pipeline-catalog-layer-3.md)): Filters which tests to list
- **Layer 6** ([Display Control](cycodt-list-filtering-pipeline-catalog-layer-6.md)): Controls how tests are displayed
- **Contrast with `run` Layer 9** ([run Actions](cycodt-run-filtering-pipeline-catalog-layer-9.md)): Shows active execution vs passive listing
