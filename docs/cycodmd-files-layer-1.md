# cycodmd File Search - Layer 1: Target Selection

**Command**: `cycodmd [globs...]` (FindFilesCommand)

## Purpose

Layer 1 (Target Selection) specifies **what to search** - the primary search space for the file search operation. This layer determines which files from the filesystem will be considered for processing.

## Options

### Positional Arguments: Glob Patterns

**Syntax**: `cycodmd <glob1> [glob2] ...`

**Purpose**: Specify file patterns to search for using glob syntax.

**Examples**:
```bash
cycodmd "**/*.cs"           # All C# files recursively
cycodmd "*.md"              # All markdown files in current directory
cycodmd "src/**/*.js" "tests/**/*.js"  # Multiple patterns
```

**Default**: If no globs are provided, defaults to `**` (all files recursively)

**Parser Location**: [CycoDmdCommandLineOptions.cs:457-460](cycodmd-files-layer-1-proof.md#positional-glob-patterns)

**Command Property**: `FindFilesCommand.Globs` (List<string>)

---

### `--exclude <patterns...>`

**Syntax**: `cycodmd <globs> --exclude <pattern1> [pattern2] ...`

**Purpose**: Exclude files matching glob or regex patterns.

**Pattern Types**:
- **Glob patterns**: Patterns containing `/` or `\` (path separators)
- **Regex patterns**: Patterns without path separators (matched against filename only)

**Examples**:
```bash
cycodmd "**/*.cs" --exclude "*.g.cs"              # Exclude generated files
cycodmd "**/*" --exclude "bin/**" "obj/**"        # Exclude build directories
cycodmd "**/*.cs" --exclude "Test" "Mock"         # Exclude files with Test or Mock in name
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:282-289](cycodmd-files-layer-1-proof.md#exclude-option)

**Command Properties**:
- `FindFilesCommand.ExcludeGlobs` (List<string>) - for path-based exclusions
- `FindFilesCommand.ExcludeFileNamePatternList` (List<Regex>) - for filename-based exclusions

---

### Time-Based Filtering Options

Time-based options filter files by their filesystem timestamps.

#### `--modified <timespec>`

**Syntax**: `cycodmd <globs> --modified <timespec>`

**Purpose**: Include only files modified within the specified time range.

**Timespec Formats**:
- Relative: `7d` (last 7 days), `2h` (last 2 hours), `30m` (last 30 minutes)
- Range: `2024-01-01..2024-12-31`, `7d..`, `..yesterday`
- Absolute: `2024-01-01`, `2024-01-01T10:30:00`

**Examples**:
```bash
cycodmd "**/*.cs" --modified 7d           # Modified in last 7 days
cycodmd "**/*.cs" --modified 2024-01-01.. # Modified since Jan 1, 2024
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:188-195](cycodmd-files-layer-1-proof.md#modified-option)

**Command Properties**:
- `FindFilesCommand.ModifiedAfter` (DateTime?)
- `FindFilesCommand.ModifiedBefore` (DateTime?)

---

#### `--modified-after`, `--after`, `--time-after <timespec>`

**Syntax**: `cycodmd <globs> --modified-after <timespec>`

**Purpose**: Include only files modified after the specified time.

**Examples**:
```bash
cycodmd "**/*.cs" --modified-after 2024-01-01
cycodmd "**/*.cs" --after yesterday
cycodmd "**/*.cs" --time-after 7d
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:212-217](cycodmd-files-layer-1-proof.md#modified-after-options)

**Command Property**: `FindFilesCommand.ModifiedAfter` (DateTime?)

---

#### `--modified-before`, `--before`, `--time-before <timespec>`

**Syntax**: `cycodmd <globs> --modified-before <timespec>`

**Purpose**: Include only files modified before the specified time.

**Examples**:
```bash
cycodmd "**/*.cs" --modified-before 2024-01-01
cycodmd "**/*.cs" --before yesterday
cycodmd "**/*.cs" --time-before 30d
```

**Parser Location**: [CycoDmdCommandLineOptions.cs:218-223](cycodmd-files-layer-1-proof.md#modified-before-options)

**Command Property**: `FindFilesCommand.ModifiedBefore` (DateTime?)

---

#### `--created <timespec>`

**Syntax**: `cycodmd <globs> --created <timespec>`

**Purpose**: Include only files created within the specified time range.

**Parser Location**: [CycoDmdCommandLineOptions.cs:196-203](cycodmd-files-layer-1-proof.md#created-option)

**Command Properties**:
- `FindFilesCommand.CreatedAfter` (DateTime?)
- `FindFilesCommand.CreatedBefore` (DateTime?)

---

#### `--created-after <timespec>`

**Parser Location**: [CycoDmdCommandLineOptions.cs:224-229](cycodmd-files-layer-1-proof.md#created-after-option)

**Command Property**: `FindFilesCommand.CreatedAfter` (DateTime?)

---

#### `--created-before <timespec>`

**Parser Location**: [CycoDmdCommandLineOptions.cs:230-235](cycodmd-files-layer-1-proof.md#created-before-option)

**Command Property**: `FindFilesCommand.CreatedBefore` (DateTime?)

---

#### `--accessed <timespec>`

**Syntax**: `cycodmd <globs> --accessed <timespec>`

**Purpose**: Include only files accessed within the specified time range.

**Parser Location**: [CycoDmdCommandLineOptions.cs:204-211](cycodmd-files-layer-1-proof.md#accessed-option)

**Command Properties**:
- `FindFilesCommand.AccessedAfter` (DateTime?)
- `FindFilesCommand.AccessedBefore` (DateTime?)

---

#### `--accessed-after <timespec>`

**Parser Location**: [CycoDmdCommandLineOptions.cs:236-241](cycodmd-files-layer-1-proof.md#accessed-after-option)

**Command Property**: `FindFilesCommand.AccessedAfter` (DateTime?)

---

#### `--accessed-before <timespec>`

**Parser Location**: [CycoDmdCommandLineOptions.cs:242-247](cycodmd-files-layer-1-proof.md#accessed-before-option)

**Command Property**: `FindFilesCommand.AccessedBefore` (DateTime?)

---

#### `--anytime <timespec>`

**Syntax**: `cycodmd <globs> --anytime <timespec>`

**Purpose**: Include files where ANY timestamp (modified, created, or accessed) falls within the range.

**Parser Location**: [CycoDmdCommandLineOptions.cs:248-255](cycodmd-files-layer-1-proof.md#anytime-option)

**Command Properties**:
- `FindFilesCommand.AnyTimeAfter` (DateTime?)
- `FindFilesCommand.AnyTimeBefore` (DateTime?)

---

#### `--anytime-after <timespec>`

**Parser Location**: [CycoDmdCommandLineOptions.cs:256-261](cycodmd-files-layer-1-proof.md#anytime-after-option)

**Command Property**: `FindFilesCommand.AnyTimeAfter` (DateTime?)

---

#### `--anytime-before <timespec>`

**Parser Location**: [CycoDmdCommandLineOptions.cs:262-267](cycodmd-files-layer-1-proof.md#anytime-before-option)

**Command Property**: `FindFilesCommand.AnyTimeBefore` (DateTime?)

---

### Special File: `.cycodmdignore`

**Purpose**: Automatically load exclusion patterns from a `.cycodmdignore` file.

**Behavior**: 
- Searches parent directories for `.cycodmdignore` file
- Loads glob and regex patterns from the file
- Adds patterns to `ExcludeGlobs` and `ExcludeFileNamePatternList`

**Implementation**: [FindFilesCommand.cs:80-86](cycodmd-files-layer-1-proof.md#cycodmdignore-file)

---

## Data Flow

```
User Input (globs, exclusions, time filters)
    ↓
CycoDmdCommandLineOptions.TryParseOtherCommandArg()  [positional globs]
CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions()  [options]
    ↓
FindFilesCommand properties populated:
    - Globs (List<string>)
    - ExcludeGlobs (List<string>)
    - ExcludeFileNamePatternList (List<Regex>)
    - ModifiedAfter/Before (DateTime?)
    - CreatedAfter/Before (DateTime?)
    - AccessedAfter/Before (DateTime?)
    - AnyTimeAfter/Before (DateTime?)
    ↓
FindFilesCommand.Validate()
    - Applies default glob "**" if none provided
    - Loads .cycodmdignore patterns
    ↓
File enumeration and filtering (Layer 2)
```

## Integration with Other Layers

### Feeds Into Layer 2 (Container Filter)
- The files matched by globs and time filters become the **candidate set** for Layer 2
- Layer 2 further filters these files based on content (e.g., `--file-contains`)
- Exclusion patterns from Layer 1 are applied **before** content-based filtering

### Relationship to Layer 3+ (Content Layers)
- Layer 1 determines **which files** to examine
- Layers 3-9 determine **what to show** from those files

---

## See Also

- [Proof Document](cycodmd-files-layer-1-proof.md) - Source code evidence and line numbers
- [Layer 2: Container Filter](cycodmd-files-layer-2.md) - Content-based file filtering
- [TimeSpecHelpers Implementation](../src/common/Helpers/TimeSpecHelpers.cs) - Time parsing logic
