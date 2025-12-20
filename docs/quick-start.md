# Quick Start Implementation Guide

## Phase 1: Get Something Working (Day 1)

### Goal
Read JSONL files and list conversations. No fancy features yet.

### Tasks
1. **Create the project**
   ```bash
   cd src
   dotnet new console -n cycodj
   dotnet sln add cycodj/cycodj.csproj
   cd cycodj
   dotnet add package CommandLineParser
   ```

2. **Create basic Program.cs**
   ```csharp
   using CommandLine;
   
   [Verb("list", HelpText = "List conversations")]
   public class ListOptions
   {
       [Option('d', "date", HelpText = "Filter by date (YYYY-MM-DD)")]
       public string? Date { get; set; }
   }
   
   class Program
   {
       static int Main(string[] args)
       {
           return Parser.Default.ParseArguments<ListOptions>(args)
               .MapResult(
                   (ListOptions opts) => RunList(opts),
                   errs => 1);
       }
       
       static int RunList(ListOptions opts)
       {
           Console.WriteLine("Listing conversations...");
           return 0;
       }
   }
   ```

3. **Test it works**
   ```bash
   dotnet run -- list
   dotnet run -- list --date 2024-12-20
   ```

4. **Find history files**
   ```csharp
   // In HistoryFileHelpers.cs
   public static List<string> FindAllHistoryFiles()
   {
       var historyDir = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
           ".cycod",
           "history"
       );
       
       if (!Directory.Exists(historyDir))
           return new List<string>();
       
       return Directory.GetFiles(historyDir, "chat-history-*.jsonl")
           .OrderByDescending(f => f)
           .ToList();
   }
   ```

5. **Parse timestamps from filenames**
   ```csharp
   // In TimestampHelpers.cs
   public static DateTime ParseTimestamp(string filename)
   {
       var name = Path.GetFileNameWithoutExtension(filename);
       var parts = name.Split('-');
       if (parts.Length >= 3 && long.TryParse(parts[2], out var timestamp))
       {
           return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
       }
       return DateTime.MinValue;
   }
   ```

6. **Update RunList to show files**
   ```csharp
   static int RunList(ListOptions opts)
   {
       var files = HistoryFileHelpers.FindAllHistoryFiles();
       
       foreach (var file in files)
       {
           var timestamp = TimestampHelpers.ParseTimestamp(file);
           var name = Path.GetFileNameWithoutExtension(file);
           Console.WriteLine($"{timestamp:yyyy-MM-dd HH:mm:ss} - {name}");
       }
       
       return 0;
   }
   ```

### Checkpoint
You should now be able to:
- Run `cycodj list` and see all your chat history files
- See timestamps parsed correctly

## Phase 2: Read JSONL Content (Day 2)

### Goal
Parse JSONL files and display message counts.

### Tasks
1. **Create ChatMessage model**
   ```csharp
   public class ChatMessage
   {
       [JsonPropertyName("role")]
       public string Role { get; set; } = "";
       
       [JsonPropertyName("content")]
       public string Content { get; set; } = "";
       
       [JsonPropertyName("tool_calls")]
       public List<ToolCall>? ToolCalls { get; set; }
       
       [JsonPropertyName("tool_call_id")]
       public string? ToolCallId { get; set; }
   }
   
   public class ToolCall
   {
       [JsonPropertyName("id")]
       public string Id { get; set; } = "";
       
       [JsonPropertyName("type")]
       public string Type { get; set; } = "";
       
       [JsonPropertyName("function")]
       public JsonElement? Function { get; set; }
   }
   ```

2. **Create Conversation model**
   ```csharp
   public class Conversation
   {
       public string Id { get; set; } = "";
       public string FilePath { get; set; } = "";
       public DateTime Timestamp { get; set; }
       public List<ChatMessage> Messages { get; set; } = new();
       public List<string> ToolCallIds { get; set; } = new();
   }
   ```

