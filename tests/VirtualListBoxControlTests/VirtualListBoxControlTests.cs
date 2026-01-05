using System;
using System.Collections.Generic;
using ConsoleGui.Controls;

namespace ConsoleGui.Tests;

/// <summary>
/// Tests for VirtualListBoxControl - virtual list rendering with scrolling
/// </summary>
class VirtualListBoxControlTests
{
    // Concrete implementation of VirtualListBoxControl for testing
    class TestListBox : VirtualListBoxControl
    {
        private List<string> items;

        public TestListBox(Window? parent, Rect rect, List<string> items, Colors colorNormal, Colors colorSelected, string? border = null, bool fEnabled = true)
            : base(parent, rect, colorNormal, colorSelected, border, fEnabled)
        {
            this.items = items;
        }

        public override int GetNumRows()
        {
            return items.Count;
        }

        public override int GetNumColumns()
        {
            // Find the longest item
            int maxLen = 0;
            foreach (var item in items)
            {
                if (item.Length > maxLen)
                    maxLen = item.Length;
            }
            return maxLen;
        }

        public override void DisplayRow(int row)
        {
            if (row >= 0 && row < items.Count)
            {
                var colors = ColorsFromRow(row);
                WriteClientText(colors, 0, row, items[row]);
            }
        }

        // Helper to get access to protected members for testing
        public int GetSelectedRowPublic() => SelectedRow;
        public int GetRowOffsetPublic() => RowOffset;
        public int GetColumnOffsetPublic() => ColumnOffset;
    }

    static void Main(string[] args)
    {
        bool interactive = args.Length > 0 && args[0] == "--interactive";

        if (interactive)
        {
            RunInteractiveDemo();
        }
        else
        {
            RunAutomatedTests();
        }
    }

