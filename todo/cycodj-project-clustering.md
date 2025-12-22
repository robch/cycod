# TODO: cycodj - Project Clustering (Auto-Detect Topics)

## The Pain 😫

**Current State:**
When I look at a day's work, I see a flat list of conversations:

```bash
cycodj list --date 2025-12-21
```

**Output:**
```
240 conversations found:
- 2025-12-21 09:45 - CDR Project Restart Instructions
- 2025-12-21 10:01 - Read Status Next Step
- 2025-12-21 10:06 - Read Status Next Step  
- 2025-12-21 10:21 - Read Status Next Step
... 236 more lines ...
```

**The Problems:**
- **No grouping:** Everything is a flat list
- **Can't see projects:** Were you working on one thing or five things?
- **Pattern invisible:** 186 conversations say "Read STATUS..." but I don't know they're all the same project
- **No summary:** How many conversations per project?
- **Have to manually cluster:** I read titles and mentally group them

**Real-world frustration:**
Today analyzing Saturday (Dec 21):
- 240 conversations total
- I had to manually figure out: "Most are CDR project, some are repo setup, a few are documentation"
- Took me 20 minutes to understand the breakdown
- **The tool should do this for me!**

---

## The Cure 💊

**What I Want:**
Automatically detect and group conversations by project/topic:

```bash
cycodj topics --date 2025-12-21
```

**Output:**
```
## Topics on December 21, 2025

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
Activities:
  - Migrated from ThinkManGames to CycoLand
  - Created new repository
  - Pushed branches

### Tool Discussion (2 conversations, 0.8%)
Pattern: "How should cycodj work?"
Time: 21:09 - 21:15
Status: Discussion
Topics:
  - Help documentation standardization
  - Feature planning

### Total: 240 conversations across 3 projects
Most active: CDR Documentation (77.5%)
Most complex: CDR Documentation (186 sequential tasks)
```

**Why This Helps:**
- **Instant overview:** See all projects at a glance
- **Proportion visible:** 77% of day was CDR work
- **Pattern detection:** Tool finds the themes
- **No manual work:** Automatic clustering
- **Shareable:** Export as summary report

---

## User Stories

### Story 1: Daily Standup
**As a user,** I want to quickly summarize what I worked on yesterday  
**So that** I can report in standup without spending 10 minutes figuring it out  
**Currently:** Read through all conversations manually, try to remember  
**Desired:** Run `cycodj topics --yesterday` and get instant project breakdown

### Story 2: Weekly Review
**As a user,** I want to see what projects consumed my time this week  
**So that** I can identify where I'm spending effort  
**Currently:** No way to group multi-day work by project  
**Desired:** Run `cycodj topics --last-days 7` and see project distribution

### Story 3: Project Tracking
**As a user,** I want to track progress on a specific project over time  
**So that** I can see if it's moving forward or stalled  
**Currently:** Have to manually grep for project name across days  
**Desired:** Tool shows "cycodgr: 15 convs Mon, 8 convs Tue, 2 convs Wed" trend

### Story 4: Time Audit
**As a user,** I want to know if I'm spending too much time on one thing  
**So that** I can rebalance my work  
**Currently:** No visibility into time/conversation distribution  
**Desired:** See "80% of conversations were debugging, only 10% new features" warning

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

1. ✅ Can run `cycodj topics` and see automatic grouping
2. ✅ Groups show: name, count, percentage, time range
3. ✅ Can see most active project at a glance
4. ✅ Can drill down into a specific topic
5. ✅ Works across date ranges (weekly, monthly)

**Bonus points if:**
- Shows project status (active, completed, stalled)
- Detects when projects start/end
- Shows dependencies between projects
- Warns about unbalanced time distribution
- Can filter other commands by topic: `cycodj list --topic cycodgr`

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

**Questions I want answered:**
- What did I work on today/this week/this month?
- How much time goes to each project?
- Am I balanced or focused?
- Which projects are active vs. stalled?
- Where should I focus tomorrow?

**Without clustering:**
- I have raw data but no insights
- I'm the data analyst, not the user
- The tool shows me everything but tells me nothing

**With clustering:**
- Tool provides insights, not just data
- I understand patterns immediately
- Can make better decisions about time allocation

**Data → Information → Insight** 📊
