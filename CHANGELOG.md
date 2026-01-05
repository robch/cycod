# Changelog

All notable changes to the CycoD project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Console GUI Framework:
  - Ported comprehensive console GUI framework from Azure AI CLI for enhanced user interaction
  - Foundation components for interactive terminal UI (Screen, Window, Rect, Cursor)
  - Base controls for scrolling, keyboard navigation, and list management
  - Interactive controls with proper focus management and color schemes
- Interactive List Picker:
  - Added ListBoxPicker control for selecting items from lists
  - Keyboard navigation: arrow keys, Page Up/Down, Home/End
  - Type-to-filter search functionality (press '?' or Ctrl+F, or just start typing)
  - Multiple search modes: starts-with, contains, regex, character sequence
  - Tab/Shift+Tab to cycle through search matches
  - Used in chat context menu and available for future features
- Text Editing Controls:
  - Added EditBox control for single-line text input with validation
  - Insert/overwrite modes, Home/End/arrows navigation
  - Input validation with picture formats (@#, @A, custom patterns)
  - Horizontal scrolling for long text input
- Text Viewing Controls:
  - Added TextViewer control for viewing and selecting text content
  - Column navigation with left/right arrows
  - Integrated search functionality from SpeedSearchListBoxControl
  - Syntax highlighting with backtick markers
- Interactive Help System:
  - Added HelpViewer control for interactive help documentation
  - Clickable help links that execute commands
  - URL detection and browser launching
  - "Try it" command execution support
  - Custom key bindings (Ctrl+H, Tab, F3 for search)
- Testing Infrastructure:
  - Added InOutPipeServer for automated testing of interactive controls
  - JSON-based protocol for simulating user input
  - Comprehensive test suites for all GUI components (47 tests, 100% pass rate)
  - Cross-platform compatibility verified on Windows

- Chat Mode:
  - Added support for multiline input using backtick code blocks
  - Users can now paste multiline content by starting with three or more backticks and ending with a matching number of backticks
  - Added interactive context menu on empty input in interactive mode
    - Press ENTER on an empty line to display a menu with options
    - "Continue chatting" - returns to chat input
    - "Reset conversation" - clears chat history and starts fresh
    - "Exit" - cleanly exits the chat session
    - Menu navigation: use arrow keys to select, ENTER to confirm, ESC to cancel
- Speech Recognition:
  - Added `--speech` flag to enable speech-to-text input in interactive mode
  - Integration with Azure Cognitive Services Speech SDK
  - Speech input accessible via context menu when speech is enabled
  - Real-time interim results display during recognition
  - Configuration via `speech.key` and `speech.region` files in .cycod/ directory
  - Comprehensive setup documentation in docs/speech-setup.md

### Fixed
- GitHub Copilot integration: Fixed token expiration issue by implementing automatic token refresh
  - Tokens are now automatically refreshed before they expire
  - Added CopilotTokenRefreshPolicy that intercepts requests and updates authorization headers
  - Enhanced GitHubCopilotHelper to better track token expiration information

## [1.0.0-alpha-20250402] - 2025-04-02

### Added
- Slash Commands:
  - Added new slash commands (/find, /file, /files, /get, /search) that integrate with CYCODMD functionality
  - These provide convenient shortcuts for common operations like file searching and web content retrieval
- Range Support for --foreach:
  - Added support for numeric ranges in --foreach using the #..# syntax
  - Example: --foreach i in 1..5 will iterate over values 1, 2, 3, 4, 5
- Token Trimming:
  - Added functionality to trim tokens to a target limit
  - Ensures chat history doesn't exceed token limits, particularly important when using --input-chat-history

### Changed
- Behavior Changes:
  - Force non-interactive mode when using --foreach to ensure proper batch processing
- Code Refactoring:
  - Refactored profile methods into ProfileFileHelpers.cs for better organization
  - Refactored alias methods into AliasFileHelpers.cs for better organization

### Documentation
- Updated documentation for slash commands, foreach ranges, and token trimming features
- Added --prerelease flag for dotnet install instructions in README

## [1.0.0-alpha-20250401.2] - 2025-04-01

### Added
- Initial release of CycoD
- Interactive chat functionality with AI assistant
- Function calling capabilities:
  - Shell command execution (Bash, CMD, PowerShell)
  - File operations (list, view, create, edit)
  - Time and date utilities
- Command line options for customizing AI behavior
- Chat history loading and saving
- Command aliases for easy reuse
- Help system for documentation

### Changed
- N/A (initial release)

### Fixed
- N/A (initial release)