# cycodt list - Layer 4: Content Removal - PROOF

## Source Code Evidence

This document provides **line-by-line proof** from source code showing how Layer 4 (Content Removal) is implemented for the `cycodt list` command.

---

## 1. Command-Line Parsing: `--remove` Option

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 71-76**: Parsing `--remove` option

```csharp
71:         else if (arg == "--remove")
72:         {
73:             var removePatterns = GetInputOptionArgs(i + 1, args);
74:             var validRemove = ValidateStrings(arg, removePatterns, "remove patterns");
75:             command.Remove.AddRange(validRemove);
76:             i += removePatterns.Count();
```

**Evidence**:
- Line 71: Checks for `--remove` argument
- Line 73: Gets all following arguments as removal patterns (until next `--` option)
- Line 74: Validates patterns are non-empty strings
- Line 75: Adds patterns to `command.Remove` list
- Line 76: Advances argument index past consumed patterns

**Data Flow**:
```
Command line: cycodt list --remove "skip" "slow"
                                    ↓       ↓
                              Pattern 1  Pattern 2
                                    ↓
                         TestBaseCommand.Remove = ["skip", "slow"]
```

---

## 2. Command Data Structure: `Remove` Property

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 14-15, 25**: `Remove` property declaration and initialization

```csharp
14:         Contains = new List<string>();
15:         Remove = new List<string>();
16:         IncludeOptionalCategories = new List<string>();
...
25:     public List<string> Remove { get; set; }
```

**Evidence**:
- Line 15: `Remove` list initialized as empty in constructor
- Line 25: Public property stores removal patterns
- Type: `List<string>` - multiple removal patterns supported

---

## 3. Filter Construction: Converting `Remove` to `-pattern`

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 97-113**: Building filter list with removal patterns

```csharp
 97:     protected List<string> GetTestFilters()
 98:     {
 99:         var filters = new List<string>();
100:         
101:         filters.AddRange(Tests);
102:         foreach (var item in Contains)
103:         {
104:             filters.Add($"+{item}");
105:         }
106:         
107:         foreach (var item in Remove)
108:         {
109:             filters.Add($"-{item}");
110:         }
111: 
112:         return filters;
113:     }
```

**Evidence**:
- Line 97: Method creates unified filter list from multiple sources
- Line 101: Adds explicit test names (Layer 3)
- Lines 102-105: Adds `--contains` patterns with `+` prefix (Layer 3 - must match)
- Lines 107-110: Adds `--remove` patterns with `-` prefix (Layer 4 - must NOT match)
- Line 109: **CRITICAL** - Prepends `-` to convert removal pattern to exclusion filter

**Data Flow Example**:
```
Input:
  Tests = []
  Contains = ["api"]
  Remove = ["skip", "slow"]

Output filters = ["+api", "-skip", "-slow"]

Meaning:
  - Test MUST contain "api" (Layer 3)
  - Test must NOT contain "skip" (Layer 4)
  - Test must NOT contain "slow" (Layer 4)
```

---

## 4. Filter Application: Must-NOT-Match Logic

### File: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 6-65**: Filter execution with must-NOT-match criteria

**Lines 29-42**: Parsing filter criteria

```csharp
29:         var sourceCriteria = new List<string>();
30:         var mustMatchCriteria = new List<string>();
31:         var mustNotMatchCriteria = new List<string>();
32: 
33:         foreach (var criterion in criteria)
34:         {
35:             var isMustMatch = criterion.StartsWith("+");
36:             var isMustNotMatch = criterion.StartsWith("-");
37:             var isSource = !isMustMatch && !isMustNotMatch;
38: 
39:             if (isSource) sourceCriteria.Add(criterion);
40:             if (isMustMatch) mustMatchCriteria.Add(criterion.Substring(1));
41:             if (isMustNotMatch) mustNotMatchCriteria.Add(criterion.Substring(1));
42:         }
```

**Evidence**:
- Lines 29-31: Three separate lists for different filter types
- Line 35: Detect `+pattern` (must match - Layer 3)
- Line 36: Detect `-pattern` (must NOT match - Layer 4) **← CRITICAL**
- Line 37: Plain patterns are "source" criteria (Layer 3)
- Line 40: Strip `+` prefix from must-match criteria
- Line 41: Strip `-` prefix from must-NOT-match criteria **← CRITICAL**

**Lines 57-62**: Applying must-NOT-match filters

```csharp
57:         if (mustNotMatchCriteria.Count > 0)
58:         {
59:             unfiltered = unfiltered.Where(test =>
60:                 mustNotMatchCriteria.All(criterion =>
61:                     !TestContainsText(test, criterion)));
62:         }
```

