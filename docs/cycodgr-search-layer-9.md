# cycodgr search - Layer 9: ACTIONS ON RESULTS

## Purpose

Layer 9 defines **what to do with search results** - actions that operate on the discovered repositories.

## Command-Line Options

### `--clone`

**Purpose**: Clone matching repositories

**Examples**:
```bash
cycodgr --repo-contains "terminal" --min-stars 1000 --clone
```

**Behavior**:
- Clones repositories to local filesystem
- Uses `git clone` command
- Skips repositories that already exist locally
- Works with `--max-clone` and `--clone-dir`

---

### `--max-clone <number>`

**Purpose**: Limit the number of repositories to clone

**Default**: 10

**Examples**:
```bash
cycodgr --repo-contains "ml" --clone --max-clone 5
```

---

### `--clone-dir <directory>`

**Purpose**: Specify directory for cloned repositories

**Default**: `external`

**Examples**:
```bash
cycodgr --repo-contains "tools" --clone --clone-dir vendor
```

---

### `--as-submodules`

**Purpose**: Add repositories as git submodules instead of cloning

**Examples**:
```bash
cycodgr --repo-contains "library" --clone --as-submodules
```

**Behavior**:
- Uses `git submodule add` instead of `git clone`
- Assumes current directory is a git repository
- Creates `.gitmodules` file

---

## Data Flow

```
Repository Search Results
  ↓
If --clone specified:
  ↓
  Take top N repos (min(MaxClone, repos.Count))
  ↓
  For each repo:
    ↓
    Check if target directory exists
      → If exists: Skip (report yellow warning)
      → If not exists: Clone/Add submodule
    ↓
    If --as-submodules:
      → git submodule add {url} {targetPath}
    Else:
      → git clone {url} {targetPath}
    ↓
    Report success (green) or error (red)
  ↓
  Report total cloned count
```

---

## Key Characteristics

1. **Repository search only**: Cloning only works with repository search, not code search
2. **Automatic directory creation**: Creates clone directory if it doesn't exist
3. **Skip existing**: Does not overwrite existing repositories
4. **Error handling**: Continues cloning even if individual repos fail

---

For detailed source code evidence, see: [**Layer 9 Proof Document**](cycodgr-search-layer-9-proof.md)
