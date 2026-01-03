# TODO: cycodj - Branch Context (Why Did This Branch?)

## The Pain 😫

**Current State:**
The `branches` command shows me the tree structure beautifully:

```
📁 08:27 - Chat History Journal Tool
  ├─ 08:56 - Chat History Journal Tool
  ├─ 09:10 - Chat History Journal Tool
  ├─ 09:22 - Chat History Journal Tool
  └─ 09:34 - Chat History Journal Tool
```

**But I can't tell WHY they branched!**

All I see is:
- Timestamp
- Title (often same for branches)
- That it branched

**The Problems:**
- **Branches look identical:** "Chat History Journal Tool" repeated 5 times - which is which?
- **No context:** Why did I branch at 08:56? What was I trying differently?
- **Have to investigate:** Must open each conversation to understand
- **Pattern invisible:** Can't see "Phase 0 → Phase 1 → Phase 2" progression
- **Story lost:** The branching tells a story but I can't read it

**Real-world frustration:**
Today analyzing your weekend, I saw:
```
📁 09:45 - CDR Project Restart Instructions
  ├─ 09:46 - CDR Project Restart Instructions (iteration 1)
    ├─ 09:58 - CDR Project Restart Instructions (iteration 2)
      ├─ 10:01 - CDR Project Restart Instructions (iteration 3)
```

12 branches deep! But WHY? What changed each time?

I had to:
1. Note the conversation IDs
2. Use `show` on each one
3. Read the first user message
4. Manually reconstruct the story

**It took 15 minutes to understand a tree that should explain itself.**

---

## The Cure 💊

**What I Want:**
Show me WHY each branch exists - give me context:

```bash
cycodj branches --date 2025-12-20 --with-context
```

**Output:**
```
📁 08:27 - Chat History Journal Tool (73 msgs)
   > "can you make a new worktree for cycodj..."
   
  ├─ 08:56 - Phase 0: Setup (79 msgs)
  │  > "research cycodgr first..."
  │  Branched: Added research phase
  │
  ├─ 09:10 - Phase 1: Implementation (161 msgs)
  │  > "now implement the list command..."
  │  Branched: Starting implementation
  │
  ├─ 09:22 - Phase 2: Search Feature (152 msgs)
  │  > "add search capability..."
  │  Branched: New feature branch
  │
  └─ 09:34 - Phase 3: Testing (131 msgs)
     > "let's test what we built..."
     Branched: Moving to testing phase
```

**What Changed:**
- First user message shown (truncated)
- Can instantly see what each branch is about
- Branch reason/purpose visible
- Story readable without opening conversations

---

## User Stories

### Story 1: Understand Decision Points
**As a user,** I want to see why I branched a conversation  
**So that** I can understand my decision-making process  
**Currently:** Branch tree shows THAT I branched but not WHY  
**Desired:** Branch tree shows first message explaining the branch purpose

### Story 2: Quick Pattern Recognition
**As a user,** I want to scan branch contexts without opening each conversation  
**So that** I can quickly understand what happened  
**Currently:** Must `show` each conversation ID individually (slow)  
**Desired:** Branch context visible inline in tree (fast)

### Story 3: Debugging Workflow
**As a user,** I want to see the evolution of problem-solving  
**So that** I can learn from what worked/didn't work  
**Currently:** 10 branches that all say "Fix Bug" - can't tell them apart  
**Desired:** Can see "Try approach A" → "That failed, try B" → "B works!"

### Story 4: Documentation
**As a user,** I want to document a project's evolution  
**So that** others can understand the journey  
**Currently:** Have to manually piece together the story  
**Desired:** Export branch tree with context = instant timeline

---

## Success Criteria

**This is solved when:**

1. ✅ `branches` command shows first user message for each conversation (truncated)
2. ✅ Can see why each branch exists without opening it
3. ✅ Branch tree is readable as a narrative
4. ✅ Can optionally show more context (not just first message)
5. ✅ Works with `--verbose` to show even more detail

**Bonus points if:**
- Shows branch "type" (exploration, bug fix, iteration, feature)
- Highlights branches that succeeded vs. failed
- Shows how many messages before branching (context depth)
- Can filter to show only certain types of branches

---

## What Makes a Good Context?

**Minimum (Default):**
- First user message (truncated to ~80 chars)
- Enough to understand intent

**Better (`--verbose`):**
- First user message (full text)
- Message count
- Duration
- Whether branch led to more branches

**Best (`--detailed`):**
- First user message
- Last message (outcome)
- Key decisions made
- Links to artifacts created

---

## The Value

**Understanding:**
- Current: 15 minutes to understand a complex branch tree
- Future: 30 seconds to scan and understand
- **30x faster comprehension**

**Documentation:**
- Branch tree with context = instant project timeline
- Can export to markdown = shareable story
- Others can understand your workflow

**Learning:**
- See what approaches worked vs. failed
- Understand iteration patterns
- Improve future workflows

---

## Example Usage (Dream State)

```bash
# Basic: Show first message for context
cycodj branches --date 2025-12-20 --with-context

# Verbose: Show more details
cycodj branches --date 2025-12-21 --verbose --with-context

# Filter: Only show branches that led to more branches (complex problems)
cycodj branches --date 2025-12-14 --deep-only

# Export: Create a markdown timeline with context
cycodj export -o timeline.md --date 2025-12-20 --branch-context

# Search: Find branches about specific topic
cycodj branches --contains "cycodgr" --with-context
```

---

## Real Example

**What I saw today:**
```
📁 06:45 - Implement Cycodgr AI Task (359 messages)
  ├─ 07:03 - Implement Cycodgr AI Task (440 messages)
```

**What I wanted to see:**
```
📁 06:45 - Implement Cycodgr AI Task (359 msgs)
   > "Read todo/implement-cycodgr-ai.md and begin..."
   Initial implementation attempt
   
  ├─ 07:03 - Implement Cycodgr AI Task (440 msgs)
     > "trainer of tigers... let's try different approach..."
     Branched: Previous approach hit limits, trying new strategy
     Result: Led to successful implementation ✅
```

**The difference:**
- Before: Mystery branch
- After: Clear story

---

## Why This Matters

**Branches are gold.** They show:
- Decision points
- Iteration patterns  
- Problem-solving approaches
- What worked vs. didn't

**But only if I can see what they're about!**

Without context, branches are just cryptic tree structure.  
With context, branches tell the story of how work evolved.

**Make the invisible visible.** 🔍
