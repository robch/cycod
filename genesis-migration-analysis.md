# Genesis Migration Analysis

Based on analysis of the C:\src\genesis\ folder structure and the content in the current branch, here's what should be moved to genesis and where it should go.

## Genesis Folder Current Structure

The genesis repository appears to be focused on:
- Fractal tool agent architecture
- Collaborative intelligence frameworks  
- Pitch materials for the vision
- Structured content organization

## Content That Should Move to Genesis

### 1. Core Vision Documents (Root Level)
These foundational documents should go at the root level of genesis alongside the existing architecture specification:

- `VISION-FRACTAL-001.md` → `C:\src\genesis\VISION-FRACTAL-001.md`
- `FRACTAL-TOOLS-VISION-AI.md` → `C:\src\genesis\FRACTAL-TOOLS-VISION-AI.md`  
- `FRACTAL-TOOLS-VISION-ROB.md` → `C:\src\genesis\FRACTAL-TOOLS-VISION-ROB.md`
- `FEATURES-FRACTAL-TOOLS-001.md` → `C:\src\genesis\FEATURES-FRACTAL-TOOLS-001.md`

### 2. Pitch Materials (pitch/ folder)
These business and market-focused documents belong in the existing pitch structure:

- `MARKET-VALIDATION-001.md` → `C:\src\genesis\pitch\MARKET-VALIDATION-001.md`
- `The Pitch - Why Authentic AI Development Changes Everything.md` → `C:\src\genesis\pitch\authentic-ai-development-pitch.md`

### 3. Research and Framework Content (new frameworks/ folder)
Create a new frameworks/ directory for research and foundational content:

- `todo/self-help-books-2016-to-2023.md` → `C:\src\genesis\frameworks/self-help-books-2016-to-2023.md`
- `todo/top-self-help-books-frameworks.jsonl` → `C:\src\genesis\frameworks/top-self-help-books-frameworks.jsonl`

### 4. AI Philosophy and Meta-Insights (new philosophy/ folder)
The extensive AI philosophy content from docs/guide-development/meta-insights/ should move to genesis:

- `docs/guide-development/meta-insights/` → `C:\src\genesis\philosophy/meta-insights/`
- `docs/guide-development/knowledge-movement-system/` → `C:\src\genesis\philosophy/knowledge-movement-system/`

This includes files like:
- identity-first-ai-development-paradigm.md
- collaborative-intelligence-emergence-patterns.md
- multi-dimensional-learning-dynamics.md
- And 30+ other philosophical framework files

### 5. Documentation Framework Process (new process/ folder)
The documentation framework content should also move to genesis since it appears to be part of the broader vision:

- `docs/framework/` → `C:\src\genesis\process/documentation-framework/`

This includes the extensive process documentation for creating and validating documentation frameworks.

## Content That Should Stay in Current Repo

### Core Coding Style Files
These are the actual purpose of the current branch and should remain:
- `docs/C#-Coding-Style-Essential.md`
- `docs/C#-Coding-Style-Expanded.md`

### Code Review Process Files (Maybe)
These are borderline - they could stay in the current repo since they're about code quality:
- `docs/Code-Review-Process.md`
- `docs/Lightweight-Code-Review-Process.md`
- `todo/code-review-tool-integration-ideas.md`

### Educational Analogy Drafts (Unclear)
The analogy-based educational materials are unclear:
- `docs/guide-development/airport-parameter-handling-draft.md`
- `docs/guide-development/cooking-null-handling-draft.md`
- And 20+ other analogy files

These could either:
1. Stay in the current repo if they're meant to be part of the coding education
2. Move to genesis if they're part of the broader educational framework vision

## Recommended Migration Strategy

1. **Create new folders in genesis:**
   ```
   C:\src\genesis\frameworks/
   C:\src\genesis\philosophy/
   C:\src\genesis\process/
   ```

2. **Move files in batches:**
   - First: Core vision documents to root
   - Second: Pitch materials to pitch folder
   - Third: Research content to frameworks folder
   - Fourth: Philosophy content to philosophy folder
   - Fifth: Process documentation

3. **Update genesis README.md** to reflect the new structure and content

4. **Clean the current branch** to focus only on coding style guidelines

## File Count Impact
- Files moving to genesis: ~150+ files
- Files staying in current repo: ~20 files (mostly coding style and code review)

This would make the current branch much more focused on its stated purpose (coding style documentation) while properly organizing the vision/framework content in the genesis repository where it belongs.