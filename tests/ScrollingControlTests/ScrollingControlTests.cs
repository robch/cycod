//
// ScrollingControlTests - Comprehensive tests for ScrollingControl.cs
//

#nullable enable

using System;
using ConsoleGui.Controls;

namespace ConsoleGui.Tests
{
    // Test class that implements ScrollingControl
    public class TestScrollingControl : ScrollingControl
    {
        private int _numRows;
        private int _numColumns;

        public TestScrollingControl(Window? parent, Rect rect, Colors colors, string? border, bool fEnabled, int rows, int cols) 
            : base(parent, rect, colors, border, fEnabled)
        {
            _numRows = rows;
            _numColumns = cols;
        }

        public override int GetNumRows() => _numRows;
        public override int GetNumColumns() => _numColumns;

        public void SetRows(int rows) => _numRows = rows;
        public void SetColumns(int cols) => _numColumns = cols;

        // Expose protected methods for testing
        public new bool SetSelectedRow(int row) => base.SetSelectedRow(row);
        public new bool SetSelectedColumn(int col, int width = 1) => base.SetSelectedColumn(col, width);
        public new bool SetRowOffset(int offset) => base.SetRowOffset(offset);
        public new bool SetColumnOffset(int offset) => base.SetColumnOffset(offset);
    }

