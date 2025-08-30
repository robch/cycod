# Porting Guide: C# to TypeScript CLI Implementation

This document captures the hard-earned lessons from porting the `cycod config` commands to TypeScript, achieving 25/25 passing tests with 100% C# compatibility.

## Overview

The config command implementation was challenging, requiring multiple iterations to achieve exact compatibility. This guide documents the major pain points and provides a proven approach for porting the remaining commands (`chat`, `history`, etc.).

## Major Pain Points & Lessons Learned

### 1. **Output Format Precision is Critical**

**Problem**: TypeScript showed `TestKey=TestValue (Local)` but C# expected location headers + indented format:
```
LOCATION: /Users/user/.cycod/config.yaml (local)
  TestKey: TestValue
```

**Lesson**: Don't assume "similar" output is acceptable - match the C# format exactly, character by character.

**Solution**: Study C# output patterns first, then implement display logic to match precisely:
```typescript
// Wrong approach - "similar" formatting
ConsoleHelpers.writeLine(`${key}=${value} (${scope})`);

// Correct approach - exact C# format matching
ConsoleHelpers.displayLocationHeader(locationPath);
ConsoleHelpers.writeLine(`  ${key}: ${displayValue}`, true);
```

### 2. **String Enums Break Type Checking**

**Problem**: `typeof ConfigFileScope.Local === 'string'` returned true, breaking command routing logic.

**Lesson**: TypeScript string enums behave as strings at runtime, breaking standard typeof checks.

**Solution**: Use proper enum detection:
```typescript
// Wrong - breaks with string enums
if (typeof scopeOrFileName === 'string') {
  // This executes for enum values!
}

// Correct - proper enum detection
function isValidScope(value: any): value is ConfigFileScope {
  return Object.values(ConfigFileScope).includes(value as ConfigFileScope);
}
```

### 3. **CLI Framework Quirks Matter**

**Problem**: Commander.js provides `{U: true}` for `--user` flag, but code checked `options.user`.

**Lesson**: Different CLI frameworks handle flags differently than expected - Commander.js capitalizes short flags.

**Solution**: Check all flag variations:
```typescript
// Handle Commander.js flag variations
function parseScopeFromFlags(options: any): ConfigFileScope {
  if (options.global || options.g || options.G) return ConfigFileScope.Global;
  if (options.user || options.u || options.U) return ConfigFileScope.User;
  if (options.local || options.l || options.L) return ConfigFileScope.Local;
  // ...
}
```

### 4. **Scope Logic is Complex**

**Problem**: Missing critical redirect in `getFromScope()` when scope is `Any` - a single line that broke everything.

**Lesson**: Configuration inheritance has subtle edge cases. One missing condition can break the entire system.

**Solution**: Port the exact C# logic, including all redirects and fallbacks:
```typescript
async getFromScope(key: string, scope: ConfigFileScope): Promise<ConfigValue | undefined> {
  // CRITICAL: Match C# behavior - redirect Any scope
  if (scope === ConfigFileScope.Any) {
    return this.getFromAnyScope(key);
  }
  // ... rest of method
}
```

### 5. **Test Recreation ≠ Implementation Validation**

**Problem**: Jest tests passed, but manual CLI commands still failed with "unknown option '--user'".

**Lesson**: Converting YAML tests to Jest doesn't guarantee the implementation works. Tests can pass while the actual CLI is broken.

**Solution**: Test both converted automated tests AND manual CLI commands throughout development.

## Recommended Approach for Future Commands

### Phase 1: Analysis (30 minutes)
Document all C# command behavior before writing any code:

```bash
# Document exact output patterns, arguments, error messages
cycod chat --help
cycod chat "hello world"  
cycod chat --system-prompt "You are helpful" "hello"
cycod history list
cycod history show 1
cycod history clear

# Save output to files for reference
cycod chat "test" > expected-chat-output.txt
cycod history list > expected-history-output.txt
```

**Key Questions**:
- What are the exact CLI argument patterns?
- What's the precise output format for each command?
- What error messages does C# show?
- How does streaming output work?

### Phase 2: Core Implementation (Focus on Business Logic)
Implement the core functionality before worrying about CLI formatting:

- `ChatClient` class - handle API calls, streaming, system prompts
- `HistoryManager` - save/load/list conversations  
- `PromptManager` - handle custom prompts, variables
- `StreamingHelpers` - handle real-time output

**Critical**: Don't worry about CLI formatting yet. Get the business logic working first.

