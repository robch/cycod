# cycodmd WebGet - Layer 7: Output Persistence

**[← Back to WebGet Overview](cycodmd-filtering-pipeline-catalog-README.md#3-web-get)** | **[Proof →](cycodmd-webget-layer-7-proof.md)**

## Purpose

Layer 7 (Output Persistence) for WebGet controls **where and how retrieved web page content is saved** to files. WebGet shares the same output options as WebSearch since both inherit from `WebCommand`.

## Command-Line Options

### `--save-output [file]`

**Type**: Shared option (all commands)  
**Default**: `output.md`  
**Purpose**: Save combined content from all retrieved pages to a single file

**Examples**:
```bash
# Save all pages to default output.md
cycodmd web get https://example.com/page1 https://example.com/page2 --save-output

# Save to custom file
cycodmd web get https://docs.example.com --save-output documentation.md
```

---

### `--save-page-output [template]`

**Type**: WebCommand-specific option  
**Aliases**: `--save-web-output`, `--save-web-page-output`  
**Default**: `{filePath}/{fileBase}-output.md`  
**Purpose**: Save each page's content separately using a template

**Examples**:
```bash
# Save each page separately
cycodmd web get https://example.com/doc1 https://example.com/doc2 --save-page-output

# Custom template
cycodmd web get https://api.example.com/reference --save-page-output "docs/{fileBase}.md"
```

---

### `--save-page-folder [directory]`

**Type**: WebCommand-specific option  
**Default**: `web-pages`  
**Purpose**: Save raw/original web pages to a folder for offline access

**Examples**:
```bash
# Archive pages
cycodmd web get https://example.com/important-page --save-page-folder "archive"

# Organized archival
cycodmd web get https://docs.example.com --save-page-folder "offline-docs/$(date +%Y-%m-%d)"
```

---

### `--save-chat-history [file]`

**Type**: Shared option (all commands with AI processing)  
**Default**: `chat-history-{time}.jsonl`  
**Purpose**: Save AI interaction history when using `--instructions` or `--page-instructions`

**Examples**:
```bash
# Save AI analysis history
cycodmd web get https://research-paper.com \
  --instructions "Summarize methodology" \
  --save-chat-history analysis.jsonl
```

---

## Key Differences from WebSearch

### Input Source
- **WebSearch**: Search provider returns URLs → then fetches (with `--get`)
- **WebGet**: URLs provided directly as arguments → always fetches content

### Default Behavior
- **WebSearch**: Requires `--get` to fetch content
- **WebGet**: Always fetches content (implicit `--get`)

### Use Cases
- **WebSearch**: Discovery and batch processing
- **WebGet**: Targeted retrieval of known URLs

---

## Common Patterns

### Archive Specific Pages
```bash
cycodmd web get https://blog.example.com/important-post \
  --save-page-folder "archive" \
  --save-output "archive/summary.md"
```

### Process Multiple Known URLs
```bash
cycodmd web get \
  https://docs.example.com/api \
  https://docs.example.com/tutorials \
  https://docs.example.com/faq \
  --strip \
  --save-page-output "docs/{fileBase}.md" \
  --save-output "docs/combined.md"
```

### AI-Assisted Content Extraction
```bash
cycodmd web get https://research-paper.com/paper.html \
  --strip \
  --instructions "Extract abstract, methodology, and conclusions" \
  --save-output paper-summary.md \
  --save-chat-history paper-analysis.jsonl
```

---

## Data Flow

1. **URLs Provided** (Layer 1): Command-line arguments
2. **Fetch Pages** (implicit): Download via browser automation
3. **Process Content** (Layers 2-6): Filter, convert, format
4. **AI Processing** (Layer 8, optional): Apply instructions
5. **Write Output** (Layer 7):
   - Raw pages → `--save-page-folder`
   - Processed pages → `--save-page-output`
   - Combined → `--save-output`
   - AI history → `--save-chat-history`

---

## See Also

- **[WebSearch Layer 7](cycodmd-websearch-layer-7.md)** - Identical options (shared WebCommand base)
- **[Layer 8: AI Processing](cycodmd-webget-layer-8.md)** - Generates content for chat history
- **[Proof Document](cycodmd-webget-layer-7-proof.md)** - Source code evidence

---

**[← Back to WebGet Overview](cycodmd-filtering-pipeline-catalog-README.md#3-web-get)** | **[Proof →](cycodmd-webget-layer-7-proof.md)**
