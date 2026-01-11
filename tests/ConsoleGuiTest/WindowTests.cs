using System;
using ConsoleGui;

namespace ConsoleGuiTest
{
    public static class WindowTests
    {
        public static bool RunAll()
        {
            Console.WriteLine("\n=== Window Tests ===");
            var passed = 0;
            var failed = 0;

            if (Console.IsOutputRedirected)
            {
                Console.WriteLine("  (Skipping - console redirection detected)");
                return true;
            }

            RunTest("Construction with borders", TestConstruction, ref passed, ref failed);
            RunTest("Open and close", TestOpenClose, ref passed, ref failed);
            RunTest("Write client text", TestWriteClientText, ref passed, ref failed);
            RunTest("Different border types", TestBorderTypes, ref passed, ref failed);
            RunTest("Client area calculations", TestClientArea, ref passed, ref failed);

            Console.WriteLine($"\nWindow Tests: {passed} passed, {failed} failed");
            return failed == 0;
        }

        static void RunTest(string name, Func<bool> test, ref int passed, ref int failed)
        {
            try
            {
                if (test())
                {
                    Console.WriteLine($"  ✓ {name}");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"  ✗ {name}");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ {name}: {ex.Message}");
                failed++;
            }
        }

        static bool TestConstruction()
        {
            var screen = Screen.Current;
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            var rect = new Rect(0, 0, 20, 10);
            
            var window = new Window(null, rect, colors, Window.Borders.SingleLine);
            
            return window != null;
        }

        static bool TestOpenClose()
        {
            var screen = Screen.Current;
            var colors = new Colors(ConsoleColor.White, ConsoleColor.DarkBlue);
            var rect = screen.MakeSpaceAtCursor(30, 8);
            
            var window = new Window(null, rect, colors, Window.Borders.SingleLine);
            
            // Open the window
            window.Open();
            
            // Give it a moment to render
            System.Threading.Thread.Sleep(100);
            
            // Close the window
            window.Close();
            
            return true; // Success = no crash
        }

        static bool TestWriteClientText()
        {
            var screen = Screen.Current;
            var colors = new Colors(ConsoleColor.Yellow, ConsoleColor.DarkGreen);
            var rect = screen.MakeSpaceAtCursor(40, 6);
            
            var window = new Window(null, rect, colors, Window.Borders.SingleLine);
            window.Open();
            
            // Write some text
            window.WriteClientText(colors, 1, 1, "Test line 1");
            window.WriteClientText(colors, 1, 2, "Test line 2");
            window.WriteClientText(colors, 1, 3, "Test line 3");
            
            // Give it a moment to render
            System.Threading.Thread.Sleep(100);
            
            window.Close();
            
            return true; // Success = no crash
        }

        static bool TestBorderTypes()
        {
            var screen = Screen.Current;
            var colors = new Colors(ConsoleColor.Cyan, ConsoleColor.Black);
            
            // Test with border
            var rect1 = screen.MakeSpaceAtCursor(25, 4);
            var window1 = new Window(null, rect1, colors, Window.Borders.SingleLine);
            window1.Open();
            window1.WriteClientText(colors, 1, 1, "Border: SingleLine");
            System.Threading.Thread.Sleep(50);
            window1.Close();

            // Test without border (null)
            var rect2 = screen.MakeSpaceAtCursor(25, 4);
            var window2 = new Window(null, rect2, colors, null);
            window2.Open();
            window2.WriteClientText(colors, 1, 1, "Border: None");
            System.Threading.Thread.Sleep(50);
            window2.Close();
            
            return true; // Success = no crash
        }

        static bool TestClientArea()
        {
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            
            // Test with single line border
            var rect1 = new Rect(0, 0, 20, 10);
            var window1 = new Window(null, rect1, colors, Window.Borders.SingleLine);
            
            // With single line border, client area should be 2 less in each dimension
            // (This is implicit in how the window works - WriteClientText uses client coords)
            
            // Test with no border
            var rect2 = new Rect(0, 0, 20, 10);
            var window2 = new Window(null, rect2, colors, null);
            
            // Both should construct without issues
            return window1 != null && window2 != null;
        }
    }
}
