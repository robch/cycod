using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CycoDj.CommandLineCommands
{
    public class CleanupCommand : CommandLine.CycoDjCommand
    {
        public bool FindDuplicates { get; set; }
        public bool RemoveDuplicates { get; set; }
        public bool FindEmpty { get; set; }
        public bool RemoveEmpty { get; set; }
        public int? OlderThanDays { get; set; }
        public bool DryRun { get; set; } = true;

        public override async Task<int> ExecuteAsync()
        {
            ConsoleHelpers.WriteLine("## Chat History Cleanup", ConsoleColor.Cyan, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            if (!FindDuplicates && !FindEmpty && !OlderThanDays.HasValue)
            {
                ConsoleHelpers.WriteErrorLine("Please specify at least one cleanup operation:");
                ConsoleHelpers.WriteLine("  --find-duplicates     Find duplicate conversations", overrideQuiet: true);
                ConsoleHelpers.WriteLine("  --find-empty          Find empty conversations", overrideQuiet: true);
                ConsoleHelpers.WriteLine("  --older-than-days N   Find conversations older than N days", overrideQuiet: true);
                return 1;
            }

            var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
            var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();

            ConsoleHelpers.WriteLine($"Scanning {files.Count} conversation files...", overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var toRemove = new List<string>();

            if (FindDuplicates || RemoveDuplicates)
            {
                toRemove.AddRange(await FindDuplicateConversationsAsync(files));
            }

            if (FindEmpty || RemoveEmpty)
            {
                toRemove.AddRange(FindEmptyConversations(files));
            }

            if (OlderThanDays.HasValue)
            {
                toRemove.AddRange(FindOldConversations(files, OlderThanDays.Value));
            }

            // Remove duplicates from the list
            toRemove = toRemove.Distinct().ToList();

            if (!toRemove.Any())
            {
                ConsoleHelpers.WriteLine("✓ No files need cleanup!", ConsoleColor.Green, overrideQuiet: true);
                return 0;
            }

            ConsoleHelpers.WriteLine($"Found {toRemove.Count} file(s) to clean up:", ConsoleColor.Yellow, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            foreach (var file in toRemove)
            {
                var size = new FileInfo(file).Length / 1024;
                ConsoleHelpers.WriteLine($"  - {Path.GetFileName(file)} ({size} KB)", ConsoleColor.DarkGray, overrideQuiet: true);
            }

            ConsoleHelpers.WriteLine(overrideQuiet: true);

            if (DryRun && (RemoveDuplicates || RemoveEmpty))
            {
                ConsoleHelpers.WriteLine("DRY RUN - No files will be deleted.", ConsoleColor.Yellow, overrideQuiet: true);
                ConsoleHelpers.WriteLine("Add --execute to actually remove files.", overrideQuiet: true);
                return 0;
            }

            if (!DryRun && (RemoveDuplicates || RemoveEmpty))
            {
                ConsoleHelpers.WriteLine("⚠️  WARNING: About to delete files!", ConsoleColor.Red, overrideQuiet: true);
                ConsoleHelpers.Write("Type 'DELETE' to confirm: ", ConsoleColor.Yellow, overrideQuiet: true);
                
                var confirmation = Console.ReadLine();
                if (confirmation?.Trim().ToUpperInvariant() != "DELETE")
                {
                    ConsoleHelpers.WriteLine("Cancelled.", overrideQuiet: true);
                    return 0;
                }

                var deletedCount = 0;
                var totalSize = 0L;

                foreach (var file in toRemove)
                {
                    try
                    {
                        var size = new FileInfo(file).Length;
                        File.Delete(file);
                        deletedCount++;
                        totalSize += size;
                        ConsoleHelpers.WriteLine($"  ✓ Deleted {Path.GetFileName(file)}", ConsoleColor.Green, overrideQuiet: true);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelpers.WriteErrorLine($"  ✗ Failed to delete {Path.GetFileName(file)}: {ex.Message}");
                    }
                }

                ConsoleHelpers.WriteLine(overrideQuiet: true);
                ConsoleHelpers.WriteLine($"Deleted {deletedCount} file(s), freed {totalSize / 1024 / 1024} MB", 
                    ConsoleColor.Green, overrideQuiet: true);
            }

            return 0;
        }

        private async Task<List<string>> FindDuplicateConversationsAsync(List<string> files)
        {
            ConsoleHelpers.WriteLine("### Finding Duplicate Conversations", ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var duplicates = new List<string>();
            var conversationsByContent = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                try
                {
                    var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
                    if (conversation == null) continue;

                    // Create a signature based on message content
                    var signature = string.Join("|", conversation.Messages
                        .Where(m => m.Role == "user" || m.Role == "assistant")
                        .Take(10) // First 10 messages
                        .Select(m => $"{m.Role}:{m.Content?.Length ?? 0}"));

                    if (!conversationsByContent.ContainsKey(signature))
                    {
                        conversationsByContent[signature] = new List<string>();
                    }
                    conversationsByContent[signature].Add(file);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to analyze {file}: {ex.Message}");
                }
            }

            var duplicateGroups = conversationsByContent.Where(kv => kv.Value.Count > 1).ToList();

            if (duplicateGroups.Any())
            {
                ConsoleHelpers.WriteLine($"Found {duplicateGroups.Count} group(s) of duplicates:", overrideQuiet: true);
                ConsoleHelpers.WriteLine(overrideQuiet: true);

                foreach (var group in duplicateGroups)
                {
                    ConsoleHelpers.WriteLine($"  Duplicate group ({group.Value.Count} files):", ConsoleColor.Yellow, overrideQuiet: true);
                    
                    // Keep the newest, mark others for removal
                    var sorted = group.Value.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f)).ToList();
                    var keep = sorted.First();
                    var remove = sorted.Skip(1).ToList();

                    ConsoleHelpers.WriteLine($"    KEEP: {Path.GetFileName(keep)}", ConsoleColor.Green, overrideQuiet: true);
                    foreach (var file in remove)
                    {
                        ConsoleHelpers.WriteLine($"    remove: {Path.GetFileName(file)}", ConsoleColor.DarkGray, overrideQuiet: true);
                        duplicates.Add(file);
                    }
                    ConsoleHelpers.WriteLine(overrideQuiet: true);
                }
            }
            else
            {
                ConsoleHelpers.WriteLine("  No duplicates found.", ConsoleColor.Green, overrideQuiet: true);
            }

            ConsoleHelpers.WriteLine(overrideQuiet: true);
            return duplicates;
        }

        private List<string> FindEmptyConversations(List<string> files)
        {
            ConsoleHelpers.WriteLine("### Finding Empty Conversations", ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var empty = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
                    if (conversation == null) continue;

                    var meaningfulMessages = conversation.Messages.Count(m => 
                        m.Role == "user" || m.Role == "assistant");

                    if (meaningfulMessages == 0)
                    {
                        empty.Add(file);
                        ConsoleHelpers.WriteLine($"  Empty: {Path.GetFileName(file)}", ConsoleColor.Yellow, overrideQuiet: true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to analyze {file}: {ex.Message}");
                }
            }

            if (empty.Any())
            {
                ConsoleHelpers.WriteLine($"Found {empty.Count} empty conversation(s).", ConsoleColor.Yellow, overrideQuiet: true);
            }
            else
            {
                ConsoleHelpers.WriteLine("  No empty conversations found.", ConsoleColor.Green, overrideQuiet: true);
            }

            ConsoleHelpers.WriteLine(overrideQuiet: true);
            return empty;
        }

        private List<string> FindOldConversations(List<string> files, int olderThanDays)
        {
            ConsoleHelpers.WriteLine($"### Finding Conversations Older Than {olderThanDays} Days", ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
            var old = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var timestamp = CycoDj.Helpers.TimestampHelpers.ParseTimestamp(file);
                    if (timestamp < cutoffDate)
                    {
                        old.Add(file);
                        ConsoleHelpers.WriteLine($"  Old: {Path.GetFileName(file)} ({timestamp:yyyy-MM-dd})", 
                            ConsoleColor.DarkGray, overrideQuiet: true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to analyze {file}: {ex.Message}");
                }
            }

            if (old.Any())
            {
                ConsoleHelpers.WriteLine($"Found {old.Count} old conversation(s).", ConsoleColor.Yellow, overrideQuiet: true);
            }
            else
            {
                ConsoleHelpers.WriteLine("  No old conversations found.", ConsoleColor.Green, overrideQuiet: true);
            }

            ConsoleHelpers.WriteLine(overrideQuiet: true);
            return old;
        }
    }
}
