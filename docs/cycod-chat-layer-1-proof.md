# cycod chat - Layer 1: TARGET SELECTION - Proof

## Source Code Evidence

This document provides line-by-line source code evidence for all Layer 1 (TARGET SELECTION) features in the chat command.

---

## 1. Input Instructions

### 1.1 Parsing --input, --instruction, --question, -q

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 481-502**: Parsing input options
```csharp
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

**Behavior**:
- Lines 481-484: Accepts `--input`, `--instruction`, `--question`, `-q`
- Lines 483-485: Auto-expands file references (if arg is existing file, reads content)
- Lines 487-491: `--question` and `-q` set quiet + non-interactive modes
- Lines 493-497: If `--question` with no args, reads from stdin
- Line 500: Joins multiple args with newlines
- Line 501: Adds to `InputInstructions` list

### 1.2 Parsing --inputs, --instructions, --questions

**Lines 503-522**:
```csharp
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

**Behavior**:
- Lines 503-507: Multiple input variant
- Line 520: `allowEmptyStrings: true` - empty inputs allowed
- Line 521: Adds each input separately (not joined)

### 1.3 Implicit Stdin Reading

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 9-29**: Automatic stdin reading for chat command
```csharp
public static bool Parse(string[] args, out CommandLineOptions? options, out CommandLineException? ex)
{
    options = new CycoDevCommandLineOptions();

    var parsed = options.Parse(args, out ex);
    if (parsed && options.Commands.Count == 1)
    {
        var command = options.Commands.FirstOrDefault();
        var oneChatCommandWithNoInput =  command is ChatCommand chatCommand && chatCommand.InputInstructions.Count == 0;
        var inOrOutRedirected = Console.IsInputRedirected || Console.IsOutputRedirected;
        var implictlyUseStdIn = oneChatCommandWithNoInput && inOrOutRedirected;
        if (implictlyUseStdIn)
        {
            var stdinLines = ConsoleHelpers.GetAllLinesFromStdin();
            var joined = string.Join("\n", stdinLines);
            (command as ChatCommand)!.InputInstructions.Add(joined);
        }
    }

    return parsed;
}
```

**Behavior**:
- Line 17: Checks if ChatCommand with no input instructions
- Line 18: Checks if stdin/stdout redirected
- Line 19: Implicit stdin usage when both conditions true
- Lines 22-24: Reads all lines from stdin and adds to InputInstructions

### 1.4 Storage in ChatCommand

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 1210**:
```csharp
public List<string> InputInstructions = new();
```

**Lines 284-290**: Grounding input instructions
```csharp
private List<string> GroundInputInstructions()
{
    return InputInstructions
        .Select(x => UseTemplates ? ProcessTemplate(x) : x)
        .Select(x => _namedValues != null ? x.ReplaceValues(_namedValues) : x)
        .ToList();
}
```

**Usage in ExecuteAsync** (Lines 149-153):
```csharp
// Check to make sure we're either in interactive mode, or have input instructions.
if (!interactive && InputInstructions.Count == 0)
{
    ConsoleHelpers.WriteWarning("\nNo input instructions provided. Exiting.");
    return 1;
}
```

---

## 2. System Prompts

### 2.1 Parsing --system-prompt

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 458-463**:
```csharp
else if (arg == "--system-prompt")
{
    var promptArgs = GetInputOptionArgs(i + 1, args);
    var prompt = ValidateString(arg, string.Join("\n\n", promptArgs), "system prompt");
    command.SystemPrompt = prompt;
    i += promptArgs.Count();
}
```

**Behavior**:
- Line 460: Gets all following non-option args
- Line 461: Joins with double newlines
- Line 462: Sets `SystemPrompt` property

### 2.2 Parsing --add-system-prompt

