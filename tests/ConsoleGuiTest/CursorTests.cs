using System;
using ConsoleGui;

namespace ConsoleGuiTest
{
    public static class CursorTests
    {
        public static bool RunAll()
        {
            Console.WriteLine("\n=== Cursor Tests ===");
            var passed = 0;
            var failed = 0;

            if (Console.IsOutputRedirected)
            {
                Console.WriteLine("  (Skipping - console redirection detected)");
                return true;
            }

            // Note: Cursor class is internal, so we test it indirectly through Screen
            RunTest("Cursor operations via Screen", TestCursorViaScreen, ref passed, ref failed);

            Console.WriteLine($"\nCursor Tests: {passed} passed, {failed} failed");
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

        static bool TestCursorViaScreen()
        {
            var screen = Screen.Current;
            
            // Save current position
            var originalX = Console.CursorLeft;
            var originalY = Console.CursorTop;

            // Try to set a new position via Screen
            var success = screen.SetCursorPosition(5, 5);
            
            // Check if it worked
            var moved = Console.CursorLeft == 5 && Console.CursorTop == 5;

            // Restore original position
            screen.SetCursorPosition(originalX, originalY);

            return success && moved;
        }
    }
}

