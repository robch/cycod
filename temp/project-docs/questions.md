# Questions & Decisions

## Questions for Later Discussion

1. **Search Modes**: Should we support multiple search modes?
   - Repo name search (e.g., repos with "cli" in name)
   - Code search (e.g., files containing "Microsoft.Extensions.AI")
   - Topic search (e.g., repos tagged with "machine-learning")
   - **Decision**: Start with intelligent default - if search term looks like a package/namespace, use code search; otherwise repo search

2. **File Extension Context**: When searching for "Microsoft.Extensions.AI" should we automatically:
   - Assume user wants files with .csproj extension?
   - Let user specify with --file-extension flag?
   - **Decision**: Add --in-files or --file-extension flag, default to repo search

3. **Clone Behavior**: If repo already exists in external/:
   - Skip it?
   - Update/pull it?
   - Fail with error?
   - **Decision**: Skip with warning message, add --update flag for later

4. **Submodules**: 
   - Always add cloned repos as git submodules?
   - Only with --as-submodules flag?
   - **Decision**: Separate --as-submodules flag (safer default)

5. **Default Clone Location**:
   - Always use "external/" relative to current directory?
   - Configurable in settings?
   - **Decision**: Default to "./external", add --clone-dir for override

6. **GitHub Authentication**:
   - Require gh CLI to be authenticated?
   - Support API token in settings?
   - **Decision**: Rely on gh CLI authentication first, can add token support later

## Implementation Decisions Made

- **Start with GH CLI**: Simpler, handles auth, well-tested
- **Flag naming**: --clone (more intuitive than --hydrate for repos)
- **Default folder**: external/ in current directory
- **Max results default**: 10 (reasonable for search)
- **Max clone default**: 10 (if --clone specified, clone all results unless limited)
- **Search strategy**: Use `gh search repos` for basic search, `gh search code` when searching for content in specific file types
