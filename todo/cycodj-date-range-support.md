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
Express date ranges naturally, like I think about them.

**Note:** cycodmd already has an excellent TIMESPEC system we should adopt for consistency!

```bash
# Range syntax (like cycodmd)
cycodj journal --date-range "2025-12-14..2025-12-22"
cycodj journal --date-range "7d..today"        # From 7 days ago to today
cycodj journal --date-range "3d.."             # From 3 days ago to now
cycodj journal --date-range "..yesterday"      # Up to yesterday

# Or explicit from/to (more obvious?)
cycodj journal --from 2025-12-14 --to 2025-12-22
cycodj journal --from 7d --to today
cycodj export -o week.md --from 7d --to today

# Relative times (like cycodmd)
cycodj journal --date 7d              # Conversations from 7 days ago
cycodj stats --date "3d"              # Stats for 3 days ago
cycodj list --after 2d                # After 2 days ago
cycodj search "bug" --before yesterday

# Keywords (like cycodmd)
cycodj journal --date today
cycodj journal --date yesterday
cycodj list --after yesterday

# The "last N days" pattern (existing --last-days)
cycodj journal --last-days 7          # Already works!
cycodj stats --last-days 30
cycodj export -o month.md --last-days 30

# Consistency: All commands should support same date options
--date <timespec>           # On/in this date/period
--after <timespec>          # After this time
--before <timespec>         # Before this time  
--date-range <from>..<to>   # Between two times
--last-days <n>             # Last N days
```

**TIMESPEC Format (from cycodmd):**
- Absolute: "2023-09-01", "September 1, 2023"
- Relative: "3d" (days), "4h" (hours), "5m" (minutes)
- Combined: "2d4h3m"
- Keywords: "today", "yesterday"
- Ranges: "2023-01-01..2023-12-31", "3d..", "..yesterday"

**Why This Helps:**
- **One command** instead of 7-9 commands
- **Clear intent:** "I want last week" → `--last-days 7` or `--date-range "7d..today"`
- **No mental math:** Tool figures out date range
- **Consistent with cycodmd:** Same TIMESPEC format across all cycod tools
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

1. ✅ All commands accept TIMESPEC format (like cycodmd)
2. ✅ Support: `--date <timespec>`, `--after <timespec>`, `--before <timespec>`
3. ✅ Support: `--date-range <from>..<to>` for ranges
4. ✅ Support: `--last-days N` consistently (not just journal)
5. ✅ Relative times work: "7d", "yesterday", "3d.."
6. ✅ Date ranges work consistently across `list`, `show`, `search`, `branches`, `journal`, `export`, `stats`, `cleanup`
7. ✅ I can express "last week" in one command
8. ✅ The tool handles date ranges internally (I don't have to loop)
9. ✅ Output clearly shows what date range was used

**Bonus points if:**
- Shortcuts like `--week`, `--month` work (might be overkill with "7d" syntax)
- Can do relative ranges like `--from 7d --to 1d` (last week except today/yesterday)
- Error messages reference cycodmd's date system for help
- Consistent behavior across all cycod tools (cycod, cycodmd, cycodgr, cycodj)

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
