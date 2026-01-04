# Iterative Implementation Scripts

This folder contains scripts for **automated iterative development** using the **Memento pattern**.

## Quick Start

```bash
cd /c/src/cycod-console-gui

# Option 1: Full featured bash script (recommended)
./implement-iteratively.sh

# Option 2: Simple foreach loop
./implement-foreach.sh

# Option 3: Windows batch file
implement-iteratively.bat
```

## How It Works

### The Memento Pattern

Each script reads `console-gui-implementation-memento.md` which contains:
- **Current Position** - Where we are now
- **Next Action Required** - What to do next
- **Completed Work** - Checklist of what's done
- **Phases** - Organized implementation plan

### Iteration Loop

For each iteration (1-100):

1. **Read Memento** → AI reads current position and next action
2. **Execute Step** → AI does the work (port code, test, etc.)
3. **Document** → AI creates/updates `console-gui-day-N.md`
4. **Update Memento** → AI updates current position
5. **Commit** → AI commits changes to git
6. **Verify** → AI double-checks completion

The loop continues until:
- All phases complete (Position says "COMPLETE")
- Hit a blocker (AI reports "BLOCKED")
- Reach max iterations (100)

## Script Details

### 1. `implement-iteratively.sh` (Bash - Full Featured)

**Best for**: Maximum reliability and verification

**Features**:
- Two-step verification per iteration
- Checks for completion after each step
- Creates daily memento files
- Commits each step to git
- Stops automatically when complete

**Usage**:
```bash
./implement-iteratively.sh
```

### 2. `implement-foreach.sh` (Bash - Simple)

**Best for**: Quick iterations when you trust the AI

**Features**:
- Uses cycod's built-in `--foreach` loop
- Single prompt per iteration
- Minimal overhead
- Still checks for completion

**Usage**:
```bash
./implement-foreach.sh
```

### 3. `implement-iteratively.bat` (Windows Batch)

**Best for**: Windows users without bash/WSL

**Features**:
- Same logic as the bash script
- Native Windows batch file
- Works in cmd.exe

**Usage**:
```cmd
implement-iteratively.bat
```

## What Gets Created

### During Execution

- **`console-gui-day-1.md`** - Documentation of day 1 work
- **`console-gui-day-2.md`** - Documentation of day 2 work
- **`console-gui-day-N.md`** - One file per work session
- **Git commits** - One commit per completed step

### Directory Structure After Execution

```
/c/src/cycod-console-gui/
  console-gui-implementation-memento.md  ← Always updated
  console-gui-day-1.md                   ← Created during iteration
  console-gui-day-2.md
  console-gui-day-N.md
  src/common/ConsoleGui/                 ← Code gets created here
    Core/
      Screen.cs                          ← Ported during Phase 1.1
      Window.cs
      ...
```

## Monitoring Progress

### Check Current Status

```bash
# View current position
head -20 console-gui-implementation-memento.md

# See what was done today
ls -lt console-gui-day-*.md | head -5

# Check git commits
git log --oneline -10
```

### If Script Stops

The script stops when:

1. **Success** - AI reports work is complete
   ```
   Current Position: Phase 7 COMPLETE - All testing done
   ```

2. **Blocked** - AI hits a problem
   ```
   Response: BLOCKED: Compilation error in Screen.cs line 42
   ```

3. **Max Iterations** - Hit 100 iterations (unlikely)

### Resuming After Stop

The scripts are **idempotent** - safe to run multiple times:

1. Read the memento to see current position
2. Fix any blockers if needed
3. Run the script again - it picks up where it left off

## Example Iteration

```
==========================================
ITERATION #5
==========================================

>>> Step 1: Reading memento and executing next action...

[AI reads memento]
Current Position: Phase 1.1 - Porting Screen.cs
Next Action: Port Window.cs from AI CLI

[AI does the work]
- Copied Window.cs from ../ai/src/common/details/console/gui/
- Updated namespace from Azure.AI.Details.Common.CLI.ConsoleGui to ConsoleGui
- Fixed 3 compilation errors
- Added basic test
- Works on Windows

>>> Step 2: Verifying completion...

[AI double-checks]
✓ Updated memento with "Phase 1.2 COMPLETE"
✓ Created console-gui-day-5.md
✓ Committed to git: "Phase 1.2: Port Window.cs"
✓ No compilation errors
✓ Test passes

Response: VERIFIED COMPLETE

>>> Iteration 5 complete!
```

## Customization

### Change Max Iterations

Edit the script:
```bash
MAX_ITERATIONS=50  # Instead of 100
```

### Modify Prompts

Edit the `--input` text in the scripts to:
- Add more detailed instructions
- Change verification criteria
- Adjust commit message format

### Run Single Iteration Manually

```bash
cycod --input "
Read console-gui-implementation-memento.md.
Execute the Next Action Required.
Update memento when done.
"
```

## Tips

### For Best Results

1. **Start Fresh** - Make sure you're on the correct branch and worktree
2. **Monitor First Few** - Watch the first 2-3 iterations to ensure quality
3. **Check Daily Mementos** - Review what was documented each day
4. **Read Git Commits** - Verify each commit makes sense
5. **Test Manually** - Build and test after every few iterations

### If Things Go Wrong

1. **Stop the Script** - Ctrl+C to interrupt
2. **Review Last Changes** - Check what was just committed
3. **Revert if Needed** - `git reset --hard HEAD~1` to undo last commit
4. **Fix the Issue** - Manually correct any problems
5. **Update Memento** - Adjust "Next Action Required"
6. **Resume** - Run script again

## Architecture

### Why This Pattern Works

1. **Context Preservation** - Memento always shows current state
2. **Incremental Progress** - One small step at a time
3. **Verification** - Double-check before moving on
4. **Documentation** - Daily mementos preserve reasoning
5. **Git History** - Each step is a commit
6. **Recovery** - Can resume from any point

### Similar to Chess Variants

Just like the chess work that processed variants one-by-one:
- Read instructions
- Do ONE thing
- Document it
- Mark complete
- Move to next

This script automates that pattern!

## See Also

- `console-gui-implementation-memento.md` - The main memento
- `todo-console-gui-components.md` - What to implement
- `implementation-locations.md` - Where to put things
- `../genesis/meta-canon-system-memento.md` - Original memento pattern

---

**Ready to start?**

```bash
cd /c/src/cycod-console-gui
./implement-iteratively.sh
```

Let the AI do the incremental work while you watch it progress! 🚀
