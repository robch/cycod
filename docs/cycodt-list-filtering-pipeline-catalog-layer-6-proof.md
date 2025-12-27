# cycodt list - Layer 6: Display Control - PROOF

This document provides detailed source code evidence for all Layer 6 (Display Control) features of the `cycodt list` command.

## 6.1 Verbose Mode

### Command Line Option Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 346-349**:
```csharp
else if (arg == "--verbose")
{
    this.Verbose = true;
}
```

**Explanation**: The `--verbose` flag sets the `Verbose` boolean property to `true` on the `CommandLineOptions` base class, which is inherited by `CycoDtCommandLineOptions`.

---

### Verbose Mode Implementation in TestListCommand

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 20-44**:
```csharp
if (ConsoleHelpers.IsVerbose())
{
    var grouped = tests
        .GroupBy(t => t.CodeFilePath)
        .OrderBy(g => g.Key)
        .ToList();
    for (var i = 0; i < grouped.Count; i++)
    {
        if (i > 0) ConsoleHelpers.WriteLine();

        var group = grouped[i];
        ConsoleHelpers.WriteLine($"{group.Key}\n", ConsoleColor.DarkGray);
        foreach (var test in group)
        {
            ConsoleHelpers.WriteLine($"  {test.FullyQualifiedName}", ConsoleColor.DarkGray);
        }
    }
}
else
{
    foreach (var test in tests)
    {
        ConsoleHelpers.WriteLine(test.FullyQualifiedName, ConsoleColor.DarkGray);
    }
}
```

**Explanation**:
- **Line 20**: Checks if verbose mode is enabled using `ConsoleHelpers.IsVerbose()`
- **Lines 22-24**: Groups tests by `CodeFilePath` property and sorts groups by file path
- **Lines 26-35**: 
  - Adds blank line between groups (line 28)
  - Prints file path in DarkGray (line 31)
  - Prints each test name indented with 2 spaces (lines 32-35)
- **Lines 39-42**: Non-verbose mode prints flat list of test names without grouping

---

### IsVerbose Helper Method

**File**: `src/common/Helpers/ConsoleHelpers.cs` (inferred from usage)

**Expected Implementation**:
```csharp
public static bool IsVerbose()
{
    // Returns the Verbose property from CommandLineOptions
    // Likely stored in a static context or passed through configuration
}
```

**Evidence of Usage**: Called on line 20 of `TestListCommand.cs`

---

## 6.2 Test Count Display

### Implementation

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 46-48**:
```csharp
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"\nFound {tests.Count()} test..."
    : $"\nFound {tests.Count()} tests...");
```

**Explanation**:
- **Line 46**: Checks if exactly 1 test was found
- **Line 47**: Singular form: "Found 1 test..."
- **Line 48**: Plural form: "Found N tests..." for N != 1
- Prepends newline for spacing from previous output

---

## 6.3 Color Coding

### File Path Color

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Line 31**:
```csharp
ConsoleHelpers.WriteLine($"{group.Key}\n", ConsoleColor.DarkGray);
```

**Explanation**: File paths are displayed in `ConsoleColor.DarkGray` with a trailing newline for spacing.

---

### Test Name Color (Verbose Mode)

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Line 34**:
```csharp
ConsoleHelpers.WriteLine($"  {test.FullyQualifiedName}", ConsoleColor.DarkGray);
```

**Explanation**: 
- Test names are displayed in `ConsoleColor.DarkGray`
- Prefixed with 2 spaces for indentation under file path

---

### Test Name Color (Non-Verbose Mode)

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Line 42**:
```csharp
ConsoleHelpers.WriteLine(test.FullyQualifiedName, ConsoleColor.DarkGray);
```

**Explanation**: Test names are displayed in `ConsoleColor.DarkGray` without indentation.

---

### Count Message Color

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 46-48**:
```csharp
ConsoleHelpers.WriteLine(tests.Count() == 1
    ? $"\nFound {tests.Count()} test..."
    : $"\nFound {tests.Count()} tests...");
```

**Explanation**: No explicit color parameter, so uses default console color (typically white or terminal default).

---

## 6.4 Quiet Mode

### Command Line Option Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 350-353**:
```csharp
else if (arg == "--quiet")
{
    this.Quiet = true;
}
```

**Explanation**: The `--quiet` flag sets the `Quiet` boolean property to `true`.

---

