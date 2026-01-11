using System;
using ConsoleGui;

namespace ConsoleGuiTest
{
    public static class RectTests
    {
        public static bool RunAll()
        {
            Console.WriteLine("=== Rect Tests ===");
            var passed = 0;
            var failed = 0;

            RunTest("Construction", TestConstruction, ref passed, ref failed);
            RunTest("Properties", TestProperties, ref passed, ref failed);
            RunTest("Zero dimensions", TestZeroDimensions, ref passed, ref failed);
            RunTest("Negative values", TestNegativeValues, ref passed, ref failed);

            Console.WriteLine($"\nRect Tests: {passed} passed, {failed} failed");
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
            var rect = new Rect(10, 20, 30, 40);
            return rect.X == 10 && rect.Y == 20 && rect.Width == 30 && rect.Height == 40;
        }

        static bool TestProperties()
        {
            var rect = new Rect(0, 0, 10, 10);
            rect.X = 5;
            rect.Y = 15;
            rect.Width = 20;
            rect.Height = 25;
            return rect.X == 5 && rect.Y == 15 && rect.Width == 20 && rect.Height == 25;
        }

        static bool TestZeroDimensions()
        {
            var rect = new Rect(0, 0, 0, 0);
            return rect.Width == 0 && rect.Height == 0;
        }

        static bool TestNegativeValues()
        {
            // Rect should allow negative values (they may have meaning for offset calculations)
            var rect = new Rect(-10, -20, 30, 40);
            return rect.X == -10 && rect.Y == -20 && rect.Width == 30 && rect.Height == 40;
        }
    }
}
