# cycod chat - Layer 1: TARGET SELECTION

## Purpose

Layer 1 defines **what inputs to process** - the primary data sources that the chat command operates on.

## Options Summary

| Option | Type | Description | Status |
|--------|------|-------------|--------|
| `--input`, `--instruction`, `--question`, `-q` | Content | User input/question | ✅ |
| `--inputs`, `--instructions`, `--questions` | Content | Multiple user inputs | ✅ |
| `--system-prompt` | Content | Override system prompt | ✅ |
| `--add-system-prompt` | Content | Add to system prompt | ✅ |
| `--add-user-prompt`, `--prompt` | Content | Add persistent user message | ✅ |
| `--chat-history` | File | Both input and output history file | ✅ |
| `--input-chat-history` | File | Load conversation history | ✅ |
| `--continue` | Flag | Load most recent history | ✅ |
| `--image` | Patterns | Add images to conversation | ✅ |
| `--var`, `--vars` | Variables | Template variables | ✅ |
| `--foreach` | Iteration | Iterate over variable values | ✅ |
| Positional (stdin) | Content | Input from stdin | ✅ |
| Built-in prompts | Content | `/prompt-name` references | 🔍 |
| AGENTS.md | Content | Project context | 🔍 |

## Details

### Input Instructions

The primary way to provide input to the chat command.

#### Single Input
```bash
cycod --input "What is the weather?"
cycod --instruction "Explain async/await"
cycod --question "How does this work?"  # Also sets --quiet, non-interactive
cycod -q "Quick question"                # Alias for --question
```

#### Multiple Inputs
```bash
cycod --inputs "First question" "Second question"
cycod --instructions "Do this" "Then do that"
cycod --questions "Q1" "Q2"  # Also sets --quiet, non-interactive
```

#### From File
```bash
cycod --input @instructions.txt
cycod --inputs @task1.txt @task2.txt
```

#### From Stdin
```bash
echo "Hello AI" | cycod
cat instructions.txt | cycod
cycod < input.txt

# Implicit stdin usage with redirection
cycod | grep "result"  # Reads from stdin if no --input provided
```

**Special Behavior**: 
- `--question`/`-q` aliases set `--quiet` and `--interactive false`
- If no input and stdin redirected, automatically reads from stdin
- Empty strings in `--inputs` are allowed

### System Prompts

Control the AI's system-level instructions.

#### Override System Prompt
```bash
cycod --system-prompt "You are a helpful coding assistant."
cycod --system-prompt @custom-prompt.txt
```

#### Add to System Prompt
```bash
cycod --add-system-prompt "Additional context about the project"
cycod --add-system-prompt @project-guidelines.txt
```

**Multiple additions** are joined with `\n\n`.

### User Prompts

Persistent user messages that appear before each turn.

```bash
cycod --add-user-prompt "Remember to be concise"
cycod --prompt "Format output as markdown"  # --prompt adds "/" prefix if missing
cycod --prompt "/code-review"               # Use prompt file
```

### Chat History

Load previous conversations and save new ones.

#### Bidirectional History
```bash
cycod --chat-history chat-history.jsonl
# Loads if exists, saves to same file
```

#### Input Only
```bash
cycod --input-chat-history previous-chat.jsonl
```

#### Continue Most Recent
```bash
cycod --continue
# Loads the most recent chat history file
```

#### Output Only
```bash
cycod --output-chat-history new-conversation-{time}.jsonl
```

**File Templates**:
- `{time}`: timestamp
- `{filePath}`, `{fileBase}`, `{fileExt}`: file components
- Template variables from `--var`

### Images

Add images to the conversation.

```bash
cycod --image screenshot.png
cycod --image "diagrams/*.png"
cycod --image "*.jpg" "*.png"
```

**Image Resolution**:
- Glob patterns are expanded
- Multiple patterns supported
- Images attached to next user message

### Template Variables

Define variables for template substitution.

#### Single Variable
```bash
cycod --var "name=John" --input "Hello {{name}}"
```

#### Multiple Variables
```bash
cycod --vars "name=John" "age=30" --input "{{name}} is {{age}}"
```

#### For-Each Iteration
```bash
cycod --foreach "file in *.cs" --input "Process {{file}}"
# Runs the command once per file
```

**Variable Sources**:
- `--var` / `--vars`: Command-line definitions
- `AGENTS.md`: Automatically added as `{{agents.md}}`, `{{agents.file}}`, `{{agents.path}}`
- Environment: Accessible via templates

### Implicit Inputs

#### Built-in Prompts

```bash
cycod --system-prompt code-review
# If "code-review" exists as /code-review, expands it

cycod --add-user-prompt /analyze
# Slash prompts are automatically expanded
```

#### AGENTS.md

If an `AGENTS.md` file exists in the current directory or parent directories:
- Content available as `{{agents.md}}`
- Filename available as `{{agents.file}}`
- Full path available as `{{agents.path}}`

## Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│ TARGET SELECTION (Layer 1)                                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Input Sources:                                               │
│  ┌──────────────────┐                                         │
│  │ --input/-q       │─────┐                                   │
│  │ --inputs         │─────┤                                   │
│  │ stdin            │─────┤                                   │
│  └──────────────────┘     │                                   │
│                            ├──→ InputInstructions[]           │
│  ┌──────────────────┐     │                                   │
│  │ --system-prompt  │─────┤                                   │
│  │ --add-system-*   │─────┤                                   │
│  └──────────────────┘     │                                   │
│                            ├──→ SystemPrompt                  │
│  ┌──────────────────┐     │    SystemPromptAdds[]            │
│  │ --add-user-*     │─────┤                                   │
│  │ --prompt         │─────┘                                   │
│  └──────────────────┘          UserPromptAdds[]              │
│                                                               │
│  History Sources:                                             │
│  ┌──────────────────┐                                         │
│  │ --chat-history   │─────┐                                   │
│  │ --input-*-history│─────├──→ InputChatHistory              │
│  │ --continue       │─────┘    LoadMostRecentChatHistory    │
│  └──────────────────┘                                         │
│                                                               │
│  Additional Sources:                                          │
│  ┌──────────────────┐                                         │
│  │ --image          │─────→ ImagePatterns[]                  │
│  │ --var / --vars   │─────→ Variables{}                      │
│  │ --foreach        │─────→ ForEachVariables[]               │
│  │ AGENTS.md        │─────→ Variables{"agents.md"}           │
│  └──────────────────┘                                         │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                             │
                             ▼
                    [Layer 3: Content Filter]
                    (Template processing)
```

## Proof

See [cycod-chat-layer-1-proof.md](cycod-chat-layer-1-proof.md) for source code evidence.
