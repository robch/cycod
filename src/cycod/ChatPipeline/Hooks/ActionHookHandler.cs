namespace CycoDev.ChatPipeline.Hooks;

/// <summary>
/// Hook handler that wraps a synchronous action.
/// </summary>
internal class ActionHookHandler : IHookHandler
{
    private readonly Action<ChatContext, HookData> _handler;
    
    public string Name { get; set; } = "ActionHook";
    public int Priority { get; set; } = 0;
    
    public ActionHookHandler(Action<ChatContext, HookData> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    
    public Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        _handler(context, data);
        return Task.FromResult(HookResult.Continue());
    }
}
