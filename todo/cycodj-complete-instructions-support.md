# ✅ COMPLETE - --instructions Support for All cycodj Commands

## Status
**COMPLETE** - All commands now support --instructions!

## What's Done
✅ Added `--instructions`, `--use-built-in-functions`, and `--save-chat-history` properties to base `CycoDjCommand` class  
✅ Added helper method `ApplyInstructionsIfProvided()` to base class  
✅ Added global parsing for these options in `CycoDjCommandLineOptions`  
✅ **JournalCommand** - fully supports --instructions  
✅ **ExportCommand** - fully supports --instructions  
✅ **ListCommand** - fully supports --instructions  
✅ **ShowCommand** - fully supports --instructions  
✅ **BranchesCommand** - fully supports --instructions  
✅ **SearchCommand** - fully supports --instructions  
✅ **StatsCommand** - fully supports --instructions  

All 7 main commands now support AI-powered customization!

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
