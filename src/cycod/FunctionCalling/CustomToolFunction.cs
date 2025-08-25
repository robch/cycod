using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

/// <summary>
/// AIFunction implementation for custom tools.
/// </summary>
public class CustomToolFunction : AIFunction
{
    private readonly ToolDefinition _toolDefinition;
    private JsonElement? _cachedJsonSchema;
    
    /// <summary>
    /// Creates a new CustomToolFunction from a tool definition.
    /// </summary>
    /// <param name="toolDefinition">The tool definition to wrap.</param>
    public CustomToolFunction(ToolDefinition toolDefinition)
    {
        _toolDefinition = toolDefinition;
    }
    
    public override string Name => _toolDefinition.Name;
    public override string Description => _toolDefinition.Description;
    
    /// <summary>
    /// Gets the JSON Schema for this function.
    /// </summary>
    public override JsonElement JsonSchema
    {
        get
        {
            if (_cachedJsonSchema.HasValue)
            {
                return _cachedJsonSchema.Value;
            }
            
            // Create the parameters object
            var schemaObject = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>()
            };
            
            // Create a list of required parameters
            var requiredParameters = new List<string>();
            
            // Add each parameter to the properties object
            foreach (var param in _toolDefinition.Parameters)
            {
                var paramName = param.Key;
                var paramDef = param.Value;
                
                // Create parameter schema
                var paramSchema = new Dictionary<string, object>
                {
                    ["type"] = ConvertToolParameterTypeToSchemaType(paramDef.Type),
                    ["description"] = paramDef.Description
                };
                
                // Add default value if present
                if (paramDef.Default != null)
                {
                    paramSchema["default"] = paramDef.Default;
                }
                
                // Add to properties
                ((Dictionary<string, object>)schemaObject["properties"])[paramName] = paramSchema;
                
                // Add to required list if parameter is required
                if (paramDef.Required)
                {
                    requiredParameters.Add(paramName);
                }
            }
            
            // Add required parameters array if any parameters are required
            if (requiredParameters.Count > 0)
            {
                schemaObject["required"] = requiredParameters;
            }
            
            // Set additionalProperties to false
            schemaObject["additionalProperties"] = false;
            
            // Convert to JsonElement and cache it
            var json = JsonSerializer.Serialize(schemaObject);
            _cachedJsonSchema = JsonDocument.Parse(json).RootElement;
            
            return _cachedJsonSchema.Value;
        }
    }
    
    /// <summary>
    /// Converts tool parameter types to schema types.
    /// </summary>
    private string ConvertToolParameterTypeToSchemaType(string toolType)
    {
        // Convert from tool parameter types to schema types
        return toolType switch
        {
            "string" => "string",
            "number" => "number",
            "integer" => "integer",
            "boolean" => "boolean",
            "array" => "array",
            "object" => "object",
            _ => "string" // Default to string for unknown types
        };
    }
    
    /// <summary>
    /// Core implementation of the function invocation.
    /// </summary>
    /// <param name="arguments">Arguments for the function call.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the function execution.</returns>
    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        try
        {
            // Convert AIFunctionArguments to IDictionary<string, object?>
            var parameters = new Dictionary<string, object?>();
            foreach (var param in arguments)
            {
                parameters[param.Key] = param.Value;
            }
            
            // Execute the tool and return the result
            var executor = new ToolExecutor(_toolDefinition, parameters);
            var resultTask = executor.ExecuteAsync();
            
            // If the task is already completed, return the result directly
            if (resultTask.IsCompleted)
            {
                return new ValueTask<object?>(resultTask.Result);
            }
            
            // Otherwise, create a ValueTask from the Task
            return new ValueTask<object?>(ConvertTaskToValueTaskAsync(resultTask, cancellationToken));
        }
        catch (Exception ex)
        {
            return new ValueTask<object?>(Task.FromException<object?>(ex));
        }
    }
    
    private async Task<object?> ConvertTaskToValueTaskAsync(Task<object?> task, CancellationToken cancellationToken)
    {
        // Register the cancellation token to cancel the task if requested
        using var _ = cancellationToken.Register(() => 
        {
            try
            {
                // Try to cancel the task if it supports cancellation
                if (task is Task taskAsCancelable && 
                    taskAsCancelable.Status != TaskStatus.RanToCompletion && 
                    taskAsCancelable.Status != TaskStatus.Faulted && 
                    taskAsCancelable.Status != TaskStatus.Canceled)
                {
                    // This will only work if the task supports cancellation
                    (taskAsCancelable as IDisposable)?.Dispose();
                }
            }
            catch
            {
                // Ignore errors during cancellation
            }
        }, useSynchronizationContext: false);
        
        // Await the task and return its result
        return await task.ConfigureAwait(false);
    }
}