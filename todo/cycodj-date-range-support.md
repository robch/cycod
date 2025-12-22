# TODO: cycodj - Date Range Support

## The Pain 😫

**Current State:**
When I want to analyze a week's worth of work (or any multi-day period), I have to:

1. Run the same command multiple times with different `--date` values:
   ```bash
   cycodj journal --date 2025-12-14
   cycodj journal --date 2025-12-15
   cycodj journal --date 2025-12-16
   # ... repeat for each day
   ```

2. OR use `--last N` which gives me "last N conversations" but I don't know what date range that covers:
   ```bash
   cycodj journal --last 1000  # Is this a week? A month? No idea until I run it.
   ```

**The Problems:**
- **Tedious:** Have to run the command 7+ times for a week analysis
- **Error-prone:** Easy to miss a day or typo a date
- **Mental math:** Have to calculate date ranges in my head
- **Inconsistent:** `journal` has `--last-days` but other commands don't
- **Can't express intent:** I think "I want last week" but have to say "conversations 1-200"

**Real-world frustration:**
Today I wanted to analyze Dec 14-22 (9 days). I had to:
- Try `--last-days` (doesn't work on most commands)
- Fall back to multiple `--date` queries
- Manually piece together the results
- Export got truncated because 9 days = too much data
- Gave up and used `--last 1000` hoping it covered the range

---

## The Cure 💊

**What I Want:**
Express date ranges naturally, like I think about them:

```bash
# The week pattern
cycodj journal --from 2025-12-14 --to 2025-12-22
cycodj export -o week.md --from 2025-12-14 --to 2025-12-22
cycodj stats --from 2025-12-14 --to 2025-12-22

# The "last N days" pattern  
cycodj journal --last-days 7
cycodj stats --last-days 30
cycodj export -o month.md --last-days 30

# The "this period" pattern
cycodj journal --week          # This week (Mon-Sun)
cycodj journal --month         # This month
cycodj journal --yesterday     # Just yesterday
```

**Why This Helps:**
- **One command** instead of 7-9 commands
- **Clear intent:** "I want last week" → `--last-days 7`
- **No mental math:** Tool figures out date range
- **Consistent:** Works across all commands
- **Natural language:** Matches how I think about time

---

## User Stories

### Story 1: Weekly Review
**As a user,** I want to review my entire week's work in one command  
**So that** I can see patterns and progress without running 7 separate queries  
**Currently:** I run `cycodj journal --date YYYY-MM-DD` 7 times and manually combine  
**Desired:** I run `cycodj journal --last-days 7` once and get the full week

### Story 2: Project Timeline
**As a user,** I want to export all conversations from when I started a project to when I finished  
**So that** I can document the complete journey  
**Currently:** I guess at dates, use `--last N`, hope I got them all  
**Desired:** I run `cycodj export --from 2025-12-14 --to 2025-12-19` and get exactly the project timeline

### Story 3: Monthly Stats
**As a user,** I want to see my monthly activity statistics  
**So that** I can track productivity trends over time  
**Currently:** I use `--last 1000` and hope it's roughly a month  
**Desired:** I run `cycodj stats --last-days 30` or `cycodj stats --month` and get exact monthly data

### Story 4: Compare Periods
**As a user,** I want to compare two time periods (this week vs. last week)  
**So that** I can see if I'm more/less productive  
**Currently:** Impossible - can't even get a clean week's data  
**Desired:** I can get clean date ranges and compare them

---

## Success Criteria

**This is solved when:**

1. ✅ All commands accept `--from DATE --to DATE`
2. ✅ All commands accept `--last-days N` (not just journal)
3. ✅ Date ranges work consistently across `list`, `show`, `search`, `branches`, `journal`, `export`, `stats`, `cleanup`
4. ✅ I can express "last week" in one command
5. ✅ The tool handles date ranges internally (I don't have to loop)
6. ✅ Output clearly shows what date range was used

**Bonus points if:**
- Shortcuts like `--week`, `--month`, `--yesterday` work
- Can do relative dates like `--from 7d` (7 days ago)
- Error messages are helpful ("date range too large, try smaller range")

---

## The Value

**Time Saved:**
- Current: ~5 minutes to analyze a week (multiple commands + manual combining)
- Future: ~10 seconds (one command)
- **30x faster**

**Frustration Removed:**
- No more "did I get all the days?"
- No more mental date calculations
- No more inconsistent `--last` vs `--last-days` confusion

**New Capabilities Unlocked:**
- Can actually analyze multi-day periods
- Can compare time ranges
- Can generate clean week/month reports
- Can track trends over time

---

## Example Usage (Dream State)

```bash
# What I did last week
cycodj journal --last-days 7

# Export December's work
cycodj export -o december.md --from 2025-12-01 --to 2025-12-31

# Stats for a specific project's timeline
cycodj stats --from 2025-12-14 --to 2025-12-19 --show-tools

# This week vs last week comparison
cycodj stats --last-days 7 > this-week.txt
cycodj stats --from 7d --to 1d > last-week.txt
diff this-week.txt last-week.txt

# Find all conversations about cycodgr during development
cycodj search "cycodgr" --from 2025-12-14 --to 2025-12-19

# Clean up old conversations from 2024
cycodj cleanup --find-empty --from 2024-01-01 --to 2024-12-31
```

**This would be amazing.** ✨
