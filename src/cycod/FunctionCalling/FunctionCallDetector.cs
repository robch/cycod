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
                var callId = functionCall.CallId;
                
                // Create or get the existing partial call
                if (!_partialCalls.TryGetValue(callId, out var call))
                {
                    ConsoleHelpers.WriteDebugLine($"Function call detected: {functionCall.Name} with call ID {callId}");
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
                    ConsoleHelpers.WriteDebugLine($"Function call partial arguments: {functionCall.Arguments}");
                    call.ArgumentsBuilder ??= new StringBuilder();
                    call.ArgumentsBuilder.Append(JsonSerializer.Serialize(functionCall.Arguments));
                    ConsoleHelpers.WriteDebugLine($"Function call arguments in full thus far: {call.ArgumentsBuilder}");
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
    public List<ReadyToCallFunctionCall> GetReadyToCallFunctionCalls()
    {
        ConsoleHelpers.WriteDebugLine("Getting ready to call function calls...");
        var result = new List<ReadyToCallFunctionCall>();
        foreach (var call in _partialCalls.Values)
        {
            if (!string.IsNullOrEmpty(call.Name))
            {
                var arguments = call.ArgumentsBuilder?.ToString() ?? "{}";
                ConsoleHelpers.WriteDebugLine($"Function call ready to call: {call.Name} with arguments: {call.ArgumentsBuilder}");
                result.Add(new ReadyToCallFunctionCall
                {
                    Name = call.Name,
                    CallId = call.CallId,
                    Arguments = arguments
                });
            }
        }

        ConsoleHelpers.WriteDebugLine($"Function calls ready to call: {result.Count}");        
        return result;
    }
   
    /// <summary>
    /// Creates a FunctionCallContent from a complete function call
    /// </summary>
    /// <param name="call">The complete function call</param>
    /// <returns>A FunctionCallContent</returns>
    public FunctionCallContent CreateFunctionCallContent(ReadyToCallFunctionCall call)
    {
        // Parse the arguments from JSON string to dictionary
        var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(call.Arguments);
        
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
    /// Checks if there are any partial function calls
    /// </summary>
    public bool HasFunctionCalls()
    {
        return _partialCalls.Count > 0;
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
    public class ReadyToCallFunctionCall
    {
        public string CallId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
    }

}

public static class ReadyToCallFunctionCallExtensions
{
    public static FunctionCallContent AsAIContent(this FunctionCallDetector.ReadyToCallFunctionCall call)
    {
        var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(call.Arguments);
        return new FunctionCallContent(call.CallId, call.Name, arguments);
    }

    public static IList<AIContent> AsAIContentList(this List<FunctionCallDetector.ReadyToCallFunctionCall> readyToCallFunctionCalls)
    {
        return readyToCallFunctionCalls
            .Select(call => call.AsAIContent())
            .Cast<AIContent>()
            .ToList();
    }
}
