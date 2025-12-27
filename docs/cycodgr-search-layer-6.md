# cycodgr search - Layer 6: DISPLAY CONTROL

## Purpose

Layer 6 controls **how results are presented** - formatting, output styles, and display options.

## Command-Line Options

### `--format <type>`

**Purpose**: Control the output format

**Valid values**:
- `detailed` (default) - Full repository and code details with context
- `repos` - Repository names only
- `urls` - Repository URLs only
- `files` - File URLs only
- `filenames` - File names only
- `json` - JSON format
- `csv` - CSV format
- `table` - Markdown table (repo search only)

**Examples**:
```bash
cycodgr --repo-contains "terminal" --format table
cycodgr --file-contains "async" --format json
```

**Storage**: `SearchCommand.Format` (string, default: "detailed")

**Source Reference**: See [Layer 6 Proof](cycodgr-search-layer-6-proof.md)

---

## Built-In Display Features

### Line Numbers
- **Always enabled** for code display
- Passed as `includeLineNumbers: true` to `LineHelpers.FilterAndExpandContext()`

### Syntax Highlighting
- **Always enabled** for code display
- Language detection based on file extension
- Uses code fences with language specifier

### Match Highlighting
- **Always enabled** (`highlightMatches: true`)
- Highlights matching text within lines

### Repository Information
- Star count with ⭐ emoji
- Language tag
- Description
- URL
- Topics (when available)
- Last updated date

---

## Format Examples

### Detailed Format (Default)

```markdown
## microsoft/terminal (⭐ 92.5k) (C++)

Repo: https://github.com/microsoft/terminal
Desc: The new Windows Terminal

Found 3 file(s) with 15 matches:
- https://github.com/microsoft/terminal/blob/main/src/Terminal.cpp (5 matches)

## Terminal.cpp

```cpp
  15: void Terminal::CreateProcess()
* 16:     m_conptyProcess = ConptyProcess::Create(...);
  17: }
```
```

### JSON Format

```json
[
  {
    "repository": {
      "url": "https://github.com/microsoft/terminal",
      "name": "terminal",
      "owner": "microsoft",
      "stars": 92500,
      "language": "C++"
    },
    "path": "src/Terminal.cpp",
    "textMatches": [...]
  }
]
```

---

## Related Layers

- **Layer 5 (Context Expansion)**: Provides lines to display
- **Layer 6 (Display Control)**: Formats those lines
- **Layer 7 (Output Persistence)**: Saves formatted output

---

For detailed source code evidence, see: [**Layer 6 Proof Document**](cycodgr-search-layer-6-proof.md)
