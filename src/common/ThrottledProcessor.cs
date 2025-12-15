using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ThrottledProcessor
{
    private readonly SemaphoreSlim _throttler;

    public ThrottledProcessor(int maxParallelism)
    {
        _throttler = new SemaphoreSlim(maxParallelism);
    }

    /// <summary>
    /// Starts processing items with throttling and returns the tasks.
    /// All tasks are started when this returns, but not all are completed.
    /// </summary>
    public async Task<List<Task<TOutput>>> StartTasksAsync<TInput, TOutput>(
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

        return tasks;
    }

    /// <summary>
    /// Processes items with throttling and awaits all results.
    /// </summary>
    public async Task<List<TOutput>> ProcessAsync<TInput, TOutput>(
        List<TInput> items,
        Func<TInput, Task<TOutput>> processor)
    {
        var tasks = await StartTasksAsync(items, processor);
        return (await Task.WhenAll(tasks)).ToList();
    }
}
