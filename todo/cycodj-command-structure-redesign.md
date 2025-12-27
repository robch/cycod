# TODO: Simplify and Unify cycodj Command Structure

## The Problem

Current commands are confusing and redundant:
- `list` - shows 1 message preview
- `journal` - shows 3 message previews + time grouping (morning/afternoon/evening)
- `show` - shows all messages for ONE conversation
- `branches` - shows tree with no messages
- `export` - shows all messages for multiple conversations, saves to file
- `stats` - shows statistics (completely different output)

**Confusion:**
- Users don't know which command to use
- Same information shown in multiple ways
- Arbitrary differences (1 vs 3 messages)
- Tool is named "cycodj" (journal) but `journal` is just one command

---

## The Solution

**Core commands (what data to get):**
- `list` - Get conversations (with filters)
- `search` - Get conversations matching text
- `branches` - Get conversation tree
- `show` - Get one specific conversation

**Flags (how to display):**
- `--messages[=N]` - Show N messages per conversation
- `--branches` - Show branch indicators (↳) and indentation
- `--stats` - Add statistics summary at end
- `--save-output <file>` - Save to file (consistent with cycodmd)

**Default message counts:**
- `list` → 3 messages (was 1)
- `search` → 3 messages (was 1)
- `branches` → 0 messages (structure only)
- `show` → all messages (unchanged)

---

## Detailed Design

### Default Behavior Changes

**Before:**
```bash
cycodj list           # 1 message preview
cycodj journal        # 3 message previews + time grouping
cycodj export -o x    # All messages to file
cycodj stats          # Statistics only
cycodj branches       # Tree, no messages
```

**After:**
```bash
cycodj list           # 3 message previews (better default)
cycodj list --messages=1              # Old list behavior
cycodj list --messages=all            # Old journal/export behavior
cycodj list --stats                   # Show list + stats at end
cycodj list --save-output file.md     # Save to file
cycodj branches                       # Tree, no messages (same)
cycodj branches --messages            # Tree with 3 messages each
```

### Flag Behaviors

#### `--messages[=N]`
- No value: Use command default (list=3, branches=0, show=all)
- `--messages=N`: Show exactly N messages
- `--messages=all`: Show all messages
- Works on: list, search, branches

**Examples:**
```bash
cycodj list --messages           # 3 (default)
cycodj list --messages=1         # 1 message
cycodj list --messages=5         # 5 messages
cycodj list --messages=all       # All messages

cycodj branches --messages       # Add 3 messages to tree
cycodj branches --messages=1     # Add 1 message to tree
cycodj branches --messages=all   # Add all messages to tree
```

#### `--branches`
- Show branch indicators (↳)
- Indent child conversations
- Show parent-child relationships inline
- Only makes sense on: list, search

**Examples:**
```bash
cycodj list --branches           # List with ↳ and indentation
cycodj search "bug" --branches   # Search results showing branches
```

#### `--stats`
- **Additive** - adds statistics at the end
- Doesn't replace content
- Shows: message counts, conversation counts, branches, per-date breakdown
- Works on: list, search, branches, show

**Examples:**
```bash
cycodj list --stats              # Show conversations + stats at end
cycodj search "bug" --stats      # Show matches + stats at end
cycodj branches --stats          # Show tree + stats at end
cycodj show 12345 --stats        # Show conversation + stats at end
```

