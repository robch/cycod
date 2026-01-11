using System;
using ConsoleGui.Controls;

namespace ConsoleGuiTests
{
    public class EditBoxQuickEditTests
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--interactive")
            {
                RunInteractiveTests();
            }
            else
            {
                RunAutomatedTests();
            }
        }

        private static void RunAutomatedTests()
        {
            Console.WriteLine("EditBoxQuickEdit Tests");
            Console.WriteLine("=====================");
            Console.WriteLine();

            var passCount = 0;
            var totalCount = 0;

            // Test 1: EditBoxQuickEdit inherits from EditBoxControl
            totalCount++;
            try
            {
                var type = typeof(EditBoxQuickEdit);
                var baseType = type.BaseType;
                if (baseType?.Name == "EditBoxControl")
                {
                    Console.WriteLine($"✓ Test {totalCount}: EditBoxQuickEdit inherits from EditBoxControl");
                    passCount++;
                }
                else
                {
                    Console.WriteLine($"✗ Test {totalCount}: Expected base type EditBoxControl, got {baseType?.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Test {totalCount}: Exception - {ex.Message}");
            }

            // Test 2: Static Edit method exists
            totalCount++;
            try
            {
                var method = typeof(EditBoxQuickEdit).GetMethod("Edit", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null && method.IsStatic && method.IsPublic)
                {
                    Console.WriteLine($"✓ Test {totalCount}: Static Edit method exists");
                    passCount++;
                }
                else
                {
                    Console.WriteLine($"✗ Test {totalCount}: Static Edit method not found or not public/static");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Test {totalCount}: Exception - {ex.Message}");
            }

            // Test 3: Edit method has correct return type
            totalCount++;
            try
            {
                var method = typeof(EditBoxQuickEdit).GetMethod("Edit",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method?.ReturnType == typeof(string))
                {
                    Console.WriteLine($"✓ Test {totalCount}: Edit method returns string?");
                    passCount++;
                }
                else
                {
                    Console.WriteLine($"✗ Test {totalCount}: Expected return type string?, got {method?.ReturnType}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Test {totalCount}: Exception - {ex.Message}");
            }

            // Test 4: ProcessKey method is overridden
            totalCount++;
            try
            {
                var method = typeof(EditBoxQuickEdit).GetMethod("ProcessKey",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (method != null && method.DeclaringType == typeof(EditBoxQuickEdit))
                {
                    Console.WriteLine($"✓ Test {totalCount}: ProcessKey method is overridden");
                    passCount++;
                }
                else
                {
                    Console.WriteLine($"✗ Test {totalCount}: ProcessKey method not properly overridden");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Test {totalCount}: Exception - {ex.Message}");
            }

            // Test 5: Edit method parameters
            totalCount++;
            try
            {
                var method = typeof(EditBoxQuickEdit).GetMethod("Edit",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var parameters = method?.GetParameters();
                if (parameters != null && parameters.Length >= 3)
                {
                    var hasWidth = parameters[0].Name == "width" && parameters[0].ParameterType == typeof(int);
                    var hasHeight = parameters[1].Name == "height" && parameters[1].ParameterType == typeof(int);
                    var hasNormal = parameters[2].Name == "normal" && parameters[2].ParameterType == typeof(Colors);
                    
                    if (hasWidth && hasHeight && hasNormal)
                    {
                        Console.WriteLine($"✓ Test {totalCount}: Edit method has correct parameters (width, height, normal)");
                        passCount++;
                    }
                    else
                    {
                        Console.WriteLine($"✗ Test {totalCount}: Edit method parameter mismatch");
                    }
                }
                else
                {
                    Console.WriteLine($"✗ Test {totalCount}: Edit method has wrong number of parameters");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Test {totalCount}: Exception - {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine($"Results: {passCount}/{totalCount} tests passed");
            Environment.Exit(passCount == totalCount ? 0 : 1);
        }

        private static void RunInteractiveTests()
        {
            Console.WriteLine("EditBoxQuickEdit Interactive Tests");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("This test demonstrates EditBoxQuickEdit functionality:");
            Console.WriteLine("1. Creates a quick edit box");
            Console.WriteLine("2. User can type text");
            Console.WriteLine("3. Press Enter to accept, Escape to cancel");
            Console.WriteLine();
            Console.WriteLine("Note: This requires Screen.Current to be properly initialized.");
            Console.WriteLine("Run this test in a real console environment.");
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);

            try
            {
                // Test basic quick edit
                Console.WriteLine();
                Console.WriteLine("Test 1: Quick edit with default settings");
                Console.WriteLine("(Type some text and press Enter, or press Escape to cancel)");
                Console.WriteLine();

                var result = EditBoxQuickEdit.Edit(40, 3, new Colors(ConsoleColor.White, ConsoleColor.Black), "Initial text");
                
                Console.WriteLine();
                if (result != null)
                {
                    Console.WriteLine($"You entered: '{result}'");
                }
                else
                {
                    Console.WriteLine("Edit was cancelled");
                }
                Console.WriteLine();
                Console.WriteLine("✓ Quick edit test completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error during interactive test: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Interactive tests complete!");
        }
    }
}
