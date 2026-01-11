# Chat Context Menu Tests

This directory contains tests for the interactive context menu in `cycod chat`.

## Feature

When chatting with cycod interactively, pressing ENTER on an empty line now shows a context menu with options:
- **Continue chatting** - Returns to the chat prompt
- **Reset conversation** - Clears the chat history and starts fresh
- **Exit** - Exits the chat

## Interactive Test

Run `./test-context-menu.sh` to start an interactive session and manually test:

1. Empty input shows menu
2. Menu navigation works (arrow keys)
3. Menu selection works (Enter)
4. "Continue chatting" returns to prompt
5. "Reset conversation" clears history
6. "Exit" exits cleanly

## Technical Details

- Context menu only appears in interactive mode (not when input is piped)
- Uses `ListBoxPicker.PickIndexOf()` from the ConsoleGui library
- Integrates with `FunctionCallingChat.ClearChatHistory()` for reset functionality
- Preserves existing behavior for piped input and explicit "exit" command

## Implementation

See `src/cycod/CommandLineCommands/ChatCommand.cs`:
- Added `using ConsoleGui.Controls;`
- Modified main loop to detect empty input and show menu
- Added reset logic to clear chat history and reset title generation flag
