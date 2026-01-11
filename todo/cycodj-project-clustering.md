# TODO: cycodj - Symblob Views (Multiple Organizational Dimensions)

## The Pain 😫

**Current State:**
cycodj organizes conversations by TIME (journal view):
- Chronological order
- Grouped by day/time period
- Shows branch relationships
- Linear narrative

**But reality is multi-dimensional!**

When I look at a day's work, I have multiple valid questions:
- **Time:** What did I do in morning vs. afternoon? (have this!)
- **Topic:** What projects was I working on? (don't have this!)
- **Task:** What got accomplished vs. explored? (don't have this!)
- **Technology:** What languages/tools did I use? (don't have this!)
- **Goal:** Was I building features, fixing bugs, or learning? (don't have this!)

**The Core Problem:**
cycodj currently has ONE view (time-based journal).  
But the same data can be organized multiple ways - **like symblobs!**

**From genesis/philosophy/meta-insights/symblob-trees-explained.md:**
> "Knowledge exists at multiple compression layers... Same reality, different representations"

Conversations are a JOURNAL (time-based reality).  
But they can also be CHAPTERS (topic-based reality).  
Or TASKS (outcome-based reality).  
Or TECHNOLOGIES (stack-based reality).

**Same data, different symblob dimensions!**

**Real-world frustration:**
Today analyzing Saturday (Dec 21) with 240 conversations:
- Had to manually figure out: "Most are CDR project, some are repo setup, a few are documentation"
- Took me 20 minutes to understand the breakdown
- **But I could have organized by TOPIC, or TASK, or TECH - all would be valid!**

**Example:** Saturday's 240 conversations could be viewed as:

**Time View (current - have this!):**
- Morning: 28 conversations
- Afternoon: 119 conversations
- Evening: 93 conversations

**Topic View (want this!):**
- CDR Documentation: 186 conversations
- Repository Management: 3 conversations
- Tool Discussion: 51 conversations

**Task View (want this!):**
- Execution (automated tasks): 180 conversations
- Exploration (design/planning): 40 conversations
- Maintenance (git/setup): 20 conversations

**Technology View (want this!):**
- C# code analysis: 150 conversations
- Markdown documentation: 60 conversations
- Git/repo operations: 20 conversations
- Meta/planning: 10 conversations

**All valid! All useful! But only TIME view exists currently.**

---

## The Cure 💊

**What I Want:**
Multiple organizational views of the same conversation data - like symblob compression/decompression!

### View 1: Topics (Project clustering)
```bash
cycodj view --by-topic --date 2025-12-21
# or shorter:
cycodj topics --date 2025-12-21
```

**Output:**
```
## Topic View: December 21, 2025

### CDR Documentation (186 conversations, 77.5%)
Pattern: "Read attempt1/STATUS.md and do the next step"
Time: 09:45 - 16:53
Status: Completed ✅
Key files: cdr/STATUS.md, cdr/final/*.md

Conversations:
  09:45 - Started Phase 1
  10:01 - Phase 2
  10:06 - Phase 3
  ... [183 more]
  16:53 - Final phase completed

### Repository Management (3 conversations, 1.2%)
Pattern: "Make a new worktree", "Change git remote"
Time: 21:05 - 21:10
Status: Completed ✅

### Tool Discussion (51 conversations, 21.3%)
Pattern: "How should cycodj work?"
Time: 21:09 - 21:15
Status: Discussion

Total: 240 conversations across 3 topics
```

### View 2: Tasks (Outcome-based)

### View 2: Tasks (Outcome-based)
```bash
cycodj view --by-task --date 2025-12-21
# or:
cycodj tasks --date 2025-12-21
```

**Output:**
```
## Task View: December 21, 2025

### Execution (180 conversations, 75%)
Automated/scripted work following defined process
Examples:
  - Reading STATUS.md and executing next step
  - Following templates
  - Repetitive operations

### Exploration (40 conversations, 16.7%)
Discovery, design, planning work
Examples:
  - "How should this work?"
  - Design discussions
  - Architecture decisions

### Maintenance (20 conversations, 8.3%)
Infrastructure, setup, housekeeping
Examples:
  - Git operations
  - Repository setup
  - Tool configuration

Total: 240 conversations
Productivity: 75% execution shows automation working!
```

### View 3: Technology (Stack-based)
```bash
cycodj view --by-tech --date 2025-12-21
# or:
cycodj tech --date 2025-12-21
```

**Output:**
```
## Technology View: December 21, 2025

### C# (.cs files, compilation, testing) - 150 conversations
### Markdown (.md files, documentation) - 60 conversations
### Git (repos, branches, commits) - 20 conversations
### Meta (planning, discussion) - 10 conversations

Total: 240 conversations
Focus: 62.5% coding, 25% documentation, 12.5% infrastructure
```

### View 4: Goals (Intent-based)
```bash
cycodj view --by-goal --date 2025-12-21
```

**Output:**
```
## Goal View: December 21, 2025

### Feature Development (180 conversations, 75%)
Building new capabilities

### Bug Fixes (30 conversations, 12.5%)
Fixing issues

### Learning/Research (20 conversations, 8.3%)
Understanding existing code or new concepts

### Refactoring (10 conversations, 4.2%)
Improving existing code

Total: 240 conversations
Balance: 75% building, 12.5% fixing, 12.5% improving/learning
```

**Why This Helps:**
- **Multiple perspectives:** Same data, different insights
- **Choose your view:** Pick the organization that answers your question
- **Pattern detection:** Tool finds themes automatically
- **Symblob compression:** Each view is a valid compression of reality
- **Shareable:** Export any view as summary report

---

## User Stories

### Story 1: Daily Standup (Topic View)
**As a user,** I want to quickly summarize what projects I worked on yesterday  
**So that** I can report in standup without spending 10 minutes figuring it out  
**Currently:** Read through all conversations manually, try to remember  
**Desired:** Run `cycodj topics --yesterday` and get instant project breakdown

### Story 2: Weekly Review (Multiple Views)
**As a user,** I want to see what projects consumed my time AND how much was execution vs. exploration  
**So that** I can identify where I'm spending effort and how balanced my work is  
**Currently:** No way to group multi-day work by any dimension  
**Desired:** Run `cycodj topics --last-days 7` then `cycodj tasks --last-days 7` to see different perspectives

### Story 3: Project Tracking (Topic View Over Time)
**As a user,** I want to track progress on a specific project over time  
**So that** I can see if it's moving forward or stalled  
**Currently:** Have to manually grep for project name across days  
**Desired:** Tool shows "cycodgr: 15 convs Mon, 8 convs Tue, 2 convs Wed" trend

### Story 4: Work Balance Audit (Goal/Task Views)
**As a user,** I want to know if I'm spending too much time on one type of work  
**So that** I can rebalance between building, fixing, and learning  
**Currently:** No visibility into intent/goal distribution  
**Desired:** See "80% building features, 15% bug fixes, 5% learning" and realize I need more learning time

### Story 5: Technology Focus (Tech View)
**As a user,** I want to see what languages/tools dominated my week  
**So that** I can report on technical work or identify skill gaps  
**Currently:** No way to filter/group by technology used  
**Desired:** "Worked with C# (60%), TypeScript (30%), Markdown (10%)" view

### Story 6: Cross-Dimensional Analysis
**As a user,** I want to combine views  
**So that** I can ask complex questions like "What percentage of C# work was bug fixes vs. features?"  
**Currently:** Impossible - only have time view  
**Desired:** Filter topics by tech, or tech by goal, etc.

---

## How Should Clustering Work?

### Approach 1: Pattern Matching (Simple)
Detect common patterns:
- Same repeated user message → Same project
- Same file patterns (cdr/*, todo/*, etc.) → Same project
- Same tools used → Likely related
- Branching from same root → Same project

### Approach 2: Semantic Grouping (Better)
Use conversation titles:
- "CDR Project" → CDR project
- "Book Summary" → Book automation
- "cycodgr" → Tool development

### Approach 3: AI Clustering (Best)
Use `--instructions` to let AI cluster:
```bash
cycodj topics --date 2025-12-21 \
  --instructions "Group these conversations by project/theme"
```

---

## Success Criteria

**This is solved when:**

1. ✅ Can run `cycodj view --by-<dimension>` for multiple organizational dimensions
2. ✅ Minimum dimensions supported: `--by-topic`, `--by-task`, `--by-tech`, `--by-goal`
3. ✅ Each view shows: grouping, counts, percentages, time ranges
4. ✅ Can see most active group at a glance in any view
5. ✅ Works across date ranges (daily, weekly, monthly)
6. ✅ Can export any view to markdown
7. ✅ Views work with existing filter options (--date, --from/--to, etc.)

**Bonus points if:**
- Can combine views: `cycodj view --by-topic --filter-tech "C#"` (C# topics)
- Shows cross-dimensional insights: "cycodgr project: 60% C#, 40% docs"
- Detects when projects start/end automatically
- Shows project status (active, completed, stalled)
- Can define custom dimensions via --instructions
- Warns about unbalanced distributions: "90% bug fixes, only 10% features!"
- Can filter other commands by dimension: `cycodj list --topic cycodgr`
- Supports "chapterization" within single conversations (topics within one chat)

---

## The Value

**Time Saved:**
- Current: 15-20 minutes to manually analyze daily work
- Future: 10 seconds to see automatic breakdown
- **100x faster**

**Insights Gained:**
- See patterns: "I spend 70% of time debugging, 30% building"
- Spot problems: "This project has been stalled for a week"
- Track progress: "We moved from 5 convs/day to 50 convs/day" (automation working!)

**Communication:**
- Quick summaries for standups
- Project status reports
- Time tracking for billing/reporting

---

## Example Usage (Dream State)

```bash
# What am I working on?
cycodj topics --yesterday

# Show weekly project distribution
cycodj topics --last-days 7

# Focus on specific project
cycodj topics --date 2025-12-21 --project "CDR"

# Export project summary
cycodj topics --month --format markdown > december-projects.md

# Find stalled projects
cycodj topics --last-days 30 --stalled

# Compare time distribution this week vs last
cycodj topics --this-week
cycodj topics --last-week

# Use AI for smart grouping
cycodj topics --last-days 7 \
  --instructions "Group by strategic goal, not just project name"
```

---

## Real-World Example

**What I did manually today:**

Looking at Saturday Dec 21 (240 conversations):

1. Read through journal output (5 minutes)
2. Noticed pattern: Lots of "Read STATUS..." (2 minutes)
3. Searched for specific terms to confirm (3 minutes)
4. Counted conversations manually (5 minutes)
5. Figured out: CDR project dominated the day (5 minutes)

**Total: 20 minutes of manual analysis**

**What I wanted:**

```bash
cycodj topics --date 2025-12-21
```

Output in 3 seconds:
- CDR Documentation: 186 convs (77.5%)
- Repo Management: 3 convs (1.2%)
- Other: 51 convs (21.3%)

**The tool should work for me, not make me work for it.**

---

## Why This Matters

**The Symblob Principle:**
Same conversation data, multiple valid representations - like how a book exists as:
- Title (ultra-compressed)
- Table of contents (structural)
- Chapter summaries (compressed)
- Full text (decompressed)

Conversations exist as:
- **Time view:** When did work happen? (journal - have this!)
- **Topic view:** What projects were involved? (chapters - need this!)
- **Task view:** What got accomplished? (outcomes - need this!)
- **Tech view:** What technologies were used? (stack - need this!)
- **Goal view:** What was the intent? (purpose - need this!)

**Questions each view answers:**

**Time View:**
- When was I most productive?
- What did I do in the afternoon?
- How did my day flow?

**Topic View:**
- What projects did I work on?
- How much time per project?
- Which projects are active vs. stalled?

**Task View:**
- How much was execution vs. exploration?
- Am I in "doing" mode or "thinking" mode?
- Is automation working? (high % execution = yes!)

**Tech View:**
- What languages/tools dominated?
- Am I balanced across stack?
- Where are my skill gaps?

**Goal View:**
- Am I building or fixing?
- Too much bug fixing, not enough features?
- Enough time for learning/research?

**Without multiple views:**
- I have raw data but limited insights
- I'm the data analyst, not the user
- The tool shows me everything but tells me little

**With symblob views:**
- Tool provides multiple perspectives automatically
- I choose the view that answers MY question
- Same data, infinite insights

**Chapterization:**
Not just time-based narrative, but meaning-based organization.  
Like a meeting transcript becomes meeting notes organized by topic, not by chronology.

**Data → Information → Insight → Wisdom** 📊

This is the difference between a journal and an understanding.
