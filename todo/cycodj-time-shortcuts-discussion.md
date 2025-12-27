# Discussion: Time Shortcut Flags (--today, --yesterday, --this-week)

## The Question

Should we add dedicated shortcut flags for common time periods?

```bash
cycodj list --today
cycodj list --yesterday
cycodj list --this-week
cycodj list --this-month
cycodj list --last-week
cycodj list --last-month
```

Instead of:
```bash
cycodj list --last today
cycodj list --last yesterday
cycodj list --last 7d
cycodj list --last 30d
# etc.
```

---

## Comparison

### With Smart --last (Current Plan)
```bash
cycodj list --last today
cycodj list --last yesterday  
cycodj list --last 7d
cycodj list --last 30d
cycodj journal --last 7d
```

### With Shortcut Flags (Proposed)
```bash
cycodj list --today
cycodj list --yesterday
cycodj list --this-week
cycodj list --this-month
cycodj journal --this-week
```

---

## Pros ✅

### 1. **Shorter for Common Cases**
```bash
--today              # 1 word (7 chars)
--last today         # 2 words (12 chars)
```
5 characters shorter, one less word to type.

### 2. **Super Intuitive**
`cycodj list --today` reads like plain English.  
Can't get more obvious than that!

### 3. **Common Use Cases**
Probably 50%+ of queries are for today/yesterday/this-week.  
Optimizing for the common case makes sense.

### 4. **Discoverability**
Easier to discover via `--help` or tab completion:
- See `--today` and immediately know what it does
- Don't need to learn TIMESPEC syntax first

### 5. **Pattern from Other Tools**
Some tools have shortcuts for common cases:
- `find . -name "*.txt" -newermt yesterday` (has keyword)
- Backup tools often have `--daily`, `--weekly` flags

---

## Cons ❌

### 1. **Flag Explosion** 🎆
Where do we draw the line?

**Obvious shortcuts:**
- `--today` ✓
- `--yesterday` ✓
- `--this-week` ✓

**Probably also want:**
- `--this-month` ?
- `--last-week` ?
- `--last-month` ?
- `--this-year` ?
- `--last-year` ?

**What about:**
- `--last-2-weeks` ?
- `--last-3-months` ?
- `--last-quarter` ?
- `--this-quarter` ?

**Suddenly we have 15+ flags!**

### 2. **Definition Ambiguity** 🤔

**"This week" means what?**
- Last 7 days? (rolling window)
- Monday-Sunday (ISO 8601)
- Sunday-Saturday (US standard)
- This calendar week (could be 1-7 days depending on today)

**"Last week" means what?**
- Previous 7 days?
- Previous calendar week (Mon-Sun)?
- 7-14 days ago?

**"This month" means what?**
- Last 30 days?
- This calendar month (could be 1-31 days)?

**Different users expect different things!**

### 3. **Inconsistency with TIMESPEC Philosophy** 📏

The whole point of TIMESPEC is to have **one unified way** to express time:
- Absolute dates
- Relative times
- Keywords
- Ranges

Adding shortcuts breaks this unification:
- Some time expressions use `--last <timespec>`
- Some use dedicated flags
- **Now users need to learn two systems!**

### 4. **Maintenance Burden** 🔧

Every shortcut flag needs:
- Implementation
- Help documentation
- Testing
- Edge case handling
- Consistent behavior across all commands

With TIMESPEC, we implement once, works everywhere.

### 5. **Redundancy** 🔄

```bash
# Three ways to say the same thing?
cycodj list --today
cycodj list --last today
cycodj list --date today
```

More ways = more confusion about "which one should I use?"

### 6. **Learning Curve Trade-off** 📚

**With shortcuts:**
- Beginners: Easy to start (`--today` is obvious)
- Intermediate: Confused about relationship between shortcuts and --last
- Advanced: Use TIMESPEC for flexibility

