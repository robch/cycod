using System.ClientModel.Primitives;

public class TrafficEventPolicy : PipelinePolicy
{
    public event EventHandler<PipelineRequest>? OnRequest;
    public event EventHandler<PipelineResponse>? OnResponse;

    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        FireEvents(message);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        FireEvents(message);
        await ProcessNextAsync(message, pipeline, currentIndex).ConfigureAwait(false);
    }

    public void FireEvents(PipelineMessage message)
    {
        ConsoleHelpers.WriteDebugLine($"===== FIRING EVENTS ======");
        if (message?.Request is not null)
        {
            OnRequest?.Invoke(this, message.Request);
        }
        if (message?.Response is not null)
        {
            OnResponse?.Invoke(this, message.Response);
        }
    }
}