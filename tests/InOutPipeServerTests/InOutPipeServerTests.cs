using System;
using System.Linq;
using ConsoleGui;

class InOutPipeServerTests
{
    static void Main(string[] args)
    {
        Console.WriteLine("InOutPipeServer Tests");
        Console.WriteLine("====================\n");

        var tests = new Action[]
        {
            Test_IsInOutPipeServer_PropertyExists,
            Test_GetInputFromUser_MethodExists,
            Test_GetSelectionFromUser_MethodExists,
            Test_OutputTemplateList_MethodExists,
        };

        var passed = 0;
        var failed = 0;

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

        Console.WriteLine($"\nPassed: {passed}, Failed: {failed}");
        Environment.Exit(failed > 0 ? 1 : 0);
    }

    static void Test_IsInOutPipeServer_PropertyExists()
    {
        // Just verify the API exists and is callable
        var isServer = InOutPipeServer.IsInOutPipeServer;
        
        // When CYCOD_IN_OUT_PIPE_SERVER is not set, should be false
        // (assuming clean test environment)
        if (isServer)
        {
            Console.WriteLine("  (Note: CYCOD_IN_OUT_PIPE_SERVER environment variable is set)");
        }
    }

    static void Test_GetInputFromUser_MethodExists()
    {
        // Verify the method signature exists via reflection
        var method = typeof(InOutPipeServer).GetMethod("GetInputFromUser");
        if (method == null)
            throw new Exception("GetInputFromUser method not found");
        
        if (!method.IsStatic)
            throw new Exception("GetInputFromUser should be static");
        
        if (!method.IsPublic)
            throw new Exception("GetInputFromUser should be public");
        
        var parameters = method.GetParameters();
        if (parameters.Length != 2)
            throw new Exception($"Expected 2 parameters, got {parameters.Length}");
        
        if (parameters[0].Name != "prompt")
            throw new Exception($"Expected first parameter 'prompt', got '{parameters[0].Name}'");
        
        if (parameters[1].Name != "value")
            throw new Exception($"Expected second parameter 'value', got '{parameters[1].Name}'");
    }

    static void Test_GetSelectionFromUser_MethodExists()
    {
        // Verify the method signature exists via reflection
        var method = typeof(InOutPipeServer).GetMethod("GetSelectionFromUser");
        if (method == null)
            throw new Exception("GetSelectionFromUser method not found");
        
        if (!method.IsStatic)
            throw new Exception("GetSelectionFromUser should be static");
        
        if (!method.IsPublic)
            throw new Exception("GetSelectionFromUser should be public");
        
        var parameters = method.GetParameters();
        if (parameters.Length != 2)
            throw new Exception($"Expected 2 parameters, got {parameters.Length}");
        
        if (parameters[0].Name != "items")
            throw new Exception($"Expected first parameter 'items', got '{parameters[0].Name}'");
        
        if (parameters[1].Name != "selected")
            throw new Exception($"Expected second parameter 'selected', got '{parameters[1].Name}'");
    }

    static void Test_OutputTemplateList_MethodExists()
    {
        // Verify the method signature exists via reflection
        var method = typeof(InOutPipeServer).GetMethod("OutputTemplateList");
        if (method == null)
            throw new Exception("OutputTemplateList method not found");
        
        if (!method.IsStatic)
            throw new Exception("OutputTemplateList should be static");
        
        if (!method.IsPublic)
            throw new Exception("OutputTemplateList should be public");
        
        var parameters = method.GetParameters();
        if (parameters.Length != 1)
            throw new Exception($"Expected 1 parameter, got {parameters.Length}");
        
        if (parameters[0].Name != "groupsJson")
            throw new Exception($"Expected parameter 'groupsJson', got '{parameters[0].Name}'");
    }
}

