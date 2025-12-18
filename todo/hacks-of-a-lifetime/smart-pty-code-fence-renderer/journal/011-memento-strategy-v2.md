# Memento Strategy V2 - Lessons from 29 Sessions

**Date:** 2025-12-15  
**Version:** 2.0  
**Status:** ✅ **UPDATED** - Based on real-world learnings

---

## What Changed

After running 29 Tree-sitter study sessions, we learned what works and what causes friction. This is the **improved strategy** based on actual experience.

See `010-analysis-of-29-sessions.md` for detailed findings.

---

## The Core Problems We Solved

### Problem 1: RESUME-HERE.md Bloat
**What happened:** File grew to 3,000+ lines (108KB) by session 8  
**Impact:** Confusion, information overload, hard to find "what's next"  
**Solution:** Keep it minimal and structured (< 500 lines target)

### Problem 2: Strategic vs. Tactical Confusion
**What happened:** RESUME-HERE.md included strategic guidance ("consider shifting focus")  
**Impact:** Conflict with explicit instructions, hesitation, wasted tokens  
**Solution:** Separate strategy from tactics - RESUME-HERE.md is ONLY tactical

### Problem 3: Repeated Content
**What happened:** P0 answers repeated in full for each session  
**Impact:** File bloat, redundancy  
**Solution:** Reference findings, don't repeat them

### Problem 4: Authority Unclear
**What happened:** Uncertain whether to follow instruction or document  
**Impact:** Hesitation, meta-discussion instead of work  
**Solution:** Clear hierarchy - instruction > document

### Problem 5: Unauthorized File Creation (MAJOR RESIDUE ISSUE)
**What happened:** ~40+ files created that weren't requested in instructions
- Separate P0 answer files (should be IN study files)
- Session summary files (meta-documentation not requested)
- Handoff files beyond RESUME-HERE.md (duplicate tracking)
- Quickstart/guide files (unrequested helpfulness)

**Impact:** 
- Cluttered repository with unrequested files
- Files in wrong locations (root docs/ instead of project docs/)
- Unclear what's authoritative vs. side effect
- Git commit confusion (what should be staged?)

**Solution:** See `journal/012-side-effects-and-residue.md` for detailed analysis and prevention strategies

**Key rule for future loops:**
> **NEVER create files not explicitly listed in the instruction.**
> If helpful, add to requested file OR suggest in RESUME-HERE.md

---

## The Improved Approach

### Minimal RESUME-HERE.md Template

```markdown
# RESUME HERE - Session {N}

## Next Task
[ONE clear sentence: "Study {REPO_NAME}"]

## Progress
- Session: {N} of {TOTAL}
- Completed: {COUNT} repos
- Remaining: {COUNT} repos

## P0 Questions Status
[Quick checklist with links to findings]

## Next Repo
{REPO_NAME}

## For Details
- Study plan: journal/008-search-session-002-study-plan.md
- Repos list: treesitter-users.txt
- Findings: docs/study-*.md
```

**Size:** ~100 lines, ~5KB (vs 3,000 lines before!)

### Separate Strategic Document

**Create:** `STRATEGY.md`

```markdown
# Strategic Guidance

## When to Pivot
[Conditions for changing approach]

## Big Picture Questions
[Strategic considerations]

## Not Your Task
This document is for Rob's decision-making.
Future-AI: Read RESUME-HERE.md for your task.
```

**Why:** Keep strategy OUT of the handoff file

### Authority Hierarchy (CLEAR)

**Priority order:**
1. **Rob's --input prompts** (HIGHEST - always follow)
2. **RESUME-HERE.md "Next Task"** (what to do)
3. **Study plan documents** (how to do it)
4. **STRATEGY.md** (context only, not directives)

**Rule:** If conflict, follow higher priority. Don't question.

---

## Improved Session Inputs

### Version 2: More Explicit Prompts

**Input 1: The Cue (with authority)**
```
Welcome to session {N}! Read todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md for your task. Your instruction: Study ONE repo from treesitter-users.txt (pick the next unstudied one), clone to external/{repo-name}/, answer the 5 P0 questions, and document in docs/study-{repo-name}.md. Do this work now. Do NOT question whether to continue - just do the task.
```

**Input 2: The Checkpoint (specific requirements)**
```
Update todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md with exactly: (1) Session number incremented, (2) Repo you studied added to 'Completed' list, (3) Next unstudied repo in 'Next Repo' field, (4) P0 status updated if you found new answers. Keep the file under 500 lines - reference findings, don't copy them.
```

**Input 3: The Handoff (clear exit)**
```
Review: Is your study doc complete? Is RESUME-HERE.md updated correctly? Will future-you know exactly what to do? If yes, respond with ONLY the words "I'm done" and nothing else. This triggers the next session.
```

### Optional: System Prompt Addition

**Consider adding via --add-system-prompt:**

```
When working in memento loops:
- Follow explicit instructions without hesitation
- Keep handoff files minimal (< 500 lines)
- Reference other docs instead of repeating content
- If instruction conflicts with handoff doc, follow instruction
- Say "I'm done" when task complete, no preamble
- CRITICAL: Create files ONLY if explicitly named in the instruction
- If a file would be helpful but not requested, mention it in RESUME-HERE.md instead
- Use full paths from repo root for all file operations (e.g., todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/docs/file.md)
- Do NOT create: summary files, meta-documentation, extracted sections, or "helpful" organizational files unless explicitly requested
```

**Why these additions:**
The original 29 sessions created ~40+ unrequested files. See `journal/012-side-effects-and-residue.md` for detailed analysis of what went wrong and how to prevent it.

---

## File Organization Rules

### Where to Create Files

