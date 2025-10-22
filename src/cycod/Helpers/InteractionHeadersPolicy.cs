using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.ClientModel.Primitives;

/// <summary>
/// Pipeline policy that adds interaction-related headers to requests.
/// Adds X-Interaction-Id and X-Initiator headers, which are used by
/// OpenAI's API for request tracking and attribution.
/// </summary>
public class InteractionHeadersPolicy : PipelinePolicy
{
    private readonly InteractionService _interactionService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionHeadersPolicy"/> class.
    /// </summary>
    /// <param name="interactionService">The interaction service to get the interaction ID from.</param>
    public InteractionHeadersPolicy(InteractionService interactionService)
    {
        _interactionService = interactionService;
    }
    
    /// <summary>
    /// Adds the X-Interaction-Id and X-Initiator headers to the request.
    /// </summary>
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        message.Request.Headers.Set("X-Interaction-Id", _interactionService.InteractionId);
        
        // Determine if this is a user-initiated request
        bool isUserInitiated = IsUserInitiatedRequest(message);
        message.Request.Headers.Set("X-Initiator", isUserInitiated ? "user" : "agent");
        
        ConsoleHelpers.WriteDebugLine($"Added X-Interaction-Id: {_interactionService.InteractionId}");
        ConsoleHelpers.WriteDebugLine($"Added X-Initiator: {(isUserInitiated ? "user" : "agent")}");
        
        // Proceed with the next policy in the pipeline
        ProcessNext(message, pipeline, currentIndex);
    }
    
    /// <summary>
    /// Adds the X-Interaction-Id and X-Initiator headers to the request.
    /// </summary>
    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        message.Request.Headers.Set("X-Interaction-Id", _interactionService.InteractionId);
        
        // Determine if this is a user-initiated request
        bool isUserInitiated = IsUserInitiatedRequest(message);
        message.Request.Headers.Set("X-Initiator", isUserInitiated ? "user" : "agent");
        
        ConsoleHelpers.WriteDebugLine($"Added X-Interaction-Id: {_interactionService.InteractionId}");
        ConsoleHelpers.WriteDebugLine($"Added X-Initiator: {(isUserInitiated ? "user" : "agent")}");
        
        // Proceed with the next policy in the pipeline
        await ProcessNextAsync(message, pipeline, currentIndex);
    }
    
    /// <summary>
    /// Determines if this is a user-initiated request by examining the message array using simple string operations.
    /// If the last message is from the user, it's user-initiated.
    /// If the last message is from the assistant or tool, it's agent-initiated.
    /// </summary>
    private bool IsUserInitiatedRequest(PipelineMessage message)
    {
        try
        {
            if (message?.Request?.Content == null)
                return true; // Default to user-initiated if can't determine
                
            // Read the request body to examine the messages
            using MemoryStream contentStream = new();
            message.Request.Content.WriteTo(contentStream);
            contentStream.Position = 0;
            
            var contentData = BinaryData.FromStream(contentStream);
            var contentString = contentData.ToString();
            
            // Reset the stream position for the next policy
            contentStream.Position = 0;
            
            // Use simple string operations to find role occurrences
            int lastUserRoleIndex = contentString.LastIndexOf("\"role\":\"user\"");
            int lastAssistantRoleIndex = contentString.LastIndexOf("\"role\":\"assistant\"");

            // If there's no assistant role, it's user-initiated
            // If the assistant role appears after the last user role, it's agent-initiated
            bool isUserInitiated = lastAssistantRoleIndex < 0 || lastAssistantRoleIndex < lastUserRoleIndex;
            ConsoleHelpers.WriteDebugLine($"IsUserInitiatedRequest: {isUserInitiated} (lastUserRoleIndex: {lastUserRoleIndex}, lastAssistantRoleIndex: {lastAssistantRoleIndex})");

            return isUserInitiated;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error determining if request is user-initiated: {ex.Message}");
            return true; // Default to user-initiated if there was an error
        }
    }
}