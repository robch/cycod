# cycodt `expect check` - Layer 8 Proof: AI Processing

## Source Code Evidence

This document provides detailed source code evidence for how Layer 8 (AI Processing) is implemented in the `expect check` command.

---

## 1. Command Line Parsing

### File: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`

**Lines 79-84**: Parsing `--instructions` option

```csharp
79:         else if (command is ExpectCheckCommand checkCommand5 && arg == "--instructions")
80:         {
81:             var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
82:             var instructions = ValidateString(arg, max1Arg.FirstOrDefault(), "instructions");
83:             checkCommand5.Instructions = instructions;
84:             i += max1Arg.Count();
```

**Evidence**: When `--instructions` is encountered, the parser:
1. Validates one argument is provided
2. Validates the string is non-empty
3. Stores it in `ExpectCheckCommand.Instructions` property
4. Advances the argument index

---

## 2. Command Class Definition

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Line 19**: Instructions property declaration

```csharp
19:     public string? Instructions { get; set; }
```

**Evidence**: The `Instructions` property stores the user-provided natural language validation instructions. It's nullable, indicating AI processing is optional.

---

## 3. Command Execution with AI

### File: `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs`

**Lines 31-63**: Execution method showing AI usage

```csharp
31:     private int ExecuteCheck()
32:     {
33:         try
34:         {
35:             var message = "Checking expectations...";
36:             ConsoleHelpers.Write($"{message}");
37: 
38:             var lines = FileHelpers.ReadAllLines(Input!);
39:             var text = string.Join("\n", lines);
40: 
41:             var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
42:             if (!linesOk)
43:             {
44:                 ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
45:                 return 1;
46:             }
47: 
48:             var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
49:             if (!instructionsOk)
50:             {
51:                 ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
52:                 return 1;
53:             }
54: 
55:             ConsoleHelpers.WriteLine($"\r{message} PASS!");
56:             return 0;
57:         }
58:         catch (Exception ex)
59:         {
60:             ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
61:             return 1;
62:         }
63:     }
```

**Evidence**:
- **Line 38-39**: Input is read and joined into single text string
- **Line 41**: First, regex patterns are checked (Layer 3 filtering)
- **Line 48**: AI instruction checking is performed via `CheckExpectInstructionsHelper.CheckExpectations()`
- **Line 48 parameters**:
  - `text`: The full input text to validate
  - `Instructions`: The user's natural language instructions
  - `null`: Working directory (not needed here)
  - Three `out` parameters for capturing AI output
- **Line 49-52**: If AI validation fails, display error and return exit code 1
- **Line 55-56**: If both regex and AI validation pass, display success and return exit code 0

**Flow**: Regex validation first, then AI validation. Both must pass.

---

## 4. AI Processing Implementation

### File: `src/common/Helpers/CheckExpectInstructionsHelper.cs`

**Lines 14-22**: Main entry point with null check

```csharp
14:     public static bool CheckExpectations(string output, string? instructions, string? workingDirectory, out string gptStdOut, out string gptStdErr, out string gptMerged)
15:     {
16:         gptStdOut = string.Empty;
17:         gptStdErr = string.Empty;
18:         gptMerged = string.Empty;
19: 
20:         var noInstructions = string.IsNullOrEmpty(instructions);
21:         if (noInstructions) return true;
22: 
```

**Evidence**: 
- **Line 20-21**: If no instructions provided, immediately return `true` (pass)
- AI processing is **opt-in** via `--instructions` flag

---

### Lines 23-32: Prompt construction

```csharp
23:         ConsoleHelpers.WriteDebugLine($"CheckExpectInstructionsHelper.CheckExpectations: Checking for {instructions} in '{output}'");
24: 
25:         var prompt = new StringBuilder();
26:         prompt.AppendLine($"Here's the console output:\n\n{output}\n");
27:         prompt.AppendLine($"Here's the expectation:\n\n{instructions}\n");
28:         prompt.AppendLine("You **must always** answer \"PASS\" if the expectation is met.");
29:         prompt.AppendLine("You **must always** answer \"FAIL\" if the expectation is not met.");
30:         prompt.AppendLine("You **must only** answer \"PASS\" with no additional text if the expectation is met.");
31:         prompt.AppendLine("If you answer \"FAIL\", you **must** provide additional text to explain why the expectation was not met (without using the word \"PASS\" as we will interpret that as a \"PASS\").");
32:         var promptTempFile = FileHelpers.WriteTextToTempFile(prompt.ToString())!;
```

