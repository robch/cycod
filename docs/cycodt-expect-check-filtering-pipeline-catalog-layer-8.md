# cycodt `expect check` - Layer 8: AI Processing

## Overview

The `expect check` command is the **ONLY command** in cycodt CLI that implements AI Processing (Layer 8). It uses AI to validate output against natural language instructions.

## Implementation Status

✅ **Implemented** - via `--instructions` option

## Command Line Options

### `--instructions <text>`

**Purpose**: Provide natural language instructions for AI to validate the input against

**Parsed in**: `CycoDtCommandLineOptions.cs` lines 79-84
**Stored in**: `ExpectCheckCommand.Instructions` property (line 19)
**Used in**: `ExpectCheckCommand.ExecuteCheck()` method (line 48)

## How It Works

1. **User provides instructions** via `--instructions` option
2. **Input is read** from file or stdin
3. **AI is invoked** via `CheckExpectInstructionsHelper.CheckExpectations()`
4. **Result is evaluated** - pass/fail based on AI's assessment

## Data Flow

```
Command Line
  ↓
--instructions "text"
  ↓
CycoDtCommandLineOptions.TryParseExpectCommandOptions() [lines 79-84]
  ↓
ExpectCheckCommand.Instructions = "text"
  ↓
ExpectCheckCommand.ExecuteCheck() [line 48]
  ↓
CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, ...)
  ↓
AI evaluates whether text matches instructions
  ↓
Return true/false + reason
```

## Usage Example

```bash
# Check if output is valid JSON
cat output.txt | cycodt expect check --instructions "Verify this is valid JSON"

# Check if log contains errors
cycodt expect check --input app.log --instructions "Check if any errors occurred"

# Check if output matches expected format
command | cycodt expect check --instructions "Output should contain exactly 3 columns"
```

## AI Integration Details

The AI processing is delegated to `CheckExpectInstructionsHelper.CheckExpectations()` which:
- Takes the input text
- Takes the user's instructions
- Uses an AI model to evaluate the match
- Returns success/failure with a reason

## Limitations

- Only available in `expect check` command
- No configuration for which AI provider to use (inherited from global config)
- No fine-tuning of AI parameters (temperature, model, etc.)
- No ability to save the AI conversation/reasoning

## Related Layers

- **Layer 1 (Target Selection)**: Determines what input to validate
- **Layer 3 (Content Filtering)**: Regex patterns can be combined with AI instructions
- **Layer 6 (Display Control)**: Controls how AI validation results are shown
- **Layer 9 (Actions on Results)**: AI validation determines pass/fail exit code

## See Also

- [Layer 8 Proof](cycodt-expect-check-filtering-pipeline-catalog-layer-8-proof.md) - Source code evidence
- [Layer 3: Content Filtering](cycodt-expect-check-filtering-pipeline-catalog-layer-3.md) - Regex pattern filtering
- [Layer 9: Actions on Results](cycodt-expect-check-filtering-pipeline-catalog-layer-9.md) - How validation affects outcomes
