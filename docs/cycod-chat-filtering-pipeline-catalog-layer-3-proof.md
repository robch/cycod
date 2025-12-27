# cycod chat - Layer 3: Content Filter - PROOF

This document provides detailed source code evidence for all Layer 3 (Content Filter) mechanisms in the `cycod chat` command.

## Table of Contents

1. [Token-Based Filtering](#token-based-filtering)
2. [Template Variable Substitution](#template-variable-substitution)
3. [System Prompt Control](#system-prompt-control)
4. [User Prompt Control](#user-prompt-control)
5. [Input Instruction Content](#input-instruction-content)
6. [Template Processing](#template-processing)
7. [Image Content](#image-content)

---

## Token-Based Filtering

### Parsing: Configuration Settings

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 72-77: Loading token limit configuration
MaxOutputTokens = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxOutputTokens).AsInt(defaultValue: 0);
if (maxOutputTokens > 0) MaxOutputTokens = maxOutputTokens;

MaxPromptTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxPromptTokens).AsInt(DefaultMaxPromptTokenTarget);
MaxToolTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxToolTokens).AsInt(DefaultMaxToolTokenTarget);
MaxChatTokenTarget = ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppMaxChatTokens).AsInt(DefaultMaxChatTokenTarget);
```

**Evidence**: The command loads three token limit settings from configuration:
- `app.max.prompt.tokens` → `MaxPromptTokenTarget`
- `app.max.tool.tokens` → `MaxToolTokenTarget`  
- `app.max.chat.tokens` → `MaxChatTokenTarget`

### Application: Chat History Loading with Token Limits

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 135-142: Loading chat history with token limits applied
var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
if (loadChatHistory)
{
    chat.Conversation.LoadFromFile(InputChatHistory!,
        maxPromptTokenTarget: MaxPromptTokenTarget,
        maxToolTokenTarget: MaxToolTokenTarget,
        maxChatTokenTarget: MaxChatTokenTarget,
        useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
```

**Evidence**: When loading chat history, the `LoadFromFile` method receives the three token limits as parameters. These limits control how many tokens of each message type are loaded.

### Application: Persistent User Messages with Token Limits

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 128-132: Adding user prompt messages with token limits
chat.Conversation.AddPersistentUserMessages(
    UserPromptAdds,
    maxPromptTokenTarget: MaxPromptTokenTarget,
    maxChatTokenTarget: MaxChatTokenTarget);
```

**Evidence**: User prompt additions (from `--add-user-prompt`, `--prompt`) are added as "persistent" messages with token limits applied.

---

## Template Variable Substitution

### Parsing: Variable Definitions

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Single Variable (`--var`)**:

```csharp
// Lines 405-412: Parsing --var option
else if (arg == "--var")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var assignment = ValidateAssignment(arg, max1Arg.FirstOrDefault());
    command.Variables[assignment.Item1] = assignment.Item2;
    ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
    i += max1Arg.Count();
}
```

**Evidence**: `--var NAME=VALUE` is parsed, validated as a `NAME=VALUE` assignment, and stored in:
1. `command.Variables` dictionary
2. `ConfigStore` under `Var.NAME` key

**Multiple Variables (`--vars`)**:

```csharp
// Lines 413-423: Parsing --vars option
else if (arg == "--vars")
{
    var varArgs = GetInputOptionArgs(i + 1, args);
    var assignments = ValidateAssignments(arg, varArgs);
    foreach (var assignment in assignments)
    {
        command.Variables[assignment.Item1] = assignment.Item2;
        ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
    }
    i += varArgs.Count();
}
```

**Evidence**: `--vars NAME1=VALUE1 NAME2=VALUE2 ...` parses multiple assignments and stores each in the same locations.

### Application: Template Variable Setup

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 56-58: Initializing template variables
_namedValues = new TemplateVariables(Variables);
AddAgentsFileContentToTemplateVariables();
```

**Evidence**: A `TemplateVariables` object is created from the `Variables` dictionary, and AGENTS.md content is added to it.

### Application: Template Variable Substitution on Prompts

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 97-100: Grounding prompts and instructions with template substitution
SystemPrompt = GroundSystemPrompt();
UserPromptAdds = GroundUserPromptAdds();
InputInstructions = GroundInputInstructions();
```

**Evidence**: Three "grounding" methods are called that perform template variable substitution on:
1. System prompt
2. User prompt additions
3. Input instructions

### Application: File Path Substitution

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 80-84: Grounding file paths with template substitution
InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
OutputTrajectory = OutputTrajectory != null ? FileHelpers.GetFileNameFromTemplate(OutputTrajectory, OutputTrajectory)?.ReplaceValues(_namedValues) : null;
```

**Evidence**: All file paths are processed through `.ReplaceValues(_namedValues)` which performs template variable substitution. This allows file paths like `chat-history-{time}.jsonl` to be expanded.

---

## System Prompt Control

### Parsing: System Prompt Override

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 470-476: Parsing --system-prompt option
else if (arg == "--system-prompt")
{
    var promptArgs = GetInputOptionArgs(i + 1, args);
    var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "system prompt");
    command.SystemPrompt = prompt;
    i += promptArgs.Count();
}
```

**Evidence**: `--system-prompt` can take multiple arguments which are joined with `\n\n` and stored in `command.SystemPrompt`. This REPLACES the default system prompt.

### Parsing: System Prompt Additions

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 477-486: Parsing --add-system-prompt option
else if (arg == "--add-system-prompt")
{
    var promptArgs = GetInputOptionArgs(i + 1, args);
    var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "additional system prompt");
    if (!string.IsNullOrEmpty(prompt))
    {
        command.SystemPromptAdds.Add(prompt);
    }
    i += promptArgs.Count();
}
```

**Evidence**: `--add-system-prompt` can take multiple arguments joined with `\n\n` and APPENDS to `command.SystemPromptAdds` list. Multiple `--add-system-prompt` options accumulate.

### Application: System Prompt Grounding

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Line 98: System prompt grounding
SystemPrompt = GroundSystemPrompt();
```

**Evidence**: The `GroundSystemPrompt()` method performs template variable substitution on the system prompt.

**Note**: The actual `GroundSystemPrompt()` implementation would need to be examined to see how `SystemPrompt` and `SystemPromptAdds` are combined, but the call shows that grounding (template substitution) is performed.

---

## User Prompt Control

### Parsing: User Prompt Additions

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 487-498: Parsing --add-user-prompt and --prompt options
else if (arg == "--add-user-prompt" || arg == "--prompt")
{
    var promptArgs = GetInputOptionArgs(i + 1, args);
    var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "additional user prompt");
    if (!string.IsNullOrEmpty(prompt))
    {
        var needsSlashPrefix = arg == "--prompt" && !prompt.StartsWith("/");
        var prefix = needsSlashPrefix ? "/" : string.Empty;
        command.UserPromptAdds.Add($"{prefix}{prompt}");
    }
    i += promptArgs.Count();
}
```

**Evidence**: 
- Both `--add-user-prompt` and `--prompt` are handled by the same code
- Multiple arguments are joined with `\n\n`
- If `--prompt` is used and the prompt doesn't start with `/`, a `/` prefix is added (for slash command support)
- Result is appended to `command.UserPromptAdds` list

### Application: User Prompt Grounding

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Line 99: User prompt grounding
UserPromptAdds = GroundUserPromptAdds();
```

**Evidence**: The `GroundUserPromptAdds()` method performs template variable substitution on user prompt additions.

### Application: Adding User Prompts to Conversation

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 128-132: Adding persistent user messages
chat.Conversation.AddPersistentUserMessages(
    UserPromptAdds,
    maxPromptTokenTarget: MaxPromptTokenTarget,
    maxChatTokenTarget: MaxChatTokenTarget);
```

**Evidence**: The grounded `UserPromptAdds` list is added to the conversation as "persistent" user messages with token limits applied.

---

## Input Instruction Content

### Parsing: Single Input Instruction

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 499-523: Parsing --input, --instruction, --question, -q options
else if (arg == "--input" || arg == "--instruction" || arg == "--question" || arg == "-q")
{
    var inputArgs = GetInputOptionArgs(i + 1, args)
        .Select(x => FileHelpers.FileExists(x)
            ? FileHelpers.ReadAllText(x)
            : x);

    var isQuietNonInteractiveAlias = arg == "--question" || arg == "-q";
    if (isQuietNonInteractiveAlias)
    {
        this.Quiet = true;
        this.Interactive = false;
    }

    var implictlyUseStdIn = isQuietNonInteractiveAlias && inputArgs.Count() == 0;
    if (implictlyUseStdIn)
    {
        inputArgs = ConsoleHelpers.GetAllLinesFromStdin();
    }

    var joined = ValidateString(arg, string.Join("\n", inputArgs), "input");
    command.InputInstructions.Add(joined!);

    i += inputArgs.Count();
}
```

**Evidence**:
- Four option variants: `--input`, `--instruction`, `--question`, `-q`
- File content expansion: If argument is a valid file path, content is read via `FileHelpers.ReadAllText()`
- Stdin auto-loading: For `--question`/`-q` with no arguments, stdin is read automatically
- Question mode side effects: Sets `Quiet = true` and `Interactive = false`
- Multiple arguments joined with `\n`
- Result appended to `command.InputInstructions`

### Parsing: Multiple Input Instructions

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 524-548: Parsing --inputs, --instructions, --questions options
else if (arg == "--inputs" || arg == "--instructions" || arg == "--questions")
{
    var inputArgs = GetInputOptionArgs(i + 1, args)
        .Select(x => FileHelpers.FileExists(x)
            ? FileHelpers.ReadAllText(x)
            : x);

    var isQuietNonInteractiveAlias = arg == "--questions";
    if (isQuietNonInteractiveAlias)
    {
        this.Quiet = true;
        this.Interactive = false;
    }

    var implictlyUseStdIn = isQuietNonInteractiveAlias && inputArgs.Count() == 0;
    if (implictlyUseStdIn)
    {
        inputArgs = ConsoleHelpers.GetAllLinesFromStdin();
    }

    var inputs = ValidateStrings(arg, inputArgs, "input", allowEmptyStrings: true);
    command.InputInstructions.AddRange(inputs);

    i += inputArgs.Count();
}
```

**Evidence**:
- Three option variants: `--inputs`, `--instructions`, `--questions`
- Same file content expansion and stdin auto-loading as single variants
- Multiple inputs are added individually to `command.InputInstructions` (not joined)
- Empty strings are allowed (`allowEmptyStrings: true`)

### Parsing: Stdin Auto-Detection (Post-Parse)

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 11-26: Post-parsing stdin auto-detection for chat commands
var parsed = options.Parse(args, out ex);
if (parsed && options.Commands.Count == 1)
{
    var command = options.Commands.FirstOrDefault();
    var oneChatCommandWithNoInput = command is ChatCommand chatCommand && chatCommand.InputInstructions.Count == 0;
    var inOrOutRedirected = Console.IsInputRedirected || Console.IsOutputRedirected;
    var implictlyUseStdIn = oneChatCommandWithNoInput && inOrOutRedirected;
    if (implictlyUseStdIn)
    {
        var stdinLines = ConsoleHelpers.GetAllLinesFromStdin();
        var joined = string.Join("\n", stdinLines);
        (command as ChatCommand)!.InputInstructions.Add(joined);
    }
}
```

**Evidence**: After parsing completes, if:
1. There's exactly one command
2. It's a `ChatCommand`
3. It has no input instructions
4. Stdin or stdout is redirected

Then stdin is automatically read and added as an input instruction.

### Application: Input Instruction Grounding

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Line 100: Input instruction grounding
InputInstructions = GroundInputInstructions();
```

**Evidence**: The `GroundInputInstructions()` method performs template variable substitution on all input instructions.

### Application: Using Input Instructions in Chat Loop

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 156-160: Reading input from instructions or user
while (true)
{
    DisplayUserPrompt();
    var userPrompt = interactive && !Console.IsInputRedirected
        ? InteractivelyReadLineOrSimulateInput(InputInstructions, "exit")
        : ReadLineOrSimulateInput(InputInstructions, "exit");
```

**Evidence**: The chat loop uses `InputInstructions` to simulate user input when in non-interactive mode. The `ReadLineOrSimulateInput()` method consumes instructions from the list.

---

## Template Processing

### Parsing: Template Control

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Explicit Template Control**:

```csharp
// Lines 432-438: Parsing --use-templates option
else if (arg == "--use-templates")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var useTemplates = max1Arg.FirstOrDefault() ?? "true";
    command.UseTemplates = useTemplates.ToLower() == "true" || useTemplates == "1";
    i += max1Arg.Count();
}
```

**Evidence**: `--use-templates` accepts an optional boolean argument (defaults to `true`). Accepts `true`, `1`, or omitted for true.

**Template Disabling**:

```csharp
// Lines 439-442: Parsing --no-templates option
else if (arg == "--no-templates")
{
    command.UseTemplates = false;
}
```

**Evidence**: `--no-templates` sets `UseTemplates = false` without needing an argument.

### Application: Template System Initialization

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 56-58: Template variables setup
_namedValues = new TemplateVariables(Variables);
AddAgentsFileContentToTemplateVariables();
```

**Evidence**: Template processing is initialized by:
1. Creating `TemplateVariables` from the `Variables` dictionary
2. Adding AGENTS.md content to the variables

**Note**: The actual template substitution happens in the `Ground*()` methods and in `.ReplaceValues(_namedValues)` calls, but the `UseTemplates` flag would control whether these operations are performed.

---

## Image Content

### Parsing: Image Patterns

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 658-664: Parsing --image option
else if (arg == "--image")
{
    var imageArgs = GetInputOptionArgs(i + 1, args);
    var imagePatterns = ValidateStrings(arg, imageArgs, "image pattern");
    command.ImagePatterns.AddRange(imagePatterns);
    i += imageArgs.Count();
}
```

**Evidence**: `--image` accepts multiple glob pattern arguments which are validated and added to `command.ImagePatterns` list.

### Application: Image Resolution

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 185-186: Resolving image patterns to files
var imageFiles = ImagePatterns.Any() ? ImageResolver.ResolveImagePatterns(ImagePatterns) : new List<string>();
ImagePatterns.Clear();
```

**Evidence**: 
- Image patterns are resolved to actual file paths via `ImageResolver.ResolveImagePatterns()`
- Patterns are cleared after each message (images apply only to next message)

### Application: Images in Chat Completion

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

```csharp
// Lines 188-192: Sending message with images
var response = await CompleteChatStreamingAsync(chat, giveAssistant, imageFiles,
    (messages) => HandleUpdateMessages(messages),
    (update) => HandleStreamingChatCompletionUpdate(update),
    (name, args) => HandleFunctionCallApproval(factory, name, args!),
    (name, args, result) => HandleFunctionCallCompleted(name, args, result));
```

**Evidence**: The resolved `imageFiles` list is passed to `CompleteChatStreamingAsync()` which embeds them in the user message content.

---

## Summary

This proof document demonstrates that cycod chat Layer 3 (Content Filter) is implemented through:

1. **Token-based filtering**: Via `MaxPromptTokenTarget`, `MaxToolTokenTarget`, `MaxChatTokenTarget` configuration settings applied during chat history loading
2. **Template variable substitution**: Via `--var`/`--vars` options and `TemplateVariables` class with `.ReplaceValues()` calls
3. **System prompt control**: Via `--system-prompt` (override) and `--add-system-prompt` (append) options
4. **User prompt control**: Via `--add-user-prompt` and `--prompt` options (with slash prefix handling)
5. **Input instruction content**: Via `--input`/`--instruction`/`--question` (single) and `--inputs`/`--instructions`/`--questions` (multiple) with file expansion and stdin auto-loading
6. **Template processing control**: Via `--use-templates` and `--no-templates` options
7. **Image content**: Via `--image` option with glob pattern resolution

All content filtering mechanisms are applied through a combination of command-line parsing (`CycoDevCommandLineOptions.cs`) and execution-time processing (`ChatCommand.cs`).
