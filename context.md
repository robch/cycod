Based on my exploration of the cycod codebase, here's a comprehensive report on slash commands, /title commands, and the notification system:

# Cycod Slash Commands, Title System, and Notifications Report

## Overview of Cycod
Cycod is a command-line AI chat interface that supports multiple AI providers and includes advanced features like function calling, conversation history management, and extensible slash commands. It's part of a larger toolkit that includes cycodmd (markdown processing) and cycodt (testing framework).

## Slash Command System Architecture

### Core Components

**SlashCommandBase** (`src/cycod/SlashCommands/SlashCommandBase.cs`)
- Abstract base class providing universal argument parsing and subcommand routing
- Defines `SlashCommandResult` enum with three states:
  - `Handled` - Command processed, continue normally
  - `PassToAssistant` - Command not recognized, send to AI
  - `NeedsSave` - Command processed and requires immediate file save
- Provides sophisticated argument parsing with quote handling for nested quotes
- Each command defines its `CommandName` property and implements `HandleDefault()` method

**Integration Point** (`src/cycod/CommandLineCommands/ChatCommand.cs`)
- Slash commands are processed in `TryHandleChatCommandAsync()` method
- Built-in commands: `/save`, `/clear`, `/cost`, `/help`, and `/title`
- External handlers: `SlashCycoDmdCommandHandler`, `SlashPromptCommandHandler`, `SlashTitleCommandHandler`
- Commands returning `NeedsSave` trigger immediate save operations for both auto-save and output files

## /Title Command System

### Command Structure
The `SlashTitleCommandHandler` provides comprehensive title management with these subcommands:

**Subcommands:**
- **Default** (no args): Shows current title and help
- **`view`**: Shows current title and lock status only
- **`set "title"`**: Sets user title and locks it (requires double quotes)
- **`lock`**: Locks current title from AI regeneration
- **`unlock`**: Unlocks title to allow AI regeneration  
- **`refresh`**: Generates new title from conversation content

### Key Features

**Quote Parsing and Validation:**
- `set` command requires exact double-quote syntax: `/title set "My Title"`
- Supports nested quotes: `/title set "Title with "inner" quotes"`
- Uses raw input parsing to preserve quote content exactly
- Rejects unquoted, empty, or malformed input with clear error messages

**File Path Management:**
- Uses `SetFilePaths()` to configure input and auto-save paths
- `GetConversationReadFilePath()` intelligently chooses source file:
  - Prefers input file if it exists and has content
  - Falls back to auto-save file
  - Returns null if no valid files available
- Commands that modify metadata require valid conversation files

**State Management:**
- Tracks title lock status (`IsTitleLocked`) to prevent AI overwrites
- Integrates with generation tracking system to show "AI title generation in progress..."
- Clears pending notifications when user views/sets title

### Title Generation System

**TitleGenerationHelpers** (`src/cycod/Helpers/TitleGenerationHelpers.cs`)
- Async title generation using cycodmd subprocess
- **Environment Isolation**: Sets environment variables to prevent infinite loops:
  - `CYCOD_DISABLE_TITLE_GENERATION=true`
  - `CYCOD_AUTO_SAVE_CHAT_HISTORY=false`
  - `CYCOD_AUTO_SAVE_TRAJECTORY=false`
  - `CYCOD_AUTO_SAVE_LOG=false`
- **Content Filtering**: Creates temp file with only user/assistant messages
- **Format Detection**: Tries both OpenAI and Extensions.AI formats automatically
- **Title Sanitization**: Removes command output, limits length (80 chars, truncates to 77)
- **Validation**: Requires at least one assistant message for meaningful generation

**Automatic Title Generation:**
- Triggers after first assistant response if no title exists and not locked
- Uses background Task to avoid blocking UI
- Controlled by environment variable `CYCOD_DISABLE_TITLE_GENERATION`
- Integrates with notification system to inform user of updates

## Notification System

### Core Components

**NotificationType** (enum in `FunctionCallingChat.cs`):
- `Title` - Title update notifications
- `Description` - Description update notifications (extensible)

**NotificationMessage** class:
- `Type` - Notification category (lowercase string)
- `Content` - The notification content
- `Timestamp` - When notification was created