**Lines 464-472**:
```csharp
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

**Behavior**:
- Line 467: Validates and joins prompt args
- Line 470: Adds to `SystemPromptAdds` list (multiple allowed)

### 2.3 Storage and Processing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 1193-1194**:
```csharp
public string? SystemPrompt { get; set; }
public List<string> SystemPromptAdds { get; set; } = new List<string>();
```

**Lines 218-226**: Grounding system prompt
```csharp
private string GroundSystemPrompt()
{
    SystemPrompt ??= GetBuiltInSystemPrompt();
    SystemPrompt = GroundPromptName(SystemPrompt);
    SystemPrompt = GroundSlashPrompt(SystemPrompt);

    var processed = ProcessTemplate(SystemPrompt + "\n\n" + GetSystemPromptAdds());
    return _namedValues != null ? processed.ReplaceValues(_namedValues) : processed;
}
```

**Lines 303-310**: Getting system prompt additions
```csharp
private string GetSystemPromptAdds()
{
    var processedAdds = SystemPromptAdds
        .Select(x => UseTemplates ? ProcessTemplate(x) : x)
        .ToList();
    var joined = string.Join("\n\n", processedAdds);
    return joined.Trim(new char[] { '\n', '\r', ' ' });
}
```

**Lines 292-301**: Built-in system prompt
```csharp
private string GetBuiltInSystemPrompt()
{
    if (EmbeddedFileHelpers.EmbeddedStreamExists("prompts.system.md"))
    {
        var text = EmbeddedFileHelpers.ReadEmbeddedStream("prompts.system.md")!;
        return ProcessTemplate(text);
    }

    return "You are a helpful AI assistant.";
}
```

---

## 3. User Prompt Additions

### 3.1 Parsing --add-user-prompt, --prompt

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 473-480**:
```csharp
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

**Behavior**:
- Lines 475-476: Gets and validates prompt args
- Lines 479-480: `--prompt` automatically adds "/" prefix if missing
- Line 481: Adds to `UserPromptAdds` list

### 3.2 Storage and Processing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 1195**:
```csharp
public List<string> UserPromptAdds { get; set; } = new List<string>();
```

**Lines 252-259**: Grounding user prompt additions
```csharp
private List<string> GroundUserPromptAdds()
{
    return UserPromptAdds
        .Select(x => GroundSlashPrompt(x))
        .Select(x => UseTemplates ? ProcessTemplate(x) : x)
        .Select(x => _namedValues != null ? x.ReplaceValues(_namedValues) : x)
        .ToList();
}
```

**Lines 128-132**: Usage in ExecuteAsync
```csharp
// Add the user prompt messages to the chat.
chat.Conversation.AddPersistentUserMessages(
    UserPromptAdds,
    maxPromptTokenTarget: MaxPromptTokenTarget,
    maxChatTokenTarget: MaxChatTokenTarget);
```

---

## 4. Chat History

### 4.1 Parsing --chat-history

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 524-532**:
```csharp
else if (arg == "--chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var chatHistory = max1Arg.FirstOrDefault() ?? DefaultSimpleChatHistoryFileName;
    command.InputChatHistory = !FileHelpers.FileExists(chatHistory) ? command.InputChatHistory : chatHistory;
    command.OutputChatHistory = chatHistory;
    command.LoadMostRecentChatHistory = false;
    i += max1Arg.Count();
}
```

**Behavior**:
- Line 526: Defaults to `"chat-history.jsonl"` if no arg
- Line 527: Sets InputChatHistory only if file exists
- Line 528: Always sets OutputChatHistory
- Line 529: Disables LoadMostRecentChatHistory flag

**Line 726**: Default constant
```csharp
private const string DefaultSimpleChatHistoryFileName = "chat-history.jsonl";
```

### 4.2 Parsing --input-chat-history

**Lines 533-540**:
```csharp
else if (arg == "--input-chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
    command.InputChatHistory = inputChatHistory;
    command.LoadMostRecentChatHistory = false;
    i += max1Arg.Count();
}
```

**Behavior**:
- Line 536: Validates file exists
- Line 537: Sets InputChatHistory
- Line 538: Disables LoadMostRecentChatHistory

### 4.3 Parsing --continue

**Lines 541-545**:
```csharp
else if (arg == "--continue")
{
    command.LoadMostRecentChatHistory = true;
    command.InputChatHistory = null;
}
```

**Behavior**:
- Line 543: Sets LoadMostRecentChatHistory flag
- Line 544: Clears any explicit InputChatHistory

### 4.4 Parsing --output-chat-history

**Lines 546-552**:
```csharp
else if (arg == "--output-chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var outputChatHistory = max1Arg.FirstOrDefault() ?? DefaultOutputChatHistoryFileNameTemplate;
    command.OutputChatHistory = outputChatHistory;
    i += max1Arg.Count();
}
```

**Line 727**: Default template
```csharp
private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";
```

### 4.5 Storage in ChatCommand

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 1202-1204**:
```csharp
public bool LoadMostRecentChatHistory = false;
public string? InputChatHistory;
public string? OutputChatHistory;
```

