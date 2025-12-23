# TODO: Improve Partial File Read Warnings - Bias Prevention

**Date:** 2025-12-20
**Priority:** High
**Category:** Cognitive Bias Prevention, Tool UX

## The Problem

When using `ViewFile` with truncation, the output shows:
```
[Showing lines 1-100 of 260 total] [160 lines remaining]
```

**But this warning is too subtle!** AI (and humans?) can easily:
1. See the warning but not process it
2. Continue with incomplete information
3. Draw conclusions from partial data
4. Not realize they're working with truncated info

## Real Incident (2025-12-20, ~11:00 AM)

User asked for code review of today's work. AI:
1. ✅ Started reading coding guidelines
2. ❌ Only read first 100 lines (file has more)
3. ❌ Only read first 100 lines of SearchCommand.cs (260 total lines)
4. ❌ Was about to write code review based on partial info
5. ❌ Didn't react to "[160 lines remaining]" warning

User caught it: "hold up... you were about to make a doc? but ... you didn't read the full coding guideine? why?"

## Why This Matters

This is a **data incompleteness bias** - similar to today's earlier incident but different:
- Earlier incident: Had complete data, filtered it mentally
- This incident: Had incomplete data, didn't notice/request more

Both lead to incorrect conclusions.

## Proposed Solutions

### 1. More Prominent Warning
Instead of:
```
[Showing lines 1-100 of 260 total] [160 lines remaining]
```

Show:
```
⚠️  WARNING: INCOMPLETE FILE - Only 100 of 260 lines shown (38.5%)
⚠️  160 lines NOT shown - conclusions may be BIASED
⚠️  To see full file, increase maxTotalChars or use multiple ViewFile calls
```

### 2. Bias Checklist Prompt
When truncation occurs, append:
```
🔍 BIAS CHECK:
- [ ] Am I about to draw conclusions from incomplete data?
- [ ] Do I need to see the rest before proceeding?
- [ ] Will my analysis be valid with only 38.5% of the file?
```

### 3. Auto-Suggest Next Action
```
💡 SUGGESTION: 
To continue reading, use:
  ViewFile --startLine 101 --endLine 260 --path <file>
```

### 4. Require Acknowledgment Flag
For important operations (reviews, analysis), require explicit flag:
```
--acknowledge-incomplete-data
```

To force conscious decision about proceeding with partial info.

### 5. Summary Stats in Warning
```
⚠️  INCOMPLETE FILE SUMMARY:
File: C#-Coding-Style-Essential.md
Shown: 100 lines (38.5%)
Hidden: 160 lines (61.5%)
Character limit hit: 50,000 / 50,000

⚠️  RISK: Majority of file not seen - high bias risk
```

## Implementation Ideas

### Option A: Enhanced ViewFile Output
Modify the tool's output format to be more alarming/noticeable.

### Option B: Post-Processing Layer
Add a layer that analyzes tool outputs and adds warnings/checklists.

### Option C: Cognitive Interrupt Pattern
When truncation detected, require explicit acknowledgment:
```
> File truncated. Type 'CONTINUE' to proceed with partial data, or 'READ_MORE' to see rest:
```

### Option D: Metadata Tracking
Track what files were read partially and warn if later referenced:
```
⚠️  WARNING: You're about to reference "SearchCommand.cs" but you only read 38.5% of it earlier!
```

## Related Work

- `case-study-2025-12-20-tool-vs-interpretation.md` - Filtering bias after seeing complete data
- This incident: Not seeing complete data in the first place
- Both are about **incomplete information leading to biased conclusions**

## Questions

1. Is this an AI-specific problem or human problem too?
2. How often does this happen without being caught?
3. Should there be different warning levels based on % truncated?
4. Can we detect when someone is about to use truncated data for analysis?

## Action Items

- [ ] Enhance ViewFile warning output format
- [ ] Create bias checklist for partial data scenarios
- [ ] Add this incident to bias-corrections-tracker.md
- [ ] Consider implementing cognitive interrupt patterns
- [ ] Test with humans - do they notice truncation warnings?

## Meta-Note

This TODO was created after being caught making this exact mistake. We're documenting the failure mode while still in the failure mode. Very meta. Very turtles.

**User quote:** "maybe we need to 'show' when reading 'not the full file' a more 'warning' like thing... not just what you saw above, but... more like a BIAS warning checklist... or something"

---

**Related Incidents:**
- Bias Incident #4: Tool vs. Interpretation (filtering after seeing data)
- Bias Incident #5: Incomplete Data (not seeing full data) ← THIS ONE

**Status:** Needs implementation
**Impact:** High - affects quality of all file-based analysis