### Notification Management in FunctionCallingChat

**Storage and Queuing:**
```csharp
private readonly ConcurrentQueue<NotificationMessage> _pendingNotifications = new();
private readonly HashSet<string> _activeGenerations = new();
```

**Key Methods:**
- `SetPendingNotification(type, content)` - Enqueue notification for display
- `HasPendingNotifications()` - Check if notifications exist
- `GetAndClearPendingNotifications()` - Retrieve and clear all notifications
- `ClearPendingNotificationsOfType(type)` - Clear specific notification types
- `SetGenerationInProgress(type)` - Mark content type as being generated
- `ClearGenerationInProgress(type)` - Clear generation status
- `IsGenerationInProgress(type)` - Check if generation is active

### Display Integration

**Timing and Display:**
- Notifications shown before assistant responses in chat loop
- Format: `[Title updated to: "New Title"]` in dark gray
- Cleared when user explicitly views related content (e.g., `/title view` clears title notifications)
- Prevents missing updates when operations complete asynchronously

**Chat Loop Integration** (`ChatCommand.RunInteractiveChatLoop`):
```csharp
// Before assistant response
if (chat.HasPendingNotifications()) {
    CheckAndShowPendingNotifications(chat);
}

// After assistant response
if (chat.HasPendingNotifications()) {
    CheckAndShowPendingNotifications(chat);
}

// Before exit
CheckAndShowPendingNotifications(chat);
```

## Conversation Metadata System

### ConversationMetadata Class
```csharp
public class ConversationMetadata {
    public string? Title { get; set; }
    public bool IsTitleLocked { get; set; }
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new();
}
```

**Extensibility:**
- Uses `[JsonExtensionData]` to preserve unknown properties for future features
- All operations (set, lock, unlock) preserve custom metadata fields
- Designed for backward/forward compatibility

### Storage Format
- **Location**: First line of JSONL conversation files
- **Format**: `{"_meta":{"title":"My Title","titleLocked":true,"customField":"value"}}`
- **Parsing**: `ConversationMetadataHelpers.TryParseMetadata()` handles malformed data gracefully
- **Serialization**: Compact JSON without indentation

### Helper Methods

**ConversationMetadataHelpers:**
- `CreateDefault()` - New empty metadata
- `SetUserTitle(metadata, title)` - Set title and lock it
- `SetGeneratedTitle(metadata, title)` - Set title only if unlocked
- `ShouldGenerateTitle(metadata)` - Check if generation needed
- `GetDisplayTitle(metadata, filePath)` - Fallback to filename-based titles

## Console Integration

**ConsoleTitleHelper** (`src/cycod/Helpers/ConsoleTitleHelper.cs`):
- Updates terminal window title based on conversation metadata
- **Display Logic**:
  - Shows actual user/AI titles when set
  - Falls back to "cycod" for missing, default, or generated titles
  - Gracefully handles environments where title setting fails (CI/CD, terminals)

## Error Handling and Edge Cases

### File System Resilience
- **Read-only files**: Commands execute but may not persist changes
- **Missing files**: Clear error messages guide users to use `--input-chat-history`
- **Corrupted metadata**: Graceful fallback to default values, operations create valid metadata

### Input Validation
- **Empty titles**: Rejected with clear error messages
- **Malformed quotes**: Specific validation for double-quote requirement
- **Long titles**: Accepted but truncated for display (no storage limits)
- **Unicode support**: Full support for international characters and emoji

### Concurrency and State
- **Thread safety**: Uses `ConcurrentQueue` for notifications, locks for generation tracking
- **Async operations**: Title generation runs in background without blocking chat
- **State consistency**: Clear separation between UI state and persistent metadata

## Testing Infrastructure

The system includes comprehensive YAML-based tests (`tests/cycod-yaml/cycod-slash-title-commands.yaml`) covering:
- All subcommand variations and edge cases
- Quote parsing and validation scenarios
- File system error conditions
- Metadata corruption recovery
- Extensibility preservation
- Performance stress testing
- Unicode and special character handling

This architecture provides a robust, extensible foundation for conversation metadata management while maintaining excellent user experience and data integrity.

