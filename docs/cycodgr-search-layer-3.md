# cycodgr search - Layer 3: CONTENT FILTERING

## Purpose

Layer 3 defines **what content within selected containers to show** - filtering at the line and content level within files that have already been selected.

## Command-Line Options

### `--contains <term>`

**Purpose**: Unified search - searches both repository metadata AND code content

**Examples**:
```bash
cycodgr --contains "terminal emulator"
```

**Behavior**:
- Triggers **dual search**: Both repository search and code search
- Searches repo names, descriptions, README, AND file content
- Returns both repository results and code match results
- Stored in `SearchCommand.Contains` (string)

**Source Reference**: See [Layer 3 Proof](cycodgr-search-layer-3-proof.md#contains)

---

### `--line-contains <pattern...>`

**Purpose**: Filter displayed lines to only those matching specific patterns (post-fetch filtering)

**Examples**:
```bash
cycodgr microsoft/terminal --file-contains "ConPTY" --line-contains "Create.*Process"
cycodgr --cs-file-contains "async" --line-contains "Task" --line-contains "await"
```

**Behavior**:
- **Post-fetch filtering**: Applied after fetching full file content
- Multiple patterns can be specified (OR logic - show lines matching ANY pattern)
- Uses regex matching
- Stored in `SearchCommand.LineContainsPatterns` (List<string>)
- Applied in `ProcessFileGroupAsync()` when displaying code

**Default behavior**: If no `--line-contains` specified, uses the search query as the line filter

**Source Reference**: See [Layer 3 Proof](cycodgr-search-layer-3-proof.md#line-contains)

---

## Implementation Details

### File Content Fetching

**File**: `src/cycodgr/Program.cs`

**Lines 758-780** - In `ProcessFileGroupAsync`:
- Fetches full file content from GitHub raw URL
- Creates `FoundTextFile` with lazy-loading lambda
- Loads content asynchronously via HTTP

### Line Filtering

**Lines 783-816** - In `ProcessFileGroupAsync`:
- Determines which patterns to use (`LineContainsPatterns` or fallback to search query)
- Converts patterns to `Regex` objects
- Uses `LineHelpers.FilterAndExpandContext()` to filter lines
- Includes line numbers and syntax highlighting

### Fallback Behavior

**Lines 829-851** - In `ProcessFileGroupAsync`:
- If file fetching fails, falls back to GitHub's text match fragments
- Displays fragments in code fences
- No line filtering in fallback mode

---

## Data Flow

```
Search Results (CodeMatch list)
    ↓
FormatAndOutputCodeResults()
    ↓
ProcessFileGroupAsync() (for each file)
    ↓
Fetch file content (HTTP to raw.githubusercontent.com)
    ↓
Determine line filter patterns:
    - Use LineContainsPatterns if specified
    - Otherwise, use search query
    ↓
LineHelpers.FilterAndExpandContext()
    - Apply include patterns (regex)
    - Apply context expansion (lines before/after)
    - Include line numbers
    - Highlight matches
    ↓
Display filtered content in code fence
```

---

## Related Layers

- **Layer 2 (Container Filtering)**: Determines which files to fetch
- **Layer 5 (Context Expansion)**: Controls lines before/after matches
- **Layer 6 (Display Control)**: Controls formatting and highlighting

---

For detailed source code evidence with line numbers and call traces, see: [**Layer 3 Proof Document**](cycodgr-search-layer-3-proof.md)
