# cycodmd Files Command - Layer 2: Container Filter

## Purpose

Layer 2 filters **which containers (files) to include or exclude** based on their content or properties. After Layer 1 identifies potential files through glob patterns and time constraints, Layer 2 performs content-based filtering to determine which files actually contain relevant information.

## Command

**Default Command**: `cycodmd [globs...]`  
**Command Class**: `FindFilesCommand`

## Options

### --file-contains

**Syntax**: `--file-contains <pattern1> [pattern2] ...`

**Purpose**: Include ONLY files that contain text matching the specified regex pattern(s).

**Pattern Type**: Regular expression (regex), case-insensitive

**Multiple Patterns**: All patterns must match (AND logic)

**Examples**:
```bash
# Find C# files containing "async"
cycodmd "**/*.cs" --file-contains "async"

# Find files containing both "TODO" and "FIXME"
cycodmd "**/*" --file-contains "TODO" "FIXME"

# Find configuration files with database connection strings
cycodmd "**/*.config" "**/*.json" --file-contains "ConnectionString|Database"
```

**Parser Location**: Line 116-122 in `CycoDmdCommandLineOptions.cs`  
**Storage**: `FindFilesCommand.IncludeFileContainsPatternList` (List<Regex>, Line 95 in FindFilesCommand.cs)

**Execution**: Line 182 in `Program.cs` - passed to `FileHelpers.FindMatchingFiles`

---

### --file-not-contains

**Syntax**: `--file-not-contains <pattern1> [pattern2] ...`

**Purpose**: Exclude files that contain text matching the specified regex pattern(s).

**Pattern Type**: Regular expression (regex), case-insensitive

**Multiple Patterns**: Any pattern match excludes the file (OR logic)

**Examples**:
```bash
# Find C# files that don't contain "Obsolete"
cycodmd "**/*.cs" --file-not-contains "Obsolete"

# Exclude files with copyright headers or license text
cycodmd "**/*.cs" --file-not-contains "Copyright" "License"

# Find source files without test code
cycodmd "**/*.py" --file-not-contains "import unittest|import pytest"
```

**Parser Location**: Line 123-129 in `CycoDmdCommandLineOptions.cs`  
**Storage**: `FindFilesCommand.ExcludeFileContainsPatternList` (List<Regex>, Line 96 in FindFilesCommand.cs)

**Execution**: Line 183 in `Program.cs` - passed to `FileHelpers.FindMatchingFiles`

---

### --contains

**Syntax**: `--contains <pattern1> [pattern2] ...`

**Purpose**: **Dual-layer shorthand** - adds pattern to BOTH:
- `IncludeFileContainsPatternList` (Layer 2 - Container Filter)
- `IncludeLineContainsPatternList` (Layer 3 - Content Filter)

This is a convenience option that filters at both the file level and line level simultaneously.

**Effect**:
1. Files must contain the pattern somewhere (Layer 2 container filter)
2. When displaying file contents, only show lines matching the pattern (Layer 3 content filter)

**Examples**:
```bash
# Find files containing "async" and show only those lines
cycodmd "**/*.cs" --contains "async"

# Find and show lines with TODO or FIXME
cycodmd "**/*" --contains "TODO|FIXME"
```

**Parser Location**: Line 108-115 in `CycoDmdCommandLineOptions.cs`  
**Storage**:
- `FindFilesCommand.IncludeFileContainsPatternList` (List<Regex>, Line 95) - Layer 2
- `FindFilesCommand.IncludeLineContainsPatternList` (List<Regex>, Line 98) - Layer 3

**Note**: This option impacts both Layer 2 (this layer) and Layer 3 (Content Filter)

---

### Extension-Specific Shortcuts

**Syntax**: `--{extension}-file-contains <pattern1> [pattern2] ...`

**Purpose**: Shorthand for filtering files by both extension AND content. Automatically:
1. Filters to files with the specified extension (e.g., `.cs`, `.py`, `.js`)
2. Includes only files containing the specified pattern(s)