## Additional Technical Details for Testing Implementation

### Test File Locations and Structure
- **Test files**: `tests/cycod-yaml/cycod-slash-title-commands.yaml`
- **Test data**: `tests/cycod-yaml/testfiles/` directory contains sample conversation files
- **Test patterns**: Each test uses `run:` with specific cycod arguments and `expect-regex:` patterns

### Key Test File Patterns Used
- `testfiles/default-metadata-full-conversation.jsonl` - Empty title, unlocked
- `testfiles/unlocked-title-full-conversation.jsonl` - Has title, unlocked  
- `testfiles/locked-title-full-conversation.jsonl` - Has title, locked
- `testfiles/no-metadata-full-conversation.jsonl` - No metadata at all
- `testfiles/malformed-metadata.jsonl` - Corrupted metadata for error testing

### Common Test Command Patterns
```bash
# Basic pattern for testing title commands
cycod --input-chat-history <test-file> --input "/title <subcommand>" --input exit --openai-api-key fake --auto-save-chat-history 0 --use-openai

# Pattern for testing without files (should fail)
cycod --input "/title <subcommand>" --input exit --openai-api-key fake --auto-save-chat-history 0 --use-openai

# Pattern for testing with auto-save (creates new files)
cycod --input "/title <subcommand>" --input exit --openai-api-key fake --auto-save-chat-history 1 --use-openai
```

### Regex Patterns for Expected Output
```regex
# Title display pattern
Title:\s+<expected-title>\r?\n
Status:\s+<locked|unlocked>\r?\n

# Success message patterns
Title updated to: "<title>" \(locked from AI changes\)\r?\n
Title locked from AI changes\.\r?\n
Title unlocked - AI can now regenerate the title\.\r?\n

# Error message patterns
Error: Titles must be enclosed in double quotes\. Usage: /title set \"<title>\"\r?\n
Error: No conversation file to save metadata to\. Use --input-chat-history or create a conversation first\.\r?\n
```

### Test Categories Already Covered
1. **Default Command Tests** - `/title` with no subcommand
2. **View Command Tests** - `/title view` in various states
3. **Set Command Tests** - `/title set "title"` with validation
4. **Lock/Unlock Tests** - State management and idempotency
5. **Quote Parsing Tests** - Edge cases with malformed quotes
6. **File System Tests** - Read-only files, missing files
7. **Metadata Corruption Tests** - Graceful fallback behavior
8. **Extensibility Tests** - Preservation of unknown metadata fields
9. **Unicode Support Tests** - International characters and emoji
10. **Stress Tests** - Rapid sequential operations

### Missing Test Areas (Potential Gaps)
1. **`/title refresh` command** - Not extensively tested in current suite
2. **Generation in progress states** - Testing notification system integration
3. **Long-running async operations** - Title generation timing
4. **Cross-platform path handling** - Windows vs Unix paths
5. **Large file handling** - Performance with big conversation files
6. **Network/subprocess failures** - cycodmd subprocess error handling

### Key Implementation Constants
```csharp
// From TitleGenerationHelpers.cs
public const int TitleGenerationTimeoutMs = 30000;
public const int MaxTitleDisplayLength = 80;
public const int TitleTruncationLength = 77;

// From ConsoleTitleHelper.cs  
private const string DefaultTitle = "cycod";
private const string NoTitleSetText = "No title set";
private const string GeneratedTitlePrefix = "conversation-";
```

### Environment Variables That Affect Testing
- `CYCOD_DISABLE_TITLE_GENERATION=true` - Disables automatic title generation
- `CYCOD_AUTO_SAVE_CHAT_HISTORY=false` - Used during title generation to prevent loops
- `CYCOD_AUTO_SAVE_TRAJECTORY=false` - Used during title generation to prevent loops  
- `CYCOD_AUTO_SAVE_LOG=false` - Used during title generation to prevent loops

### File Format Details
**Metadata line format (first line of JSONL):**
```json
{"_meta":{"title":"My Title","titleLocked":true}}
```

**Chat message format (subsequent lines):**
```json
{"role":"user","content":"Hello"}
{"role":"assistant","content":"Hi there!"}
```