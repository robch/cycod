using System.ClientModel.Primitives;
using System.Text.Json.Nodes;
using System.ClientModel;

/// <summary>
/// Custom pipeline policy that removes a specified property from JSON request content
/// </summary>
public class CustomJsonPropertyRemovalPolicy : PipelinePolicy
{
    private readonly string _propertyName;

    /// <summary>
    /// Creates a new instance of the CustomJsonPropertyRemovalPolicy
    /// </summary>
    /// <param name="propertyName">The name of the JSON property to remove</param>
    public CustomJsonPropertyRemovalPolicy(string propertyName)
    {
        _propertyName = propertyName;
    }

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
            // Get the content as string for initial check
            using MemoryStream contentStream = new();
            message?.Request?.Content?.WriteTo(contentStream);
            contentStream.Position = 0;

            var contentData = BinaryData.FromStream(contentStream);
            var contentString = contentData.ToString();

            // Quick check if the property exists before parsing the JSON
            var quickCheckIfStringExists = contentString.Contains($"\"{_propertyName}\"") || contentString.Contains($"'{_propertyName}'");
            if (quickCheckIfStringExists)
            {
                JsonNode? jsonNode = JsonNode.Parse(contentString);
                if (jsonNode is JsonObject jsonObject)
                {
                    if (jsonObject.ContainsKey(_propertyName))
                    {
                        jsonObject.Remove(_propertyName);
                        
                        var modifiedContent = jsonObject.ToJsonString();
                        var data = BinaryData.FromString(modifiedContent);
                        var newContent = BinaryContent.Create(data);
                        message!.Request.Content = newContent;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error while trying to process JSON content: {ex.Message}");
        }
    }
}