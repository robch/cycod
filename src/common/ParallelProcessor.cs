using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ParallelProcessor
{
    private readonly SemaphoreSlim _throttler;

    public ParallelProcessor(int maxParallelism)
    {
        _throttler = new SemaphoreSlim(maxParallelism);
    }

    public async Task<List<TOutput>> ProcessAsync<TInput, TOutput>(
        List<TInput> items,
        Func<TInput, Task<TOutput>> processor)
    {
        var tasks = new List<Task<TOutput>>();

        foreach (var item in items)
        {
            await _throttler.WaitAsync();
            var task = processor(item).ContinueWith(t =>
            {
                _throttler.Release();
                return t.Result;
            });
            tasks.Add(task);
        }

        return (await Task.WhenAll(tasks)).ToList();
    }
}
