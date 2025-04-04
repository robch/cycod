using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text.Json;

public static class MessageConversionExtensions
{
    /// <summary>
    /// Converts an OpenAI ChatMessage to a Microsoft.Extensions.AI ChatMessage
    /// </summary>
    public static ChatMessage ToExtensionsAIChatMessage(this OpenAI.Chat.ChatMessage message)
    {
        // Determine the role
        var role = message switch
        {
            SystemChatMessage => ChatRole.System,
            UserChatMessage => ChatRole.User,
            AssistantChatMessage => ChatRole.Assistant,
            ToolChatMessage => ChatRole.Tool,
            DeveloperChatMessage => new ChatRole("developer"),
            _ => new ChatRole(message.Role.ToString()),
        };

        // Create the message
        var extensionsMessage = new ChatMessage { Role = role };

        // Add the participant name if available
        if (!string.IsNullOrEmpty(message.ParticipantName))
        {
            extensionsMessage.AuthorName = message.ParticipantName;
        }

        // Add the content
        foreach (var contentPart in message.Content)
        {
            if (contentPart.Kind == ChatMessageContentPartKind.Text)
            {
                extensionsMessage.Contents.Add(new TextContent(contentPart.Text));
            }
            else if (contentPart.Kind == ChatMessageContentPartKind.Image && contentPart.ImageUri != null)
            {
                extensionsMessage.Contents.Add(new UriContent(contentPart.ImageUri, contentPart.ImageBytesMediaType ?? "image/*"));
            }
            else if (contentPart.Kind == ChatMessageContentPartKind.Image && contentPart.ImageBytes != null)
            {
                extensionsMessage.Contents.Add(new DataContent(contentPart.ImageBytes.ToMemory(), contentPart.ImageBytesMediaType ?? "image/*"));
            }
            // Add more content types as needed
        }

        // Handle tool message case specifically
        if (message is ToolChatMessage toolMessage)
        {
            // Create a function result content
            extensionsMessage.Contents.Add(new FunctionResultContent(toolMessage.Id, toolMessage.Content[0].Text));
        }

        // Handle assistant message with tool calls
        if (message is AssistantChatMessage assistantMessage && assistantMessage.ToolCalls.Count > 0)
        {
            foreach (var toolCall in assistantMessage.ToolCalls)
            {
                if (toolCall.Type == "function")
                {
                    // Parse the function arguments
                    var argsBinary = toolCall.FunctionArguments;
                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(argsBinary);
                    
                    // Create a function call content
                    extensionsMessage.Contents.Add(new FunctionCallContent(toolCall.Id, toolCall.FunctionName, args));
                }
            }
        }

        return extensionsMessage;
    }

    /// <summary>
    /// Converts a Microsoft.Extensions.AI ChatMessage to an OpenAI ChatMessage
    /// </summary>
    public static OpenAI.Chat.ChatMessage ToOpenAIChatMessage(this ChatMessage message)
    {
        // Extract text content parts
        var contentParts = new List<ChatMessageContentPart>();
        foreach (var content in message.Contents)
        {
            if (content is TextContent textContent)
            {
                contentParts.Add(ChatMessageContentPart.CreateTextPart(textContent.Text));
            }
            else if (content is UriContent uriContent && uriContent.Uri != null)
            {
                if (uriContent.MediaType?.StartsWith("image/") == true)
                {
                    contentParts.Add(ChatMessageContentPart.CreateImagePart(uriContent.Uri));
                }
            }
            else if (content is DataContent dataContent)
            {
                if (dataContent.MediaType?.StartsWith("image/") == true)
                {
                    contentParts.Add(ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(dataContent.Data.ToArray()), dataContent.MediaType));
                }
            }
            // Add more content types as needed
        }

        // Create the appropriate message based on role
        OpenAI.Chat.ChatMessage resultMessage;
        if (message.Role == ChatRole.System)
        {
            resultMessage = new SystemChatMessage(contentParts);
        }
        else if (message.Role == ChatRole.User)
        {
            resultMessage = new UserChatMessage(contentParts);
        }
        else if (message.Role == ChatRole.Assistant)
        {
            var assistantMessage = new AssistantChatMessage(contentParts);
            
            // Add any function calls
            foreach (var content in message.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    var binaryArgs = BinaryData.FromString(JsonSerializer.Serialize(functionCall.Arguments));
                    assistantMessage.ToolCalls.Add(ChatToolCall.CreateFunctionToolCall(
                        functionCall.CallId,
                        functionCall.Name,
                        binaryArgs));
                }
            }
            
