namespace CycoDev.ChatPipeline.Hooks;

/// <summary>
/// Hook handler that wraps an async lambda function.
/// </summary>
internal class LambdaHookHandler : IHookHandler
{
    private readonly Func<ChatContext, HookData, Task<HookResult>> _handler;
    
    public string Name { get; set; } = "LambdaHook";
    public int Priority { get; set; } = 0;
    
    public LambdaHookHandler(Func<ChatContext, HookData, Task<HookResult>> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    
    public async Task<HookResult> HandleAsync(ChatContext context, HookData data)
    {
        return await _handler(context, data);
    }
}