**Evidence**:
- Line 57: Only apply if there are exclusion patterns
- Line 59-61: LINQ `Where` clause filters tests
- Line 60: `.All()` - test must satisfy ALL exclusion criteria
- Line 61: `!TestContainsText()` - test must NOT contain the pattern **← CRITICAL**

**Logic**:
```
For each test:
  - Check ALL mustNotMatchCriteria
  - Test passes if: !contains(criterion1) AND !contains(criterion2) AND ...
  - If ANY criterion matches, test is EXCLUDED
```

**Example**:
```
filters = ["-skip", "-slow"]
mustNotMatchCriteria = ["skip", "slow"]

For test "test-api-slow":
  - !TestContainsText(test, "skip") = true  (doesn't contain "skip")
  - !TestContainsText(test, "slow") = false (DOES contain "slow")
  - Result: false (excluded because it contains "slow")

For test "test-api-fast":
  - !TestContainsText(test, "skip") = true  (doesn't contain "skip")
  - !TestContainsText(test, "slow") = true  (doesn't contain "slow")
  - Result: true (included)
```

---

## 5. Test Content Search: `TestContainsText()`

### File: `src/cycodt/TestFramework/YamlTestCaseFilter.cs`

**Lines 138-147**: Searching test properties for pattern

```csharp
138:     private static bool TestContainsText(TestCase test, string text)
139:     {
140:         var fqn = test.FullyQualifiedName;
141:         var fqnStripped = StripHash(fqn);
142:         return test.DisplayName.Contains(text)
143:             || fqn.Contains(text)
144:             || fqnStripped.Contains(text)
145:             || test.Traits.Any(x => x.Name == text || x.Value.Contains(text))
146:             || supportedFilterProperties.Any(property => GetPropertyValue(test, property)?.ToString()?.Contains(text) == true);
147:     }
```

**Evidence**:
- Line 140-141: Check FullyQualifiedName (with and without hash)
- Line 142: Check DisplayName
- Line 143-144: Check FQN variations
- Line 145: Check all traits (tags, categories, etc.)
- Line 146: Check all supported properties (cli, run, script, expect, etc.)

**Fields Searched**:
```
1. DisplayName            - e.g., "Test API login"
2. FullyQualifiedName     - e.g., "my-tests.yaml::Test API login@abc123"
3. FullyQualifiedNameBase - e.g., "my-tests.yaml::Test API login"
4. Traits                 - e.g., tag: "api", category: "integration"
5. Properties             - cli, run, script, bash, expect, etc.
```

**Line 150**: Supported filterable properties

```csharp
150:     private static readonly string[] supportedFilterProperties = { "DisplayName", "FullyQualifiedName", "fullyQualifiedNameBase", "Category", "cli", "run", "script", "bash", "foreach", "arguments", "input", "expect", "expect-regex", "not-expect-regex", "expect-exit-code", "parallelize", "skipOnFailure" };
```

---

## 6. Optional Test Filtering

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 47-61**: Main filtering pipeline

```csharp
47:     protected IList<TestCase> FindAndFilterTests()
48:     {
49:         var files = FindTestFiles();
50:         var filters = GetTestFilters();
51: 
52:         var atLeastOneFileSpecified = files.Any();
53:         var tests = atLeastOneFileSpecified
54:             ? files.SelectMany(file => GetTestsFromFile(file))
55:             : Array.Empty<TestCase>();
56: 
57:         var withOrWithoutOptional = FilterOptionalTests(tests, IncludeOptionalCategories).ToList();
58:         var filtered = YamlTestCaseFilter.FilterTestCases(withOrWithoutOptional, filters).ToList();
59: 
60:         return filtered;
61:     }
```

**Evidence**:
- Line 50: Get filters including `-` removal patterns
- Line 57: **Filter optional tests BEFORE other filters** (Layer 4 - removal)
- Line 58: Apply remaining filters (Layers 3 & 4 combined)

**Execution Order**:
1. Discover test files (Layer 1)
2. Load tests from files (Layer 2)
3. **Remove optional tests** (Layer 4) ← Line 57
4. Apply pattern filters including removal patterns (Layer 3 & 4) ← Line 58

**Lines 115-138**: Optional test filtering logic