            resultMessage = assistantMessage;
        }
        else if (message.Role == ChatRole.Tool)
        {
            // For tool messages, find the function result content
            string toolCallId = string.Empty;
            string toolResult = string.Empty;
            
            foreach (var content in message.Contents)
            {
                if (content is FunctionResultContent functionResult)
                {
                    toolCallId = functionResult.CallId;
                    toolResult = functionResult.Result?.ToString() ?? string.Empty;
                    break;
                }
            }
            
            resultMessage = new ToolChatMessage(toolCallId, toolResult);
        }
        else if (message.Role.Name == "developer")
        {
            resultMessage = new DeveloperChatMessage(contentParts);
        }
        else
        {
            // Default to assistant for any other roles
            resultMessage = new AssistantChatMessage(contentParts);
        }

        // Set the participant name if available
        if (!string.IsNullOrEmpty(message.AuthorName))
        {
            resultMessage.ParticipantName = message.AuthorName;
        }

        return resultMessage;
    }

    /// <summary>
    /// Converts a list of OpenAI ChatMessages to Microsoft.Extensions.AI ChatMessages
    /// </summary>
    public static List<ChatMessage> ToExtensionsAIChatMessages(this IEnumerable<OpenAI.Chat.ChatMessage> messages)
    {
        return messages.Select(m => m.ToExtensionsAIChatMessage()).ToList();
    }

    /// <summary>
    /// Converts a list of Microsoft.Extensions.AI ChatMessages to OpenAI ChatMessages
    /// </summary>
    public static List<OpenAI.Chat.ChatMessage> ToOpenAIChatMessages(this IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => m.ToOpenAIChatMessage()).ToList();
    }

    /// <summary>
    /// Converts an OpenAI ChatCompletionOptions to Microsoft.Extensions.AI ChatOptions
    /// </summary>
    public static ChatOptions ToExtensionsAIChatOptions(this ChatCompletionOptions options)
    {
        var chatOptions = new ChatOptions
        {
            FrequencyPenalty = options.FrequencyPenalty,
            MaxOutputTokens = options.MaxOutputTokenCount,
            PresencePenalty = options.PresencePenalty,
            Seed = options.Seed,
            Temperature = options.Temperature,
            TopP = options.TopP
        };

        // Copy stop sequences
        if (options.StopSequences != null && options.StopSequences.Count > 0)
        {
            chatOptions.StopSequences = options.StopSequences.ToList();
        }

        // Copy tools
        if (options.Tools != null && options.Tools.Count > 0)
        {
            foreach (var tool in options.Tools)
            {
                if (tool.Type == "function")
                {
                    var aiFunction = new AIFunction(
                        tool.FunctionName,
                        tool.FunctionDescription,
                        tool.FunctionParameters.ToString());
                    
                    chatOptions.Tools.Add(aiFunction);
                }
            }
        }

        // Set tool choice mode
        if (options.ToolChoice != null)
        {
            if (options.ToolChoice.Type == "none")
            {
                chatOptions.ToolMode = new NoneChatToolMode();
            }
            else if (options.ToolChoice.Type == "auto")
            {
                chatOptions.ToolMode = new AutoChatToolMode();
            }
            else if (options.ToolChoice.Type == "required")
            {
                chatOptions.ToolMode = new RequiredChatToolMode();
            }
            else if (options.ToolChoice.Type == "function" && options.ToolChoice.FunctionName != null)
            {
                chatOptions.ToolMode = new RequiredChatToolMode(options.ToolChoice.FunctionName);
            }
        }

        // Set response format
        if (options.ResponseFormat != null)
        {
            if (options.ResponseFormat.Type == "text")
            {
                chatOptions.ResponseFormat = new ChatResponseFormatText();
            }
            else if (options.ResponseFormat.Type == "json_object")
            {
                chatOptions.ResponseFormat = new ChatResponseFormatJson();
            }
        }

        return chatOptions;
    }

    /// <summary>
    /// Converts a Microsoft.Extensions.AI ChatOptions to OpenAI ChatCompletionOptions
    /// </summary>
    public static ChatCompletionOptions ToOpenAIChatCompletionOptions(this ChatOptions options)
    {
        var completionOptions = new ChatCompletionOptions
        {
            FrequencyPenalty = options.FrequencyPenalty,
            MaxOutputTokenCount = options.MaxOutputTokens,
            PresencePenalty = options.PresencePenalty,
            Seed = options.Seed,
            Temperature = options.Temperature,
            TopP = options.TopP
        };

        // Copy stop sequences
        if (options.StopSequences != null && options.StopSequences.Count > 0)
        {
            foreach (var sequence in options.StopSequences)
            {
                completionOptions.StopSequences.Add(sequence);
            }
        }

        // Copy tools
        if (options.Tools != null && options.Tools.Count > 0)
        {
            foreach (var tool in options.Tools)
            {
                if (tool is AIFunction aiFunction)
                {
                    var binaryParams = BinaryData.FromString(aiFunction.JsonSchema);
                    
                    completionOptions.Tools.Add(ChatTool.CreateFunctionTool(
                        aiFunction.Name,
                        aiFunction.Description,
                        binaryParams));
                }
            }
        }

        // Set tool choice mode
        if (options.ToolMode != null)
        {
            if (options.ToolMode is NoneChatToolMode)
            {
                completionOptions.ToolChoice = ChatToolChoice.CreateNoneChoice();
            }
            else if (options.ToolMode is AutoChatToolMode)
            {
                completionOptions.ToolChoice = ChatToolChoice.CreateAutoChoice();
            }
            else if (options.ToolMode is RequiredChatToolMode requiredMode)
            {
                if (requiredMode.RequiredFunctionName != null)
                {
                    completionOptions.ToolChoice = ChatToolChoice.CreateFunctionChoice(requiredMode.RequiredFunctionName);
                }
                else
                {
                    completionOptions.ToolChoice = ChatToolChoice.CreateRequiredChoice();
                }
            }
        }

        // Set response format
        if (options.ResponseFormat != null)
        {
            if (options.ResponseFormat is ChatResponseFormatText)
            {
                completionOptions.ResponseFormat = OpenAI.Chat.ChatResponseFormat.CreateTextFormat();
            }
            else if (options.ResponseFormat is ChatResponseFormatJson)
            {
                completionOptions.ResponseFormat = OpenAI.Chat.ChatResponseFormat.CreateJsonObjectFormat();
            }
        }

        return completionOptions;
    }
}