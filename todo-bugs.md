This PR introduces a comprehensive custom tools framework for the CYCOD system, allowing users to define and execute custom tools via YAML configurations. It's a significant addition that adds flexibility to the system.

## Issues and Concerns

1. **src/common/Model/ToolDefinition.cs:81**  
   `public string? Shell { get; set; } = "default";` - The default value is set here but not consistently applied in child classes. This could lead to inconsistent behavior.

2. **src/common/Model/ToolParameter.cs:150-152**  
   The `ParseBoolean` method has hardcoded values for string-to-boolean conversion. Consider extracting these to constants or an enum.

3. **src/cycod/FunctionCalling/ToolExecutor.cs:137-145**  
   Calling `GetAwaiter().GetResult()` on the `ExecuteAsync()` task could lead to deadlocks in synchronous contexts. Consider a better async/sync boundary pattern.

4. **src/cycod/FunctionCalling/ToolExecutor.cs:175**  
   The step index starts at 0 but the displayed step number starts at 1. This inconsistency could lead to confusion when debugging.

5. **src/cycod/FunctionCalling/ToolExecutor.cs:298-299**  
   There's no validation that `finalResult` is actually set to a non-null value. If all steps fail, this could return null unexpectedly.

6. **src/cycod/FunctionCalling/ToolExecutor.cs:362-364**  
   The boolean value parsing logic is duplicated from `ToolParameter.cs`. Consider consolidating this logic.

7. **src/cycod/FunctionCalling/ToolExecutor.cs:469-490**  
   The `GetFunctionFactory` method relies on a static reference `_currentFactory` which might cause issues with concurrency or testing.

8. **src/cycod/FunctionCalling/CustomToolFunction.cs:159-172**  
   The cancellation registration logic tries to dispose a task, which isn't a standard pattern and might not work as expected.

9. **src/cycod/Tools/ToolManager.cs:65-73**  
   The debug message says "Found no tools matching criteria" when no tools are found, but doesn't indicate what scope was searched. This could make debugging harder.

## Security Concerns

1. **src/cycod/FunctionCalling/ToolExecutor.cs:365-371**  
   Parameter substitution in command execution could potentially lead to command injection if user input isn't properly sanitized.

2. **src/cycod/FunctionCalling/ToolExecutor.cs:424-431**  
   Raw parameter substitution (using `{RAW:paramName}`) bypasses the escaping logic, which could be a security risk if used improperly.

## Performance Concerns

1. **src/cycod/FunctionCalling/CustomToolFunction.cs:94-95**  
   The JSON schema is serialized and then immediately deserialized, which is inefficient. Consider building the JsonElement directly.

2. **src/cycod/FunctionCalling/ToolExecutor.cs:307-386**  
   The parameter substitution logic uses string replacement repeatedly, which could be inefficient for large strings or many replacements.

## Recommendations

1. Add comprehensive unit tests for parameter substitution, especially focused on security aspects.
2. Consider adding validation for tool dependencies to prevent circular references.
3. Implement better error handling for multi-step tools, particularly to report which step failed.
4. Consolidate duplicate code for boolean parsing and other shared functionality.
5. Add more detailed logging for debugging complex tool executions.
