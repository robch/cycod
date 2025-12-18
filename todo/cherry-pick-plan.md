# Cherry-Pick Plan for Clean cycodgr Branch

## Analysis Date: 2025-12-16

This document identifies which commits to cherry-pick for a clean cycodgr PR branch.

---

## Commit List (22 commits from newest to oldest)

```
e0809073 Phase E: Smart behaviors + bugfix for --line-contains
5a3bbfcf Add Phase D Implementation Plan and Repository Filtering Workflow
12015afc Add comprehensive tests for LineHelpers and ThrottledProcessor
318b9939 docs: update architecture doc with completion status
2a85fec5 feat: add final/global instructions support to cycodgr
7ec731e2 feat: add repo instructions support to cycodgr
7618ae2b feat: add file instructions support to cycodgr
d708c01c feat: add parallel file processing to cycodgr
3fe41109 Phase 1: Create shared components and fix cycodgr line numbers
ae3c87a8 Refactor help documentation and command options for cycodgr
31190558 feat: Add language shortcuts (--cs, --py, --js, etc.)
f88b0794 feat: Enhance search functionality and documentation - support phrase search and update sorting options
74352fae fix: Align release workflow with CI - add TRX cleanup and remove unnecessary fetch-depth
9a9d78de fix: Add cycodgr build artifacts upload to CI workflow
a736b1eb fix: Add cycodgr to solution file for CI builds
3f1f2199 feat: Add cycodgr to NuGet packaging and CI/CD pipeline
13cbbd10 feat: Complete cycodgr (GitHub search CLI) - Phases 1-5 implemented
a2068bec Add rich metadata output to cycodgh with multiple formats
2dd9fe25 Add end-to-end demonstration results for cycodgh
8c03ee89 Add implementation summary for cycodgh
aa89d3f1 Implement cycodgh - GitHub repository search and clone CLI
aae702f6 Add cycodgh project README - GitHub search CLI design doc
a91af1ec Resolve merge conflicts after git pull
```

---

## Decision for Each Commit

