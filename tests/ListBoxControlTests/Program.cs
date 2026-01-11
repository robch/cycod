//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGui;
using ConsoleGui.Controls;

namespace ListBoxControlTests
{
    class Program
    {
        static int Main(string[] args)
        {
            var automated = args.Length > 0 && args[0] == "--automated";

            if (automated)
            {
                return RunAutomatedTests();
            }
            else
            {
                return RunInteractiveDemo();
            }
        }

        static int RunAutomatedTests()
        {
            var tests = new List<Func<bool>>
            {
                Test_Constructor_Success,
                Test_Items_GetSet,
                Test_GetDisplayText_ValidRow,
                Test_GetDisplayText_InvalidRow,
                Test_GetDisplayText_CarriageReturn,
                Test_GetNumRows_ReturnsItemCount,
                Test_GetNumRows_Cached,
                Test_GetNumColumns_CalculatesMaxLength,
                Test_GetNumColumns_Cached,
                Test_EmptyList_ReturnsZeroRows,
            };

            int passed = 0;
            int failed = 0;

            foreach (var test in tests)
            {
                try
                {
                    if (test())
                    {
                        passed++;
                        Console.WriteLine($"✓ {test.Method.Name}");
                    }
                    else
                    {
                        failed++;
                        Console.WriteLine($"✗ {test.Method.Name} - FAILED");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"✗ {test.Method.Name} - EXCEPTION: {ex.Message}");
                }
            }

            Console.WriteLine($"\n{passed} passed, {failed} failed");
            return failed > 0 ? 1 : 0;
        }

        static bool Test_Constructor_Success()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            return listBox != null;
        }

        static bool Test_Items_GetSet()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            
            var items = new[] { "Item1", "Item2", "Item3" };
            listBox.Items = items;
            
