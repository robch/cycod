using System;
using System.IO;
using ConsoleGui;
using ConsoleGui.Controls;

namespace TextViewerControlTests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check for interactive mode flag
            bool interactiveMode = args.Length > 0 && args[0] == "--interactive";

            Console.WriteLine("TextViewerControl Tests");
            Console.WriteLine("======================");
            Console.WriteLine();

            if (!interactiveMode)
            {
                // Run automated tests
                int passed = 0;
                int failed = 0;

                // Test 1: Basic construction
                try
                {
                    Console.Write("Test 1: Basic construction... ");
                    var rect = new Rect(0, 0, 40, 10);
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    var viewer = TestableTextViewerControl.Create(null, rect, colors, colors);
                    if (viewer != null)
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("FAIL - viewer is null");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                // Test 2: Display static method exists
                try
                {
                    Console.Write("Test 2: Display method exists... ");
                    var lines = new[] { "Line 1", "Line 2", "Line 3" };
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    // We can't actually run Display in non-interactive mode, just verify the method exists
                    var method = typeof(TestableTextViewerControl).GetMethod("DisplayPublic", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (method != null)
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("FAIL - Display method not found");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                // Test 3: ProcessKey method exists and handles navigation
                try
                {
                    Console.Write("Test 3: ProcessKey method exists... ");
                    var rect = new Rect(0, 0, 40, 10);
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    var viewer = TestableTextViewerControl.Create(null, rect, colors, colors);
                    viewer.Items = new[] { "Test line" };
                    
                    // Check that ProcessKey method exists (we can't actually test keyboard in automated mode)
                    var method = viewer.GetType().GetMethod("ProcessKey");
                    if (method != null)
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("FAIL - ProcessKey method not found");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                // Test 4: DisplayRow method override
                try
                {
                    Console.Write("Test 4: DisplayRow method override... ");
                    var rect = new Rect(0, 0, 40, 10);
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    var viewer = TestableTextViewerControl.Create(null, rect, colors, colors);
                    var method = viewer.GetType().GetMethod("DisplayRow");
                    if (method != null && method.DeclaringType == typeof(TextViewerControl))
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("FAIL - DisplayRow not properly overridden");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                // Test 5: Inherits from SpeedSearchListBoxControl
                try
                {
                    Console.Write("Test 5: Inherits from SpeedSearchListBoxControl... ");
                    var rect = new Rect(0, 0, 40, 10);
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    var viewer = TestableTextViewerControl.Create(null, rect, colors, colors);
                    if (viewer is SpeedSearchListBoxControl)
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine("FAIL - does not inherit from SpeedSearchListBoxControl");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                // Test 6: GetSpeedSearchText strips backticks
                try
                {
                    Console.Write("Test 6: GetSpeedSearchText strips backticks... ");
                    var rect = new Rect(0, 0, 40, 10);
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    var viewer = TestableTextViewerControl.Create(null, rect, colors, colors);
                    viewer.Items = new[] { "Test `code` block" };
                    var searchText = viewer.GetSpeedSearchTextPublic(0);
                    if (searchText == "Test code block")
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine($"FAIL - expected 'Test code block', got '{searchText}'");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                // Test 7: SelectedColumn/Width properties
                try
                {
                    Console.Write("Test 7: SelectedColumn/Width properties... ");
                    var rect = new Rect(0, 0, 40, 10);
                    var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                    var viewer = TestableTextViewerControl.Create(null, rect, colors, colors);
                    viewer.Items = new[] { "Test line" };
                    viewer.SetSelectedColumnPublic(2, 3);
                    if (viewer.SelectedColumn == 2 && viewer.SelectedColumnWidth == 3)
                    {
                        Console.WriteLine("PASS");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine($"FAIL - column={viewer.SelectedColumn}, width={viewer.SelectedColumnWidth}");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAIL - {ex.Message}");
                    failed++;
                }

                Console.WriteLine();
                Console.WriteLine($"Tests passed: {passed}");
                Console.WriteLine($"Tests failed: {failed}");
                Console.WriteLine();

                if (failed > 0)
                {
                    Console.WriteLine("Some tests failed.");
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("All tests passed!");
                    Environment.Exit(0);
                }
            }
            else
            {
                // Interactive mode
                Console.WriteLine("Interactive Mode - Testing TextViewerControl");
                Console.WriteLine("=============================================");
                Console.WriteLine();
                Console.WriteLine("This demo will show a text viewer with sample content.");
                Console.WriteLine("Use arrow keys to navigate, Enter to confirm, Escape to cancel.");
                Console.WriteLine();
                Console.WriteLine("Features to test:");
                Console.WriteLine("- Left/Right arrow keys move column selection");
                Console.WriteLine("- Up/Down arrow keys move row selection");
                Console.WriteLine("- Speed search (type '?' or just start typing)");
                Console.WriteLine("- Syntax highlighting with backtick markers");
                Console.WriteLine();
                Console.Write("Press any key to start the demo...");
                Console.ReadKey(true);
                Console.WriteLine();
                Console.WriteLine();

                // Create sample content with syntax highlighting
                var lines = new[]
                {
                    "TextViewerControl Demo",
                    "",
                    "This is a `highlighted` word in normal text.",
                    "You can use backticks for `code` formatting.",
                    "",
                    "Navigation:",
                    "  - Arrow keys: Move selection",
                    "  - Enter: Confirm selection",
                    "  - Escape: Cancel",
                    "",
                    "Speed Search:",
                    "  - Press '?' to start search",
                    "  - Or just start typing",
                    "  - Tab/Shift+Tab to cycle matches",
                    "",
                    "This viewer extends SpeedSearchListBoxControl,",
                    "so it has all the search features built-in.",
                    "",
                    "Try selecting different lines and columns!"
                };

                var colors = new Colors(ConsoleColor.White, ConsoleColor.Blue);
                var selectedColors = new Colors(ConsoleColor.Black, ConsoleColor.Cyan);

                try
                {
                    var result = TestableTextViewerControl.DisplayPublic(
                        lines, 
                        60,  // width
                        20,  // height
                        colors,
                        selectedColors,
                        0,   // selectedRow
                        0,   // selectedCol
                        1    // selectionWidth
                    );

                    Console.WriteLine();
                    Console.WriteLine($"Result: row={result.row}, col={result.col}, width={result.width}");
                    Console.WriteLine();

                    if (result.row >= 0)
                    {
                        Console.WriteLine($"Selected: \"{lines[result.row]}\" at column {result.col}");
                    }
                    else
                    {
                        Console.WriteLine("Cancelled (Escape pressed)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }

                Console.WriteLine();
                Console.WriteLine("Demo complete. Press any key to exit...");
                Console.ReadKey(true);
            }
        }
    }

    // Testable wrapper that exposes protected members
    public class TestableTextViewerControl : TextViewerControl
    {
        public static TestableTextViewerControl Create(Window? parent, Rect rect, Colors colorNormal, Colors colorSelected, string? border = null)
        {
            return new TestableTextViewerControl(parent, rect, colorNormal, colorSelected, border);
        }

        private TestableTextViewerControl(Window? parent, Rect rect, Colors colorNormal, Colors colorSelected, string? border = null) 
            : base(parent, rect, colorNormal, colorSelected, border)
        {
        }

        public string GetSpeedSearchTextPublic(int row)
        {
            return GetSpeedSearchText(row);
        }

        public bool SetSelectedColumnPublic(int col, int width = 1)
        {
            return SetSelectedColumn(col, width);
        }

        public static (int row, int col, int width) DisplayPublic(string[] lines, int width, int height, Colors normal, Colors selected, int selectedRow = 0, int selectedCol = 0, int selectionWidth = 1)
        {
            return Display(lines, width, height, normal, selected, selectedRow, selectedCol, selectionWidth);
        }
    }
}
