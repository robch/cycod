# cycod chat - Layer 4: CONTENT REMOVAL

[← Back to chat command](cycod-chat-README.md)

## Layer Purpose

**Content Removal**: What content to actively remove from display during chat interactions

## Implementation Status

🔍 **Implicit Implementation** - The chat command doesn't have explicit content removal flags like `--remove-all-lines` found in other tools, but implements content removal through:
1. Token management (removing old context when limits are reached)
2. Template conditionals (removing sections based on variables)
3. History trimming (removing messages to fit within token budgets)

## Content Removal Mechanisms

### 1. Token-Based Content Trimming

**Purpose**: Automatically remove older chat messages when token limits are exceeded

**Options**: (Configuration-based, not CLI flags)
- `MaxPromptTokenTarget`: Maximum tokens for system/user prompts (default: 50,000)
- `MaxToolTokenTarget`: Maximum tokens for tool definitions (default: 50,000)
- `MaxChatTokenTarget`: Maximum tokens for conversation history (default: 160,000)
- `MaxOutputTokens`: Maximum tokens for AI responses

**Source Evidence**:
- [Proof: Token Management Implementation](cycod-chat-layer-4-proof.md#token-management)

**Behavior**:
- When loading chat history, older messages are automatically removed if token limits are exceeded
- Messages are removed from oldest to newest to stay within `MaxChatTokenTarget`
- System prompts and user prompts are kept within `MaxPromptTokenTarget`
- Tool definitions are kept within `MaxToolTokenTarget`

### 2. Template Conditional Removal

**Purpose**: Remove content sections based on variable conditions in templates

**Options**:
- `--use-templates` (enabled by default)
- `--no-templates` (disable template processing)

**Source Evidence**:
- [Proof: Template Processing](cycod-chat-layer-4-proof.md#template-conditionals)

**Behavior**:
- Templates can include conditional sections like `{{#if var}}...{{/if}}`
- Content within false conditional blocks is removed from the final prompt
- Supports `{{#unless}}`, `{{#each}}` and other Handlebars-style conditionals

**Example**:
```bash
# System prompt template with conditional
cycod --system-prompt "You are an assistant.{{#if debug}} Debug mode enabled.{{/if}}"

# Without --var debug=true → "You are an assistant."
# With --var debug=true → "You are an assistant. Debug mode enabled."
```

### 3. History Pruning on Load

**Purpose**: Remove messages from loaded history to fit token limits

**Options**:
- `--input-chat-history <file>`: Load history (triggers pruning if needed)
- `--continue`: Load most recent history (triggers pruning if needed)
- `--chat-history <file>`: Load and save to same file (triggers pruning if needed)

**Source Evidence**:
- [Proof: History Loading and Pruning](cycod-chat-layer-4-proof.md#history-pruning)

**Behavior**:
- When loading history with `LoadFromFile`, messages exceeding token limits are removed
- Removal starts from oldest messages first
- User messages and assistant responses are paired and removed together when possible
- Tool call messages are kept with their results for coherence

### 4. Persistent Message Filtering

**Purpose**: Only certain user-added messages persist across history

**Options**:
- `--add-user-prompt <text>`: Adds persistent user message
- `--prompt <text>`: Shorthand for `--add-user-prompt` with `/` prefix handling

**Source Evidence**:
- [Proof: Persistent Messages](cycod-chat-layer-4-proof.md#persistent-messages)

**Behavior**:
- Messages added with `--add-user-prompt` are marked as "persistent"
- These messages are NOT removed during token-based pruning
- Regular interactive messages ARE subject to removal when token limits are reached

### 5. Image Pattern Clearing

**Purpose**: Remove image patterns after they're processed

**Options**:
- `--image <pattern>`: Attach images (patterns are cleared after use)

**Source Evidence**:
- [Proof: Image Pattern Clearing](cycod-chat-layer-4-proof.md#image-clearing)

**Behavior**:
- Image patterns provided via `--image` are collected
- After images are resolved and attached to a message, the patterns are cleared
- This prevents images from being re-attached to subsequent messages

**Code Location**: `src/cycod/CommandLineCommands/ChatCommand.cs:185-186`

```csharp
var imageFiles = ImagePatterns.Any() ? ImageResolver.ResolveImagePatterns(ImagePatterns) : new List<string>();
ImagePatterns.Clear();  // ← Patterns removed after use
```

## Missing Explicit Removal Features

The following content removal features found in other CLI tools are **NOT** implemented in the chat command:

❌ **`--remove-all-lines <pattern>`**: Remove lines matching regex (available in cycodmd)
❌ **`--not-regex <pattern>`**: Explicitly exclude content matching pattern (available in cycodt)
❌ **`--remove <pattern>`**: Remove items matching pattern (available in cycodt, cycodj)
❌ **`--exclude`**: Generic exclusion filter (available in cycodmd, cycodgr)

## Comparison to Other Tools

| Feature | chat | cycodmd | cycodj | cycodgr | cycodt |
|---------|------|---------|--------|---------|--------|
| Explicit line removal | ❌ | ✅ `--remove-all-lines` | ❌ | ❌ | ⚠️ Limited |
| Pattern-based exclusion | ❌ | ✅ `--exclude` | ❌ | ✅ `--exclude` | ✅ `--remove` |
| Token-based pruning | ✅ | ❌ | ❌ | ❌ | ❌ |
| Template conditionals | ✅ | ⚠️ Limited | ❌ | ❌ | ❌ |
| History trimming | ✅ | N/A | ⚠️ By count | N/A | N/A |

## Potential Enhancements

Possible improvements to content removal in the chat command:

1. **Explicit Message Filtering**:
   ```bash
   cycod --continue --remove-messages-matching "old prompt text"
   cycod --continue --keep-only-last-n-messages 10
   ```

2. **Tool Call Filtering**:
   ```bash
   cycod --hide-tool-calls
   cycod --hide-tool-output
   cycod --remove-tool-messages-older-than "1 hour ago"
   ```

3. **Content-Based Removal**:
   ```bash
   cycod --continue --remove-assistant-responses-containing "I don't know"
   cycod --continue --prune-messages-before "2024-01-01"
   ```

4. **Selective History Loading**:
   ```bash
   cycod --input-chat-history chat.jsonl --skip-messages 0-10
   cycod --input-chat-history chat.jsonl --only-messages 20-30
   ```

## Summary

The chat command's Layer 4 (Content Removal) is primarily **implicit** and **automatic**:
- ✅ Token-based removal is automatic and robust
- ✅ Template conditionals provide some control
- ⚠️ No explicit CLI flags for content removal
- ⚠️ History pruning is automatic, not user-controlled
- ❌ Missing fine-grained removal options available in other tools

**Key Insight**: The chat command prioritizes automatic content management for token budget compliance over explicit user control of content removal.

## Related Documentation

- [← Previous: Layer 3 - Content Filter](cycod-chat-layer-3.md)
- [→ Next: Layer 5 - Context Expansion](cycod-chat-layer-5.md)
- [Proof Documentation](cycod-chat-layer-4-proof.md)
- [Back to chat command](cycod-chat-README.md)