**Evidence**:
- **Line 26**: Output to validate is included in prompt
- **Line 27**: User's instructions are included in prompt
- **Lines 28-31**: Strict formatting rules for AI response (must answer "PASS" or "FAIL")
- **Line 32**: Prompt is written to temporary file for passing to cycod CLI

---

### Lines 34-46: AI invocation via cycod CLI

```csharp
34:         var passed = false;;
35:         try
36:         {
37:             var startProcess = FindCacheCli("cycod");
38:             var startArgs = $"--input @{promptTempFile} --interactive false --quiet";
39:             var commandLine = $"{startProcess} {startArgs}";
40: 
41:             ConsoleHelpers.WriteDebugLine($"CheckExpectInstructionsHelper.CheckExpectations: RunnableProcessBuilder executing '{commandLine}'");
42:             var result = new RunnableProcessBuilder()
43:                 .WithCommandLine(commandLine)
44:                 .WithWorkingDirectory(workingDirectory)
45:                 .WithTimeout(60000)
46:                 .Run();
```

**Evidence**:
- **Line 37**: Finds the `cycod` executable (the main AI chat CLI)
- **Line 38**: Constructs arguments:
  - `--input @{promptTempFile}`: Load prompt from temp file
  - `--interactive false`: Non-interactive mode
  - `--quiet`: Suppress extra output
- **Line 42-46**: Executes cycod as subprocess with:
  - Custom command line
  - Optional working directory
  - 60-second timeout (60000ms)

**Key Insight**: AI processing is delegated to the **cycod CLI itself**, using it as a subprocess. This means:
- Uses whatever AI provider/model is configured globally
- No command-specific AI configuration
- Inherits all AI capabilities from cycod

---

### Lines 48-63: Capturing and interpreting results

```csharp
48:             gptStdOut = result.StandardOutput;
49:             gptStdErr = result.StandardError;
50:             gptMerged = result.MergedOutput;
51: 
52:             var exitedNotKilled = result.CompletionState == ProcessCompletionState.Completed;
53:             var exitedNormally = exitedNotKilled && result.ExitCode == 0;
54:             passed = exitedNormally;
55: 
56:             var timedoutOrKilled = !exitedNotKilled;
57:             if (timedoutOrKilled)
58:             {
59:                 var message = "CheckExpectInstructionsHelper.CheckExpectations: WARNING: Timedout or killed!";
60:                 gptStdErr += $"\n{message}\n";
61:                 gptMerged += $"\n{message}\n";
62:                 ConsoleHelpers.WriteDebugLine(message);
63:             }
```

**Evidence**:
- **Lines 48-50**: Capture all output from AI subprocess
- **Lines 52-54**: Initial pass condition: process completed successfully (exit code 0)
- **Lines 56-63**: Handle timeout/kill scenarios gracefully

---

### Lines 74-79: Final validation logic

```csharp
74:         if (passed)
75:         {
76:             ConsoleHelpers.WriteDebugLine($"CheckExpectInstructionsHelper.CheckExpectations: Checking for 'PASS' in '{gptMerged}'");
77:             passed = gptMerged.Contains("PASS") || gptMerged.Contains("TRUE") || gptMerged.Contains("YES");
78:             ConsoleHelpers.WriteDebugLine($"CheckExpectInstructionsHelper.CheckExpectations: {passed}");
79:         }
```

**Evidence**:
- **Line 77**: Final determination of pass/fail based on AI's text output
- Looks for keywords: "PASS", "TRUE", or "YES" in the merged output
- **Simple string matching** approach (not JSON parsing)

---

## 5. CLI Finding Mechanism

### Lines 84-115: Finding cycod executable

```csharp
84:     private static string FindCacheCli(string cli)
85:     {
86:         if (_cliCache.ContainsKey(cli))
87:         {
88:             return _cliCache[cli];
89:         }
90: 
91:         var found = FindCli(cli);
92:         _cliCache[cli] = found;
93: 
94:         return found;
95:     }
96: 
97:     private static string FindCli(string cli)
98:     {
99:         var specified = !string.IsNullOrEmpty(cli);
100:         if (specified)
101:         {
102:             var found = FindCliOrNull(cli);
103:             return found != null
104:                 ? CliFound(cli, found)              // use what we found
105:                 : CliNotFound(cli);                 // use what was specified
106:         }
107:         else
108:         {
109:             var clis = new[] { "cycod" };
110:             var found = PickCliOrNull(clis);
111:             return found != null
112:                 ? PickCliFound(clis, found)         // use what we found
113:                 : PickCliNotFound(clis, clis[0]);   // use cycod
114:         }
115:     }
```