```csharp
115:     private IEnumerable<TestCase> FilterOptionalTests(IEnumerable<TestCase> tests, List<string> includeOptionalCategories)
116:     {
117:         var allTests = tests.ToList();
118:         
119:         // If we're including all optional tests, just return everything
120:         var includeAllOptional = includeOptionalCategories.Count == 1 && string.IsNullOrEmpty(includeOptionalCategories[0]);
121:         if (includeAllOptional) return allTests;
122: 
123:         // Determine which tests will be excluded
124:         var excludeAllOptional = includeOptionalCategories.Count == 0;
125:         var excludedTests = allTests
126:             .Where(test => HasOptionalTrait(test) && 
127:                           (excludeAllOptional || !HasMatchingOptionalCategory(test, includeOptionalCategories)))
128:             .ToList();
129: 
130:         if (excludedTests.Count > 0)
131:         {
132:             // Repair the test chain by updating nextTestCaseId and afterTestCaseId properties
133:             RepairTestChain(allTests, excludedTests);
134:         }
135: 
136:         // Return the filtered tests (without excluded ones)
137:         return allTests.Except(excludedTests);
138:     }
```

**Evidence**:
- Line 120: Check if `--include-optional` was specified without categories (means include ALL)
- Line 121: If including all optional, no filtering needed
- Line 124: Default behavior (`includeOptionalCategories` is empty) = EXCLUDE all optional
- Lines 125-128: Find tests to exclude:
  - Line 126: Test has `optional` trait, AND
  - Line 127: Either we're excluding all optional OR test doesn't match any included category
- Lines 130-134: Repair test execution chain if any tests were excluded
- Line 137: Return tests EXCLUDING the excluded ones

**Exclusion Logic**:
```
includeOptionalCategories = []  (default)
  → excludeAllOptional = true
  → Exclude ALL tests with "optional" trait

includeOptionalCategories = [""]  (--include-optional with no args)
  → includeAllOptional = true
  → Include ALL tests (no exclusion)

includeOptionalCategories = ["nightly"]  (--include-optional nightly)
  → Include tests with optional: nightly
  → Exclude tests with optional: broken, slow, etc.
```

---

## 7. Parsing `--include-optional`

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 77-82**: Parsing optional inclusion

```csharp
77:         else if (arg == "--include-optional")
78:         {
79:             var optionalCategories = GetInputOptionArgs(i + 1, args);
80:             var validCategories = optionalCategories.Any()
81:                 ? ValidateStrings(arg, optionalCategories, "optional categories")
82:                 : new List<string> { string.Empty };
```

**Evidence**:
- Line 77: Detect `--include-optional` argument
- Line 79: Get categories (may be empty)
- Lines 80-82: **If no categories provided, add empty string** - signals "include ALL optional"
- This empty string is checked on line 120 of `FilterOptionalTests()`

**Behavior**:
```
--include-optional                 → IncludeOptionalCategories = [""]
--include-optional nightly         → IncludeOptionalCategories = ["nightly"]
--include-optional nightly slow    → IncludeOptionalCategories = ["nightly", "slow"]
(no flag)                          → IncludeOptionalCategories = [] (empty list)
```

---

## 8. Test Chain Repair

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 140-231**: Repairing test execution order when tests are excluded

**Lines 140-145**: Entry point

```csharp
140:     private void RepairTestChain(List<TestCase> allTests, List<TestCase> excludedTests)
141:     {
142:         // Create a dictionary to quickly look up tests by ID
143:         var testsById = allTests.ToDictionary(test => test.Id.ToString());
144:         
145:         // For each excluded test
146:         foreach (var excludedTest in excludedTests)
```

**Lines 148-150**: Get connections for excluded test

```csharp
148:             string? prevTestId = YamlTestProperties.Get(excludedTest, "afterTestCaseId");
149:             string? nextTestId = YamlTestProperties.Get(excludedTest, "nextTestCaseId");
150:             
151:             // Skip if no connections to repair
152:             if (string.IsNullOrEmpty(prevTestId) && string.IsNullOrEmpty(nextTestId))
153:                 continue;
```

**Evidence**:
- Line 148: Get previous test ID from `afterTestCaseId` property
- Line 149: Get next test ID from `nextTestCaseId` property
- Lines 151-153: Skip if test has no connections (isolated test)

**Lines 220-227**: Update connections

```csharp
220:             // Update the connections to skip over excluded tests
221:             if (prevTest != null && nextTest != null)
222:             {
223:                 // Connect previous test to next test
224:                 YamlTestProperties.Set(prevTest, "nextTestCaseId", nextTest.Id.ToString());
225:                 // Connect next test to previous test
226:                 YamlTestProperties.Set(nextTest, "afterTestCaseId", prevTest.Id.ToString());
227:                 
228:                 TestLogger.Log($"Repaired test chain: {prevTest.DisplayName} -> {nextTest.DisplayName} (skipping excluded tests)");
```

**Evidence**:
- Line 221: Only repair if both previous and next tests exist
- Line 224: Update previous test's `nextTestCaseId` to point to next test
- Line 226: Update next test's `afterTestCaseId` to point to previous test
- Line 228: Log the repair action

