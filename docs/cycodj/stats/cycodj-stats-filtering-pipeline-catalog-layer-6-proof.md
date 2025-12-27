# cycodj stats - Layer 6: Display Control - PROOF

## Parser Evidence

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 488-500**: Parsing stats-specific display options
```csharp
else if (arg == "--show-tools")
{
    command.ShowTools = true;
    return true;
}
else if (arg == "--no-dates")
{
    command.ShowDates = false;
    return true;
}
```

## Property Evidence

**File**: `src/cycodj/CommandLineCommands/StatsCommand.cs`

**Lines 10-13**: Properties
```csharp
public string? Date { get; set; }
public int? Last { get; set; }
public bool ShowTools { get; set; }
public bool ShowDates { get; set; } = true;
```

## Execution Evidence

**Lines 103-113**: Statistics sections controlled by `ShowDates` (line 103) and `ShowTools` (line 109).

**Lines 118-147**: Overall statistics (always shown).

**Lines 149-174**: Date-based activity (controlled by `ShowDates`).

**Lines 176-216**: Tool usage statistics (controlled by `ShowTools`).

## Summary

Layer 6 options: `--show-tools` enables tool usage section, `--no-dates` disables date breakdown. Both control optional sections in statistics output.