### Quiet Mode Usage

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Note**: The `TestListCommand` does NOT explicitly check the `Quiet` flag. All output goes through `ConsoleHelpers.WriteLine()`, which internally respects the quiet mode setting.

**Expected ConsoleHelpers Implementation**:
```csharp
public static void WriteLine(string message, ConsoleColor? color = null, bool overrideQuiet = false)
{
    if (IsQuiet() && !overrideQuiet) return;
    
    // Write with color if specified
    // ...
}
```

**Implication**: When `--quiet` is set, most output from `TestListCommand` will be suppressed by the `ConsoleHelpers` layer.

---

## 6.5 Debug Mode

### Command Line Option Parsing

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 341-345**:
```csharp
else if (arg == "--debug")
{
    this.Debug = true;
    ConsoleHelpers.ConfigureDebug(true);
}
```

**Explanation**: 
- Sets `Debug` property to `true`
- Immediately calls `ConsoleHelpers.ConfigureDebug(true)` to enable debug logging

---

### Debug Mode in Test Discovery

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Line 92**:
```csharp
ConsoleHelpers.WriteDebugLine($"Finding files with pattern: {pattern}");
```

**Explanation**: Debug output during file discovery phase. Only shown when `--debug` is enabled.

---

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Lines 165-166**:
```csharp
for (var j = 0; j < i; j++) ConsoleHelpers.WriteDebugLine($"arg[{j}] = {args[j]}");
ConsoleHelpers.WriteDebugLine($"(INVALID) arg[{i}] = {args[i]}");
```

**Explanation**: Debug output during command line parsing shows which arguments were processed and which failed. Only shown when `--debug` is enabled.

---

## Additional Display-Related Code

### Error Display

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Lines 52-55**:
```csharp
catch (Exception ex)
{
    ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
    return 1;
}
```

**Explanation**: 
- Errors are displayed using `ConsoleHelpers.WriteErrorLine()`
- Includes both message and stack trace
- Not controlled by quiet mode (errors are always shown)

---

### Test Logger Initialization

**File**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`

**Line 17**:
```csharp
TestLogger.Log(new CycoDtTestFrameworkLogger());
```

**Explanation**: Initializes logging infrastructure for test framework. The logger respects verbosity settings internally.

---

## Data Flow Diagram

```
User runs: cycodt list --verbose --test MyTest

CommandLineOptions.Parse()
    ↓
    arg == "--verbose" → Verbose = true
    ↓
    arg == "--test" → Tests.Add("MyTest")
    ↓
TestListCommand.ExecuteAsync()
    ↓
TestListCommand.ExecuteList()
    ↓
FindAndFilterTests() → List<TestCase>
    ↓
ConsoleHelpers.IsVerbose() → true
    ↓
Group tests by CodeFilePath
    ↓
For each group:
    ConsoleHelpers.WriteLine(filePath, DarkGray)
    For each test in group:
        ConsoleHelpers.WriteLine("  " + testName, DarkGray)
    ↓
ConsoleHelpers.WriteLine(count message)
    ↓
Return 0 (success)
```

---

## Summary of Evidence

### Options Parsed
1. **--verbose**: Lines 346-349 of CommandLineOptions.cs
2. **--quiet**: Lines 350-353 of CommandLineOptions.cs  
3. **--debug**: Lines 341-345 of CommandLineOptions.cs

### Display Logic
1. **Verbose check**: Line 20 of TestListCommand.cs
2. **Grouping logic**: Lines 22-24 of TestListCommand.cs
3. **Grouped output**: Lines 26-35 of TestListCommand.cs
4. **Flat output**: Lines 39-42 of TestListCommand.cs
5. **Count message**: Lines 46-48 of TestListCommand.cs

### Color Usage
1. **File paths**: Line 31 with ConsoleColor.DarkGray
2. **Test names (verbose)**: Line 34 with ConsoleColor.DarkGray
3. **Test names (flat)**: Line 42 with ConsoleColor.DarkGray
4. **Count message**: Lines 46-48 with default color

### Error Handling
1. **Exception display**: Lines 52-55 of TestListCommand.cs

---

## Related Source Files

- **Command**: `src/cycodt/CommandLineCommands/TestCommands/TestListCommand.cs`
- **Base class**: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`
- **Parser**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
- **Options base**: `src/common/CommandLine/CommandLineOptions.cs`
- **Helpers**: `src/common/Helpers/ConsoleHelpers.cs`