**Without shortcuts:**
- Beginners: Learn `--last <timespec>` pattern once
- Intermediate: Use it everywhere
- Advanced: Use TIMESPEC power (ranges, combinations)

**One pattern > Many shortcuts**

---

## Real-World Usage Patterns

Let's think about actual usage:

### Common Queries (Probably 80% of use)
```bash
cycodj list                    # Today (default behavior)
cycodj list --last today       # Explicit today
cycodj list --last yesterday   # Yesterday
cycodj journal --last 7d       # This week
cycodj journal --last 30d      # This month
```

### Less Common (Probably 15% of use)
```bash
cycodj list --last 3d          # Last 3 days
cycodj stats --last 14d        # Last 2 weeks
cycodj export --last 90d       # Last quarter
```

### Rare (Probably 5% of use)
```bash
cycodj list --date 2023-12-20         # Specific date
cycodj list --date-range "7d..3d"     # Complex range
cycodj search "bug" --last 2d4h       # Precise time
```

**Insight:** The common cases are ALREADY short with smart --last!
- `--last today` (2 words, 12 chars)
- `--last 7d` (2 words, 9 chars)

**Is saving 5-7 characters worth the complexity?**

---

## Alternative: Make Defaults Smart

Instead of shortcuts, optimize defaults:

### Option 1: Smart Default for Journal
```bash
# These are equivalent:
cycodj journal              # Defaults to today
cycodj journal --today      # Explicit today
```

Already implemented! Default is today.

### Option 2: Smart Default for List
```bash
# These are equivalent:
cycodj list                 # Defaults to recent conversations
cycodj list --last 20       # Explicit count
```

Already implemented! Default is last 20.

### Option 3: Position-Based Shorthand
```bash
cycodj list today           # First arg = date
cycodj journal yesterday    # First arg = date
cycodj stats 7d             # First arg = timespec
```

**Problem:** Conflicts with conversation ID for show command:
```bash
cycodj show 1754437373970   # Show conversation by ID
cycodj show today           # Show... what? Ambiguous!
```

---

## Comparison with Other Tools

### Git
```bash
git log --since=yesterday       # Uses --since, not --yesterday
git log --since="2 weeks ago"   # Uses --since, not --last-2-weeks
git log --since=2023-01-01      # Unified interface
```

**Git chose:** One pattern (--since/--until), not shortcuts.

### Docker
```bash
docker ps -a --filter since=1h      # Uses --filter since
docker ps -a --filter until=1h      # Not --last-hour
```

**Docker chose:** Filters with time expressions.

### find
```bash
find . -mtime -1        # Modified in last 1 day
find . -mtime -7        # Modified in last 7 days
find . -newermt "2023-01-01"    # Unified -newermt
```

**find chose:** Numeric expressions, not --today.

### Most Unix Tools
Generally prefer **one unified time interface** over many shortcuts.

**Exception:** Backup/rotation tools (`--daily`, `--weekly`) but that's for scheduling, not querying.

---

## My Recommendation

### ❌ **NO - Don't Add Shortcut Flags**

**Why:**

### 1. **Smart --last is Already Short Enough**
```bash
--last today        # 12 chars, 2 words
--today             # 7 chars, 1 word
```

