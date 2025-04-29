using Azure;
using OpenAI;
using OpenAI.Chat;
using Azure.AI.OpenAI;
using Azure.Identity;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class OpenAIChatCompletionsClass
{    
    public static void Configure(string openAIEndpoint, string openAIAPIKey, string openAIChatDeploymentName, string openAISystemPrompt)
    {
        _configuredOpenAIEndpoint = openAIEndpoint;
        _configuredOpenAIAPIKey = openAIAPIKey;
        _configuredOpenAIChatDeploymentName = openAIChatDeploymentName;
        _configuredOpenAISystemPrompt = openAISystemPrompt ?? "You are a helpful assistant.";
    }

    public static bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_configuredOpenAIEndpoint) && !string.IsNullOrEmpty(_configuredOpenAIAPIKey) && !string.IsNullOrEmpty(_configuredOpenAIChatDeploymentName) && !string.IsNullOrEmpty(_configuredOpenAISystemPrompt);
    }

    public OpenAIChatCompletionsClass(string openAIEndpoint = null, string openAIAPIKey = null, string openAIChatDeploymentName = null, string openAISystemPrompt = null)
    {
        openAIEndpoint = string.IsNullOrEmpty(openAIEndpoint) ? _configuredOpenAIEndpoint : openAIEndpoint;
        openAIAPIKey = string.IsNullOrEmpty(openAIAPIKey) ? _configuredOpenAIAPIKey : openAIAPIKey;
        openAIChatDeploymentName = string.IsNullOrEmpty(openAIChatDeploymentName) ? _configuredOpenAIChatDeploymentName : openAIChatDeploymentName;
        openAISystemPrompt = string.IsNullOrEmpty(openAISystemPrompt) ? _configuredOpenAISystemPrompt : openAISystemPrompt;

        _openAISystemPrompt = openAISystemPrompt;

        _client = string.IsNullOrEmpty(openAIAPIKey)
            ? new AzureOpenAIClient(new Uri(openAIEndpoint), new DefaultAzureCredential())
            : new AzureOpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIAPIKey));

        _chatClient = _client.GetChatClient(openAIChatDeploymentName);
        _messages = new List<ChatMessage>();

        ClearConversation();
    }

    public void ClearConversation()
    {
        _messages.Clear();
        _messages.Add(ChatMessage.CreateSystemMessage(_openAISystemPrompt));
    }

    public string GetChatCompletion(string userPrompt)
    {
        var userMessage = ChatMessage.CreateUserMessage(userPrompt);
        return GetChatCompletion(userMessage);
    }

    public string GetChatCompletion(string userPrompt, string imageFileName)
    {
        var imageBytes = File.ReadAllBytes(imageFileName);
        var contentType = ImageTypeDetector.GetContentType(imageFileName, imageBytes);

        var detail = ImageChatMessageContentPartDetail.High;
        var imagePart = ChatMessageContentPart.CreateImageMessageContentPart(BinaryData.FromBytes(imageBytes), contentType, detail);
        var userPart = ChatMessageContentPart.CreateTextMessageContentPart(userPrompt);

        return GetChatCompletion(new UserChatMessage(imagePart, userPart));
    }

    public string GetChatCompletion(ChatMessage userMessage)
    {
        _messages.Add(userMessage);

        var response = _chatClient.CompleteChat(_messages);
        var responseContent = string.Join("", response.Value.Content
            .Where(x => x.Kind == ChatMessageContentPartKind.Text)
            .Select(x => x.Text)
            .ToList());

        _messages.Add(ChatMessage.CreateAssistantMessage(responseContent));
        return responseContent;
    }

    private string _openAISystemPrompt;
    private OpenAIClient _client;
    private ChatClient _chatClient;
    private List<ChatMessage> _messages;

    private static string _configuredOpenAIEndpoint;
    private static string _configuredOpenAIAPIKey;
    private static string _configuredOpenAIChatDeploymentName;
    private static string _configuredOpenAISystemPrompt;
}