**Lines 80-84**: Grounding filenames
```csharp
// Ground the filenames (in case they're templatized, or auto-save is enabled).
InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
```

**Lines 134-146**: Loading chat history
```csharp
// Load the chat history from the file.
var loadChatHistory = !string.IsNullOrEmpty(InputChatHistory);
if (loadChatHistory)
{
    chat.Conversation.LoadFromFile(InputChatHistory!,
        maxPromptTokenTarget: MaxPromptTokenTarget,
        maxToolTokenTarget: MaxToolTokenTarget,
        maxChatTokenTarget: MaxChatTokenTarget,
        useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
    
    // Update console title with loaded conversation title
    ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
}
```

---

## 5. Images

### 5.1 Parsing --image

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 648-653**:
```csharp
else if (arg == "--image")
{
    var imageArgs = GetInputOptionArgs(i + 1, args);
    var imagePatterns = ValidateStrings(arg, imageArgs, "image pattern");
    command.ImagePatterns.AddRange(imagePatterns);
    i += imageArgs.Count();
}
```

**Behavior**:
- Line 650: Gets all following non-option args
- Line 651: Validates as strings (patterns)
- Line 652: Adds to ImagePatterns list

### 5.2 Storage and Resolution

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Line 1216**:
```csharp
public List<string> ImagePatterns = new();
```

**Lines 185-186**: Resolving image patterns
```csharp
var imageFiles = ImagePatterns.Any() ? ImageResolver.ResolveImagePatterns(ImagePatterns) : new List<string>();
ImagePatterns.Clear();
```

**Usage** (Line 188):
```csharp
var response = await CompleteChatStreamingAsync(chat, giveAssistant, imageFiles,
```

---

## 6. Template Variables

### 6.1 Parsing --var

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 403-410**:
```csharp
else if (arg == "--var")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var assignment = ValidateAssignment(arg, max1Arg.FirstOrDefault());
    command.Variables[assignment.Item1] = assignment.Item2;
    ConfigStore.Instance.SetFromCommandLine($"Var.{assignment.Item1}", assignment.Item2);
    i += max1Arg.Count();
}
```

**Behavior**:
- Line 406: Validates format `NAME=VALUE`
- Line 407: Stores in Variables dictionary
- Line 408: Also stores in ConfigStore for access elsewhere

### 6.2 Parsing --vars