**5 character savings** doesn't justify:
- Definition ambiguity (what's "this week"?)
- Flag explosion (where does it stop?)
- Two systems to learn (shortcuts + TIMESPEC)
- Maintenance burden
- Breaking unified interface

### 2. **One Pattern is Better**
```bash
--last <timespec>   # Learn once, use everywhere
```

Works for:
- Common: `today`, `yesterday`, `7d`
- Uncommon: `3d`, `14d`, `90d`
- Rare: `2d4h`, `3d..today`, `2023-01-01..2023-12-31`

**Consistency > Convenience**

### 3. **Defaults Already Optimize Common Cases**
```bash
cycodj list              # Already defaults to recent
cycodj journal           # Already defaults to today
```

The most common cases are already optimized!

### 4. **TIMESPEC is Learnable**
```bash
cycodj list --last today       # English word
cycodj list --last 7d          # 7 days - obvious!
cycodj list --last yesterday   # English word
```

Not hard to learn, very flexible once you know it.

---

## However... 🤔

**IF you really want shortcuts, here's the minimal set:**

### Minimal Shortcut Set (Only if we must)

**Only add these TWO:**
- `--today` → `--last today`
- `--yesterday` → `--last yesterday`

**Why only these:**
1. Single words (no definition ambiguity)
2. Extremely common (probably 60% of queries)
3. No cultural/timezone ambiguity
4. Won't lead to more requests (clear boundary)

**Do NOT add:**
- ❌ `--this-week` (ambiguous: calendar week? 7 days?)
- ❌ `--last-week` (ambiguous: previous calendar week? 7-14 days ago?)
- ❌ `--this-month` (ambiguous: calendar month? 30 days?)
- ❌ Any other time period

**Implementation:**
```csharp
else if (arg == "--today")
{
    command.After = DateTime.Today;
    command.Before = DateTime.Today.AddDays(1).AddTicks(-1);
}
else if (arg == "--yesterday")
{
    command.After = DateTime.Today.AddDays(-1);
    command.Before = DateTime.Today.AddTicks(-1);
}
```

**Help example:**
```
TIME FILTERING OPTIONS:

  --last <timespec>          Primary time filtering (recommended)
    --last 20                Last 20 conversations
    --last 7d                Last 7 days
    --last today             Today
    --last yesterday         Yesterday
    
  --today                    Shortcut for --last today
  --yesterday                Shortcut for --last yesterday
  
  --after <timespec>         After specific time
  --before <timespec>        Before specific time
  --date-range <range>       Date range (e.g., "7d..today")
```

---

## Final Recommendation

### Option A: **No Shortcuts** (Preferred) ✅
- Use `--last <timespec>` for everything
- One unified interface
- Already short enough for common cases
- Consistent, predictable, flexible

**Best for:** Clean, maintainable, consistent tool

### Option B: **Minimal Shortcuts** (Acceptable)
- Add ONLY `--today` and `--yesterday`
- Keep `--last <timespec>` as primary
- Shortcuts are convenience aliases
- Stop there (no --this-week, etc.)

**Best for:** Optimizing the 60% most common case slightly

---

## DECISION: Add --today and --yesterday Shortcuts ✅

### **VERDICT: Option B - Minimal Shortcuts**

After discussion, the decision is to add ONLY these two shortcuts:
- `--today`
- `--yesterday`

### Why These Work

**They are NOT ambiguous:**
- `--today` means "this calendar day from midnight to now"
- `--yesterday` means "yesterday's calendar day (full day)"
- Culturally universal - everyone agrees what these mean
- NOT rolling 24/48 hours - these are CALENDAR DAYS

**Exact definitions:**
```csharp
--today      → DateTime.Today to DateTime.Now
--yesterday  → DateTime.Today.AddDays(-1) to DateTime.Today.AddTicks(-1)
```

**Clear boundary:**
- ✅ Single-day periods only (--today, --yesterday)
- ❌ Multi-day periods (--this-week, --last-week, --this-month)
- Use `--last Nd` for multi-day periods

### Benefits

1. **Optimizes common cases** - Probably 60%+ of queries
2. **Unambiguous** - Calendar days are clear
3. **Won't escalate** - Clear "no" to multi-day shortcuts
4. **Complements `--last`** - Shortcuts for common, --last for flexible

### Usage

```bash
# Shortcuts (most common):
cycodj list --today
cycodj list --yesterday

# Smart --last (everything else):
cycodj list --last 7d
cycodj list --last 30d
cycodj journal --last 3d..today
```

---

## ~~My Strong Preference: Option A (No Shortcuts)~~

**DECISION MADE: We're implementing Option B (--today and --yesterday only)**
