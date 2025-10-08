//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;

/// <summary>
/// Function calling tools for managing conversation metadata
/// </summary>
public class ConversationMetadataHelperFunctions
{
    private readonly ChatCommand _chatCommand;
    
    public ConversationMetadataHelperFunctions(ChatCommand chatCommand)
    {
        _chatCommand = chatCommand ?? throw new ArgumentNullException(nameof(chatCommand));
    }
    
    [Description("Updates the title of the current conversation. You should only use this function when the conversation has no title, or when the conversation has shifted significantly from the previous title. NEVER update a title more than once in a single reply.")]
    public string UpdateConversationTitle(
        [Description("A concise, descriptive title for the conversation (max 200 characters)")] string title)
    {
        try
        {
            if (title.Equals(_chatCommand.ConversationMetadata.Title, StringComparison.OrdinalIgnoreCase))
            {
                return "Title is already set to the specified value.";
            }
            _chatCommand.UpdateConversationTitle(title);
            return $"Successfully updated conversation title to: \"{title}\"";
        }
        catch (ArgumentException ex)
        {
            return $"Error updating title: {ex.Message}";
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Error updating conversation title", showToUser: false);
            return "Error: Failed to update conversation title";
        }
    }
    
    [Description("Updates the description of the current conversation. Use this freely whenever you feel the conversation has evolved, new topics have been introduced, or you want to better summarize what we've discussed. This helps maintain an accurate summary of our interaction.")]
    public string UpdateConversationDescription(
        [Description("A brief description summarizing the conversation content and purpose (max 1000 characters)")] string description)
    {
        try
        {
            _chatCommand.UpdateConversationDescription(description);
            // Return success but keep it brief to avoid cluttering the conversation
            return "Conversation description updated.";
        }
        catch (ArgumentException ex)
        {
            return $"Error updating description: {ex.Message}";
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Error updating conversation description", showToUser: false);
            return "Error: Failed to update conversation description";
        }
    }
    
    [Description("Gets the current conversation metadata including title and description.")]
    public string GetConversationMetadata()
    {
        var metadata = _chatCommand.ConversationMetadata;
        var result = new StringBuilder();
        
        result.AppendLine("Current conversation metadata:");
        result.AppendLine($"Title: {metadata.Title ?? "Not set"}");
        result.AppendLine($"Description: {metadata.Description ?? "Not set"}");
        result.AppendLine($"Created: {metadata.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        result.AppendLine($"Updated: {metadata.UpdatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return result.ToString();
    }
    
    [Description("Use this function when you determine that no metadata update is needed for the current conversation. This allows you to explicitly decline updating title or description when the existing metadata is appropriate or when the conversation doesn't warrant specific metadata.")]
    public string DeclineMetadataUpdate(
        [Description("Reason why no metadata update is needed (e.g., 'conversation too brief', 'existing title appropriate', 'no clear topic yet')")] string reason)
    {
        ConsoleHelpers.WriteDebugLine($"AI declined metadata update: {reason}");
        return $"Metadata update declined: {reason}";
    }
}