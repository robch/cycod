using System;
using ConsoleGui;

namespace ConsoleGuiTest
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("Console GUI Comprehensive Test Suite");
                Console.WriteLine("=====================================\n");

                var allPassed = true;

                // Run non-interactive tests first (these work even with redirected output)
                allPassed &= RectTests.RunAll();

                // Run interactive tests if we have a real console
                if (!Console.IsOutputRedirected)
                {
                    allPassed &= CursorTests.RunAll();
                    allPassed &= ScreenTests.RunAll();
                    allPassed &= WindowTests.RunAll();

                    // Run visual demo if all tests passed
                    if (allPassed)
                    {
                        Console.WriteLine("\n=== Visual Demo ===");
                        Console.WriteLine("Running visual demonstration of Window functionality...\n");
                        RunVisualDemo();
                    }
                }
                else
                {
                    Console.WriteLine("\n(Interactive tests skipped - console redirection detected)");
                }

                // Summary
                Console.WriteLine("\n=====================================");
                if (allPassed)
                {
                    Console.WriteLine("✓ ALL TESTS PASSED");
                    return 0;
                }
                else
                {
                    Console.WriteLine("✗ SOME TESTS FAILED");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ FATAL ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        static void RunVisualDemo()
        {
            var screen = Screen.Current;
            var colors = new Colors(ConsoleColor.White, ConsoleColor.DarkBlue);
            var rect = screen.MakeSpaceAtCursor(50, 8);
            var window = new Window(null, rect, colors, Window.Borders.SingleLine);
            
            window.Open();
            window.WriteClientText(colors, 2, 1, "╔═══════════════════════════════════════════╗");
            window.WriteClientText(colors, 2, 2, "║  Console GUI Foundation Components Demo  ║");
            window.WriteClientText(colors, 2, 3, "╚═══════════════════════════════════════════╝");
            window.WriteClientText(colors, 2, 5, "✓ Screen management");
            window.WriteClientText(colors, 2, 6, "✓ Window rendering with borders");
            window.WriteClientText(colors, 2, 7, "✓ Color management");
            window.WriteClientText(colors, 2, 8, "✓ Cursor positioning");
            
            Console.WriteLine("\n\nPress any key to close and complete tests...");
            Console.ReadKey(true);
            
            window.Close();
            screen.Reset();
        }
    }
}