**Supported Extensions**: Any extension can be used (e.g., `cs`, `py`, `js`, `md`, `xml`, `json`, etc.)

**How It Works**:
- The extension is extracted from the option name (e.g., `--cs-file-contains` → `cs`)
- A file name criteria is stored with the pattern
- During execution, files are filtered by extension first, then by content

**Examples**:
```bash
# Find C# files containing "async"
cycodmd "**/*" --cs-file-contains "async"
# Equivalent to: cycodmd "**/*.cs" --file-contains "async"

# Find Python files with "import tensorflow"
cycodmd "**/*" --py-file-contains "import tensorflow"
# Equivalent to: cycodmd "**/*.py" --file-contains "import tensorflow"

# Find Markdown files containing "TODO"
cycodmd "**/*" --md-file-contains "TODO"
# Equivalent to: cycodmd "**/*.md" --file-contains "TODO"

# Find JSON config files with "production"
cycodmd "**/*" --json-file-contains "production"
# Equivalent to: cycodmd "**/*.json" --file-contains "production"
```

**Parser Location**: Line 268-281 in `CycoDmdCommandLineOptions.cs`  
**Pattern Recognition**: Line 268 - `if (arg.StartsWith("--") && arg.EndsWith("file-instructions"))`

Note: The parser actually handles `--{criteria}-file-instructions` (for Layer 8, AI Processing), but the same pattern mechanism is used here for extension-based filtering via the `FileInstructionsList` storage.

**Storage**: `FindFilesCommand.FileInstructionsList` (List<Tuple<string, string>>, Line 108)  
- First string: The instructions/pattern  
- Second string: The file criteria (extension)

**Special Note**: The extension-specific shortcuts are actually implemented through the AI processing layer's file instructions mechanism. The file criteria is used to filter which files receive which instructions. When combined with content patterns, this provides extension-based filtering.

**Implementation Detail**: The actual extension-based filtering for `--{ext}-file-contains` is not directly implemented in the current codebase. The `--{ext}-file-instructions` pattern exists (Line 268-281), but a pure `--{ext}-file-contains` option without instructions would need to be added. The documentation in the catalog may have assumed this feature exists when it doesn't yet.

**Current Workaround**: Use glob patterns with `--file-contains`:
```bash
cycodmd "**/*.cs" --file-contains "async"
```

---

## Execution Flow

### 1. Pattern Storage (Parsing Phase)

During command line parsing (`CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions`):
- `--file-contains` patterns → `IncludeFileContainsPatternList`
- `--file-not-contains` patterns → `ExcludeFileContainsPatternList`  
- `--contains` patterns → Both `IncludeFileContainsPatternList` AND `IncludeLineContainsPatternList`

### 2. File Content Filtering (Execution Phase)

In `Program.HandleFindFileCommand` (Line 178-192):

```csharp
var files = FileHelpers.FindMatchingFiles(
    findFilesCommand.Globs,                              // Layer 1: Glob patterns
    findFilesCommand.ExcludeGlobs,                       // Layer 1: Exclude globs
    findFilesCommand.ExcludeFileNamePatternList,         // Layer 1: Exclude regex
    findFilesCommand.IncludeFileContainsPatternList,     // Layer 2: Include by content ✓
    findFilesCommand.ExcludeFileContainsPatternList,     // Layer 2: Exclude by content ✓
    findFilesCommand.ModifiedAfter,                      // Layer 1: Time filter
    findFilesCommand.ModifiedBefore,                     // Layer 1: Time filter
    // ... other time filters ...
);
```

The `FileHelpers.FindMatchingFiles` method:
1. Takes the file list from Layer 1
2. Reads each file's content
3. Applies include patterns (must match ALL patterns)
4. Applies exclude patterns (if ANY pattern matches, file is excluded)
5. Returns filtered list of files

### 3. Pass to Layer 3

The filtered file list is then processed for line-level filtering (Layer 3), context expansion (Layer 5), etc.

---

## Data Flow