    class ScrollingControlTests
    {
        static void Main(string[] args)
        {
            var interactive = args.Length > 0 && args[0] == "--interactive";

            try
            {
                if (!interactive)
                {
                    // Run automated tests
                    TestBasicConstruction();
                    TestSelectedRowWithinBounds();
                    TestSelectedRowOutOfBounds();
                    TestSelectedRowWithScrolling();
                    TestSelectedColumnWithinBounds();
                    TestSelectedColumnWithScrolling();
                    TestRowOffsetCalculation();
                    TestColumnOffsetCalculation();
                    TestZeroRowsAndColumns();
                    TestScrollingWithSmallViewport();

                    Console.WriteLine("\n=== ALL TESTS PASSED ===");
                }
                else
                {
                    // Run interactive demonstration
                    RunInteractiveDemo();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n*** TEST FAILED: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        static void TestBasicConstruction()
        {
            var rect = new Rect(0, 0, 20, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            AssertEqual(control.SelectedRow, 0, "Initial SelectedRow should be 0");
            AssertEqual(control.SelectedColumn, 0, "Initial SelectedColumn should be 0");
            AssertEqual(control.SelectedColumnWidth, 1, "Initial SelectedColumnWidth should be 1");
            AssertEqual(control.RowOffset, 0, "Initial RowOffset should be 0");
            AssertEqual(control.ColumnOffset, 0, "Initial ColumnOffset should be 0");
            AssertEqual(control.GetNumRows(), 100, "Should have 100 rows");
            AssertEqual(control.GetNumColumns(), 80, "Should have 80 columns");

            Console.WriteLine("✓ TestBasicConstruction passed");
        }

        static void TestSelectedRowWithinBounds()
        {
            var rect = new Rect(0, 0, 20, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Set row within bounds - should return true (changed)
            var changed = control.SetSelectedRow(5);
            AssertTrue(changed, "SetSelectedRow should return true when changed");
            AssertEqual(control.SelectedRow, 5, "SelectedRow should be 5");

            // Set same row again - should return false (not changed)
            changed = control.SetSelectedRow(5);
            AssertFalse(changed, "SetSelectedRow should return false when not changed");

            Console.WriteLine("✓ TestSelectedRowWithinBounds passed");
        }

        static void TestSelectedRowOutOfBounds()
        {
            var rect = new Rect(0, 0, 20, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Try to set row beyond max - should clamp to GetNumRows() - 1
            control.SetSelectedRow(200);
            AssertEqual(control.SelectedRow, 99, "SelectedRow should be clamped to 99");

            // Note: Negative rows are allowed in the original implementation
            // SetSelectedRow only clamps to 0 when GetNumRows() == 0
            // Otherwise, negative values are allowed (for internal logic)

            Console.WriteLine("✓ TestSelectedRowOutOfBounds passed");
        }

        static void TestSelectedRowWithScrolling()
        {
            var rect = new Rect(0, 0, 20, 10);  // ClientRect will be 18x8 (border takes 2x2)
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Select row 0 - offset should be 0
            control.SetSelectedRow(0);
            AssertEqual(control.RowOffset, 0, "RowOffset should be 0 when row 0 selected");

            // Select row 5 - still within viewport, offset should stay 0
            control.SetSelectedRow(5);
            AssertEqual(control.RowOffset, 0, "RowOffset should be 0 when within viewport");

            // Select row 10 - beyond viewport, should scroll
            control.SetSelectedRow(10);
            AssertTrue(control.RowOffset > 0, "RowOffset should increase when scrolling down");

            // Select row 0 again - should scroll back to top
            control.SetSelectedRow(0);
            AssertEqual(control.RowOffset, 0, "RowOffset should be 0 when scrolling back to top");

            Console.WriteLine("✓ TestSelectedRowWithScrolling passed");
        }

        static void TestSelectedColumnWithinBounds()
        {
            var rect = new Rect(0, 0, 20, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Set column within bounds
            var changed = control.SetSelectedColumn(10, 5);
            AssertTrue(changed, "SetSelectedColumn should return true when changed");
            AssertEqual(control.SelectedColumn, 10, "SelectedColumn should be 10");
            AssertEqual(control.SelectedColumnWidth, 5, "SelectedColumnWidth should be 5");

            // Set same column and width - should return false
            changed = control.SetSelectedColumn(10, 5);
            AssertFalse(changed, "SetSelectedColumn should return false when not changed");

            // Set same column but different width - should return true
            changed = control.SetSelectedColumn(10, 3);
            AssertTrue(changed, "SetSelectedColumn should return true when width changes");

            Console.WriteLine("✓ TestSelectedColumnWithinBounds passed");
        }

        static void TestSelectedColumnWithScrolling()
        {
            var rect = new Rect(0, 0, 20, 10);  // ClientRect will be 18x8
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Select column 0 - offset should be 0
            control.SetSelectedColumn(0);
            AssertEqual(control.ColumnOffset, 0, "ColumnOffset should be 0 when column 0 selected");

            // Select column beyond viewport - should scroll
            control.SetSelectedColumn(30);
            AssertTrue(control.ColumnOffset > 0, "ColumnOffset should increase when scrolling right");

            // Select column 0 again - should scroll back to left
            control.SetSelectedColumn(0);
            AssertEqual(control.ColumnOffset, 0, "ColumnOffset should be 0 when scrolling back to left");

            Console.WriteLine("✓ TestSelectedColumnWithScrolling passed");
        }

        static void TestRowOffsetCalculation()
        {
            var rect = new Rect(0, 0, 20, 10);  // ClientRect will be 18x8
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Set row offset directly
            var changed = control.SetRowOffset(10);
            AssertTrue(changed, "SetRowOffset should return true when changed");
            AssertEqual(control.RowOffset, 10, "RowOffset should be 10");

            // Try to set offset beyond valid range - should clamp
            control.SetRowOffset(200);
            AssertTrue(control.RowOffset < 200, "RowOffset should be clamped");

            // Small number of rows - offset should be 0
            control.SetRows(5);
            control.SetRowOffset(10);
            AssertEqual(control.RowOffset, 0, "RowOffset should be 0 when rows fit in viewport");

            Console.WriteLine("✓ TestRowOffsetCalculation passed");
        }

        static void TestColumnOffsetCalculation()
        {
            var rect = new Rect(0, 0, 20, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 100, 80);

            // Set column offset directly
            var changed = control.SetColumnOffset(20);
            AssertTrue(changed, "SetColumnOffset should return true when changed");
            AssertEqual(control.ColumnOffset, 20, "ColumnOffset should be 20");

            // Small number of columns - offset should be 0
            control.SetColumns(10);
            control.SetColumnOffset(20);
            AssertEqual(control.ColumnOffset, 0, "ColumnOffset should be 0 when columns fit in viewport");

            Console.WriteLine("✓ TestColumnOffsetCalculation passed");
        }

        static void TestZeroRowsAndColumns()
        {
            var rect = new Rect(0, 0, 20, 10);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 0, 0);

            // With 0 rows, selected row should become 0
            control.SetSelectedRow(10);
            AssertEqual(control.SelectedRow, 0, "SelectedRow should be 0 when no rows");

            // With 0 columns, selected column should become 0
            control.SetSelectedColumn(10);
            AssertEqual(control.SelectedColumn, 0, "SelectedColumn should be 0 when no columns");

            // Negative column should become 0
            control.SetColumns(10);
            control.SetSelectedColumn(-5);
            AssertEqual(control.SelectedColumn, 0, "Negative column should become 0");

            Console.WriteLine("✓ TestZeroRowsAndColumns passed");
        }

        static void TestScrollingWithSmallViewport()
        {
            // Very small viewport: 10x5, ClientRect will be 8x3
            var rect = new Rect(0, 0, 10, 5);
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var control = new TestScrollingControl(null, rect, colors, "single", true, 50, 40);

            // Select last visible row in viewport
            control.SetSelectedRow(2);
            AssertEqual(control.RowOffset, 0, "RowOffset should be 0 when within viewport");

            // Select row just beyond viewport
            control.SetSelectedRow(3);
            AssertEqual(control.RowOffset, 1, "RowOffset should be 1 when scrolling to row 3");

            // Select row far down
            control.SetSelectedRow(20);
            AssertTrue(control.RowOffset > 10, "RowOffset should be large when scrolling far down");

            Console.WriteLine("✓ TestScrollingWithSmallViewport passed");
        }

        static void RunInteractiveDemo()
        {
            Console.WriteLine("=== ScrollingControl Interactive Demo ===\n");

            var rect = new Rect(5, 5, 40, 15);
            var colors = new Colors(ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            var control = new TestScrollingControl(null, rect, colors, "double", true, 50, 60);

            Console.WriteLine("Created scrolling control:");
            Console.WriteLine($"  Position: {rect.X},{rect.Y}");
            Console.WriteLine($"  Size: {rect.Width}x{rect.Height}");
            Console.WriteLine($"  Content: {control.GetNumRows()} rows x {control.GetNumColumns()} cols");
            Console.WriteLine($"  ClientRect: {control.ClientRect.Width}x{control.ClientRect.Height}");

            Console.WriteLine("\nSimulating row selection...");
            for (int i = 0; i < 10; i++)
            {
                control.SetSelectedRow(i * 5);
                Console.WriteLine($"  Row {i * 5}: RowOffset = {control.RowOffset}");
            }

            Console.WriteLine("\nSimulating column selection...");
            for (int i = 0; i < 5; i++)
            {
                control.SetSelectedColumn(i * 10);
                Console.WriteLine($"  Col {i * 10}: ColumnOffset = {control.ColumnOffset}");
            }

            Console.WriteLine("\n=== Demo Complete ===");
        }

        // Helper assertion methods
        static void AssertEqual(int actual, int expected, string message)
        {
            if (actual != expected)
            {
                throw new Exception($"{message} - Expected: {expected}, Actual: {actual}");
            }
        }

        static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"{message} - Expected: true, Actual: false");
            }
        }

        static void AssertFalse(bool condition, string message)
        {
            if (condition)
            {
                throw new Exception($"{message} - Expected: false, Actual: true");
            }
        }
    }
}
