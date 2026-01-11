using System;
using ConsoleGui;

namespace ConsoleGuiTest
{
    public class ColorTestStandalone
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Color Compatibility Test ===\n");

                // Test 1: Colors class instantiation
                Console.WriteLine("Test 1: Colors class instantiation");
                var colors = new Colors(ConsoleColor.White, ConsoleColor.Black);
                Console.WriteLine($"  Created Colors: Foreground={colors.Foreground}, Background={colors.Background}");
                Console.WriteLine($"  ✓ Colors class works\n");

                // Test 2: ColorHelpers.GetHighlightColors
                Console.WriteLine("Test 2: ColorHelpers.GetHighlightColors");
                var highlightColors = ColorHelpers.GetHighlightColors();
                Console.WriteLine($"  Highlight Colors: Foreground={highlightColors.Foreground}, Background={highlightColors.Background}");
                Console.WriteLine($"  ✓ ColorHelpers.GetHighlightColors works\n");

                // Test 3: ColorHelpers.GetErrorColors
                Console.WriteLine("Test 3: ColorHelpers.GetErrorColors");
                var errorColors = ColorHelpers.GetErrorColors();
                Console.WriteLine($"  Error Colors: Foreground={errorColors.Foreground}, Background={errorColors.Background}");
                Console.WriteLine($"  ✓ ColorHelpers.GetErrorColors works\n");

                // Test 4: ColorHelpers.TryParseColorStyleText
                Console.WriteLine("Test 4: ColorHelpers.TryParseColorStyleText");
                var testText = "#error;This is an error message";
                if (ColorHelpers.TryParseColorStyleText(testText, out var parsedStyle, out var parsedText))
                {
                    Console.WriteLine($"  Parsed Style: Foreground={parsedStyle!.Foreground}, Background={parsedStyle.Background}");
                    Console.WriteLine($"  Parsed Text: '{parsedText}'");
                    Console.WriteLine($"  ✓ ColorHelpers.TryParseColorStyleText works\n");
                }
                else
                {
                    Console.WriteLine("  ✗ Failed to parse color style text\n");
                    return 1;
                }

                // Test 5: ColorHelpers.MapColor
                Console.WriteLine("Test 5: ColorHelpers.MapColor");
                var mappedColor = ColorHelpers.MapColor(ConsoleColor.DarkGray);
                Console.WriteLine($"  Mapped DarkGray to: {mappedColor}");
                Console.WriteLine($"  ✓ ColorHelpers.MapColor works\n");

                // Test 6: Integration with Screen class
                Console.WriteLine("Test 6: Screen class can use Colors");
                var screen = new Screen();
                Console.WriteLine($"  Screen.ColorsStart: Foreground={screen.ColorsStart.Foreground}, Background={screen.ColorsStart.Background}");
                Console.WriteLine($"  ✓ Screen class works with Colors\n");

                Console.WriteLine("=== All Tests Passed! ===");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return 1;
            }
        }
    }
}
