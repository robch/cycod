# cycodj - Chat History Journal Tool

A CLI tool for analyzing and journaling cycod chat history files.

## Installation

```bash
dotnet tool install -g CycoDj
```

## Usage

### List Command (Phase 1 - IMPLEMENTED)

```bash
cycodj list                      # List all conversations
cycodj list --date 2025-12-20    # Filter by date
cycodj list --last 10            # Show last N conversations
```

### Coming Soon

```bash
cycodj show <conversation-id>    # Show conversation details
cycodj journal                   # Generate daily journal
cycodj branches                  # Show conversation tree
```

## Current Features (Phase 1)

- ✅ Read and parse JSONL chat history files from `~/.cycod/history/`
- ✅ Extract conversation metadata (title, timestamps)
- ✅ Parse messages by role (user, assistant, tool, system)
- ✅ Display message counts and first user message preview
- ✅ Filter conversations by date (`--date YYYY-MM-DD`)
- ✅ Limit output to last N conversations (`--last N`)
- ✅ Color-coded console output for readability

## Planned Features

- Branch detection and visualization
- Daily journal generation
- Content summarization
- Search across conversations
- Export to markdown

## Documentation

- Project Documentation: [docs/](../../docs/)
- Implementation Plan: [docs/chat-journal-plan.md](../../docs/chat-journal-plan.md)
- Quick Start Guide: [docs/quick-start.md](../../docs/quick-start.md)

## License

MIT
