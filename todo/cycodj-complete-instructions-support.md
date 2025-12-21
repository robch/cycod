# Complete --instructions Support for All cycodj Commands

## Status
Partial implementation - infrastructure in place, but only journal command fully refactored.

## What's Done
✅ Added `--instructions`, `--use-built-in-functions`, and `--save-chat-history` properties to base `CycoDjCommand` class  
✅ Added helper method `ApplyInstructionsIfProvided()` to base class  
✅ Added global parsing for these options in `CycoDjCommandLineOptions`  
✅ `journal` command fully supports --instructions

## What's Needed
Each command needs to be refactored to generate output to string first, then apply instructions:

### Commands to Update:
1. **ListCommand** - refactor `ExecuteAsync()` to use `GenerateListOutput()` pattern
2. **ShowCommand** - refactor to generate string output  
3. **BranchesCommand** - refactor to generate string output
4. **SearchCommand** - refactor to generate string output
5. **StatsCommand** - refactor to generate string output
6. **ExportCommand** - already generates markdown, just needs to call `ApplyInstructionsIfProvided()`
7. **CleanupCommand** - probably doesn't need --instructions

### Pattern to Follow (from JournalCommand):
```csharp
public override async Task<int> ExecuteAsync()
{
    var output = GenerateSomeOutput(); // Move all Console output logic here
    
    // Apply instructions if provided
    var finalOutput = ApplyInstructionsIfProvided(output);
    ConsoleHelpers.WriteLine(finalOutput);
    
    return await Task.FromResult(0);
}

private string GenerateSomeOutput()
{
    var sb = new System.Text.StringBuilder();
    
    // Generate output to StringBuilder instead of Console
    // Replace all ConsoleHelpers.WriteLine(...) with sb.AppendLine(...)
    
    return sb.ToString();
}
```

## Why This Matters
--instructions makes cycodj incredibly powerful for end-of-day reviews, status reports, and custom analysis. Users can ask for any view of their chat history without us having to hardcode every possible format.

## Related Files
- `src/cycodj/CommandLine/CycoDjCommand.cs` - base class with helper
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` - global option parsing
- `src/cycodj/CommandLineCommands/JournalCommand.cs` - reference implementation
- `src/cycodj/CommandLineCommands/*.cs` - commands that need updating

## Examples That Should Work After Completion
```bash
cycodj list --instructions "Show only conversations with file modifications"
cycodj show <id> --instructions "Summarize the key decisions made"
cycodj search "error" --instructions "List all error messages found"
cycodj stats --instructions "Create a weekly summary chart"
cycodj export --instructions "Format as a slide deck outline"
```