3. **Create JsonlReader**
   ```csharp
   public class JsonlReader
   {
       public static Conversation? ReadConversation(string filePath)
       {
           try
           {
               var lines = File.ReadAllLines(filePath);
               var messages = new List<ChatMessage>();
               
               foreach (var line in lines)
               {
                   if (string.IsNullOrWhiteSpace(line)) continue;
                   
                   var msg = JsonSerializer.Deserialize<ChatMessage>(line);
                   if (msg != null)
                       messages.Add(msg);
               }
               
               var conv = new Conversation
               {
                   Id = Path.GetFileNameWithoutExtension(filePath),
                   FilePath = filePath,
                   Timestamp = TimestampHelpers.ParseTimestamp(filePath),
                   Messages = messages
               };
               
               // Extract tool_call_ids for branch detection
               conv.ToolCallIds = messages
                   .Where(m => !string.IsNullOrEmpty(m.ToolCallId))
                   .Select(m => m.ToolCallId!)
                   .ToList();
               
               return conv;
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Error reading {filePath}: {ex.Message}");
               return null;
           }
       }
   }
   ```

4. **Update list to show message counts**
   ```csharp
   static int RunList(ListOptions opts)
   {
       var files = HistoryFileHelpers.FindAllHistoryFiles();
       
       foreach (var file in files.Take(10)) // Just first 10 for testing
       {
           var conv = JsonlReader.ReadConversation(file);
           if (conv == null) continue;
           
           var userCount = conv.Messages.Count(m => m.Role == "user");
           var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
           var toolCount = conv.Messages.Count(m => m.Role == "tool");
           
           Console.WriteLine($"{conv.Timestamp:HH:mm:ss} - {conv.Id}");
           Console.WriteLine($"  Messages: {userCount} user, {assistantCount} assistant, {toolCount} tool");
       }
       
       return 0;
   }
   ```

### Checkpoint
You should now be able to:
- Parse JSONL files into ChatMessage objects
- See message counts by role
- Handle errors gracefully

## Phase 3: Detect Branches (Day 3)

### Goal
Identify which conversations are branches of others.

### Tasks
1. **Create BranchDetector**
   ```csharp
   public class BranchDetector
   {
       public static void DetectBranches(List<Conversation> conversations)
       {
           foreach (var conv in conversations)
           {
               // Find potential parents (conversations with same prefix)
               var potentialParents = conversations
                   .Where(other => other.Id != conv.Id)
                   .Where(other => HasCommonPrefix(conv, other))
                   .OrderByDescending(other => GetCommonPrefixLength(conv, other))
                   .ToList();
               
               // Parent is the one that's an exact prefix
               foreach (var parent in potentialParents)
               {
                   if (IsExactPrefix(parent.ToolCallIds, conv.ToolCallIds))
                   {
                       conv.ParentId = parent.Id;
                       break;
                   }
               }
           }
       }
       
       private static bool HasCommonPrefix(Conversation a, Conversation b)
       {
           if (a.ToolCallIds.Count == 0 || b.ToolCallIds.Count == 0)
               return false;
           
           return a.ToolCallIds[0] == b.ToolCallIds[0];
       }
       
       private static int GetCommonPrefixLength(Conversation a, Conversation b)
       {
           var length = 0;
           var minLength = Math.Min(a.ToolCallIds.Count, b.ToolCallIds.Count);
           
           for (int i = 0; i < minLength; i++)
           {
               if (a.ToolCallIds[i] == b.ToolCallIds[i])
                   length++;
               else
                   break;
           }
           
           return length;
       }
       
       private static bool IsExactPrefix(List<string> prefix, List<string> full)
       {
           if (prefix.Count >= full.Count)
               return false;
           
           for (int i = 0; i < prefix.Count; i++)
           {
               if (prefix[i] != full[i])
                   return false;
           }
           
           return true;
       }
   }
   ```

2. **Add ParentId to Conversation**
   ```csharp
   public class Conversation
   {
       // ... existing properties ...
       public string? ParentId { get; set; }
   }
   ```

3. **Update list to show branches**
   ```csharp
   static int RunList(ListOptions opts)
   {
       var files = HistoryFileHelpers.FindAllHistoryFiles();
       var conversations = files
           .Take(20)
           .Select(f => JsonlReader.ReadConversation(f))
           .Where(c => c != null)
           .ToList();
       
       BranchDetector.DetectBranches(conversations);
       
       foreach (var conv in conversations)
       {
           var indent = conv.ParentId != null ? "  ↳ " : "";
           Console.WriteLine($"{indent}{conv.Timestamp:HH:mm:ss} - {conv.Id}");
           
           if (conv.ParentId != null)
               Console.WriteLine($"     (branch of {conv.ParentId})");
       }
       
       return 0;
   }
   ```

