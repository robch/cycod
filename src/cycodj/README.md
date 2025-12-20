# cycodj - Chat History Journal Tool

A CLI tool for analyzing and journaling cycod chat history files.

## Installation

```bash
dotnet tool install -g CycoDj
```

## Usage

```bash
cycodj list                      # List all conversations
cycodj list --date 2024-12-20    # Filter by date
cycodj show <conversation-id>    # Show conversation details
cycodj journal                   # Generate daily journal
cycodj branches                  # Show conversation tree
```

## Features

- Analyze JSONL chat history files
- Generate daily journals of conversations
- Detect and visualize conversation branches
- Extract meaningful user/assistant interactions
- Filter and summarize large conversations

## Documentation

Full documentation available at: https://github.com/robch/cycod

## License

MIT
