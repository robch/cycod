using System;
using System.Collections.Generic;
using ConsoleGui;
using ConsoleGui.Controls;

namespace SpeedSearchListBoxControlTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SpeedSearchListBoxControl Tests ===\n");

            var testResults = new List<(string name, bool passed)>();

            // Run all tests - focusing on API structure and search algorithms
            testResults.Add(("SpeedSearchListBox can be instantiated", TestInstantiation()));
            testResults.Add(("Speed search starts closed", TestSpeedSearchStartsClosed()));
            testResults.Add(("Search finds exact matches", TestExactMatch()));
            testResults.Add(("Search finds partial matches (starts with)", TestPartialMatch()));
            testResults.Add(("Search finds character sequence", TestCharSequence()));
            testResults.Add(("MinMaxRow wraps correctly", TestMinMaxRow()));
            testResults.Add(("RowStartsWith detects prefix", TestRowStartsWith()));
            testResults.Add(("RowContainsExactMatch finds substring", TestRowContainsExactMatch()));
            testResults.Add(("GetSpeedSearchText returns display text", TestGetSpeedSearchText()));
            testResults.Add(("GetSpeedSearchTooltip shows correct text", TestGetSpeedSearchTooltip()));

            // Print results
            Console.WriteLine("\n=== Test Results ===");
            var passed = 0;
            var failed = 0;
            foreach (var (name, success) in testResults)
            {
                Console.WriteLine($"[{(success ? "PASS" : "FAIL")}] {name}");
                if (success) passed++;
                else failed++;
            }

            Console.WriteLine($"\nTotal: {testResults.Count} tests, {passed} passed, {failed} failed");

            Environment.Exit(failed > 0 ? 1 : 0);
        }

        // Test helper to create a test list box
        static TestSpeedSearchListBox CreateTestListBox(string[] items)
        {
            var rect = new Rect(0, 0, 40, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var highlightColors = new Colors(ConsoleColor.Black, ConsoleColor.White);
            return new TestSpeedSearchListBox(null, rect, colors, highlightColors, null, true, items);
        }

        static bool TestInstantiation()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                return listBox != null && listBox.Items.Length == 3;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestSpeedSearchStartsClosed()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                return !listBox.IsSpeedSearchOpenPublic();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestExactMatch()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry", "Apricot" };
                var listBox = CreateTestListBox(items);
                
                // Search for "ban" (should match "Banana")
                var result = listBox.SelectRowContainingPublic("ban", 0, false, true, false, false, 1);
                
                // Should find "Banana" (index 1)
                return result && listBox.GetSelectedRow() == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestPartialMatch()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry", "Apricot" };
                var listBox = CreateTestListBox(items);
                
                // Search starting with "ap" (case insensitive)
                var result = listBox.SelectRowContainingPublic("ap", 0, true, false, false, false, 1);
                
                // Should find "Apple" (index 0)
                return result && listBox.GetSelectedRow() == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestCharSequence()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                
                // Search for "cry" (should match "CheRrY" - all chars in order)
                var result = listBox.SelectRowContainingPublic("cry", 0, false, false, false, true, 1);
                
                // Should find "Cherry" (index 2)
                return result && listBox.GetSelectedRow() == 2;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestMinMaxRow()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                
                // Test wrapping at boundaries
                var wrapNegative = listBox.MinMaxRowPublic(-1);  // Should wrap to 2
                var wrapOver = listBox.MinMaxRowPublic(3);       // Should wrap to 0
                var valid = listBox.MinMaxRowPublic(1);          // Should stay 1
                
                return wrapNegative == 2 && wrapOver == 0 && valid == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestRowStartsWith()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                
                // Test starts with "ban" for row 1 (Banana)
                var result = listBox.RowStartsWithPublic(1, "ban", out int col, out int width);
                
                return result && col == 0 && width == 3;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestRowContainsExactMatch()
        {
            try
            {
                var items = new[] { "Apple", "Banana Split", "Cherry" };
                var listBox = CreateTestListBox(items);
                
                // Test contains "split" in row 1
                var result = listBox.RowContainsExactMatchPublic(1, "split", out int col, out int width);
                
                return result && col == 7 && width == 5;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestGetSpeedSearchText()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                
                var text = listBox.GetSpeedSearchTextPublic(1);
                return text == "Banana";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }

        static bool TestGetSpeedSearchTooltip()
        {
            try
            {
                var items = new[] { "Apple", "Banana", "Cherry" };
                var listBox = CreateTestListBox(items);
                
                var tooltip = listBox.GetSpeedSearchTooltipPublic();
                
                // When closed, should show find/close options
                return tooltip.Contains("Find") && tooltip.Contains("Close");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return false;
            }
        }
    }

    // Test subclass that exposes protected methods
    public class TestSpeedSearchListBox : SpeedSearchListBoxControl
    {
        public TestSpeedSearchListBox(Window? parent, Rect rect, Colors colorNormal, Colors colorSelected, string? border, bool enabled, string[] items)
            : base(parent, rect, colorNormal, colorSelected, border, enabled)
        {
            Items = items;
        }

        public bool IsSpeedSearchOpenPublic() => IsSpeedSearchOpen();
        public bool SelectRowContainingPublic(string searchFor, int startWithRow, bool startsWith, bool containsExact, bool containsRegex, bool containsChars, int direction)
            => SelectRowContaining(searchFor, startWithRow, startsWith, containsExact, containsRegex, containsChars, direction);
        public int GetSelectedRow() => SelectedRow;
        public int MinMaxRowPublic(int row) => MinMaxRow(row);
        public bool RowStartsWithPublic(int row, string searchFor, out int col, out int width)
            => RowStartsWith(row, searchFor, out col, out width);
        public bool RowContainsExactMatchPublic(int row, string searchFor, out int col, out int width)
            => RowContainsExactMatch(row, searchFor, out col, out width);
        public string GetSpeedSearchTextPublic(int row) => GetSpeedSearchText(row);
        public string GetSpeedSearchTooltipPublic() => GetSpeedSearchTooltip();
    }
}
