# Phase C Implementation Plan: Extension-Specific File Filtering

**Date:** 2025-12-15  
**Status:** 📋 ANALYSIS - Understanding what's needed

## Phase C Goal

Add extension-specific FILE filtering to complete the three-level hierarchy:
1. **Repo level:** `--repo-csproj-file-contains` ✅ (Phase B)
2. **File level:** `--cs-file-contains` ⏳ (Phase C - THIS)
3. **Line level:** `--line-contains` ✅ (Already exists)

## Expected Features

From the plan document:
```bash
--cs-file-contains "text"   # C# files only
--js-file-contains "text"   # JavaScript files only
--py-file-contains "text"   # Python files only
# ... all extension variants
```

## Current Code Analysis

### ✅ ALREADY EXISTS: Extension-specific file-contains parsing

**Location:** `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs` (lines 70-81)

```csharp
else if (arg.StartsWith("--") && arg.EndsWith("-file-contains"))
{
    // Extract extension: --cs-file-contains → cs
    var ext = arg.Substring(2, arg.Length - 2 - "-file-contains".Length);
    var terms = i + 1 < args.Count() ? args.ElementAt(++i) : null;
    if (string.IsNullOrWhiteSpace(terms))
    {
        throw new CommandLineException($"Missing search terms for {arg}");
    }
    command.FileContains = terms!;
    command.Language = MapExtensionToLanguage(ext);
}
```

**What this does:**
- Matches pattern: `--EXTENSION-file-contains`
- Sets `FileContains` to the search text
- Sets `Language` to the mapped extension (e.g., "cs" → "csharp")

### ✅ Language filtering in GitHub search

**Location:** `src/cycodgr/Helpers/GitHubSearchHelpers.cs` (lines 412-415)

```csharp
if (!string.IsNullOrEmpty(language))
{
    args.Add("--language");
    args.Add(language);
}
```

**What this does:**
- Passes `--language LANG` flag to `gh search code`
- GitHub API filters results to only that language/file type

### ✅ Language passed from command

**Location:** `src/cycodgr/Program.cs` (line 343 in HandleCodeSearchAsync)

```csharp
var codeMatches = await CycoGr.Helpers.GitHubSearchHelpers.SearchCodeAsync(
    query,
    allRepos,
    command.Language,  // <-- Language filter passed here
    command.Owner,
    command.MinStars,
    "",
    command.MaxResults);
```

## Assessment: Is Phase C Already Done?

### Evidence that it exists:
1. ✅ Command-line parsing for `--ext-file-contains` exists
2. ✅ Language filter passed to GitHub API
3. ✅ GitHub's `--language` flag filters by file type
4. ✅ Extension mapping function exists (`MapExtensionToLanguage`)

### What might be missing:
1. ❓ Does it actually work end-to-end?
2. ❓ Is there a conflict between `--file-contains` and `--ext-file-contains`?
3. ❓ Can you use multiple extension-specific searches?
4. ❓ Does output clearly indicate extension filtering is active?

## Testing Needed

### Test 1: Basic extension-specific file search
```bash
cycodgr --cs-file-contains "async Task" --max-results 3
```

**Expected:** Search only .cs files for "async Task"

### Test 2: With repo pre-filter
```bash
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "ChatCompletion" \
        --max-results 3
```

**Expected:** 
- Pre-filter to .NET projects
- Search only .cs files within those projects

### Test 3: Different extensions
```bash
cycodgr --js-file-contains "express" --max-results 3
cycodgr --py-file-contains "import torch" --max-results 3
```

**Expected:** Each searches only files of that type

### Test 4: Three-level query (THE GOAL!)
```bash
cycodgr --repo-csproj-file-contains "Microsoft.Extensions.AI" \
        --cs-file-contains "anthropic" \
        --line-contains "AsChatClient" \
        --lines 20
```

**Expected:**
- Level 1: Repos with .csproj containing "Microsoft.Extensions.AI"
- Level 2: .cs files containing "anthropic" in those repos
- Level 3: Lines containing "AsChatClient" with 20 lines context

