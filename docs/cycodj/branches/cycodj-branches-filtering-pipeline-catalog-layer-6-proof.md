# cycodj branches - Layer 6: Display Control - PROOF

## Parser Evidence

### Option Parsing: `--verbose` / `-v`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 453-459**: Parsing `--verbose` option
```csharp
else if (arg == "--verbose" || arg == "-v")
{
    command.Verbose = true;
    return true;
}
```

### Option Parsing: `--messages` and `--stats`

Uses shared `TryParseDisplayOptions()` method (lines 125-183) which sets `MessageCount` and `ShowStats` properties.

---

## Property Evidence

**File**: `src/cycodj/CommandLineCommands/BranchesCommand.cs`

**Lines 12-17**: Properties
```csharp
public string? Date { get; set; }
public string? Conversation { get; set; }
public bool Verbose { get; set; } = false;
public int Last { get; set; } = 0;
public int? MessageCount { get; set; } = null; // null = use default (0 for branches)
public bool ShowStats { get; set; } = false;
```

---

## Execution Evidence

### Tree Visualization

**Lines 186-257**: `AppendConversationTree()` method implements tree display with indentation (line 188), branch symbols (line 189), verbose info (lines 200-214), and message previews (lines 217-241).

### Statistics Display

**Lines 140-169**: Statistics section controlled by `ShowStats` (line 141), shows tree-specific metrics including branch depth (line 166).

---

## Summary

Layer 6 options: `--verbose`, `--messages [N|all]`, `--stats` control tree visualization, metadata verbosity, message previews, and statistics display.
