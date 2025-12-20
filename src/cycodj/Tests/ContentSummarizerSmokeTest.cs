using System;
using System.Linq;
using CycoDj.Analyzers;
using CycoDj.Helpers;
using CycoDj.Models;

namespace CycoDj.Tests;

/// <summary>
/// Simple smoke tests to verify ContentSummarizer works with real data.
/// </summary>
public class ContentSummarizerSmokeTest
{
    public static void RunTests()
    {
        Console.WriteLine("=== ContentSummarizer Smoke Tests ===\n");
        
        // Test 1: Load a real conversation
        Console.WriteLine("Test 1: Loading real conversation...");
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        if (files.Count == 0)
        {
            Console.WriteLine("  ❌ No history files found. Cannot test.");
            return;
        }
        
        var testFile = files.First();
        Console.WriteLine($"  Using: {System.IO.Path.GetFileName(testFile)}");
        
        var conv = JsonlReader.ReadConversation(testFile);
        if (conv == null)
        {
            Console.WriteLine("  ❌ Failed to load conversation");
            return;
        }
        Console.WriteLine($"  ✓ Loaded conversation with {conv.Messages?.Count ?? 0} messages\n");
        
        // Test 2: GetUserMessages
        Console.WriteLine("Test 2: GetUserMessages()...");
        try
        {
            var userMsgs = ContentSummarizer.GetUserMessages(conv);
            Console.WriteLine($"  ✓ Found {userMsgs.Count} user messages");
            if (userMsgs.Count > 0)
            {
                var first = userMsgs.First();
                var preview = first.Length > 50 ? first.Substring(0, 50) + "..." : first;
                Console.WriteLine($"    First: \"{preview}\"");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 3: GetAssistantResponses
        Console.WriteLine("Test 3: GetAssistantResponses()...");
        try
        {
            var assistantMsgs = ContentSummarizer.GetAssistantResponses(conv);
            Console.WriteLine($"  ✓ Found {assistantMsgs.Count} assistant responses");
            if (assistantMsgs.Count > 0)
            {
                var first = assistantMsgs.First();
                var preview = first.Length > 50 ? first.Substring(0, 50) + "..." : first;
                Console.WriteLine($"    First: \"{preview}\"");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 4: GetToolCallsInvoked
        Console.WriteLine("Test 4: GetToolCallsInvoked()...");
        try
        {
            var toolCalls = ContentSummarizer.GetToolCallsInvoked(conv);
            Console.WriteLine($"  ✓ Found {toolCalls.Count} tool calls");
            if (toolCalls.Count > 0)
            {
                var first = toolCalls.First();
                Console.WriteLine($"    First: {first.toolName} ({first.toolCallId})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 5: GetActionSummary
        Console.WriteLine("Test 5: GetActionSummary()...");
        try
        {
            var actions = ContentSummarizer.GetActionSummary(conv);
            Console.WriteLine($"  ✓ Found {actions.Count} actions");
            if (actions.Count > 0)
            {
                Console.WriteLine($"    First: {actions.First()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 6: Summarize
        Console.WriteLine("Test 6: Summarize()...");
        try
        {
            var summary = ContentSummarizer.Summarize(conv);
            Console.WriteLine($"  ✓ Summary: \"{summary}\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 7: ExtractTitle
        Console.WriteLine("Test 7: ExtractTitle()...");
        try
        {
            var title = ContentSummarizer.ExtractTitle(conv);
            Console.WriteLine($"  ✓ Title: \"{title}\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 8: GetMessageCounts
        Console.WriteLine("Test 8: GetMessageCounts()...");
        try
        {
            var counts = ContentSummarizer.GetMessageCounts(conv);
            Console.WriteLine($"  ✓ Counts: user={counts.user}, assistant={counts.assistant}, tool={counts.tool}, system={counts.system}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 9: GetToolCallStatistics
        Console.WriteLine("Test 9: GetToolCallStatistics()...");
        try
        {
            var stats = ContentSummarizer.GetToolCallStatistics(conv);
            Console.WriteLine($"  ✓ Tool usage statistics:");
            foreach (var stat in stats.Take(5))
            {
                Console.WriteLine($"    {stat.Key}: {stat.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error: {ex.Message}");
        }
        Console.WriteLine();
        
        // Test 10: Null safety
        Console.WriteLine("Test 10: Null safety...");
        try
        {
            ContentSummarizer.GetUserMessages(null!);
            Console.WriteLine("  ❌ Should have thrown ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("  ✓ Correctly throws ArgumentNullException for null conversation");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Wrong exception type: {ex.GetType().Name}");
        }
        
        Console.WriteLine("\n=== All Tests Complete ===");
    }
}