## Potential Issues

### Issue 1: Single Language Limitation
**Problem:** `command.Language` is a single string, not a list
**Impact:** Can only filter one file type at a time
**Example:** Can't do `--cs-file-contains "X" --py-file-contains "Y"` together

**Is this a problem?** 
- Probably not for Phase C
- The use case is typically: search ONE file type at a time
- Multiple file types would be complex to implement and rare in practice

### Issue 2: Conflict with `--language` flag
**Problem:** User could do `--cs-file-contains "X" --language python`
**Impact:** Conflicting signals - which takes precedence?
**Current behavior:** Last one wins (Language gets overwritten)

**Is this a problem?**
- Minor UX issue
- Could add validation to prevent conflicting flags
- Or document that extension-specific takes precedence

### Issue 3: Output clarity
**Problem:** User might not know filtering is active
**Current behavior:** Just shows "GitHub code search for 'X'"

**Solution needed:**
- Show language/extension in output message
- Example: "GitHub code search in C# files for 'X'"

**Where to fix:** `src/cycodgr/Program.cs` line 332-334:
```csharp
var searchType = !string.IsNullOrEmpty(command.Language) 
    ? $"code search in {command.Language} files" 
    : "code search";
```

This ALREADY shows language! So output clarity is already handled.

## Implementation Plan

### If testing shows it works:
**Phase C might already be complete!** ✅
- Just need to test and document
- Update phase status
- No code changes needed

### If testing shows issues:
**Fixes needed:**

1. **If language filtering doesn't work:**
   - Debug GitHub API call
   - Check if `--language` flag is correct
   - Verify extension mapping

2. **If parsing doesn't work:**
   - Check pattern matching in CycoGrCommandLineOptions.cs
   - Verify `MapExtensionToLanguage` works correctly

3. **If output is unclear:**
   - Already handled! (line 332-334 shows language)

4. **If conflicts cause problems:**
   - Add validation to prevent `--cs-file-contains` + `--language python`
   - Or document precedence rules

## Justification for This Analysis

### Why I think it might already be done:
1. **Code pattern exists:** Lines 70-81 parse `--ext-file-contains`
2. **Similar to repo filtering:** Phase B used same pattern successfully
3. **GitHub API supports it:** `--language` flag is well-documented
4. **Extension mapping exists:** `MapExtensionToLanguage` is already implemented
5. **Output shows language:** Line 332-334 already handles this

### Why testing is crucial:
1. **Assumption verification:** Code looks right, but does it work?
2. **Edge cases:** What about yml vs yaml? Multiple extensions?
3. **Integration:** Does it work with repo pre-filtering?
4. **User experience:** Is output clear and helpful?

## Next Steps

### Step 1: Test existing functionality
Run all 4 test cases above to verify behavior

### Step 2: Document findings
- If works: Mark Phase C complete, document usage
- If broken: Identify specific issues

### Step 3: Fix or finish
- If issues found: Implement fixes
- If working: Add to documentation and examples

### Step 4: Update plan
- Mark Phase C status
- Update workflow document
- Create phase-c-complete.md

## Confidence Level

**High confidence (80%)** that Phase C is already implemented and working.

**Why:**
- All necessary code patterns exist
- Similar to Phase B which works
- GitHub API well-documented
- Output handling already in place

**Remaining 20% uncertainty:**
- Haven't tested end-to-end
- Possible edge cases or integration issues
- User experience might need polish

## Summary

Phase C implementation likely requires:
1. ✅ Command-line parsing - DONE (lines 70-81)
2. ✅ Extension mapping - DONE (MapExtensionToLanguage)
3. ✅ GitHub API call - DONE (--language flag)
4. ✅ Output clarity - DONE (lines 332-334)
5. ⏳ Testing - NEEDED
6. ⏳ Documentation - NEEDED

**Estimated effort:** 
- If working: 10 minutes (testing + docs)
- If broken: 30-60 minutes (debug + fix + test + docs)

**Most likely outcome:** Phase C is 90% done, just needs testing and documentation! 🎯
