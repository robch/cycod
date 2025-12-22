using Microsoft.Extensions.AI;

namespace CycoDev.ChatPipeline.Stages;

/// <summary>
/// Stage that executes detected function calls.
/// </summary>
public class FunctionExecutionStage : IPipelineStage
{
    public string Name => "FunctionExecution";
    public HookPoint PreHookPoint => HookPoint.PreFunctionExecution;
    public HookPoint PostHookPoint => HookPoint.PostFunctionExecution;
    
    public async Task<StageResult> ExecuteAsync(ChatContext context)
    {
        if (context.Pending.PendingToolCalls.Count == 0)
        {
            // No tool calls to execute
            return StageResult.Continue();
        }
        
        if (context.FunctionFactory == null)
        {
            return StageResult.Error("FunctionFactory is null");
        }
        
        // Build assistant message with function calls
        var responseContent = context.Pending.ResponseContent;
        var emptyResponseContent = string.IsNullOrEmpty(responseContent);
        if (emptyResponseContent) responseContent = "Calling function(s)...";
        
        var assistantContent = context.Pending.PendingToolCalls
            .AsAIContentList()
            .Prepend(new TextContent(responseContent))
            .ToList();
        
        // Add assistant message with tool calls (with hooks)
        if (context.HookExecutor != null)
        {
            await context.HookExecutor(HookPoint.PreAssistantMessageWithToolCalls, context, null);
        }
        context.Messages.Add(new ChatMessage(ChatRole.Assistant, assistantContent));
        if (context.HookExecutor != null)
        {
            await context.HookExecutor(HookPoint.PostAssistantMessageWithToolCalls, context, null);
        }
        context.Callbacks.MessageCallback?.Invoke(context.Messages);
        
        // Execute functions
        var functionResultContents = new List<AIContent>();
        
        try
        {
            ConsoleHelpers.WriteDebugLine($"Calling functions: {string.Join(", ", context.Pending.PendingToolCalls.Select(call => call.Name))}");
            
            foreach (var functionCall in context.Pending.PendingToolCalls)
            {
                // Check for cancellation
                context.CancellationToken.ThrowIfCancellationRequested();
                
                // Get approval (default to true if no callback)
                var approved = context.Callbacks.ApproveFunctionCall?.Invoke(functionCall.Name, functionCall.Arguments) ?? true;
                
                // Execute or skip based on approval
                var functionResult = approved
                    ? CallFunction(functionCall, context.FunctionFactory, context.Callbacks.FunctionCallCallback)
                    : DontCallFunction(functionCall, context.Callbacks.FunctionCallCallback);
                
                // Handle data content separately
                var asDataContent = functionResult as DataContent;
                if (asDataContent != null)
                {
                    functionResultContents.Add(asDataContent);
                    functionResultContents.Add(new FunctionResultContent(functionCall.CallId, "attaching data content"));
                }
                else
                {
                    functionResultContents.Add(new FunctionResultContent(functionCall.CallId, functionResult));
                }
            }
            
            // Add function results to conversation
            var attachToToolMessage = functionResultContents
                .Where(c => c is FunctionResultContent)
                .Cast<AIContent>()
                .ToList();
            
            // Add tool results (with hooks)
            if (context.HookExecutor != null)
            {
                await context.HookExecutor(HookPoint.PreToolResultsAdd, context, null);
            }
            context.Messages.Add(new ChatMessage(ChatRole.Tool, attachToToolMessage));
            if (context.HookExecutor != null)
            {
                await context.HookExecutor(HookPoint.PostToolResultsAdd, context, null);
            }
            context.Callbacks.MessageCallback?.Invoke(context.Messages);
            
            // Handle other content (like data content)
            var otherContentToAttach = functionResultContents
                .Where(c => c is not FunctionResultContent)
                .ToList();
            
            if (otherContentToAttach.Any())
            {
                var hasTextContent = otherContentToAttach.Any(c => c is TextContent);
                if (!hasTextContent)
                {
                    otherContentToAttach.Insert(0, new TextContent("attached content:"));
                }
                
                // Add injected content (with hooks)
                if (context.HookExecutor != null)
                {
                    await context.HookExecutor(HookPoint.PreInjectedContentAdd, context, null);
                }
                context.Messages.Add(new ChatMessage(ChatRole.User, otherContentToAttach));
                if (context.HookExecutor != null)
                {
                    await context.HookExecutor(HookPoint.PostInjectedContentAdd, context, null);
                }
                context.Callbacks.MessageCallback?.Invoke(context.Messages);
            }
            
            // Mark that functions were called
            context.State.FunctionsWereCalled = true;
            
            return StageResult.Continue();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            // Interrupted during function execution
            context.State.IsInterrupted = true;
            throw;
        }
    }
    
    private object CallFunction(
        FunctionCallDetector.ReadyToCallFunctionCall functionCall,
        FunctionFactory functionFactory,
        Action<string, string, object?>? functionCallCallback)
    {
        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, null);
        
        ConsoleHelpers.WriteDebugLine($"Calling function: {functionCall.Name} with arguments: {functionCall.Arguments}");
        var functionResult = functionFactory.TryCallFunction(functionCall.Name, functionCall.Arguments, out var functionResponse)
            ? functionResponse ?? "Function call succeeded"
            : $"Function not found or failed to execute: {functionResponse}";
        ConsoleHelpers.WriteDebugLine($"Function call result: {functionResult}");
        
        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, functionResult);
        
        return functionResult;
    }
    
    private object DontCallFunction(
        FunctionCallDetector.ReadyToCallFunctionCall functionCall,
        Action<string, string, object?>? functionCallCallback)
    {
        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, null);
        
        ConsoleHelpers.WriteDebugLine($"Function call not approved: {functionCall.Name} with arguments: {functionCall.Arguments}");
        var functionResult = "User did not approve function call";
        
        functionCallCallback?.Invoke(functionCall.Name, functionCall.Arguments, functionResult);
        
        return functionResult;
    }
}
