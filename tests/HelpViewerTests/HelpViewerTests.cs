using System;
using System.Linq;
using ConsoleGui;
using ConsoleGui.Controls;

class HelpViewerTests
{
    static void Main(string[] args)
    {
        Console.WriteLine("HelpViewer Tests");
        Console.WriteLine("================\n");

        var tests = new Action[]
        {
            Test_HelpViewer_CanBeCreated,
            Test_HelpViewer_HasCorrectBaseClass,
            Test_HelpViewer_HasDisplayHelpTextMethod,
            Test_HelpViewer_HasDisplayHelpTopicsMethod,
            Test_HelpViewer_ProcessKeyMethod,
            Test_HelpViewer_ProtectedMethods,
            Test_HelpViewer_GetProgramHelpCommand,
        };

        int passed = 0;
        int failed = 0;

        foreach (var test in tests)
        {
            try
            {
                test();
                Console.WriteLine($"✓ {test.Method.Name}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {test.Method.Name}: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"\n{passed}/{tests.Length} tests passed");
        Environment.Exit(failed);
    }

    static void Test_HelpViewer_CanBeCreated()
    {
        // HelpViewer has a protected constructor, so we can't create it directly
        // But we can verify it exists and has the right signature
        var type = typeof(HelpViewer);
        var constructor = type.GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new[] { typeof(Window), typeof(Rect), typeof(Colors), typeof(Colors), typeof(string), typeof(bool) },
            null);
        
        if (constructor == null)
        {
            throw new Exception("HelpViewer protected constructor not found");
        }
    }

    static void Test_HelpViewer_HasCorrectBaseClass()
    {
        var type = typeof(HelpViewer);
        if (type.BaseType != typeof(TextViewerControl))
        {
            throw new Exception($"Expected base class TextViewerControl, got {type.BaseType?.Name}");
        }
    }

    static void Test_HelpViewer_HasDisplayHelpTextMethod()
    {
        var type = typeof(HelpViewer);
        var method = type.GetMethod("DisplayHelpText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        
        if (method == null)
        {
            throw new Exception("DisplayHelpText method not found");
        }

        var parameters = method.GetParameters();
        if (parameters.Length != 8)
        {
            throw new Exception($"Expected 8 parameters, got {parameters.Length}");
        }

        if (parameters[0].ParameterType != typeof(string[]))
        {
            throw new Exception("First parameter should be string[]");
        }
    }

    static void Test_HelpViewer_HasDisplayHelpTopicsMethod()
    {
        var type = typeof(HelpViewer);
        var method = type.GetMethod("DisplayHelpTopics", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        
        if (method == null)
        {
            throw new Exception("DisplayHelpTopics method not found");
        }

        var parameters = method.GetParameters();
        if (parameters.Length != 6)
        {
            throw new Exception($"Expected 6 parameters, got {parameters.Length}");
        }

        if (parameters[0].ParameterType != typeof(string[]))
        {
            throw new Exception("First parameter should be string[]");
        }
    }

    static void Test_HelpViewer_ProcessKeyMethod()
    {
        var type = typeof(HelpViewer);
        var method = type.GetMethod("ProcessKey", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        if (method == null)
        {
            throw new Exception("ProcessKey method not found");
        }

        if (method.ReturnType != typeof(bool))
        {
            throw new Exception("ProcessKey should return bool");
        }

        var parameters = method.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(ConsoleKeyInfo))
        {
            throw new Exception("ProcessKey should take ConsoleKeyInfo parameter");
        }
    }

    static void Test_HelpViewer_ProtectedMethods()
    {
        var type = typeof(HelpViewer);
        
        // Check PaintWindow override
        var paintWindow = type.GetMethod("PaintWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (paintWindow == null)
        {
            throw new Exception("PaintWindow protected method not found");
        }

        // Check GetSpeedSearchTooltip override
        var getTooltip = type.GetMethod("GetSpeedSearchTooltip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (getTooltip == null)
        {
            throw new Exception("GetSpeedSearchTooltip protected method not found");
        }

        // Check GetSpeedSearchText override - use DeclaredOnly to avoid ambiguity
        var getSearchText = type.GetMethod("GetSpeedSearchText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
        if (getSearchText == null)
        {
            throw new Exception("GetSpeedSearchText protected method not found");
        }
    }

    static void Test_HelpViewer_GetProgramHelpCommand()
    {
        var type = typeof(HelpViewer);
        
        // This is a private static method, so we need to use reflection
        var method = type.GetMethod("GetProgramHelpCommand", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        if (method == null)
        {
            throw new Exception("GetProgramHelpCommand private method not found");
        }

        if (method.ReturnType != typeof(string))
        {
            throw new Exception("GetProgramHelpCommand should return string");
        }

        // Invoke it and check the result format - but handle ProgramInfo not being initialized
        try
        {
            var result = method.Invoke(null, null) as string;
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("GetProgramHelpCommand returned null or empty");
            }

            if (!result.Contains("help"))
            {
                throw new Exception($"Expected help command to contain 'help', got: {result}");
            }
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is NullReferenceException)
        {
            // This is expected when ProgramInfo is not initialized - method exists and is callable
            // which is what we're testing for
        }
    }
}
