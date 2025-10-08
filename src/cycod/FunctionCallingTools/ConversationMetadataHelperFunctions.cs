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
    
    [Description("Updates the title of the current conversation. Use this when you want to give the conversation a descriptive, concise title based on the main topic or purpose.")]
    public string UpdateConversationTitle(
        [Description("A concise, descriptive title for the conversation (max 200 characters)")] string title)
    {
        try
        {
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
}