### ❌ SKIP - a91af1ec "Resolve merge conflicts after git pull"
**Reason:** This merge commit brought in unwanted files:
- ✅ AGENTS.md changes (good)
- ❌ All docs/school-of-thought/* files (bad - 9 files)
- ❌ docs/code-craftsmanship-principles.md (uncertain)
- ❌ src/cycod/ChatClient/ChatClientFactory.cs (uncertain)

**Action:** Skip entirely. The AGENTS.md changes are minor and can be recreated if needed.

---

### ⚠️ CHERRY-PICK WITH CLEANUP - 5a3bbfcf "Add Phase D Implementation Plan and Repository Filtering Workflow"
**Reason:** This commit is MIXED - contains both good and bad:

**Good stuff (cycodgr Phase D implementation):**
- ✅ src/cycodgr/CommandLine/CycoGrCommand.cs
- ✅ src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs
- ✅ src/cycodgr/CommandLineCommands/SearchCommand.cs
- ✅ src/cycodgr/Helpers/GitHubSearchHelpers.cs
- ✅ src/cycodgr/Program.cs
- ✅ todo/phase-a-complete.md
- ✅ todo/phase-a-manual-tests.md
- ✅ todo/phase-a-test-results.md
- ✅ todo/phase-b-complete.md
- ✅ todo/phase-c-analysis.md
- ✅ todo/phase-c-complete.md
- ✅ todo/phase-d-analysis.md
- ✅ todo/repo-filtering-and-save-workflow.md
- ✅ todo/unified-processing-architecture.md (modified)

**Bad stuff (unrelated rabbit hole):**
- ❌ todo/hacks-of-a-lifetime/* (18 files)
- ❌ todo/killer-use-case-api-learning.md

**Action:** Cherry-pick, then immediately remove bad files and amend:
```bash
git cherry-pick 5a3bbfcf
git rm -r todo/hacks-of-a-lifetime
git rm todo/killer-use-case-api-learning.md
git commit --amend --no-edit
```

---

### ✅ CHERRY-PICK - All other commits (20 commits)

These are all clean cycodgr-related commits:

```
✅ e0809073 Phase E: Smart behaviors + bugfix for --line-contains
✅ 12015afc Add comprehensive tests for LineHelpers and ThrottledProcessor
✅ 318b9939 docs: update architecture doc with completion status
✅ 2a85fec5 feat: add final/global instructions support to cycodgr
✅ 7ec731e2 feat: add repo instructions support to cycodgr
✅ 7618ae2b feat: add file instructions support to cycodgr
✅ d708c01c feat: add parallel file processing to cycodgr
✅ 3fe41109 Phase 1: Create shared components and fix cycodgr line numbers
✅ ae3c87a8 Refactor help documentation and command options for cycodgr
✅ 31190558 feat: Add language shortcuts (--cs, --py, --js, etc.)
✅ f88b0794 feat: Enhance search functionality and documentation - support phrase search and update sorting options
✅ 74352fae fix: Align release workflow with CI - add TRX cleanup and remove unnecessary fetch-depth
✅ 9a9d78de fix: Add cycodgr build artifacts upload to CI workflow
✅ a736b1eb fix: Add cycodgr to solution file for CI builds
✅ 3f1f2199 feat: Add cycodgr to NuGet packaging and CI/CD pipeline
✅ 13cbbd10 feat: Complete cycodgr (GitHub search CLI) - Phases 1-5 implemented
✅ a2068bec Add rich metadata output to cycodgh with multiple formats
✅ 2dd9fe25 Add end-to-end demonstration results for cycodgh
✅ 8c03ee89 Add implementation summary for cycodgh
✅ aa89d3f1 Implement cycodgh - GitHub repository search and clone CLI
✅ aae702f6 Add cycodgh project README - GitHub search CLI design doc
```

---

## Summary

| Decision | Count | Commits |
|----------|-------|---------|
| ✅ Cherry-pick (clean) | 20 | All except a91af1ec and 5a3bbfcf |
| ⚠️ Cherry-pick + cleanup | 1 | 5a3bbfcf (remove hacks-of-a-lifetime) |
| ❌ Skip entirely | 1 | a91af1ec (merge conflict) |

---

## Step-by-Step Cherry-Pick Commands

### Phase 1: Setup
```bash
# From main repo (not the current worktree)
cd /c/src/cycod

# Create new worktree with clean branch
git worktree add ../cycod-cycodgr-clean -b robch/2512-dec11-cycodgr-clean origin/master

# Move to new worktree
cd ../cycod-cycodgr-clean
```

### Phase 2: Cherry-pick commits (oldest to newest, skipping bad ones)
```bash
# Skip a91af1ec (merge conflicts - brings school-of-thought)

# Cherry-pick the initial cycodgr commits
git cherry-pick aae702f6  # Add cycodgh project README
git cherry-pick aa89d3f1  # Implement cycodgh
git cherry-pick 8c03ee89  # Add implementation summary
git cherry-pick 2dd9fe25  # Add end-to-end demonstration
git cherry-pick a2068bec  # Add rich metadata output
git cherry-pick 13cbbd10  # Complete cycodgr Phases 1-5
git cherry-pick 3f1f2199  # Add to NuGet packaging
git cherry-pick a736b1eb  # Add to solution file
git cherry-pick 9a9d78de  # Add build artifacts upload
git cherry-pick 74352fae  # Align release workflow
git cherry-pick f88b0794  # Enhance search functionality
git cherry-pick 31190558  # Add language shortcuts
git cherry-pick ae3c87a8  # Refactor help documentation
git cherry-pick 3fe41109  # Create shared components
git cherry-pick d708c01c  # Add parallel file processing
git cherry-pick 7618ae2b  # Add file instructions support
git cherry-pick 7ec731e2  # Add repo instructions support
git cherry-pick 2a85fec5  # Add final/global instructions
git cherry-pick 318b9939  # Update architecture doc
git cherry-pick 12015afc  # Add comprehensive tests

# Cherry-pick 5a3bbfcf WITH CLEANUP
git cherry-pick 5a3bbfcf
git rm -r todo/hacks-of-a-lifetime
git rm todo/killer-use-case-api-learning.md
git commit --amend --no-edit

# Cherry-pick final commit
git cherry-pick e0809073  # Phase E + bugfix
```

---

## Alternative: Cherry-pick in one go (with range)

**NOT RECOMMENDED** because we need to skip a91af1ec and clean up 5a3bbfcf, but here's the concept:

```bash
# This would pick ALL commits including bad ones - DON'T USE
git cherry-pick aae702f6..e0809073
```

---

## Verification After Cherry-Picking

```bash
# Check what files are in the new branch
git diff --name-status origin/master

# Should NOT see:
# - docs/school-of-thought/*
# - todo/hacks-of-a-lifetime/*
# - todo/killer-use-case-api-learning.md
# - chat-history-*.jsonl files

# Should see:
# - src/cycodgr/* (all cycodgr source)
# - src/common/* (shared components)
# - tests/* (new tests)
# - todo/phase-*.md (phase docs)
# - todo/cycodgr-*.md (planning docs)
```

---

## Notes

1. **Chat history files** don't appear in any commits - they're only in your local unstaged files, so they won't come through cherry-picking.

2. **AGENTS.md changes** from a91af1ec are minor - if needed, can be manually applied later.

3. **src/cycod/ChatClient/ChatClientFactory.cs** from a91af1ec - if this turns out to be needed, can cherry-pick just that file change separately.

4. **Order matters** - cherry-picking oldest-to-newest avoids most conflicts.

5. **If conflicts occur** during cherry-pick:
   ```bash
   # Resolve conflicts manually, then:
   git add <resolved-files>
   git cherry-pick --continue
   ```

---

## Expected Outcome

A clean branch with:
- ✅ All cycodgr source code and features (Phases A-E)
- ✅ All supporting infrastructure (CI/CD, solution updates, etc.)
- ✅ All planning and phase documentation
- ✅ Shared components (LineHelpers, ThrottledProcessor, etc.)
- ✅ Tests for new components
- ❌ No school-of-thought docs
- ❌ No hacks-of-a-lifetime / smart-pty project
- ❌ No chat history files
- ❌ No killer use case docs

**Total commits in new branch:** 21 (down from 22)
**Files removed:** ~30 unwanted files
**Clean PR ready for review!** ✅