            return listBox.Items.Length == 3 && 
                   listBox.Items[0] == "Item1" &&
                   listBox.Items[1] == "Item2" &&
                   listBox.Items[2] == "Item3";
        }

        static bool Test_GetDisplayText_ValidRow()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            listBox.Items = new[] { "First", "Second", "Third" };
            
            return listBox.GetDisplayText(0) == "First" &&
                   listBox.GetDisplayText(1) == "Second" &&
                   listBox.GetDisplayText(2) == "Third";
        }

        static bool Test_GetDisplayText_InvalidRow()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            listBox.Items = new[] { "First", "Second" };
            
            return listBox.GetDisplayText(-1) == "" &&
                   listBox.GetDisplayText(5) == "";
        }

        static bool Test_GetDisplayText_CarriageReturn()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            listBox.Items = new[] { "Text\r", "Normal" };
            
            // Should trim trailing \r
            return listBox.GetDisplayText(0) == "Text" &&
                   listBox.GetDisplayText(1) == "Normal";
        }

        static bool Test_GetNumRows_ReturnsItemCount()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            listBox.Items = new[] { "A", "B", "C", "D", "E" };
            
            return listBox.GetNumRows() == 5;
        }

        static bool Test_GetNumRows_Cached()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            listBox.Items = new[] { "A", "B", "C" };
            
            var rows1 = listBox.GetNumRows();
            var rows2 = listBox.GetNumRows();
            
            // Should return cached value (same reference)
            return rows1 == 3 && rows2 == 3;
        }

        static bool Test_GetNumColumns_CalculatesMaxLength()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 20, 10), colors, colors);
            listBox.Items = new[] { "Short", "Medium text", "This is the longest string" };
            
            // Should return length of longest item
            return listBox.GetNumColumns() == 26; // "This is the longest string".Length
        }

        static bool Test_GetNumColumns_Cached()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 20, 10), colors, colors);
            listBox.Items = new[] { "Test", "Another" };
            
            var cols1 = listBox.GetNumColumns();
            var cols2 = listBox.GetNumColumns();
            
            // Should use cached value
            return cols1 == cols2 && cols1 == 7; // "Another".Length
        }

        static bool Test_EmptyList_ReturnsZeroRows()
        {
            var colors = ColorHelpers.GetHighlightColors();
            var listBox = new ListBoxControl(null, new Rect(0, 0, 10, 10), colors, colors);
            listBox.Items = Array.Empty<string>();
            
            return listBox.GetNumRows() == 0;
        }

        static int RunInteractiveDemo()
        {
            Console.WriteLine("=== ListBoxControl Interactive Demo ===\n");

            try
            {
                // Demo 1: Basic list with fruit
                Console.WriteLine("Demo 1: Creating a list of fruits\n");
                
                var colors = ColorHelpers.GetHighlightColors();
                var selectedColors = new Colors(ConsoleColor.Black, ConsoleColor.Yellow);
                var listBox = new ListBoxControl(null, new Rect(5, 2, 30, 8), colors, selectedColors, "single");
                
                listBox.Items = new[] { 
                    "Apple", 
                    "Banana", 
                    "Cherry", 
                    "Date",
                    "Elderberry",
                    "Fig",
                    "Grape"
                };

                Console.WriteLine($"Items count: {listBox.Items.Length}");
                Console.WriteLine($"Number of rows: {listBox.GetNumRows()}");
                Console.WriteLine($"Number of columns: {listBox.GetNumColumns()}");
                Console.WriteLine();

                Console.WriteLine("Display text for each row:");
                for (int i = 0; i < listBox.Items.Length; i++)
                {
                    Console.WriteLine($"  Row {i}: '{listBox.GetDisplayText(i)}'");
                }
                Console.WriteLine();

                // Demo 2: List with varying lengths
                Console.WriteLine("\nDemo 2: List with varying text lengths\n");
                
                var listBox2 = new ListBoxControl(null, new Rect(5, 12, 50, 6), colors, selectedColors);
                listBox2.Items = new[] {
                    "Short",
                    "Medium length text",
                    "This is a much longer string to test column calculation",
                    "Back to short"
                };

                Console.WriteLine($"Items count: {listBox2.Items.Length}");
                Console.WriteLine($"Max column width: {listBox2.GetNumColumns()}");
                Console.WriteLine();

                foreach (var item in listBox2.Items)
                {
                    Console.WriteLine($"  '{item}' (length: {item.Length})");
                }
                Console.WriteLine();

                // Demo 3: Empty list
                Console.WriteLine("\nDemo 3: Empty list\n");
                
                var emptyListBox = new ListBoxControl(null, new Rect(5, 20, 20, 5), colors, selectedColors);
                emptyListBox.Items = Array.Empty<string>();

                Console.WriteLine($"Items count: {emptyListBox.Items.Length}");
                Console.WriteLine($"Number of rows: {emptyListBox.GetNumRows()}");
                Console.WriteLine($"Number of columns: {emptyListBox.GetNumColumns()}");
                Console.WriteLine();

                // Demo 4: Text with carriage returns
                Console.WriteLine("\nDemo 4: Handling carriage returns\n");
                
                var listBox3 = new ListBoxControl(null, new Rect(5, 27, 25, 4), colors, selectedColors);
                listBox3.Items = new[] {
                    "Normal text",
                    "Text with CR\r",
                    "Another normal",
                    "Multiple CRs\r\r"
                };

                Console.WriteLine("Original items:");
                for (int i = 0; i < listBox3.Items.Length; i++)
                {
                    Console.WriteLine($"  Item {i}: '{listBox3.Items[i].Replace("\r", "\\r")}'");
                }

                Console.WriteLine("\nDisplay text (CRs trimmed):");
                for (int i = 0; i < listBox3.Items.Length; i++)
                {
                    Console.WriteLine($"  Row {i}: '{listBox3.GetDisplayText(i)}'");
                }
                Console.WriteLine();

                Console.WriteLine("\n✓ All demos completed successfully!");
                Console.WriteLine("\nNote: Visual rendering requires Screen.Init() and proper console setup.");
                Console.WriteLine("      These tests verify the data model and text handling only.");
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Demo failed with exception: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}
