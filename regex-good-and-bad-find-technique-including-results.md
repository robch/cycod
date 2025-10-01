# Finding Evidence of Regex Pattern Differences in Logs

This document describes the methodology used to investigate why one regex pattern (`[a-zA-Z0-9_]+[0-9]`) works while another (`\w+\d`) doesn't, despite them being theoretically equivalent.

## 1. Examining Cycod Logs

### Step 1: List the most recent logs
First, I identified the relevant log files:

```bash
ls -lat log-cycod-*
```

This showed the current log file: `log-cycod-1759259537365.log`

### Step 2: Search for evidence of the character class pattern
For the character class pattern `[a-zA-Z0-9_]+[0-9]`:

```bash
grep "Building find files command with regex pattern.*\[a-zA-Z0-9_\]" log-cycod-1759259537365.log
```

**Results:**
```
[000016]: 131652ms INFO: CycoDmdCliWrapper.cs:286 Building find files command with regex pattern: '[a-zA-Z0-9_]+[0-9]'
```

This shows that the pattern was correctly processed without escaping.

### Step 3: Search for evidence of the backslash pattern
For the backslash pattern `\w+\d`:

```bash
grep "\\\w+\\\d" log-cycod-1759259537365.log
```

**Results:**
```
[000018]: 160178ms INFO: CodeExplorationHelperFunctions.cs:51 Finding files containing regex pattern: '\w+\d'
[000018]: 160178ms INFO: CycoDmdCliWrapper.cs:286 Building find files command with regex pattern: '\w+\d'
[000018]: 160178ms VERBOSE: CycoDmdCliWrapper.cs:287 Building find files arguments with filePatterns: ["**/*.cs"], contentPattern: \w+\d, excludePatterns: ["**/bin/**", "**/obj/**"]
```

This shows the pattern was processed as-is, but note how it appears in single quotes as `'\w+\d'`, which suggests it might be treated as literal characters.

### Step 4: Check for errors related to the backslash pattern
```bash
grep -A 5 -B 5 "\w+\d.*error" log-cycod-1759259537365.log
```

This returned no results, indicating no explicit errors were logged.

## 2. Examining Cycodmd Logs

### Step 1: List the most recent cycodmd logs
```bash
ls -lat log-cycodmd-*
```

This showed two recent logs:
- `log-cycodmd-1759265860500.log` (for character class pattern)
- `log-cycodmd-1759265867928.log` (for backslash pattern)

### Step 2: Examine character class pattern in cycodmd logs
```bash
grep -A 1 -B 1 "Content pattern" log-cycodmd-1759265860500.log
```

**Results:**
```
[000002]: 68ms INFO: Program.cs:171 Finding files containing regex pattern(s):
[000002]: 68ms INFO: Program.cs:174   Content pattern: '[a-zA-Z0-9_]+[0-9]'
[000002]: 69ms INFO: FileHelpers.cs:274 Finding files with 1 glob pattern(s): '**/*.cs'
```

This shows the character class pattern was correctly processed.

### Step 3: Examine backslash pattern in cycodmd logs
```bash
grep -A 1 -B 1 "Content pattern" log-cycodmd-1759265867928.log
```

**Results:**
```
[000002]: 65ms INFO: Program.cs:171 Finding files containing regex pattern(s):
[000002]: 65ms INFO: Program.cs:174   Content pattern: '\\w+\\d'
[000002]: 66ms INFO: FileHelpers.cs:274 Finding files with 1 glob pattern(s): '**/*.cs'
```

This reveals a key difference: the backslash pattern appears with double backslashes (`\\w+\\d`) instead of single backslashes. This indicates that the backslashes are being escaped, which would cause the regex engine to look for literal `\w+\d` characters rather than interpreting them as metacharacters.

### Step 4: Check for any errors or warnings in backslash pattern log
```bash
grep -i "warning\|exception\|fail" log-cycodmd-1759265867928.log
```

**Results:**
```
[000002]: 64ms INFO: LoggingInitializer.cs:200 Memory logger configured to dump to exception-log-cycodmd-1759265867928.log only on abnormal exit.
[000002]: 110ms INFO: FileHelpers.cs:120 Checking file 'src\common\CommandLine\CommandLineException.cs' against include content pattern: '\\w+\\d'
[000002]: 110ms INFO: FileHelpers.cs:132 File 'src\common\CommandLine\CommandLineException.cs' content match result: False
...
```

The log shows files being checked against the pattern `'\\w+\\d'` (with double backslashes) and consistently finding no matches.

## 3. Evidence from Results

### Character Class Pattern:
- Was sent to regex engine as: `[a-zA-Z0-9_]+[0-9]`
- Successfully matched numerous files (over 30)
- Each match represented a variable name with letters followed by a number

### Backslash Pattern:
- Was sent to regex engine as: `\\w+\\d`
- Found no matches in any files
- No explicit error messages related to the pattern

## 4. Conclusion

The backslash notation (`\w+\d`) fails because the backslashes are being escaped when passed to the regex engine. This causes the engine to look for literal `\w` and `\d` characters rather than interpreting them as word character and digit metacharacters.

When searching logs to debug regex issues:

1. First, identify the appropriate log files (cycod for function calls, cycodmd for implementation)
2. Search for the exact pattern to see how it's being processed
3. Look for escaping issues (such as double backslashes)
4. Confirm if there are any explicit errors or warnings
5. Compare the results between working and non-working patterns

This methodology can be reused to debug similar issues in the future without having to rediscover the technique.