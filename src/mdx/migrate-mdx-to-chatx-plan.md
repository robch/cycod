# Merging Plan: mdx into chatx

## Proposed Directory Structure

```
chatx/
├── src/
│   ├── common/            (existing common code)
│   ├── cycod/             (existing cycod code)
│   ├── cycodt/            (existing cycodt code)
│   └── cycodmd/           (new directory for mdx-specific code)
```

## Code Migration and Reorganization

### 1. Files to Merge into common/

The following files from mdx should be evaluated and merged with their counterparts in chatx/src/common:

- **Helpers**:
  - `FileHelpers.cs`: Merge useful functionality with chatx's implementation
  - `MarkdownHelpers.cs`: Merge with common version
  - `ConsoleHelpers.cs`: Merge with common version
  - `ProcessHelpers.cs`: Merge with any equivalent in chatx
  - `EnvironmentHelpers.cs`: Merge with common version

### 2. Files to Move to cycodmd/ (Unique mdx Functionality)

These components are unique to mdx and should be moved as-is to cycodmd:

- **Converters** (entire directory):
  - `BinaryFileConverter.cs`
  - `DocxFileConverter.cs`
  - `FileConverters.cs`
  - `IFileConverter.cs`
  - `ImageFileConverter.cs`
  - `PdfFileConverter.cs`
  - `PptxFileConverter.cs`

- **Web-related Commands**:
  - `WebCommand.cs`
  - `WebGetCommand.cs`
  - `WebGetCommandLineException.cs`
  - `WebSearchCommand.cs`
  - `WebSearchCommandLineException.cs`

- **Other Helpers**:
  - `PlaywrightHelpers.cs`
  - `WebSearchHelpers.cs`
  - `GoogleApiWebSearchHelpers.cs`
  - `BingApiWebSearchHelpers.cs`

- **Core mdx-specific Components**:
  - `AiInstructionProcessor.cs`

### 3. Files to Refactor/Port to chatx Structure

These files need to be refactored to work with chatx's command structure:

- **Program Structure**:
  - Create `CycodMdProgramInfo.cs` (similar to CycoDevProgramInfo)
  - Create `CycodMdProgramRunner.cs` (similar to CycoDevProgramRunner)
  - Refactor `Program.cs` to be slim and use the ProgramRunner pattern

- **Command Line Commands**:
  - Port `FindFilesCommand.cs` to extend chatx Command classes
  - Port `RunCommand.cs` to extend chatx Command classes
  - Port all other commands to match chatx structure

## Implementation Details

### Command Structure Adaptation

1. **Current mdx Command Structure**:
   ```csharp
   abstract class Command {
       abstract public string GetCommandName();
       abstract public bool IsEmpty();
       abstract public Command Validate();
       // Other properties...
   }
   ```

2. **Target chatx Command Structure**:
   ```csharp
   abstract public class Command {
       abstract public bool IsEmpty();
       abstract public string GetCommandName();
       abstract public Task<int> ExecuteAsync(bool interactive);
       // Other methods...
   }
   ```

Key changes:
- Add `ExecuteAsync` implementation for each command
- Update to use chatx's command inheritance (Command → CommandWithVariables)
- Integrate with the ProgramRunner pattern

### Program Execution Pattern

1. **Current mdx**: Monolithic `Program.cs` with static methods
2. **Target chatx**: 
   - Slim `Program.cs` that delegates to CycodMdProgramRunner
   - CycodMdProgramRunner extends ProgramRunner

Example for new `Program.cs`:
```csharp
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await CycodMdProgramRunner.RunAsync(args);
    }
}
```

### MdxCliWrapper Integration

The MdxCliWrapper class in cycod should be updated to use the new integrated mdx functionality directly instead of spawning a separate process. This would become a direct API rather than a shell wrapper.

## Special Considerations

1. **Command Line Options**: mdx and chatx have different command line parsing approaches. mdx's approach needs to be refactored to work with chatx's pattern.

2. **Feature Parity**: Ensure all mdx functionality is preserved in the new structure.

3. **Thread Management**: mdx has its own thread management, which will need to be adapted to chatx's approach.

4. **AI Instruction Processing**: Consider whether mdx's instruction processing should be integrated with cycod's function calling capabilities.

5. **Dependencies**: Ensure all dependencies (e.g., for PDF/DOCX conversion) are properly migrated.

## Implementation Steps

1. Create the `cycodmd` directory structure
2. Move the converters and other unique mdx components
3. Create the new program structure (ProgramRunner pattern)
4. Port commands to use chatx command structure
5. Merge helper classes with common
6. Update MdxCliWrapper to use the new integrated functionality
7. Test all functionality to ensure nothing was lost in the migration

This approach ensures we maintain all functionality while integrating the code bases in a clean way that follows the existing chatx repository structure and patterns.