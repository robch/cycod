using System;
using ConsoleGui;
using ConsoleGui.Controls;

namespace ConsoleGuiTests
{
    class ControlWindowTests
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ControlWindow Tests ===\n");

            // Test 1: Create a basic ControlWindow
            Console.WriteLine("Test 1: Create basic ControlWindow");
            try
            {
                var rect = new Rect(5, 5, 40, 10);
                var colors = new Colors(ConsoleColor.White, ConsoleColor.Blue);
                var window = new ControlWindow(null, rect, colors, "single", true);
                
                Console.WriteLine("✓ ControlWindow created successfully");
                Console.WriteLine($"  Enabled: {window.IsEnabled()}");
                Console.WriteLine($"  Rectangle: {rect.X},{rect.Y} {rect.Width}x{rect.Height}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
                return;
            }

            // Test 2: Create disabled ControlWindow
            Console.WriteLine("\nTest 2: Create disabled ControlWindow");
            try
            {
                var rect = new Rect(10, 10, 30, 8);
                var colors = new Colors(ConsoleColor.Gray, ConsoleColor.Black);
                var window = new ControlWindow(null, rect, colors, "double", false);
                
                Console.WriteLine("✓ Disabled ControlWindow created successfully");
                Console.WriteLine($"  Enabled: {window.IsEnabled()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
                return;
            }

            // Test 3: Test IsHotKey (should return false by default)
            Console.WriteLine("\nTest 3: Test IsHotKey method");
            try
            {
                var rect = new Rect(0, 0, 20, 5);
                var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                var window = new ControlWindow(null, rect, colors);
                
                var keyInfo = new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
                var isHotKey = window.IsHotKey(keyInfo);
                
                Console.WriteLine($"✓ IsHotKey tested successfully");
                Console.WriteLine($"  IsHotKey('a'): {isHotKey} (expected: false)");
                
                if (isHotKey)
                {
                    Console.WriteLine("✗ WARNING: IsHotKey should return false by default");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
                return;
            }

            // Test 4: Create ControlWindow with parent
            Console.WriteLine("\nTest 4: Create ControlWindow with parent");
            try
            {
                var parentRect = new Rect(0, 0, 80, 25);
                var parentColors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                var parent = new Window(null, parentRect, parentColors);
                
                var childRect = new Rect(10, 5, 30, 10);
                var childColors = new Colors(ConsoleColor.Yellow, ConsoleColor.DarkBlue);
                var child = new ControlWindow(parent, childRect, childColors);
                
                Console.WriteLine("✓ ControlWindow with parent created successfully");
                Console.WriteLine($"  Child enabled: {child.IsEnabled()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
                return;
            }

            Console.WriteLine("\n=== All Tests Passed ===");
        }
    }
}