**Evidence**:
- **Lines 86-89**: CLI path is cached after first lookup (performance optimization)
- **Line 109**: Hardcoded to search for "cycod" CLI
- **Lines 102-105**: Searches PATH and current directory for cycod executable

---

### Lines 117-147: Search algorithm

```csharp
117:     private static string? FindCliOrNull(string cli)
118:     {
119:         var dll = $"{cli}.dll";
120:         var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{cli}.exe" : cli;
121: 
122:         var path1 = Environment.GetEnvironmentVariable("PATH");
123:         var path2 = Directory.GetCurrentDirectory();
124:         var path3 = FileHelpers.GetProgramAssemblyFileInfo().DirectoryName;
125:         var path = $"{path3}{Path.PathSeparator}{path2}{Path.PathSeparator}{path1}";
126: 
127:         var paths = path.Split(Path.PathSeparator);
128:         foreach (var part2 in new string[]{ "", "net6.0"})
129:         {
130:             foreach (var part1 in paths)
131:             {
132:                 var checkExe = Path.Combine(part1, part2, exe);
133:                 if (File.Exists(checkExe))
134:                 {
135:                     // Logger.TraceInfo($"FindCliOrNull: Found CLI: {checkExe}");
136:                     var checkDll = FindCliDllOrNull(checkExe, dll);
137:                     if (checkDll != null)
138:                     {
139:                         // Logger.TraceInfo($"FindCliOrNull: Found DLL: {checkDll}");
140:                         return checkExe;
141:                     }
142:                 }
143:             }
144:         }
145: 
146:         return null;
147:     }
```

**Evidence**:
- **Line 120**: Platform-specific executable name (`.exe` on Windows)
- **Lines 122-125**: Searches in order:
  1. Program's own directory
  2. Current working directory
  3. System PATH
- **Lines 128-143**: Also checks subdirectories like `net6.0/`
- **Line 136-141**: Validates both .exe AND .dll exist before accepting

---

## Data Flow Summary

```
User Command Line
  ↓
--instructions "natural language validation"
  ↓
CycoDtCommandLineOptions.TryParseExpectCommandOptions()
[lines 79-84]
  ↓
ExpectCheckCommand.Instructions property
[line 19]
  ↓
ExpectCheckCommand.ExecuteCheck()
[line 48]
  ↓
CheckExpectInstructionsHelper.CheckExpectations()
[line 14]
  ↓
Construct AI prompt with output + instructions
[lines 25-32]
  ↓
Write prompt to temp file
[line 32]
  ↓
Find cycod executable
[lines 37, 84-147]
  ↓
Execute: cycod --input @tempfile --interactive false --quiet
[lines 42-46]
  ↓
Capture stdout/stderr/merged output
[lines 48-50]
  ↓
Check if output contains "PASS", "TRUE", or "YES"
[line 77]
  ↓
Return pass/fail result
[line 81]
  ↓
Exit with code 0 (pass) or 1 (fail)
[lines 55-56 or 51-52]
```

---

## Key Observations

1. **Single Implementation**: Only `expect check` command uses AI processing
2. **Delegation Pattern**: AI logic is delegated to cycod CLI subprocess
3. **No Configuration**: Uses global AI configuration from cycod (no command-specific settings)
4. **Simple Validation**: Pass/fail determined by simple string matching, not structured parsing
5. **Timeout Protection**: 60-second timeout prevents hanging
6. **Graceful Fallback**: If cycod not found, uses "cycod" string as-is (lets OS handle PATH resolution)
7. **Caching**: CLI path cached after first lookup for performance
8. **Optional**: AI processing only runs if `--instructions` provided
9. **Combined Filtering**: Works in conjunction with regex patterns (Layer 3)

---

## Related Source Files

- `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs` - Command line parsing
- `src/cycodt/CommandLineCommands/ExpectCommands/ExpectCheckCommand.cs` - Command execution
- `src/common/Helpers/CheckExpectInstructionsHelper.cs` - AI processing logic
- `src/common/Helpers/ExpectHelper.cs` - Regex pattern checking (Layer 3)
- `src/common/Helpers/FileHelpers.cs` - File I/O operations
- `src/common/Helpers/RunnableProcessBuilder.cs` - Subprocess execution
