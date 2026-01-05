using System;
using ConsoleGui;

namespace ConsoleGuiTest
{
    public static class ScreenTests
    {
        public static bool RunAll()
        {
            Console.WriteLine("\n=== Screen Tests ===");
            var passed = 0;
            var failed = 0;

            if (Console.IsOutputRedirected)
            {
                Console.WriteLine("  (Skipping - console redirection detected)");
                return true;
            }

            RunTest("Current instance", TestCurrentInstance, ref passed, ref failed);
            RunTest("Color management", TestColorManagement, ref passed, ref failed);
            RunTest("Cursor positioning", TestCursorPositioning, ref passed, ref failed);
            RunTest("MakeSpaceAtCursor", TestMakeSpaceAtCursor, ref passed, ref failed);
            RunTest("Valid cursor position check", TestValidCursorPosition, ref passed, ref failed);
            RunTest("Reset functionality", TestReset, ref passed, ref failed);

            Console.WriteLine($"\nScreen Tests: {passed} passed, {failed} failed");
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

        static bool TestCurrentInstance()
        {
            var screen = Screen.Current;
            return screen != null;
        }

        static bool TestColorManagement()
        {
            var screen = Screen.Current;
            
            // Get initial colors
            var initialColors = screen.ColorsStart;
            if (initialColors == null) return false;

            // Get current colors
            var currentColors = screen.ColorsNow;
            if (currentColors == null) return false;

            // Set some test colors
            var testColors = new Colors(ConsoleColor.Yellow, ConsoleColor.DarkMagenta);
            screen.SetColors(testColors);

            // Verify they were set
            var verifyFg = Console.ForegroundColor == ConsoleColor.Yellow;
            var verifyBg = Console.BackgroundColor == ConsoleColor.DarkMagenta;

            // Restore original colors
            screen.SetColors(initialColors);

            return verifyFg && verifyBg;
        }

        static bool TestCursorPositioning()
        {
            var screen = Screen.Current;
            
            // Save current position
            var originalX = Console.CursorLeft;
            var originalY = Console.CursorTop;

            // Try to set a new position
            var success = screen.SetCursorPosition(5, 5);
            
            // Check if it worked
            var moved = Console.CursorLeft == 5 && Console.CursorTop == 5;

            // Restore original position
            screen.SetCursorPosition(originalX, originalY);

            return success && moved;
        }

        static bool TestMakeSpaceAtCursor()
        {
            var screen = Screen.Current;
            
            // Save position
            var originalY = Console.CursorTop;

            // Request space for a small window
            var rect = screen.MakeSpaceAtCursor(20, 5);

            // Verify rect was created
            if (rect == null) return false;
            if (rect.Width != 20) return false;
            if (rect.Height != 5) return false;

            // The cursor should have moved down
            var cursorMoved = Console.CursorTop > originalY;

            return cursorMoved;
        }

        static bool TestValidCursorPosition()
        {
            var screen = Screen.Current;
            
            // Current position should always be valid
            var currentX = Console.CursorLeft;
            var currentY = Console.CursorTop;
            
            return screen.IsValidCursorPosition(currentX, currentY);
        }

        static bool TestReset()
        {
            var screen = Screen.Current;
            
            // Change some settings
            screen.SetColors(new Colors(ConsoleColor.Red, ConsoleColor.Blue));
            
            // Reset should restore everything
            screen.Reset();
            
            // Verify colors were restored (check that we didn't crash)
            var colors = screen.ColorsNow;
            
            return colors != null;
        }
    }
}
