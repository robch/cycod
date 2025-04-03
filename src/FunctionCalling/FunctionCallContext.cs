//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using OpenAI.Chat;
using System.Buffers;
using System.Diagnostics;
using System.Text;

public class FunctionCallContext
{
    public FunctionCallContext(FunctionFactory functionFactory, IList<ChatMessage> messages)
    {
        _functionFactory = functionFactory;
        _messages = messages;
    }


    public bool CheckForUpdate(StreamingChatCompletionUpdate streamingUpdate)
    {
        var updated = false;

        foreach (var update in streamingUpdate.ToolCallUpdates)
        {
            _toolCallsBuilder.Append(update);
            updated = true;
        }

        return updated;
    }

    public bool TryCallFunctions(string content, Action<string, string, string?>? funcionCallback, Action<IList<ChatMessage>>? messageCallback)
    {
        var toolCalls = _toolCallsBuilder.Build();
        if (toolCalls.Count == 0) return false;

        // Create the assistant message with the tool calls.
        var assistantMessage = new AssistantChatMessage(toolCalls);
        // Then, if there is any textual content, add it as a content part.
        if (!string.IsNullOrEmpty(content))
        {
            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(content));
        }
        
        // Add the assistant message to the messages list.
        _messages.Add(assistantMessage);
        if (messageCallback != null) messageCallback(_messages);

        // Process each tool call.
        foreach (var toolCall in toolCalls)
        {
            var functionName = toolCall.FunctionName;
            var functionArguments = toolCall.FunctionArguments.ToArray().Length > 0
                ? toolCall.FunctionArguments.ToString()
                : "{}";

            if (funcionCallback != null) funcionCallback(functionName, functionArguments, null);

            var ok = _functionFactory.TryCallFunction(functionName, functionArguments, out var result);
            if (!ok) return false;

            result ??= string.Empty;
            if (result.Length >= MaximumToolCallResultLength)
            {
                ConsoleHelpers.WriteDebugLine($"Tool call result is too long, truncating to {MaximumToolCallResultLength} characters.");
                result = result.Substring(0, MaximumToolCallResultLength) + "<...truncated...use line ranges to see more...>";
            }

            if (funcionCallback != null) funcionCallback(functionName, functionArguments, result);

            _messages.Add(new ToolChatMessage(toolCall.Id, result));
            if (messageCallback != null) messageCallback(_messages);
        }

        return true;
    }

    public void Clear()
    {
        _toolCallsBuilder = new();
    }

    private FunctionFactory _functionFactory;
    private IList<ChatMessage> _messages;
    private StreamingChatToolCallsBuilder _toolCallsBuilder = new();

    private const int MaximumToolCallResultLength = 200000;
}

public class StreamingChatToolCallsBuilder
{
    private readonly Dictionary<int, string> _indexToToolCallId = [];
    private readonly Dictionary<int, string> _indexToFunctionName = [];
    private readonly Dictionary<int, SequenceBuilder<byte>> _indexToFunctionArguments = [];

    public void Append(StreamingChatToolCallUpdate toolCallUpdate)
    {
        // Keep track of which tool call ID belongs to this update index.
        if (toolCallUpdate.ToolCallId != null)
        {
            _indexToToolCallId[toolCallUpdate.Index] = toolCallUpdate.ToolCallId;
        }

        // Keep track of which function name belongs to this update index.
        if (toolCallUpdate.FunctionName != null)
        {
            _indexToFunctionName[toolCallUpdate.Index] = toolCallUpdate.FunctionName;
        }

        // Keep track of which function arguments belong to this update index,
        // and accumulate the arguments as new updates arrive.
        if (toolCallUpdate.FunctionArgumentsUpdate != null)
        {
            if (!_indexToFunctionArguments.TryGetValue(toolCallUpdate.Index, out var argumentsBuilder))
            {
                argumentsBuilder = new SequenceBuilder<byte>();
                _indexToFunctionArguments[toolCallUpdate.Index] = argumentsBuilder;
            }

            argumentsBuilder.Append(toolCallUpdate.FunctionArgumentsUpdate);
        }
    }

    public IReadOnlyList<ChatToolCall> Build()
    {
        List<ChatToolCall> toolCalls = [];

        foreach ((int index, string toolCallId) in _indexToToolCallId)
        {
            var builder = _indexToFunctionArguments[index];
            var sequence = builder.Build();
            var bytes = sequence.ToArray();
            if (bytes.Length == 0)
            {
                bytes = Encoding.UTF8.GetBytes("{}");
            }

            ChatToolCall toolCall = ChatToolCall.CreateFunctionToolCall(
                id: toolCallId,
                functionName: _indexToFunctionName[index],
                functionArguments: BinaryData.FromBytes(bytes));

            toolCalls.Add(toolCall);
        }

        return toolCalls;
    }
}

public class SequenceBuilder<T>
{
    Segment? _first;
    Segment? _last;

    public void Append(ReadOnlyMemory<T> data)
    {
        if (_first == null)
        {
            Debug.Assert(_last == null);
            _first = new Segment(data);
            _last = _first;
        }
        else
        {
            _last = _last!.Append(data);
        }
    }

    public ReadOnlySequence<T> Build()
    {
        if (_first == null)
        {
            Debug.Assert(_last == null);
            return ReadOnlySequence<T>.Empty;
        }

        if (_first == _last)
        {
            Debug.Assert(_first.Next == null);
            return new ReadOnlySequence<T>(_first.Memory);
        }

        return new ReadOnlySequence<T>(_first, 0, _last!, _last!.Memory.Length);
    }

    private sealed class Segment : ReadOnlySequenceSegment<T>
    {
        public Segment(ReadOnlyMemory<T> items) : this(items, 0)
        {
        }

        private Segment(ReadOnlyMemory<T> items, long runningIndex)
        {
            Debug.Assert(runningIndex >= 0);
            Memory = items;
            RunningIndex = runningIndex;
        }

        public Segment Append(ReadOnlyMemory<T> items)
        {
            long runningIndex;
            checked { runningIndex = RunningIndex + Memory.Length; }
            Segment segment = new(items, runningIndex);
            Next = segment;
            return segment;
        }
    }
}
