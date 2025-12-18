# Files in robch/2512-dec11-cycodgr-github-search That Shouldn't Be There

## Analysis Date: 2025-12-16

Comparing `origin/robch/2512-dec11-cycodgr-github-search` with `origin/master` to identify files that shouldn't be part of the cycodgr PR.

---

## ❌ DEFINITELY SHOULD NOT BE IN PR

### Category 1: Chat History Files (2 files)
**Why problematic:** These are session recordings, not source code. They contain conversation data that shouldn't be committed to the repository.

```
A	chat-history-check-to-see-if-it-wants-replace-many-in-file.jsonl
A	chat-history-github-search-cli-attempt1.jsonl
```

**Recommendation:** Remove from branch history or create new clean branch without them.

---

### Category 2: "School of Thought" Documentation (9 files)
**Why problematic:** This appears to be general AI/cognitive bias documentation unrelated to cycodgr functionality. Came in via merge conflict resolution (commit a91af1ec).

```
A	docs/school-of-thought/README-school-of-thought.md
A	docs/school-of-thought/ai-agent-system-prompt-bias-awareness.md
A	docs/school-of-thought/cognitive_biases.pdf
A	docs/school-of-thought/creative_thinking_cards.pdf
A	docs/school-of-thought/critical_thinking_cards.pdf
A	docs/school-of-thought/logical_fallacies_a1.pdf
A	docs/school-of-thought/logical_fallacies_a4.pdf
A	docs/school-of-thought/quick-reference-mid-information-detection.md
A	docs/school-of-thought/school-of-thought-analysis.md
```

**Recommendation:** These should be in a separate branch/PR about documentation or AI system prompts.

---

### Category 3: "Hacks of a Lifetime" / Smart PTY Project (14 files)
**Why problematic:** This is the "killer use case" / smart-pty-code-fence-renderer project that you went down the rabbit hole on. It's a separate project/feature, not part of cycodgr's core functionality. Added in commit 5a3bbfcf.

```
A	todo/hacks-of-a-lifetime/README.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/README.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/docs/tree-sitter-fingerprints.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/001-project-inception.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/002-understanding-2shell.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/003-the-cpp-decision.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/004-the-search-methodology.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/005-search-session-001-plan.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/006-search-session-001-reflection.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/007-search-session-001-final-reflection.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/008-search-session-002-study-plan.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/journal/009-memento-strategy.md
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/search-001-broad.txt
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/search-001-cpp.txt
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/search-001-highlighters.txt
A	todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/treesitter-users.txt
A	todo/killer-use-case-api-learning.md
```

**Recommendation:** Move to separate branch for the killer use case / smart PTY project work.

---

## ⚠️ MAYBE SHOULDN'T BE IN PR (Need Review)

### Category 4: General Documentation Changes
**Why questionable:** These might be unrelated to cycodgr.

```
M	docs/code-craftsmanship-principles.md
```

**Need to check:** Was this modified as part of cycodgr work, or is it a separate documentation update?

---

### Category 5: Cycod Core Changes
**Why questionable:** These are in cycod (not cycodgr), might be unrelated changes that snuck in.

```
M	src/cycod/ChatClient/ChatClientFactory.cs
M	src/cycod/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs
```

**Need to check:** 
- Are these changes required for cycodgr functionality?
- Or are these separate bug fixes/features that should be in their own PR?

---

## ✅ DEFINITELY SHOULD BE IN PR

### Category 6: Core cycodgr Implementation
All new cycodgr source code and supporting infrastructure:

```
A	src/cycodgr/** (all files)
A	src/common/** (shared components extracted from cycodmd)
A	tests/cycod/FoundTextFileTests.cs
A	tests/cycod/Helpers/LineHelpersTests.cs
A	tests/cycod/ThrottledProcessorTests.cs
M	src/cycodmd/** (refactoring to use shared components)
M	cycod.sln (added cycodgr project)
M	.github/workflows/ci.yml (CI for cycodgr)
M	.github/workflows/release.yml (release for cycodgr)
M	.gitignore (ignore patterns)
M	AGENTS.md (project documentation)
M	scripts/_functions.sh (build scripts)
```

### Category 7: cycodgr Planning/Documentation
Implementation plans and phase documentation for cycodgr:

```
A	todo/ai-instructions-pipeline.md
A	todo/cycodgr-implementation-plan.md
A	todo/cycodgr-redesign-implementation-plan.md
A	todo/cycodgr-redesign-spec.md
A	todo/phase-a-complete.md
A	todo/phase-a-manual-tests.md
A	todo/phase-a-test-results.md
A	todo/phase-b-complete.md
A	todo/phase-c-analysis.md
A	todo/phase-c-complete.md
A	todo/phase-d-analysis.md
A	todo/repo-filtering-and-save-workflow.md
A	todo/unified-processing-architecture.md
```

---

## Summary by Category

| Category | Count | Should Be in PR? | Action |
|----------|-------|------------------|--------|
| Chat History Files | 2 | ❌ NO | Remove/exclude |
| School of Thought Docs | 9 | ❌ NO | Move to separate PR |
| Hacks of a Lifetime / Smart PTY | 18 | ❌ NO | Move to separate branch |
| General Doc Changes | 1 | ⚠️ MAYBE | Review if related |
| Cycod Core Changes | 2 | ⚠️ MAYBE | Review if required |
| cycodgr Implementation | ~60 | ✅ YES | Keep |
| cycodgr Planning Docs | 11 | ✅ YES | Keep |

---

## Files That Shouldn't Be There (Total: 30 files)

**Definite removals (29 files):**
- 2 chat history files
- 9 school-of-thought docs
- 18 hacks-of-a-lifetime / smart-pty files

**Need review (2 files):**
- docs/code-craftsmanship-principles.md
- src/cycod/ChatClient/ChatClientFactory.cs
- src/cycod/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs

---

## Recommended Actions

### Option 1: Clean Rebase (Most Thorough)
Create a new branch from master with only cycodgr commits, excluding the problematic ones.

### Option 2: Revert Specific Commits
Identify and revert the merge commit (a91af1ec) and the hacks-of-a-lifetime commit (5a3bbfcf).

### Option 3: Cherry-Pick Good Commits
Create new branch and cherry-pick only the cycodgr-specific commits.

### Option 4: Interactive Rebase
Use `git rebase -i` to edit/drop problematic commits.

---

## Notes

1. The "school of thought" files came in via merge conflict resolution (commit a91af1ec "Resolve merge conflicts after git pull")
2. The "hacks of a lifetime" files were added in commit 5a3bbfcf "Add Phase D Implementation Plan and Repository Filtering Workflow"
3. Chat history files appear in the diff but aren't in any commit - they might be unstaged/untracked locally but showing up in comparison

---

## Next Steps Discussion Needed

1. **Should we keep planning docs in the PR?** The todo/phase-*.md and todo/*-plan.md files are helpful context but make the PR larger. Consider moving to separate documentation PR or keeping in master as living docs.

2. **Are cycod core changes required?** Need to verify if ChatClientFactory.cs and StrReplaceEditorHelperFunctions.cs changes are dependencies for cycodgr or separate fixes.

3. **What's the best cleanup strategy?** Depends on:
   - How clean you want the commit history
   - Whether you want to preserve commit messages
   - How much effort you want to spend on cleanup vs. moving forward
