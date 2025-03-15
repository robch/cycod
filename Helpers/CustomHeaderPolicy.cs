using OpenAI;
using System.ClientModel.Primitives;

// Custom pipeline policy that injects a custom header into each request
public class CustomHeaderPolicy : PipelinePolicy
{
    private readonly string _headerName;
    private readonly string _headerValue;

    public CustomHeaderPolicy(string headerName, string headerValue)
    {
        _headerName = headerName;
        _headerValue = headerValue;
    }

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        // Set the custom header on the request
        message.Request.Headers.Set(_headerName, _headerValue);

        // Proceed with the next policy in the pipeline
        if (currentIndex + 1 < pipeline.Count)
        {
            pipeline[currentIndex + 1].Process(message, pipeline, currentIndex + 1);
        }
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        // Set the custom header on the request
        message.Request.Headers.Set(_headerName, _headerValue);

        // Proceed with the next policy in the pipeline
        if (currentIndex + 1 < pipeline.Count)
        {
            await pipeline[currentIndex + 1].ProcessAsync(message, pipeline, currentIndex + 1);
        }
    }
}