### Checkpoint
You should now be able to:
- Detect parent-child relationships
- See which conversations are branches
- Verify against your real data

## Phase 4: Show Command (Day 4)

### Goal
Display details of a specific conversation.

### Tasks
1. **Add ShowOptions**
   ```csharp
   [Verb("show", HelpText = "Show conversation details")]
   public class ShowOptions
   {
       [Value(0, Required = true, HelpText = "Conversation ID or filename")]
       public string Conversation { get; set; } = "";
   }
   ```

2. **Implement RunShow**
   ```csharp
   static int RunShow(ShowOptions opts)
   {
       // Find the conversation file
       var files = HistoryFileHelpers.FindAllHistoryFiles();
       var file = files.FirstOrDefault(f => 
           f.Contains(opts.Conversation) || 
           Path.GetFileNameWithoutExtension(f) == opts.Conversation);
       
       if (file == null)
       {
           Console.WriteLine($"Conversation not found: {opts.Conversation}");
           return 1;
       }
       
       var conv = JsonlReader.ReadConversation(file);
       if (conv == null) return 1;
       
       // Display header
       Console.WriteLine($"Conversation: {conv.Id}");
       Console.WriteLine($"Timestamp: {conv.Timestamp:yyyy-MM-dd HH:mm:ss}");
       Console.WriteLine($"File: {conv.FilePath}");
       Console.WriteLine($"Messages: {conv.Messages.Count}");
       Console.WriteLine();
       
       // Display messages
       foreach (var msg in conv.Messages)
       {
           var color = msg.Role switch
           {
               "user" => ConsoleColor.Green,
               "assistant" => ConsoleColor.Cyan,
               "tool" => ConsoleColor.Gray,
               _ => ConsoleColor.White
           };
           
           Console.ForegroundColor = color;
           Console.Write($"[{msg.Role}] ");
           Console.ResetColor();
           
           var content = msg.Content.Length > 200 
               ? msg.Content.Substring(0, 200) + "..."
               : msg.Content;
           Console.WriteLine(content);
           Console.WriteLine();
       }
       
       return 0;
   }
   ```

### Checkpoint
You should now be able to:
- Show details of any conversation
- See message flow with colors
- Navigate through conversation history

## Phase 5: Daily Journal (Day 5+)

### Goal
Generate a readable daily summary.

This is more complex - see the main plan and architecture docs for details.

### Key Tasks
- Filter conversations by date
- Group by time periods (morning/afternoon/evening)
- Summarize user actions and assistant responses
- Format nicely with branch indicators

## Testing Strategy

### Manual Testing
```bash
# Test with your real data
cycodj list
cycodj list --date 2024-12-20
cycodj show chat-history-1754437373970
```

### Create Test Data
Create small JSONL test files:
```jsonl
{"role":"system","content":"test"}
{"role":"user","content":"hello"}
{"role":"assistant","content":"hi","tool_calls":[{"id":"test_001"}]}
{"role":"tool","tool_call_id":"test_001","content":"output"}
```

### Unit Tests (Later)
```csharp
[Fact]
public void TestTimestampParsing()
{
    var timestamp = TimestampHelpers.ParseTimestamp(
        "chat-history-1754437373970.jsonl");
    Assert.Equal(2024, timestamp.Year);
}
```

## Tips

1. **Start simple**: Don't try to implement everything at once
2. **Test with real data**: Use your actual history files early
3. **Handle errors gracefully**: Files may be corrupted or have unexpected formats
4. **Use colors**: Makes output much more readable
5. **Iterate**: Get basic version working, then add features

## Next Steps After MVP

- Add date filtering
- Implement journal command
- Add search functionality
- Create tests
- Add export options
- Performance tuning

## Resources

- [Main Plan](chat-journal-plan.md)
- [Architecture](architecture.md)
- [Branching Examples](branching-examples.md)
- [Investigation TODO](TODO.md)