    static void RunAutomatedTests()
    {
        Console.WriteLine("Running VirtualListBoxControl Tests...\n");

        int passed = 0;
        int failed = 0;

        // Test 1: Constructor and basic properties
        try
        {
            var items = new List<string> { "Item 1", "Item 2", "Item 3" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            if (listBox.GetNumRows() == 3 && listBox.GetNumColumns() == 6)
            {
                Console.WriteLine("✓ Test 1 PASSED: Constructor and basic properties");
                passed++;
            }
            else
            {
                Console.WriteLine($"✗ Test 1 FAILED: Expected 3 rows and 6 columns, got {listBox.GetNumRows()} rows and {listBox.GetNumColumns()} columns");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 1 FAILED: {ex.Message}");
            failed++;
        }

        // Test 2: Navigation methods
        try
        {
            var items = new List<string> { "A", "B", "C", "D", "E" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Don't open the window - just test the navigation logic
            // The methods update internal state without requiring rendering

            // Test Down
            listBox.Down();
            if (listBox.GetSelectedRowPublic() != 1)
            {
                Console.WriteLine($"✗ Test 2 FAILED: Down didn't work, row = {listBox.GetSelectedRowPublic()}");
                failed++;
            }
            else
            {
                // Test Up
                listBox.Up();
                if (listBox.GetSelectedRowPublic() != 0)
                {
                    Console.WriteLine($"✗ Test 2 FAILED: Up didn't work, row = {listBox.GetSelectedRowPublic()}");
                    failed++;
                }
                else
                {
                    Console.WriteLine("✓ Test 2 PASSED: Navigation methods (Up/Down)");
                    passed++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 2 FAILED: {ex.Message}");
            failed++;
        }

        // Test 3: Home and End
        try
        {
            var items = new List<string> { "A", "B", "C", "D", "E" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Move to middle
            listBox.Down();
            listBox.Down();

            // Test End
            listBox.End();
            if (listBox.GetSelectedRowPublic() != 4)
            {
                Console.WriteLine($"✗ Test 3 FAILED: End didn't work, row = {listBox.GetSelectedRowPublic()}");
                failed++;
            }
            else
            {
                // Test Home
                listBox.Home();
                if (listBox.GetSelectedRowPublic() != 0)
                {
                    Console.WriteLine($"✗ Test 3 FAILED: Home didn't work, row = {listBox.GetSelectedRowPublic()}");
                    failed++;
                }
                else
                {
                    Console.WriteLine("✓ Test 3 PASSED: Home and End navigation");
                    passed++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 3 FAILED: {ex.Message}");
            failed++;
        }

        // Test 4: ProcessKey with arrow keys
        try
        {
            var items = new List<string> { "A", "B", "C", "D", "E" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Test DownArrow key
            var downKey = new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false);
            if (!listBox.ProcessKey(downKey))
            {
                Console.WriteLine("✗ Test 4 FAILED: ProcessKey didn't handle DownArrow");
                failed++;
            }
            else if (listBox.GetSelectedRowPublic() != 1)
            {
                Console.WriteLine($"✗ Test 4 FAILED: DownArrow didn't move selection, row = {listBox.GetSelectedRowPublic()}");
                failed++;
            }
            else
            {
                // Test UpArrow key
                var upKey = new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false);
                if (!listBox.ProcessKey(upKey))
                {
                    Console.WriteLine("✗ Test 4 FAILED: ProcessKey didn't handle UpArrow");
                    failed++;
                }
                else if (listBox.GetSelectedRowPublic() != 0)
                {
                    Console.WriteLine($"✗ Test 4 FAILED: UpArrow didn't move selection, row = {listBox.GetSelectedRowPublic()}");
                    failed++;
                }
                else
                {
                    Console.WriteLine("✓ Test 4 PASSED: ProcessKey handles arrow keys");
                    passed++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 4 FAILED: {ex.Message}");
            failed++;
        }

        // Test 5: PageDown and PageUp
        try
        {
            var items = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                items.Add($"Item {i}");
            }
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5); // 5 rows visible
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Test PageDown
            listBox.PageDown();
            int expectedRow = 5 - 1; // Should be at bottom of first page
            if (listBox.GetSelectedRowPublic() != expectedRow)
            {
                Console.WriteLine($"✗ Test 5 FAILED: PageDown didn't work correctly, expected row {expectedRow}, got {listBox.GetSelectedRowPublic()}");
                failed++;
            }
            else
            {
                // Test PageUp
                listBox.PageUp();
                if (listBox.GetSelectedRowPublic() != 0)
                {
                    Console.WriteLine($"✗ Test 5 FAILED: PageUp didn't work correctly, expected row 0, got {listBox.GetSelectedRowPublic()}");
                    failed++;
                }
                else
                {
                    Console.WriteLine("✓ Test 5 PASSED: PageDown and PageUp navigation");
                    passed++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 5 FAILED: {ex.Message}");
            failed++;
        }

        // Test 6: SelectedColors property
        try
        {
            var items = new List<string> { "A", "B", "C" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            if (listBox.SelectedColors.Foreground == ConsoleColor.Black &&
                listBox.SelectedColors.Background == ConsoleColor.White)
            {
                Console.WriteLine("✓ Test 6 PASSED: SelectedColors property");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ Test 6 FAILED: SelectedColors property not set correctly");
                failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 6 FAILED: {ex.Message}");
            failed++;
        }

        // Test 7: Empty list handling
        try
        {
            var items = new List<string>();
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Test End on empty list
            listBox.End();
            if (listBox.GetSelectedRowPublic() != 0)
            {
                Console.WriteLine($"✗ Test 7 FAILED: End on empty list should be 0, got {listBox.GetSelectedRowPublic()}");
                failed++;
            }
            else
            {
                Console.WriteLine("✓ Test 7 PASSED: Empty list handling");
                passed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 7 FAILED: {ex.Message}");
            failed++;
        }

        // Test 8: ProcessKey with Home/End keys
        try
        {
            var items = new List<string> { "A", "B", "C", "D", "E" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Test End key
            var endKey = new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false);
            if (!listBox.ProcessKey(endKey))
            {
                Console.WriteLine("✗ Test 8 FAILED: ProcessKey didn't handle End");
                failed++;
            }
            else if (listBox.GetSelectedRowPublic() != 4)
            {
                Console.WriteLine($"✗ Test 8 FAILED: End key didn't move to last row, row = {listBox.GetSelectedRowPublic()}");
                failed++;
            }
            else
            {
                // Test Home key
                var homeKey = new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false);
                if (!listBox.ProcessKey(homeKey))
                {
                    Console.WriteLine("✗ Test 8 FAILED: ProcessKey didn't handle Home");
                    failed++;
                }
                else if (listBox.GetSelectedRowPublic() != 0)
                {
                    Console.WriteLine($"✗ Test 8 FAILED: Home key didn't move to first row, row = {listBox.GetSelectedRowPublic()}");
                    failed++;
                }
                else
                {
                    Console.WriteLine("✓ Test 8 PASSED: ProcessKey handles Home/End keys");
                    passed++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 8 FAILED: {ex.Message}");
            failed++;
        }

        // Test 9: ProcessKey with PageUp/PageDown keys
        try
        {
            var items = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                items.Add($"Item {i}");
            }
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Test PageDown key
            var pageDownKey = new ConsoleKeyInfo('\0', ConsoleKey.PageDown, false, false, false);
            if (!listBox.ProcessKey(pageDownKey))
            {
                Console.WriteLine("✗ Test 9 FAILED: ProcessKey didn't handle PageDown");
                failed++;
            }
            else
            {
                // Test PageUp key
                var pageUpKey = new ConsoleKeyInfo('\0', ConsoleKey.PageUp, false, false, false);
                if (!listBox.ProcessKey(pageUpKey))
                {
                    Console.WriteLine("✗ Test 9 FAILED: ProcessKey didn't handle PageUp");
                    failed++;
                }
                else
                {
                    Console.WriteLine("✓ Test 9 PASSED: ProcessKey handles PageUp/PageDown keys");
                    passed++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 9 FAILED: {ex.Message}");
            failed++;
        }

        // Test 10: Unhandled key returns false
        try
        {
            var items = new List<string> { "A", "B", "C" };
            var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.White);
            var rect = new Rect(0, 0, 20, 5);
            var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected);

            // Test with a key that shouldn't be handled (e.g., 'A')
            var unhandledKey = new ConsoleKeyInfo('A', ConsoleKey.A, false, false, false);
            if (listBox.ProcessKey(unhandledKey))
            {
                Console.WriteLine("✗ Test 10 FAILED: ProcessKey returned true for unhandled key");
                failed++;
            }
            else
            {
                Console.WriteLine("✓ Test 10 PASSED: Unhandled key returns false");
                passed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test 10 FAILED: {ex.Message}");
            failed++;
        }

        // Summary
        Console.WriteLine($"\n{'=',-50}");
        Console.WriteLine($"Tests Passed: {passed}");
        Console.WriteLine($"Tests Failed: {failed}");
        Console.WriteLine($"{'=',-50}");

        Environment.Exit(failed > 0 ? 1 : 0);
    }

    static void RunInteractiveDemo()
    {
        Console.WriteLine("VirtualListBoxControl Interactive Demo");
        Console.WriteLine("======================================\n");
        Console.WriteLine("This demo shows a virtual list box with navigation.");
        Console.WriteLine("Use arrow keys, Page Up/Down, Home/End to navigate.");
        Console.WriteLine("Press ESC to exit.\n");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);

        Console.Clear();

        // Create a large list of items
        var items = new List<string>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add($"Item {i:D3} - This is a test item");
        }

        var colorNormal = new Colors(ConsoleColor.White, ConsoleColor.Blue);
        var colorSelected = new Colors(ConsoleColor.Black, ConsoleColor.Cyan);

        var rect = new Rect(5, 3, 60, 15);
        var listBox = new TestListBox(null, rect, items, colorNormal, colorSelected, "single");

        listBox.Open();
        listBox.SetFocus();

        // Instructions
        var instructionWindow = new Window(null, new Rect(5, 20, 60, 5), new Colors(ConsoleColor.Yellow, ConsoleColor.Black), "single");
        instructionWindow.Open();
        instructionWindow.WriteClientText(1, 0, "Arrow Keys: Navigate  |  Page Up/Down: Scroll");
        instructionWindow.WriteClientText(1, 1, "Home: First Item  |  End: Last Item");
        instructionWindow.WriteClientText(1, 2, "ESC: Exit");

        bool running = true;
        while (running)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
            {
                running = false;
            }
            else
            {
                listBox.ProcessKey(key);
            }

            // Show current position
            instructionWindow.WriteClientText(1, 3, $"Position: {listBox.GetSelectedRowPublic() + 1}/{items.Count}  Offset: {listBox.GetRowOffsetPublic()}     ");
        }

        instructionWindow.Close();
        listBox.Close();

        Console.Clear();
        Console.WriteLine("\nDemo completed!");
    }
}