**Lines 411-419**:
```csharp
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

**Behavior**: Same as `--var` but accepts multiple assignments

### 6.3 Parsing --foreach

**Lines 420-427**:
```csharp
else if (arg == "--foreach")
{
    var foreachArgs = GetInputOptionArgs(i + 1, args).ToArray();
    var foreachVariable = ForEachVarHelpers.ParseForeachVarOption(foreachArgs, out var skipCount);
    command.ForEachVariables.Add(foreachVariable);
    this.Interactive = false;
    i += skipCount;
}
```

**Behavior**:
- Line 423: Parses foreach variable definition
- Line 424: Adds to ForEachVariables list
- Line 425: Sets Interactive = false (non-interactive when iterating)

### 6.4 Storage in ChatCommand

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Inherited from CommandWithVariables** (parent class):
```csharp
public Dictionary<string, string> Variables = new();
public List<ForEachVariable> ForEachVariables = new();
```

**Lines 56-58**: Initialization in ExecuteAsync
```csharp
// Setup the named values
_namedValues = new TemplateVariables(Variables);
AddAgentsFileContentToTemplateVariables();
```

**Lines 231-250**: Adding AGENTS.md to template variables
```csharp
/// <summary>
/// Adds AGENTS.md file content to template variables
/// </summary>
private void AddAgentsFileContentToTemplateVariables()
{
    if (_namedValues == null)
        return;
        
    var agentsFile = AgentsFileHelpers.FindAgentsFile();
    if (agentsFile != null)
    {
        var agentsContent = FileHelpers.ReadAllText(agentsFile);
        if (!string.IsNullOrEmpty(agentsContent))
        {
            // Store the AGENTS.md content as a template variable
            _namedValues.Set("agents.md", agentsContent);
            _namedValues.Set("agents.file", Path.GetFileName(agentsFile));
            _namedValues.Set("agents.path", agentsFile);

            ConsoleHelpers.WriteDebugLine($"Added AGENTS.md content from {agentsFile} as template variable");
        }
    }
}
```

---

## 7. Template Processing

### 7.1 UseTemplates Flag

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Lines 428-434**:
```csharp
else if (arg == "--use-templates")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var useTemplates = max1Arg.FirstOrDefault() ?? "true";
    command.UseTemplates = useTemplates.ToLower() == "true" || useTemplates == "1";
    i += max1Arg.Count();
}
```

**Lines 435-438**:
```csharp
else if (arg == "--no-templates")
{
    command.UseTemplates = false;
}
```

**Storage** (ChatCommand.cs, Line 1211):
```csharp
public bool UseTemplates = true;
```

### 7.2 Template Processing

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 889-898**: ProcessTemplate method
```csharp
private string ProcessTemplate(string template)
{
    if (string.IsNullOrEmpty(template))
    {
        return template;
    }

    var variables = new TemplateVariables(Variables);
    return TemplateHelpers.ProcessTemplate(template, variables);
}
```

**Usage throughout grounding methods**:
- Line 224: `ProcessTemplate(SystemPrompt + "\n\n" + GetSystemPromptAdds())`
- Line 256: `UseTemplates ? ProcessTemplate(x) : x`
- Line 288: `UseTemplates ? ProcessTemplate(x) : x`
- Line 306: `UseTemplates ? ProcessTemplate(x) : x`

---

## 8. Prompt Name Expansion

### 8.1 Grounding Prompt Names

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 261-271**: GroundPromptName method
```csharp
private string GroundPromptName(string promptOrName)
{
    var check = $"/{promptOrName}";
    var isPromptCommand = _promptHelper.CanHandle(check);
    if (isPromptCommand)
    {
        var result = _promptHelper.Handle(check, null!); // Direct sync call - no Task overhead!
        return result.ResponseText ?? promptOrName;
    }
    return promptOrName;
}
```

**Lines 273-282**: GroundSlashPrompt method
```csharp
private string GroundSlashPrompt(string promptOrSlashPromptCommand)
{
    var isPromptCommand = _promptHelper.CanHandle(promptOrSlashPromptCommand);
    if (isPromptCommand)
    {
        var result = _promptHelper.Handle(promptOrSlashPromptCommand, null!); // Direct sync call - no Task overhead!
        return result.ResponseText ?? promptOrSlashPromptCommand;
    }
    return promptOrSlashPromptCommand;
}
```

**Usage**:
- Line 221: `SystemPrompt = GroundPromptName(SystemPrompt);`
- Line 222: `SystemPrompt = GroundSlashPrompt(SystemPrompt);`
- Line 255: `UserPromptAdds.Select(x => GroundSlashPrompt(x))`

---

## Summary of Data Flow

```
Command Line Args
       │
       ├──→ --input/--inputs          → InputInstructions[]
       ├──→ --system-prompt           → SystemPrompt
       ├──→ --add-system-prompt       → SystemPromptAdds[]
       ├──→ --add-user-prompt/--prompt → UserPromptAdds[]
       ├──→ --chat-history            → InputChatHistory, OutputChatHistory
       ├──→ --input-chat-history      → InputChatHistory
       ├──→ --continue                → LoadMostRecentChatHistory
       ├──→ --output-chat-history     → OutputChatHistory
       ├──→ --image                   → ImagePatterns[]
       ├──→ --var/--vars              → Variables{}
       ├──→ --foreach                 → ForEachVariables[]
       └──→ --use-templates/--no-*    → UseTemplates
       
       (stdin)                        → InputInstructions[] (implicit)

ChatCommand Properties
       │
       ├──→ GroundInputInstructions()    → Apply templates → InputInstructions (processed)
       ├──→ GroundSystemPrompt()         → Apply templates → SystemPrompt (processed)
       ├──→ GroundUserPromptAdds()       → Apply templates → UserPromptAdds (processed)
       ├──→ ImageResolver.Resolve()      → Expand globs   → imageFiles[]
       └──→ AddAgentsFileContent...()    → AGENTS.md      → Variables{"agents.md"}

ExecuteAsync
       │
       ├──→ Load InputChatHistory → Conversation
       ├──→ Add UserPromptAdds → Persistent messages
       ├──→ Process InputInstructions → User prompts
       └──→ Attach imageFiles → Messages
```

All target selection happens in **Layer 1** before any processing or filtering occurs in subsequent layers.
