# cycodgr Redesign - Implementation Plan

**No backward compatibility needed - clean rip & replace!**

## Phase 1: Language Shortcuts (Optional - 30 min)

- [ ] 1.1: Add 18 language shortcut parsing (--cs, --py, --js, etc.)
- [ ] 1.2: Test with current commands
- [ ] 1.3: Update help files
- [ ] Commit: "feat: Add language shortcuts"

## Phase 2: Delete Old, Create New (45 min)

- [ ] 2.1: DELETE RepoCommand.cs, CodeCommand.cs, parsing methods
- [ ] 2.2: CREATE SearchCommand.cs (RepoPatterns, Contains, FileContains, RepoContains)
- [ ] 2.3: IMPLEMENT parsing (positional repos, content flags, shortcuts)
- [ ] 2.4: MAKE SearchCommand the default
- [ ] Commit: "feat!: BREAKING - SearchCommand replaces repo/code verbs"

## Phase 3: Search Execution (60 min)

- [ ] 3.1: Decision logic (which searches to run)
- [ ] 3.2: Metadata lookup helper (positional repos only)
- [ ] 3.3: Wire up helpers
- [ ] 3.4: Execute based on flags
- [ ] Commit: "feat: Search execution logic"

## Phase 4: Formatting (45 min)

- [ ] 4.1: Repo metadata display
- [ ] 4.2: Mixed results (--contains)
- [ ] 4.3: Route in Program.cs
- [ ] Commit: "feat: Formatting"

## Phase 5: Documentation (45 min)

- [ ] 5.1: Rewrite help files
- [ ] 5.2: Update README
- [ ] Commit: "docs: New syntax"

## Phase 6: Testing (45 min)

- [ ] 6.1: Manual testing
- [ ] 6.2: cycodt tests
- [ ] 6.3: AMA verification
- [ ] Commit: "test: Comprehensive tests"

**Total: 3-4 hours**