**Chain Repair Example**:
```
Before exclusion:
  Test A → Test B (optional) → Test C
  
  Test A.nextTestCaseId = "Test B"
  Test B.afterTestCaseId = "Test A"
  Test B.nextTestCaseId = "Test C"
  Test C.afterTestCaseId = "Test B"

After excluding Test B:
  Test A → Test C
  
  Test A.nextTestCaseId = "Test C"  ← Updated
  Test C.afterTestCaseId = "Test A"  ← Updated
```

---

## 9. Optional Trait Detection

### File: `src/cycodt/CommandLineCommands/TestCommands/TestBaseCommand.cs`

**Lines 233-236**: Checking if test has optional trait

```csharp
233:     private bool HasOptionalTrait(TestCase test)
234:     {
235:         return test.Traits.Any(t => t.Name == "optional");
236:     }
```

**Evidence**:
- Checks if test has ANY trait with name "optional"
- Trait values (categories like "nightly", "slow") are not checked here

**Lines 238-242**: Checking if optional test matches included categories

```csharp
238:     private bool HasMatchingOptionalCategory(TestCase test, List<string> categories)
239:     {
240:         var optionalTraits = test.Traits.Where(t => t.Name == "optional").Select(t => t.Value);
241:         return optionalTraits.Any(value => categories.Contains(value));
242:     }
```

**Evidence**:
- Line 240: Get values of all "optional" traits
- Line 241: Check if any value matches the included categories
- Multiple optional traits are supported

**Example**:
```yaml
# Test with optional trait
- name: slow-test
  optional: slow      # HasOptionalTrait() = true
                      # Trait value = "slow"

Command: cycodt list --include-optional slow
  → categories = ["slow"]
  → HasMatchingOptionalCategory() = true
  → Test INCLUDED

Command: cycodt list --include-optional nightly
  → categories = ["nightly"]
  → HasMatchingOptionalCategory() = false
  → Test EXCLUDED (because excludeAllOptional=false but no match)

Command: cycodt list (default)
  → categories = []
  → excludeAllOptional = true
  → Test EXCLUDED (regardless of category)
```

---

## 10. Complete Call Stack

### From Command Line to Exclusion

```
1. Command Line Parsing
   CycoDtCommandLineOptions.TryParseTestCommandOptions()
   └─ Lines 71-76: Parse --remove "pattern"
   └─ Lines 77-82: Parse --include-optional [category]

2. Command Initialization
   TestListCommand (or TestRunCommand)
   └─ Inherits TestBaseCommand
      ├─ Remove = ["pattern1", "pattern2"]
      └─ IncludeOptionalCategories = ["category"] or [] or [""]

3. Test Discovery and Filtering
   TestListCommand.ExecuteList()  (Line 18)
   └─ TestBaseCommand.FindAndFilterTests()  (Line 47)
      ├─ FindTestFiles()  (Line 49) - Layer 1
      ├─ GetTestsFromFile()  (Lines 53-55) - Layer 2
      ├─ FilterOptionalTests()  (Line 57) - Layer 4 REMOVAL
      │  ├─ HasOptionalTrait()  (Line 233)
      │  ├─ HasMatchingOptionalCategory()  (Line 238)
      │  └─ RepairTestChain()  (Line 140)
      └─ YamlTestCaseFilter.FilterTestCases()  (Line 58) - Layers 3 & 4
         ├─ Parse criteria  (Lines 33-42)
         │  └─ Detect "-pattern" → mustNotMatchCriteria
         └─ Apply must-NOT-match filter  (Lines 57-62)
            └─ TestContainsText()  (Line 138)

4. Display Results
   TestListCommand.ExecuteList()  (Lines 20-44)
   └─ Display test names (filtered results)
```

---

## Summary

Layer 4 (Content Removal) in `cycodt list` is implemented through:

1. **`--remove` option**: Explicit removal patterns converted to `-pattern` filters
2. **Filter syntax**: `-` prefix marks exclusion criteria in filter list
3. **Must-NOT-match logic**: Tests must NOT contain ANY exclusion pattern
4. **Optional test exclusion**: Tests with `optional` trait excluded by default
5. **`--include-optional` control**: Override default optional exclusion
6. **Category-based inclusion**: Include specific optional test categories
7. **Test chain repair**: Maintain execution order when tests are excluded
8. **Comprehensive search**: Patterns matched against name, FQN, traits, and properties

**Key Source Files**:
- `CycoDtCommandLineOptions.cs`: Lines 71-82 (parsing)
- `TestBaseCommand.cs`: Lines 97-113 (filter construction), 115-242 (optional filtering & chain repair)
- `YamlTestCaseFilter.cs`: Lines 33-42 (criteria parsing), 57-62 (exclusion logic), 138-147 (pattern matching)
