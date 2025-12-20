# Bias Incident #5 - Quick Reference

**Date:** 2025-12-20, ~11:00 AM  
**Type:** Incomplete Data Bias / Anchoring on First Impression  
**Severity:** High (would have produced invalid code review)  
**Caught by:** User observation ("hold up... you didn't read the full coding guideline?")  

## Summary
AI started code review process, read only first 100 lines of coding guidelines (file has more), read only first 100 of 260 lines of SearchCommand.cs, was about to write review conclusions. Did not react to "[160 lines remaining]" warning in tool output.

## What Went Wrong
1. ViewFile showed truncation warning: "[Showing lines 1-100 of 260 total] [160 lines remaining]"
2. AI saw the warning but didn't process its implications
3. Started to proceed with incomplete information
4. Would have produced biased/incomplete code review

## Different from Incident #4
- **Incident #4**: Had complete data, filtered it mentally after seeing it
- **Incident #5**: Never had complete data, didn't realize it was incomplete

Both lead to incomplete conclusions but via different mechanisms.

## Root Cause
Truncation warnings are too subtle/passive:
- Small gray text at bottom
- Easy to see but not "feel" the importance
- No cognitive interrupt
- No bias checklist
- No acknowledgment required

## Proposed Fix
See: `todo-improve-partial-file-warnings.md`

Key ideas:
- More prominent warnings (⚠️ symbols, percentage shown)
- Bias checklists when truncation occurs
- Cognitive interrupt patterns
- Auto-suggest how to read rest
- Track what was read partially

## Status
- ✅ Caught before damage done
- ✅ Documented
- ⏳ TODO created for improving warnings
- ⏳ To be added to: `bias-corrections-tracker.md`

## User Insight
"maybe we need to 'show' when reading 'not the full file' a more 'warning' like thing... not just what you saw above, but... more like a BIAS warning checklist... or something"

## Meta-Note
Caught this incident WHILE documenting Incident #4. The bias-detection work is already paying off - we're catching failures faster.

Stack status: 🐢🐢🐢🐢...

## Related
- case-study-2025-12-20-tool-vs-interpretation.md
- todo-improve-partial-file-warnings.md
