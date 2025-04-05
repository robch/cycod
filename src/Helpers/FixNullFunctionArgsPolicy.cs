using System.ClientModel.Primitives;
using System.Text.Json.Nodes;
using System.ClientModel;

/// <summary>
/// Custom pipeline policy that fixes null function arguments in JSON request content
/// by replacing "null" with "{}" in function arguments
/// </summary>
public class FixNullFunctionArgsPolicy : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        // Process the message content if it exists
        if (message.Request?.Content != null)
        {
            ProcessMessageContent(message);
        }

        // Proceed with the next policy in the pipeline
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        // Process the message content if it exists
        if (message.Request?.Content != null)
        {
            ProcessMessageContent(message);
        }

        // Proceed with the next policy in the pipeline
        await ProcessNextAsync(message, pipeline, currentIndex);
    }

    private void ProcessMessageContent(PipelineMessage message)
    {
        try
        {
            // Get the content as string
            using MemoryStream contentStream = new();
            message?.Request?.Content?.WriteTo(contentStream);
            contentStream.Position = 0;

            var contentData = BinaryData.FromStream(contentStream);
            var contentString = contentData.ToString();

            // Quick check if the issue might exist
            var quickCheckIfNullArgsExists = contentString.Contains("\"arguments\":\"null\"");
            if (quickCheckIfNullArgsExists)
            {
                ConsoleHelpers.WriteDebugLine("Found null function arguments in request payload");
                JsonNode? jsonNode = JsonNode.Parse(contentString);
                if (jsonNode is JsonObject jsonObject)
                {
                    bool modified = FixNullArguments(jsonObject);
                    
                    if (modified)
                    {
                        var modifiedContent = jsonObject.ToJsonString();
                        var data = BinaryData.FromString(modifiedContent);
                        var newContent = BinaryContent.Create(data);
                        message!.Request.Content = newContent;
                        ConsoleHelpers.WriteDebugLine("Fixed null function arguments in request payload");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error while trying to fix function arguments in JSON content: {ex.Message}");
        }
    }

    private bool FixNullArguments(JsonObject jsonObject)
    {
        bool modified = false;

        if (jsonObject.TryGetPropertyValue("messages", out JsonNode? messagesNode) && messagesNode is JsonArray messagesArray)
        {
            foreach (JsonNode? messageNode in messagesArray)
            {
                if (messageNode is JsonObject messageObj && messageObj.TryGetPropertyValue("tool_calls", out JsonNode? toolCallsNode))
                {
                    if (toolCallsNode is JsonArray toolCallsArray)
                    {
                        foreach (JsonNode? toolCallNode in toolCallsArray)
                        {
                            if (toolCallNode is JsonObject toolCallObj && toolCallObj.TryGetPropertyValue("function", out JsonNode? functionNode))
                            {
                                if (functionNode is JsonObject functionObj && functionObj.TryGetPropertyValue("arguments", out JsonNode? argumentsNode))
                                {
                                    if (argumentsNode is JsonValue argumentsValue && argumentsValue.ToString() == "null")
                                    {
                                        functionObj["arguments"] = "{}";
                                        modified = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return modified;
    }
}