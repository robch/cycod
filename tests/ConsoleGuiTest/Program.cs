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
                // Check if we have a console
                if (Console.IsOutputRedirected)
                {
                    Console.WriteLine("Test requires an interactive console. Skipping interactive test.");
                    return TestWithoutConsole();
                }
                
                return TestScreenAndWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static int TestWithoutConsole()
        {
            Console.WriteLine("Running basic object instantiation tests...");
            
            // Test that classes can be instantiated
            var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
            Console.WriteLine($"✓ Colors created: fg={colors.Foreground}, bg={colors.Background}");
            
            var rect = new Rect(0, 0, 10, 5);
            Console.WriteLine($"✓ Rect created: x={rect.X}, y={rect.Y}, w={rect.Width}, h={rect.Height}");
            
            Console.WriteLine("✓ All basic tests passed!");
            return 0;
        }

        static int TestScreenAndWindow()
        {
            Console.WriteLine("Testing Screen and Window classes...\n");

            // Test 1: Screen initialization
            Console.WriteLine("Test 1: Screen.Current initialized");
            var screen = Screen.Current;
            Console.WriteLine($"  Screen width: {Console.WindowWidth}");
            Console.WriteLine($"  Screen height: {Console.WindowHeight}");

            // Test 2: Colors
            Console.WriteLine("\nTest 2: Colors");
            var originalColors = screen.ColorsStart;
            Console.WriteLine($"  Original foreground: {originalColors.Foreground}");
            Console.WriteLine($"  Original background: {originalColors.Background}");
            
            // Test 3: Create a simple window
            Console.WriteLine("\nTest 3: Create and display a window with border");
            var testColors = new Colors(ConsoleColor.White, ConsoleColor.DarkBlue);
            var rect = screen.MakeSpaceAtCursor(40, 5);
            var window = new Window(null, rect, testColors, Window.Borders.SingleLine);
            
            window.Open();
            window.WriteClientText(testColors, 1, 1, "Hello from Console GUI!");
            window.WriteClientText(testColors, 1, 2, "This is a test window.");
            
            Console.WriteLine("\n\nPress any key to close the window...");
            Console.ReadKey(true);
            
            window.Close();
            screen.Reset();
            
            Console.WriteLine("\nTest complete!");
            return 0;
        }
    }
}
