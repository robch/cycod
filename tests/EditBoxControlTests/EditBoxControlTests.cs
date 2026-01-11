using System;
using ConsoleGui;
using ConsoleGui.Controls;

namespace ConsoleGuiTests
{
    class EditBoxControlTests
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== EditBoxControl Tests ===\n");
            Console.WriteLine("NOTE: EditBoxControl requires a valid console handle for full instantiation.");
            Console.WriteLine("These tests verify that the class exists and has the expected API structure.\n");

            int passed = 0;
            int total = 0;

            // Test 1: Verify EditBoxControl type exists
            total++;
            Console.WriteLine($"Test {total}: Verify EditBoxControl type exists");
            try
            {
                var type = typeof(EditBoxControl);
                Console.WriteLine("✓ EditBoxControl type found");
                Console.WriteLine($"  Type: {type.Name}");
                Console.WriteLine($"  Namespace: {type.Namespace}");
                Console.WriteLine($"  Base type: {type.BaseType?.Name}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 2: Verify EditBoxControl inherits from ScrollingControl
            total++;
            Console.WriteLine($"\nTest {total}: Verify inheritance from ScrollingControl");
            try
            {
                var type = typeof(EditBoxControl);
                var baseType = type.BaseType;
                
                if (baseType != null && baseType.Name == "ScrollingControl")
                {
                    Console.WriteLine("✓ EditBoxControl inherits from ScrollingControl");
                    passed++;
                }
                else
                {
                    Console.WriteLine($"✗ EditBoxControl does not inherit from ScrollingControl (base: {baseType?.Name})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 3: Verify public methods exist
            total++;
            Console.WriteLine($"\nTest {total}: Verify public methods exist");
            try
            {
                var type = typeof(EditBoxControl);
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | 
                                             System.Reflection.BindingFlags.Instance | 
                                             System.Reflection.BindingFlags.DeclaredOnly);
                
                var methodNames = new System.Collections.Generic.List<string>();
                foreach (var method in methods)
                {
                    if (!method.IsSpecialName) // Exclude property getters/setters
                    {
                        methodNames.Add(method.Name);
                    }
                }

                Console.WriteLine("✓ Public methods found:");
                foreach (var name in methodNames)
                {
                    Console.WriteLine($"  - {name}()");
                }
                
                // Check for expected methods
                var expectedMethods = new[] { "GetText", "Home", "End", "Left", "Right", 
                                             "BackSpace", "Delete", "Insert", "TypeChar",
                                             "DisplayCursor", "HideCursor" };
                bool allFound = true;
                foreach (var expected in expectedMethods)
                {
                    if (!methodNames.Contains(expected))
                    {
                        Console.WriteLine($"  ✗ Missing method: {expected}");
                        allFound = false;
                    }
                }
                
                if (allFound)
                {
                    passed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 4: Verify constructor signature
            total++;
            Console.WriteLine($"\nTest {total}: Verify constructor exists");
            try
            {
                var type = typeof(EditBoxControl);
                var constructors = type.GetConstructors();
                
                if (constructors.Length > 0)
                {
                    Console.WriteLine("✓ Constructor found");
                    var ctor = constructors[0];
                    var parameters = ctor.GetParameters();
                    Console.WriteLine($"  Parameters: {parameters.Length}");
                    foreach (var param in parameters)
                    {
                        Console.WriteLine($"    - {param.ParameterType.Name} {param.Name}");
                    }
                    passed++;
                }
                else
                {
                    Console.WriteLine("✗ No public constructor found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 5: Verify ProcessKey method signature
            total++;
            Console.WriteLine($"\nTest {total}: Verify ProcessKey method");
            try
            {
                var type = typeof(EditBoxControl);
                var method = type.GetMethod("ProcessKey");
                
                if (method != null)
                {
                    Console.WriteLine("✓ ProcessKey method found");
                    Console.WriteLine($"  Return type: {method.ReturnType.Name}");
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType.Name == "ConsoleKeyInfo")
                    {
                        Console.WriteLine($"  Parameter: ConsoleKeyInfo");
                        passed++;
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ Unexpected parameter signature");
                    }
                }
                else
                {
                    Console.WriteLine("✗ ProcessKey method not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 6: Verify navigation methods exist
            total++;
            Console.WriteLine($"\nTest {total}: Verify navigation methods");
            try
            {
                var type = typeof(EditBoxControl);
                var navigationMethods = new[] { "Home", "End", "Left", "Right" };
                bool allFound = true;
                
                foreach (var methodName in navigationMethods)
                {
                    var method = type.GetMethod(methodName);
                    if (method == null)
                    {
                        Console.WriteLine($"  ✗ Missing method: {methodName}");
                        allFound = false;
                    }
                    else
                    {
                        Console.WriteLine($"  ✓ {methodName}() found (returns {method.ReturnType.Name})");
                    }
                }
                
                if (allFound)
                {
                    passed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 7: Verify edit methods exist
            total++;
            Console.WriteLine($"\nTest {total}: Verify edit methods");
            try
            {
                var type = typeof(EditBoxControl);
                var editMethods = new[] { "BackSpace", "Delete", "Insert", "TypeChar" };
                bool allFound = true;
                
                foreach (var methodName in editMethods)
                {
                    var method = type.GetMethod(methodName);
                    if (method == null)
                    {
                        Console.WriteLine($"  ✗ Missing method: {methodName}");
                        allFound = false;
                    }
                    else
                    {
                        Console.WriteLine($"  ✓ {methodName}() found");
                    }
                }
                
                if (allFound)
                {
                    passed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 8: Verify GetText method exists
            total++;
            Console.WriteLine($"\nTest {total}: Verify GetText method");
            try
            {
                var type = typeof(EditBoxControl);
                var method = type.GetMethod("GetText");
                
                if (method != null && method.ReturnType == typeof(string))
                {
                    Console.WriteLine("✓ GetText method found");
                    Console.WriteLine($"  Return type: {method.ReturnType.Name}");
                    passed++;
                }
                else
                {
                    Console.WriteLine("✗ GetText method not found or has wrong return type");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 9: Verify cursor methods exist
            total++;
            Console.WriteLine($"\nTest {total}: Verify cursor methods");
            try
            {
                var type = typeof(EditBoxControl);
                var cursorMethods = new[] { "DisplayCursor", "HideCursor" };
                bool allFound = true;
                
                foreach (var methodName in cursorMethods)
                {
                    var method = type.GetMethod(methodName);
                    if (method == null)
                    {
                        Console.WriteLine($"  ✗ Missing method: {methodName}");
                        allFound = false;
                    }
                    else
                    {
                        Console.WriteLine($"  ✓ {methodName}() found");
                    }
                }
                
                if (allFound)
                {
                    passed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            // Test 10: Verify override methods
            total++;
            Console.WriteLine($"\nTest {total}: Verify override methods");
            try
            {
                var type = typeof(EditBoxControl);
                var overrideMethods = new[] { "GetNumRows", "GetNumColumns", "Open", "Close", "SetFocus", "KillFocus", "ProcessKey" };
                bool allFound = true;
                
                foreach (var methodName in overrideMethods)
                {
                    var method = type.GetMethod(methodName);
                    if (method == null)
                    {
                        Console.WriteLine($"  ✗ Missing method: {methodName}");
                        allFound = false;
                    }
                }
                
                if (allFound)
                {
                    Console.WriteLine("✓ All expected override methods found");
                    passed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed: {ex.Message}");
            }

            Console.WriteLine($"\n=== Results: {passed}/{total} Tests Passed ===");
            Console.WriteLine("\nNOTE: These tests verify the EditBoxControl API structure.");
            Console.WriteLine("Interactive testing with keyboard input requires a full console environment.");
            Console.WriteLine("To test interactively, integrate EditBoxControl into a real application.");
            
            Environment.Exit(passed == total ? 0 : 1);
        }
    }
}
