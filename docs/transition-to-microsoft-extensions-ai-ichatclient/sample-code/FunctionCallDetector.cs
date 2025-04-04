using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

/// <summary>
/// Helps detect and extract function calls from ChatResponseUpdate instances
/// </summary>
public class FunctionCallDetector
{
    private readonly Dictionary<string, PartialFunctionCall> _partialCalls = new();
    
    /// <summary>
    /// Checks a ChatResponseUpdate for function calls and accumulates them
    /// </summary>
    /// <param name="update">The update to check</param>
    /// <returns>True if a function call was found</returns>
    public bool CheckForFunctionCall(ChatResponseUpdate update)
    {
        bool foundCall = false;
        
        // Loop through all content in the update
        foreach (var content in update.Contents)
        {
            if (content is FunctionCallContent functionCall)
            {
                string callId = functionCall.CallId;
                
                // Create or get the existing partial call
                if (!_partialCalls.TryGetValue(callId, out var call))
                {
                    call = new PartialFunctionCall 
                    { 
                        Name = functionCall.Name, 
                        CallId = callId 
                    };
                    _partialCalls[callId] = call;
                }
                
                // Update arguments if available
                if (functionCall.Arguments != null)
                {
                    call.ArgumentsBuilder ??= new StringBuilder();
                    call.ArgumentsBuilder.Append(JsonSerializer.Serialize(functionCall.Arguments));
                }
                
                foundCall = true;
            }
        }
        
        return foundCall;
    }
    
    /// <summary>
    /// Gets all complete function calls that have been accumulated
    /// </summary>
    /// <returns>A list of complete function calls</returns>
    public List<CompleteFunctionCall> GetCompleteFunctionCalls()
    {
        var result = new List<CompleteFunctionCall>();
        
        foreach (var call in _partialCalls.Values)
        {
            if (!string.IsNullOrEmpty(call.Name) && call.ArgumentsBuilder != null)
            {
                result.Add(new CompleteFunctionCall
                {
                    Name = call.Name,
                    CallId = call.CallId,
                    Arguments = call.ArgumentsBuilder.ToString()
                });
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Converts a complete function call to an OpenAI StreamingChatToolCallUpdate
    /// </summary>
    /// <param name="call">The complete function call</param>
    /// <returns>A StreamingChatToolCallUpdate</returns>
    public StreamingChatToolCallUpdate ConvertToStreamingToolCallUpdate(CompleteFunctionCall call)
    {
        // Create a streaming tool call update that's compatible with the existing FunctionCallContext
        var update = new StreamingChatToolCallUpdate
        {
            Type = "function",
            Id = call.CallId,
            FunctionName = call.Name,
            FunctionArguments = BinaryData.FromString(call.Arguments)
        };
        
        return update;
    }
    
    /// <summary>
    /// Creates a FunctionCallContent from a complete function call
    /// </summary>
    /// <param name="call">The complete function call</param>
    /// <returns>A FunctionCallContent</returns>
    public FunctionCallContent CreateFunctionCallContent(CompleteFunctionCall call)
    {
        // Parse the arguments from JSON string to dictionary
        var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(call.Arguments);
        
        // Create a function call content
        return new FunctionCallContent(call.CallId, call.Name, arguments);
    }
    
    /// <summary>
    /// Clears all accumulated function calls
    /// </summary>
    public void Clear()
    {
        _partialCalls.Clear();
    }
    
    /// <summary>
    /// Helper class for tracking partial function calls
    /// </summary>
    private class PartialFunctionCall
    {
        public string CallId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public StringBuilder? ArgumentsBuilder { get; set; }
    }
    
    /// <summary>
    /// Represents a complete function call
    /// </summary>
    public class CompleteFunctionCall
    {
        public string CallId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
    }
}