#### `--save-output <file>`
- Universal flag (like cycodmd's `--save-output`)
- Saves whatever would print to screen
- Works on: ALL commands
- Replaces the `export` command (or makes it redundant)

**Examples:**
```bash
cycodj list --last 7d --save-output week.md
cycodj search "bug" --save-output bugs.md
cycodj branches --messages --save-output tree.md
cycodj list --stats --save-output report.md
```

### Combining Flags

All flags are composable:

```bash
# List with everything
cycodj list --last 7d --branches --messages=5 --stats --save-output report.md

# Search with context
cycodj search "bug" --messages=all --branches --stats

# Tree with content
cycodj branches --yesterday --messages --stats

# Minimal list
cycodj list --messages=1
```

---

## What Happens to Existing Commands?

### `journal` command
**Options:**
1. **Remove it** - functionality covered by `list --messages=3` (default)
2. **Keep as alias** - `journal` = `list` (for backward compat)
3. **Make it do something special** - ???

**Recommendation:** Remove it or make it an alias. The time grouping (morning/afternoon/evening) wasn't that useful anyway.

### `export` command
**Options:**
1. **Remove it** - functionality covered by `--save-output`
2. **Keep as shortcut** - `export -o file` = `list --messages=all --save-output file`

**Recommendation:** Keep as convenience shortcut for common "save everything" case.

### `stats` command
**Options:**
1. **Remove it** - functionality covered by `--stats` flag
2. **Keep as shortcut** - `stats` = `list --stats`

**Recommendation:** Keep as shortcut. Common enough to warrant dedicated command.

---

## Migration Path

### Phase 1: Add new flags (non-breaking)
- Add `--messages[=N]` to list, search, branches
- Add `--branches` flag to list, search
- Add `--stats` flag to all commands (additive)
- Add `--save-output` to all commands
- Change default message count in `list` from 1 to 3

### Phase 2: Deprecate (breaking changes)
- Mark `journal` as deprecated (suggest `list` instead)
- Mark `export -o` as deprecated (suggest `--save-output` instead)
- Mark standalone `stats` as deprecated (suggest `list --stats` instead)

### Phase 3: Remove (major version)
- Remove deprecated commands
- Or keep them as aliases for convenience

---

## Implementation Checklist

### Step 1: Add `--messages[=N]` flag
- [ ] Add MessageCount property to commands (nullable int?)
- [ ] Parse `--messages` and `--messages=N` in options parser
- [ ] Update ListCommand to use MessageCount (default 3)
- [ ] Update SearchCommand to use MessageCount (default 3)
- [ ] Update BranchesCommand to use MessageCount (default 0)
- [ ] ShowCommand already shows all (no change)

### Step 2: Add `--branches` flag
- [ ] Add ShowBranches property to commands
- [ ] Update ListCommand to show ↳ and indent when ShowBranches=true
- [ ] Update SearchCommand to show ↳ and indent when ShowBranches=true
- [ ] Update display helper to detect and show branch relationships

### Step 3: Make `--stats` additive
- [ ] Change StatsCommand logic to be a helper that generates stats text
- [ ] Update ListCommand to append stats at end if --stats
- [ ] Update SearchCommand to append stats at end if --stats
- [ ] Update BranchesCommand to append stats at end if --stats
- [ ] Update ShowCommand to append stats at end if --stats
- [ ] Keep standalone `stats` command as wrapper for backward compat

### Step 4: Add `--save-output`
- [ ] Add SaveOutput property to base command
- [ ] Parse `--save-output <file>` in options parser
- [ ] In each command, check if SaveOutput is set
- [ ] If set, write output to file instead of console
- [ ] Consistent with cycodmd behavior

### Step 5: Update help documentation
- [ ] Document new flags in help/options.txt
- [ ] Show examples of combinations
- [ ] Explain default message counts
- [ ] Document migration from old commands

### Step 6: Update existing commands
- [ ] Change ListCommand default from 1 to 3 messages
- [ ] Consider making `journal` an alias for `list`
- [ ] Consider making `export` use `--save-output` internally

### Step 7: Testing
- [ ] Test all flag combinations
- [ ] Test backward compatibility
- [ ] Test with various date filters
- [ ] Test output formats

---

## Success Criteria

✅ User can control message count on any command  
✅ User can see branch relationships on any listing  
✅ User can add stats to any output  
✅ User can save any output to file  
✅ Default behavior is sensible (3 messages)  
✅ Flags are composable  
✅ Consistent with cycodmd patterns (--save-output)  
✅ Less confusing than current command structure  

---

## Future Enhancements

Once this is done, could add:
- `--format json` - JSON/JSONL output for piping
- `--conversation-instructions` - AI processing per conversation
- `--branch-instructions` - AI processing on branch relationships
- `--match-instructions` - AI processing on search matches (search only)
- More stats options: `--stats-tools`, `--stats-daily`, etc.

---

## Questions to Resolve

1. Should `journal` be removed or kept as alias?
2. Should `export` be removed or kept as convenience?
3. Should standalone `stats` be removed or kept as convenience?
4. What should `--messages` default to if no value? (Current proposal: command default)
5. Should there be `--no-messages` flag for brevity? Or just `--messages=0`?

**Current recommendation:** Keep `stats` and `export` as convenience commands, deprecate `journal`.

---

## Estimated Effort

- Step 1 (--messages): 2 hours
- Step 2 (--branches): 2 hours
- Step 3 (--stats additive): 3 hours
- Step 4 (--save-output): 2 hours
- Step 5 (help docs): 1 hour
- Step 6 (updates): 1 hour
- Step 7 (testing): 2 hours

**Total: ~13 hours** (spread over 2-3 sessions)

---

## Related TODOs

- cycodj-branch-context.md - Would benefit from `--messages` on branches
- cycodj-large-output-handling.md - Partially addressed by `--messages=N`
- cycodj-symblob-views.md - Future: different organizational views

---

## Benefits

**For users:**
- One mental model instead of many commands
- Composable flags (mix and match)
- Consistent with other cycod tools
- Less confusing
- More powerful (can combine features)

**For maintainers:**
- Less code duplication
- Clearer separation of concerns (data vs presentation)
- Easier to add new features (just add flags)
- More testable (test flags independently)

**For the tool:**
- Lives up to "journal" name (not just one command)
- More Unix-like philosophy (do one thing, flags modify)
- Better foundation for future features