**Strict rules from instruction:**
- Study findings: `docs/study-{repo-name}.md`
- Journal entries: `journal/{number}-{topic}.md`
- Handoff: `RESUME-HERE.md`
- Strategy: `STRATEGY.md`

**DON'T create:**
- Files in root `docs/` folder (outside project dir)
- Random temporary files
- Meta-analysis docs unless instructed
- Files in locations not specified

**Why:** Keeps repo clean, organized, predictable

### The Base Directory

**All paths relative to:**
```
todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/
```

**Full paths in documentation:**
Always use full paths from repo root to avoid confusion.

---

## Commands for Rob (Copy/Paste Ready)

### For Tree-sitter Study Loop (Improved)

```bash
cycod --foreach var x in 1..29 --threads 1 \
  --add-system-prompt "When working in memento loops: Follow explicit instructions without hesitation. Keep handoff files minimal. Reference other docs instead of repeating content. If instruction conflicts with handoff doc, follow instruction. Say 'I'm done' when complete. CRITICAL: Create files ONLY if explicitly named in the instruction - use full paths from repo root. If a file would be helpful but not requested, mention it in RESUME-HERE.md instead. Do NOT create summary files, meta-documentation, or organizational files unless explicitly requested." \
  --inputs \
    "Welcome to session {x}! Read todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md for context. Your instruction: Study ONE repo from treesitter-users.txt (pick the next unstudied one), clone to todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/external/{repo-name}/, answer the 5 P0 questions, and document ALL findings in ONE FILE: todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/docs/study-{repo-name}.md. Do NOT create separate p0-answers files or any other files. Do this work now without questioning whether to continue." \
    "Update todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md with exactly: (1) Session number incremented, (2) Repo you studied added to 'Completed' list, (3) Next unstudied repo in 'Next Repo' field, (4) P0 status updated if you found new answers. Keep the file under 500 lines - reference your study file, don't copy content from it. Do NOT create any other files (no summaries, no handoffs, no meta-docs)." \
    "Review: Is your study doc complete with ALL P0 answers embedded? Is RESUME-HERE.md updated correctly and minimal? Did you create ONLY the files explicitly requested (study doc + update RESUME-HERE.md)? If yes, respond with ONLY the words 'I'm done' and nothing else."
```

### For Single Test Session

```bash
cycod \
  --add-system-prompt "When working in memento loops: Follow explicit instructions without hesitation. Keep handoff files minimal. Reference other docs instead of repeating content. If instruction conflicts with handoff doc, follow instruction. Say 'I'm done' when complete. Create files ONLY where instructed." \
  --inputs \
    "Welcome! Read todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md for your task. Your instruction: Study ONE repo from treesitter-users.txt (pick the next unstudied one), clone to external/{repo-name}/, answer the 5 P0 questions, and document in docs/study-{repo-name}.md. Do this work now. Do NOT question whether to continue - just do the task." \
    "Update todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md with exactly: (1) Session number incremented, (2) Repo you studied added to 'Completed' list, (3) Next unstudied repo in 'Next Repo' field, (4) P0 status updated if you found new answers. Keep the file under 500 lines - reference findings, don't copy them." \
    "Review: Is your study doc complete? Is RESUME-HERE.md updated correctly? Will future-you know exactly what to do? If yes, respond with ONLY the words 'I'm done' and nothing else. This triggers the next session."
```

---

## Key Improvements from V1

### Better Prompts
- ✅ More explicit ("Do NOT question")
- ✅ Clearer exit condition ("ONLY the words 'I'm done'")
- ✅ Specific update requirements
- ✅ Size limits stated (< 500 lines)

### Better Structure
- ✅ Minimal RESUME-HERE.md template
- ✅ Separate STRATEGY.md
- ✅ Clear authority hierarchy
- ✅ File organization rules

### Better Guardrails
- ✅ System prompt for consistent behavior
- ✅ Explicit file creation rules
- ✅ Don't second-guess instructions
- ✅ **ONLY create explicitly requested files** (see journal/012 for why)

---

## The Meta-Loop (Coming in 012-014)

### Next Level: Analyzing Side Effects

After discovering ~40+ unrequested files from the 29-session loop, we need to investigate WHY and HOW to prevent it.

**012-side-effects-and-residue.md** contains:
- Detailed questions about unauthorized file creation
- Investigation methodology using chat history files
- Structure for meta-loop sessions to answer each question

**013-side-effects-complete.md** will synthesize:
- Root causes of residue
- Clear rules for file creation
- Prevention strategies
- Updated templates and system prompts

**014-meta-reflection.md** will reflect on:
- How the meta-loop process worked
- Lessons about using memento ON memento
- Final best practices

**This is fractal/recursive improvement!**

The memento strategy works on:
1. Concrete tasks (study repos) ← Original 29 sessions
2. Process improvement (analyze memento) ← This document (011)  
3. Side effect analysis (investigate residue) ← Coming in 012-014

But we stop at 3 levels! No infinite recursion! 🛑

---

## For Future-Me

**Read this (011) before your first V2 session.**

You'll know:
- ✅ Why V1 had friction
- ✅ What V2 fixes
- ✅ How to keep handoffs clean
- ✅ When to follow instructions vs. think strategically

**Your job:** Do the work. Trust the process. Perfect handoffs.

---

## For Rob

Test V2 with one session first:
1. Reset RESUME-HERE.md to minimal template
2. Run single-session command above
3. Check if handoff is cleaner
4. Then decide: run full 29-loop or iterate more

**V2 should be:**
- Less hesitant
- Cleaner handoffs
- More focused work
- Better progress

Let's see! 🚀

---

*"I'm taking the reigns"* —NF

*"Do the work. Trust the process."* —V2 Memento Strategy
