namespace CycoDev.ChatPipeline.Hooks;

/// <summary>
/// Hook handler that wraps a simple synchronous action (without HookData parameter).
/// </summary>
internal class SimpleActionHookHandler : IHookHandler
{
    private readonly Action<ChatContext> _handler;
    
    public string Name { get; set; } = "SimpleActionHook";
    public int Priority { get; set; } = 0;
    
    public SimpleActionHookHandler(Action<ChatContext> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    
    public Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        _handler(context);
        return Task.FromResult(HookResult.Continue());
    }
}
