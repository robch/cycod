# cycodgh - GitHub Search CLI

## Goal

Create a new cycod CLI tool called `cycodgh` that enables searching for GitHub repositories and optionally cloning them locally. This tool extends the cycod ecosystem with GitHub-specific functionality.

## Purpose

The primary goal is to provide a command-line interface for:
1. **Searching GitHub repositories** by keywords
2. **Outputting repository URLs** in a format similar to `cycodmd web search`
3. **"Hydrating" repositories** by cloning them locally (similar to how web search can download page content)

## Initial Scope: Search Verb Only

For the initial implementation, we're focusing on a single verb: **`search`**

### Basic Usage
```bash
# Search for repositories and output URLs
cycodgh search dotnet cli tools

# Search and clone top repositories
cycodgh search --hydrate dotnet cli tools

# Limit number of results
cycodgh search --max-results 5 dotnet cli tools
```

## Output Format

### URL Output (Default)
Similar to `cycodmd web search`, the default behavior outputs just the repository URLs:
```
https://github.com/user1/repo1
https://github.com/user2/repo2
https://github.com/user3/repo3
```

This makes it easy to pipe to other tools or process programmatically.

### Hydration (Repository Cloning)
When the `--hydrate` (or `--clone`) flag is used:
- Clone repositories to a local directory
- Default to a configurable location (e.g., `~/.cycodgh/repos/` or current directory)
- Default max hydration limit: **3-5 repositories** (configurable)
- Prevents accidentally cloning too many repos

Example behavior:
```bash
cycodgh search --hydrate --max-hydrate 3 machine-learning
# Outputs URLs AND clones top 3 repos to local directory
```

## Architecture

### Follow cycodmd Patterns

The implementation should closely mirror `cycodmd`'s structure:

1. **Command-line Parsing**
   - `CommandLine/CycoGhCommandLineOptions.cs`
   - Parse verbs (start with `search`)
   - Parse options (`--hydrate`, `--max-results`, `--max-hydrate`, etc.)

2. **Commands**
   - `CommandLineCommands/SearchCommand.cs`
   - Inherits from base command class
   - Implements `Validate()`, `GetCommandName()`, `IsEmpty()`

3. **Helpers**
   - `Helpers/GitHubSearchHelpers.cs`
   - Encapsulate GitHub API or GH CLI interaction
   - Handle authentication, search execution, result parsing

4. **Settings Integration**
   - Follow cycod settings patterns (global/user/local scopes)
   - GitHub token configuration
   - Default clone directory
   - Default max results/hydration limits

5. **Help System**
   - Mirror help structure from other cycod tools
   - Consistent with cycodmd and cycodt help patterns

## Implementation Approach

### Option 1: GitHub CLI (gh)
**Pros:**
- Simpler integration - shell out to existing tool
- Handles authentication automatically
- Well-tested, maintained by GitHub

**Cons:**
- Requires gh CLI installed
- Less control over output format
- Parsing text output

### Option 2: GitHub REST API
**Pros:**
- Direct API access
- Full control over requests/responses
- JSON parsing (clean data)

**Cons:**
- More complex implementation
- Need to handle authentication/tokens
- Rate limiting considerations

### Recommended: Start with GH CLI
- Faster initial implementation
- Leverage existing tool
- Can migrate to API later if needed

## Command Structure

### SearchCommand Properties
```csharp
public class SearchCommand : CycoGhCommand
{
    public List<string> Keywords { get; set; }
    public int MaxResults { get; set; } = 10;
    public bool Hydrate { get; set; } = false;
    public int MaxHydrate { get; set; } = 3;
    public string? CloneDirectory { get; set; }
    public bool IncludeForks { get; set; } = false;
    public string? Language { get; set; }
    public string? SortBy { get; set; } // stars, forks, updated
}
```

### Command-line Options
- `search` - The verb
- `<keywords...>` - Search terms (positional)
- `--max-results <n>` - Limit search results (default: 10)
- `--hydrate` / `--clone` - Clone repositories locally
- `--max-hydrate <n>` - Max repos to clone (default: 3)
- `--clone-dir <path>` - Where to clone repos
- `--language <lang>` - Filter by programming language
- `--sort <field>` - Sort by stars, forks, updated
- `--include-forks` - Include forked repositories

## Future Considerations

While focusing on `search` initially, consider future verbs:
- `get` - Clone a specific repository
- `trending` - Show trending repositories
- `user` - Search/list user's repositories
- `org` - Search/list organization repositories
- `issues` - Search issues across repositories
- `code` - Search code within repositories

## Settings/Configuration

Store in cycod settings files (following existing patterns):
```json
{
  "cycodgh": {
    "github-token": "ghp_...",
    "default-clone-dir": "~/.cycodgh/repos",
    "max-results": 10,
    "max-hydrate": 3,
    "default-sort": "stars"
  }
}
```

## Success Criteria

Initial implementation is successful when:
1. Can search GitHub for repositories by keywords
2. Outputs repository URLs (one per line)
3. Can clone repositories with `--hydrate` flag
4. Respects max limits for both search and hydration
5. Help system works consistently with other cycod tools
6. Settings integration matches cycod patterns

## Development Notes

- Review `src/cycodmd/` for structural patterns
- Review `src/cycod/` for settings integration
- Use existing helper classes where possible
- Follow C# coding style from project guidelines
- Keep minimal dependencies (leverage existing cycod infrastructure)
- Write tests using cycodt framework

## Related Tools in cycod Ecosystem

- **cycod**: Main AI chat CLI
- **cycodmd**: Markdown/documentation processing, web search
- **cycodt**: Testing framework
- **cycodgh**: (this tool) GitHub repository search and management