### Phase 3: Output Formatting (Match C# Exactly)
Create display helpers that match C# output character-for-character:

```typescript
// Create helpers that match C# output exactly
class ChatDisplayHelpers {
  static displayStreamingResponse(content: string): void {
    // Match C# streaming format exactly
  }
  
  static displayHistoryItem(item: HistoryItem): void {
    // Match C# history display format exactly
  }
}
```

Test each command manually before moving to automated tests.

### Phase 4: Test Conversion
Convert YAML tests to Jest after the implementation is working:
- Use tests to catch regressions, not drive development
- Ensure manual CLI commands work before running automated tests

## Implementation Strategy

### 1. **Start with C# Code Analysis**
Before writing ANY TypeScript code:

```csharp
// Identify the 5 most critical methods in C# implementation
// Example from config commands:
ConfigStore.GetFromScope()        // Had critical Any scope redirect
ConfigStore.GetFromAnyScope()     // Complex inheritance logic
ConfigListCommand.ExecuteAsync()  // Output formatting logic
```

### 2. **Handle String Enums Properly**
Use this pattern consistently throughout the codebase:

```typescript
// Standard enum checking pattern
function isValidCommand(value: any): value is CommandType {
  return Object.values(CommandType).includes(value as CommandType);
}

// Avoid typeof checks with string enums
if (isValidCommand(userInput)) {
  // Safe to use as CommandType
}
```

### 3. **Port Critical Methods Line-by-Line**
- Identify the 5-10 most critical methods in C#
- Port these exactly, don't "improve" or refactor initially
- Add TypeScript improvements AFTER achieving parity

### 4. **Test Integration Early**
Create comparison utilities from the start:

```typescript
class OutputComparator {
  static compareWithCSharp(command: string, tsOutput: string): boolean {
    // Load expected C# output and compare
    const expectedOutput = loadExpectedOutput(command);
    return this.normalizeOutput(tsOutput) === this.normalizeOutput(expectedOutput);
  }
  
  private static normalizeOutput(output: string): string {
    // Handle platform differences (paths, line endings)
    return output.replace(/\r\n/g, '\n').trim();
  }
}
```

## Common Pitfalls to Avoid

### 1. **"Close Enough" Syndrome**
- **Wrong**: "The output is similar, good enough"
- **Right**: "The output must be character-for-character identical"

### 2. **Premature Optimization**
- **Wrong**: "Let's make the TypeScript version better than C#"
- **Right**: "Let's make it identical first, then improve"

### 3. **Test-Driven Development for Porting**
- **Wrong**: "Let's write tests first, then implement"
- **Right**: "Let's port the functionality, then convert tests"

### 4. **Framework Assumptions**
- **Wrong**: "Commander.js works like the C# argument parser"
- **Right**: "Every framework has quirks, test thoroughly"

## Success Metrics

### Mandatory Requirements
- ✅ Manual CLI commands work identically to C#
- ✅ All converted YAML tests pass (0 failures)
- ✅ Zero tolerance for "close enough" output formatting
- ✅ CLI arguments parsed identically to C# version

### Quality Gates
1. **Manual Testing**: Every command tested by hand against C# version
2. **Output Comparison**: Character-by-character output matching
3. **Error Handling**: Identical error messages and exit codes
4. **Edge Cases**: Handle same edge cases as C# implementation

## Pre-Implementation Checklist

Before starting any new command implementation:

- [ ] **C# Behavior Analysis**: Document exact C# command behavior
- [ ] **Output Samples**: Capture C# output for all scenarios
- [ ] **Argument Patterns**: Document all CLI flags and options
- [ ] **Error Cases**: Document all error conditions and messages
- [ ] **Critical Methods**: Identify 5-10 most complex C# methods
- [ ] **External Dependencies**: List C# dependencies and find TypeScript equivalents
- [ ] **Test Files**: Locate corresponding YAML test files

## Key Insight

**Precision matters more than speed.** 

The config commands taught us that taking time upfront to understand the C# behavior exactly will save days of debugging later. Every "small difference" compounds into major compatibility issues.

## Next Commands Priority

Based on complexity analysis:

1. **chat** - Most complex (streaming, API calls, prompts)
2. **history** - Medium complexity (file I/O, formatting)  
3. **system-prompts** - Lower complexity (CRUD operations)
4. **slash-commands** - Lower complexity (configuration-like)

Start with simpler commands to validate the approach before tackling streaming chat functionality.