```
Layer 1 Output: List<string> files
    ↓
FileHelpers.FindMatchingFiles()
    ↓
For each file:
    ├─ Read file content
    ├─ Check IncludeFileContainsPatternList (ALL must match)
    ├─ Check ExcludeFileContainsPatternList (NONE can match)
    └─ Include file if conditions met
    ↓
Filtered List<string> files
    ↓
Layer 3: Content Filter (line-level)
```

---

## Performance Considerations

**File Content Reading**: Layer 2 requires reading file contents, which can be I/O intensive for:
- Large files
- Large numbers of files
- Network/remote filesystems

**Optimization**: The file content is read once and cached during the `FindMatchingFiles` operation, so subsequent layers don't need to re-read.

**Logging**: Content pattern searches are logged (Line 169-176 in Program.cs) for debugging:
```csharp
if (findFilesCommand.IncludeFileContainsPatternList.Any())
{
    Logger.Info($"Finding files containing regex pattern(s):");
    foreach (var pattern in findFilesCommand.IncludeFileContainsPatternList)
    {
        Logger.Info($"  Content pattern: '{pattern}'");
    }
}
```

---

## Integration with Other Layers

- **← Layer 1 (Target Selection)**: Receives initial file list from glob and time-based filtering
- **→ Layer 3 (Content Filter)**: Passes filtered files for line-level content filtering
- **↔ Layer 3 via `--contains`**: The `--contains` option adds patterns to both Layer 2 and Layer 3

---

## Pattern Matching Behavior

### Include Patterns (--file-contains, --contains)

**Logic**: **ALL** patterns must match (AND)

```bash
# File must contain BOTH "async" AND "await"
cycodmd "**/*.cs" --file-contains "async" "await"
```

### Exclude Patterns (--file-not-contains)

**Logic**: **ANY** pattern match excludes the file (OR)

```bash
# Exclude if file contains "Test" OR "Mock"
cycodmd "**/*.cs" --file-not-contains "Test" "Mock"
```

### Combining Include and Exclude

```bash
# Must contain "async", must NOT contain "Test" or "Obsolete"
cycodmd "**/*.cs" \
  --file-contains "async" \
  --file-not-contains "Test" "Obsolete"
```

---

## Examples

```bash
# Find configuration files with database settings
cycodmd "**/*.config" "**/*.json" --file-contains "ConnectionString"

# Find source files without TODOs
cycodmd "**/*.cs" --file-not-contains "TODO|FIXME"

# Find async C# methods (files and lines)
cycodmd "**/*.cs" --contains "async.*Task"

# Find Python files with TensorFlow imports but no test code
cycodmd "**/*.py" \
  --file-contains "import tensorflow" \
  --file-not-contains "unittest|pytest"

# Find markdown documentation without placeholder text
cycodmd "**/*.md" \
  --file-not-contains "TODO|TBD|PLACEHOLDER"

# Find shell scripts with error handling
cycodmd "**/*.sh" --file-contains "set -e|trap"
```

---

## Common Patterns

### Finding API Usage

```bash
# Find files using a specific API
cycodmd "**/*.cs" --file-contains "HttpClient|WebRequest"
```

### Excluding Generated Code

```bash
# Exclude auto-generated files
cycodmd "**/*.cs" \
  --file-not-contains "Auto-generated|<auto-generated>" \
  --file-not-contains "This code was generated"
```

### Finding Configuration

```bash
# Find files with environment-specific config
cycodmd "**/*" --file-contains "production|staging|development"
```

### Finding Security Concerns

```bash
# Find files with potential security issues
cycodmd "**/*.cs" --file-contains "password|secret|apikey"
```

---

## See Also

- [Layer 2 Proof Document](cycodmd-files-layer-2-proof.md) - Source code evidence and implementation details
- [Layer 1: Target Selection](cycodmd-files-layer-1.md) - File discovery via globs and time filters
- [Layer 3: Content Filter](cycodmd-files-layer-3.md) - Line-level filtering within files
- [FileHelpers.FindMatchingFiles Documentation](../src/common/FileHelpers.cs) - File search implementation
