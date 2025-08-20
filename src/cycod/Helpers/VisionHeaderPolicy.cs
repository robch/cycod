using System.ClientModel.Primitives;
using System.Text.Json.Nodes;
using System.ClientModel;
using System.Text.Json;
using System.Text;

/// <summary>
/// Custom pipeline policy that adds the Copilot-Vision-Request header
/// when the request payload contains image content
/// </summary>
public class VisionHeaderPolicy : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Processing request");

        // Check if message contains image content
        if (ContainsImageContent(message))
        {
            // Add the vision header
            message.Request.Headers.Set("Copilot-Vision-Request", "true");
            ConsoleHelpers.WriteDebugLine("Added Copilot-Vision-Request header for image content");
        }

        // Proceed with the next policy in the pipeline
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ConsoleHelpers.WriteDebugLine("VisionHeaderPolicy: Processing request (async)");

        // Check if message contains image content
        if (ContainsImageContent(message))
        {
            // Add the vision header
            message.Request.Headers.Set("Copilot-Vision-Request", "true");
            ConsoleHelpers.WriteDebugLine("Added Copilot-Vision-Request header for image content");
        }

        // Proceed with the next policy in the pipeline
        await ProcessNextAsync(message, pipeline, currentIndex);
    }

    private bool ContainsImageContent(PipelineMessage message)
    {
        try
        {
            // Get the content as string
            if (message?.Request?.Content == null)
                return false;

            using MemoryStream contentStream = new();
            message.Request.Content.WriteTo(contentStream);
            contentStream.Position = 0;

            var contentData = BinaryData.FromStream(contentStream);
            var contentString = contentData.ToString();

            if (!contentString.Contains("attached content")) return false;
            if (!contentString.Contains("\"type\":\"image")) return false;
            if (!contentString.Contains("\"url\":\"data:image")) return false;

            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error while checking for image content: {ex.Message}");
            return false;
        }
    }
}
