namespace CycoDev.ChatPipeline.Hooks;

/// <summary>
/// Hook handler that wraps a simple async lambda (without HookData parameter).
/// </summary>
internal class SimpleLambdaHookHandler : IHookHandler
{
    private readonly Func<ChatContext, Task> _handler;
    
    public string Name { get; set; } = "SimpleLambdaHook";
    public int Priority { get; set; } = 0;
    
    public SimpleLambdaHookHandler(Func<ChatContext, Task> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        await _handler(context);
        return HookResult.Continue();
    }
}
