using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Cycod.Debugging.Dap;

public class DapClient : IDisposable
{
    readonly Process _process;
    readonly StreamReader _reader;
    readonly JsonSerializerOptions _jsonOptions;
    readonly Dictionary<int, TaskCompletionSource<Response>> _pending = new();
    readonly Dictionary<string, List<TaskCompletionSource<Event>>> _eventWaiters = new();
    readonly Dictionary<string, Queue<Event>> _eventQueue = new();
    readonly object _lock = new();
    int _seq = 1;
    Task? _listenTask;
    CancellationTokenSource? _cts;

    public string AdapterPath { get; }

    public event EventHandler<Event>? EventReceived;
public event EventHandler<string>? StdErrReceived;

    public DapClient(string adapterPath)
    {
        AdapterPath = adapterPath;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        var psi = new ProcessStartInfo
        {
            FileName = adapterPath,
            Arguments = "--interpreter=vscode",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        _process = Process.Start(psi) ?? throw new Exception("Failed to start debug adapter process");
        _reader = new StreamReader(_process.StandardOutput.BaseStream, Encoding.UTF8);
        _cts = new CancellationTokenSource();
        _process.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) StdErrReceived?.Invoke(this, e.Data); };
        _process.BeginErrorReadLine();
        StartListening(_cts.Token);
    }

    public async Task<Response> SendRequestAsync(string command, object? args = null, CancellationToken ct = default)
    {
        int seq;
        var tcs = new TaskCompletionSource<Response>();
        lock (_lock)
        {
            seq = _seq++;
            _pending[seq] = tcs;
        }
        var req = new Request { Seq = seq, Command = command, Arguments = args };
        await SendMessageAsync(req);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            return await tcs.Task.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            lock (_lock) _pending.Remove(seq);
            throw new TimeoutException($"Request {command} timed out");
        }
    }

    public Task<Event> WaitForEventAsync(string eventType, TimeSpan timeout)
    {
        lock (_lock)
        {
            if (_eventQueue.TryGetValue(eventType, out var q) && q.Count > 0) return Task.FromResult(q.Dequeue());
        }
        var tcs = new TaskCompletionSource<Event>();
        lock (_lock)
        {
            if (!_eventWaiters.ContainsKey(eventType)) _eventWaiters[eventType] = new List<TaskCompletionSource<Event>>();
            _eventWaiters[eventType].Add(tcs);
        }
        _ = Task.Delay(timeout).ContinueWith(_ =>
        {
            lock (_lock)
            {
                if (_eventWaiters.TryGetValue(eventType, out var list)) list.Remove(tcs);
            }
            tcs.TrySetException(new TimeoutException($"Event {eventType} timeout"));
        });
        return tcs.Task;
    }

    async Task SendMessageAsync(ProtocolMessage msg)
    {
        var json = JsonSerializer.Serialize(msg, msg.GetType(), _jsonOptions);
        var contentBytes = Encoding.UTF8.GetBytes(json);
        var header = Encoding.UTF8.GetBytes($"Content-Length: {contentBytes.Length}\r\n\r\n");
        await _process.StandardInput.BaseStream.WriteAsync(header, 0, header.Length);
        await _process.StandardInput.BaseStream.WriteAsync(contentBytes, 0, contentBytes.Length);
        await _process.StandardInput.BaseStream.FlushAsync();
    }

    async Task<JsonNode?> ReceiveMessageAsync(CancellationToken ct)
    {
        var headerLine = await _reader.ReadLineAsync(ct);
        if (string.IsNullOrEmpty(headerLine)) return null;
        if (!headerLine.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) return null;
        var len = int.Parse(headerLine.Substring(15).Trim());
        await _reader.ReadLineAsync(ct); // empty line
        var buffer = new char[len];
        var read = 0;
        while (read < len)
        {
            var r = await _reader.ReadAsync(buffer, read, len - read);
            if (r == 0) throw new EndOfStreamException("Stream ended while reading body");
            read += r;
        }
        var json = new string(buffer);
        return JsonNode.Parse(json);
    }

    void StartListening(CancellationToken ct)
    {
        _listenTask = Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var message = await ReceiveMessageAsync(ct);
                    if (message == null) break;
                    await HandleMessageAsync(message);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }, ct);
    }

    async Task HandleMessageAsync(JsonNode node)
    {
        var type = node["type"]?.GetValue<string>();
        if (type == "response")
        {
            var resp = JsonSerializer.Deserialize<Response>(node.ToJsonString(), _jsonOptions);
            if (resp != null)
            {
                lock (_lock)
                {
                    if (_pending.TryGetValue(resp.RequestSeq, out var tcs))
                    {
                        _pending.Remove(resp.RequestSeq);
                        tcs.SetResult(resp);
                    }
                }
            }
        }
        else if (type == "event")
        {
            var evt = JsonSerializer.Deserialize<Event>(node.ToJsonString(), _jsonOptions);
            if (evt != null)
            {
                EventReceived?.Invoke(this, evt);
                lock (_lock)
                {
                    if (_eventWaiters.TryGetValue(evt.EventType, out var waiters) && waiters.Count > 0)
                    {
                        foreach (var w in waiters.ToList()) w.SetResult(evt);
                        _eventWaiters.Remove(evt.EventType);
                    }
                    else
                    {
                        if (!_eventQueue.ContainsKey(evt.EventType)) _eventQueue[evt.EventType] = new Queue<Event>();
                        _eventQueue[evt.EventType].Enqueue(evt);
                    }
                }
            }
        }
        else if (type == "request")
        {
            // Adapter handshake requests: respond success
            var req = JsonSerializer.Deserialize<Request>(node.ToJsonString(), _jsonOptions);
            if (req != null)
            {
                var resp = new Response { Seq = Interlocked.Increment(ref _seq), RequestSeq = req.Seq, Success = true, Command = req.Command, Body = req.Arguments };
                await SendMessageAsync(resp);
            }
        }
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _listenTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
        try { if (!_process.HasExited) _process.Kill(); } catch { }
        if (_process.HasExited && _process.ExitCode != 0)
        {
            // Could raise an event or log crash; placeholder for crash detection integration
        }
        try { _reader.Dispose(); } catch { }
        try { _process.Dispose(); } catch { }
        _cts?.Dispose();
    }
}
