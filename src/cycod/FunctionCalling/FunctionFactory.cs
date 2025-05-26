//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System.Reflection;
using System.Collections;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using System.ComponentModel;

public class FunctionFactory
{
    public FunctionFactory()
    {
    }

    public FunctionFactory(Assembly assembly)
    {
        AddFunctions(assembly);
    }

    public FunctionFactory(Type type1, params Type[] types)
    {
        AddFunctions(type1, types);
    }

    public FunctionFactory(IEnumerable<Type> types)
    {
        AddFunctions(types);
    }

    public FunctionFactory(Type type)
    {
        AddFunctions(type);
    }

    public void AddFunctions(Assembly assembly)
    {
        AddFunctions(assembly.GetTypes());
    }

    public void AddFunctions(Type type1, params Type[] types)
    {
        AddFunctions(new List<Type> { type1 });
        AddFunctions(types);
    }

    public void AddFunctions(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            AddFunctions(type);
        }
    }

    public void AddFunctions(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
        foreach (var method in methods)
        {
            AddFunction(method);
        }
    }

    public void AddFunctions(object instance)
    {
        var type = instance.GetType();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        foreach (var method in methods)
        {
            AddFunction(method, instance);
        }
    }

    public void AddFunction(MethodInfo method, object? instance = null)
    {
        var attributes = method.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes.Length > 0)
        {
            var funcDescriptionAttrib = attributes[0] as DescriptionAttribute;
            var funcDescription = funcDescriptionAttrib!.Description;

            var readonlyAttrib = method.GetCustomAttribute<ReadOnlyAttribute>();
            var readOnly = readonlyAttrib?.IsReadOnly;

            var aiFunction = AIFunctionFactory.Create(method, instance, method.Name, funcDescription);
            AddFunction(aiFunction, method, instance, readOnly);
        }
    }

    public void AddFunction(AIFunction aiFunction)
    {
        var methodInfo = typeof(AIFunction).GetMethod(nameof(AIFunction.InvokeAsync), BindingFlags.Instance | BindingFlags.Public);
        AddFunction(aiFunction, methodInfo!, aiFunction);
    }

    public void AddFunction(AIFunction aiFunction, MethodInfo method, object? instance = null, bool? readOnly = null)
    {
        const int shortDescriptionMacCch = 50;
        var shortDescription = aiFunction.Description.Length > shortDescriptionMacCch
            ? aiFunction.Description.Substring(0, shortDescriptionMacCch) + "..."
            : aiFunction.Description;

        ConsoleHelpers.WriteDebugLine($"Adding function '{aiFunction.Name}' - {shortDescription}");
        _functions.TryAdd(method, (aiFunction, instance));
        _readOnlyFunctions.TryAdd(aiFunction.Name, readOnly);
    }

    public IEnumerable<AITool> GetAITools()
    {
        return _functions.Select(x => x.Value.Function);
    }

    public bool? IsReadOnlyFunction(string functionName)
    {
        if (_readOnlyFunctions.TryGetValue(functionName, out var readOnly))
        {
            return readOnly;
        }
        return null;
    }

    public virtual bool TryCallFunction(string functionName, string functionArguments, out string? result)
    {
        result = null;
        if (!string.IsNullOrEmpty(functionName) && !string.IsNullOrEmpty(functionArguments))
        {
            var function = _functions.FirstOrDefault(x => x.Value.Function.Name == functionName);
            if (function.Key != null)
            {
                result = TryCallFunction(function.Key, function.Value.Function, functionArguments, function.Value.Instance);
                return true;
            }
        }
        return false;
    }

    // operator to add to FunctionFactories together
    public static FunctionFactory operator +(FunctionFactory a, FunctionFactory b)
    {
        var newFactory = new FunctionFactory();
        a._functions.ToList().ForEach(x => newFactory._functions.Add(x.Key, x.Value));
        b._functions.ToList().ForEach(x => newFactory._functions.Add(x.Key, x.Value));
        return newFactory;
    }

    private static string? TryCallFunction(MethodInfo methodInfo, AIFunction function, string argumentsAsJson, object? instance)
    {
        try
        {
            return CallFunction(methodInfo, function, argumentsAsJson, instance);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error calling function '{function.Name}': {ex.Message}");
            return "Exception: " + ex.Message;
        }
    }

    private static string? CallFunction(MethodInfo methodInfo, AIFunction function, string argumentsAsJson, object? instance)
    {
        var parsed = JsonDocument.Parse(argumentsAsJson).RootElement;
        var arguments = new List<object?>();

        var parameters = methodInfo.GetParameters();
        foreach (var parameter in parameters)
        {
            var parameterName = parameter.Name;
            if (parameterName == null) continue;

            if (parsed.ValueKind == JsonValueKind.Object && parsed.TryGetProperty(parameterName, out var value))
            {
                var parameterValue = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
                if (parameterValue == null) continue;

                var argument = ParseParameterValue(parameterValue, parameter.ParameterType);
                arguments.Add(argument);
            }
            else if (parameter.HasDefaultValue)
            {
                arguments.Add(parameter.DefaultValue);  
            }
        }

        var args = arguments.ToArray();
        var result = CallFunction(methodInfo, args, instance);
        return ConvertFunctionResultToString(result);
    }

    private static object? CallFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        var t = methodInfo.ReturnType;
        return t == typeof(Task)
            ? CallVoidAsyncFunction(methodInfo, args, instance)
            : t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>)
                ? CallAsyncFunction(methodInfo, args, instance)
                : t.Name != "Void"
                    ? CallSyncFunction(methodInfo, args, instance)
                    : CallVoidFunction(methodInfo, args, instance);
    }

    private static object? CallVoidAsyncFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        var task = methodInfo.Invoke(instance, args) as Task;
        task!.Wait();
        return true;
    }

    private static object? CallAsyncFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        var task = methodInfo.Invoke(instance, args) as Task;
        task!.Wait();
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static object? CallSyncFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        return methodInfo.Invoke(instance, args);
    }

    private static object? CallVoidFunction(MethodInfo methodInfo, object?[] args, object? instance)
    {
        methodInfo.Invoke(instance, args);
        return true;
    }

    private static string? ConvertFunctionResultToString(object? result)
    {
        if (result is IEnumerable enumerable && !(result is string))
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
            writer.WriteStartArray();

            foreach (var item in enumerable)
            {
                var str = item.ToString();
                writer.WriteStringValue(str);
            }

            writer.WriteEndArray();
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        return result?.ToString();
    }

    private static object ParseParameterValue(string parameterValue, Type parameterType)
    {
        if (IsArrayType(parameterType))
        {
            Type elementType = parameterType.GetElementType()!;
            return CreateGenericCollectionFromJsonArray(parameterValue, typeof(Array), elementType);
        }

        if (IsTuppleType(parameterType))
        {
            Type elementType = parameterType.GetGenericArguments()[0];
            return CreateTuppleTypeFromJsonArray(parameterValue, elementType);
        }

        if (IsGenericListOrEquivalentType(parameterType))
        {
            Type elementType = parameterType.GetGenericArguments()[0];
            return CreateGenericCollectionFromJsonArray(parameterValue, typeof(List<>), elementType);
        }

        if (IsNullableType(parameterType))
        {
            Type elementType = typeof(Nullable<>).MakeGenericType(parameterType.GetGenericArguments());
            return CreateNullableTypeFrom(parameterValue, elementType);
        }

        switch (Type.GetTypeCode(parameterType))
        {
            case TypeCode.Boolean: return bool.Parse(parameterValue!);
            case TypeCode.Byte: return byte.Parse(parameterValue!);
            case TypeCode.Decimal: return decimal.Parse(parameterValue!);
            case TypeCode.Double: return double.Parse(parameterValue!);
            case TypeCode.Single: return float.Parse(parameterValue!);
            case TypeCode.Int16: return short.Parse(parameterValue!);
            case TypeCode.Int32: return int.Parse(parameterValue!);
            case TypeCode.Int64: return long.Parse(parameterValue!);
            case TypeCode.SByte: return sbyte.Parse(parameterValue!);
            case TypeCode.UInt16: return ushort.Parse(parameterValue!);
            case TypeCode.UInt32: return uint.Parse(parameterValue!);
            case TypeCode.UInt64: return ulong.Parse(parameterValue!);
            case TypeCode.String: return parameterValue!;
            default: return Convert.ChangeType(parameterValue!, parameterType);
        }
    }

    private static object CreateNullableTypeFrom(string parameterValue, Type nullableType)
    {
        var underlyingType = Nullable.GetUnderlyingType(nullableType);

        if (string.IsNullOrEmpty(parameterValue) || parameterValue.Trim().ToLower() == "null")
        {
            return null!;
        }

        return ParseParameterValue(parameterValue, underlyingType!);
    }

    private static object CreateGenericCollectionFromJsonArray(string parameterValue, Type collectionType, Type elementType)
    {
        var root = JsonDocument.Parse(parameterValue).RootElement;
        var array = root.ValueKind == JsonValueKind.Array
            ? root.EnumerateArray().ToArray()
            : Array.Empty<JsonElement>();

        if (collectionType == typeof(Array))
        {
            var collection = Array.CreateInstance(elementType, array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                var parsed = ParseParameterValue(array[i].GetRawText(), elementType);
                if (parsed != null) collection.SetValue(parsed, i);
            }
            return collection;
        }
        else if (collectionType == typeof(List<>))
        {
            var collection = Activator.CreateInstance(collectionType.MakeGenericType(elementType));
            var list = collection as IList;
            foreach (var item in array)
            {
                var parsed = ParseParameterValue(item.GetRawText(), elementType);
                if (parsed != null) list!.Add(parsed);
            }
            return collection!;
        }

        return array;
    }

    private static object CreateTuppleTypeFromJsonArray(string parameterValue, Type elementType)
    {
        var list = new List<object>();

        var root = JsonDocument.Parse(parameterValue).RootElement;
        var array = root.ValueKind == JsonValueKind.Array
            ? root.EnumerateArray().ToArray()
            : Array.Empty<JsonElement>();

        foreach (var item in array)
        {
            var parsed = ParseParameterValue(item.GetRawText(), elementType);
            if (parsed != null) list!.Add(parsed);
        }

        var collection = list.Count() switch
        {
            1 => Activator.CreateInstance(typeof(Tuple<>).MakeGenericType(elementType), list[0]),
            2 => Activator.CreateInstance(typeof(Tuple<,>).MakeGenericType(elementType, elementType), list[0], list[1]),
            3 => Activator.CreateInstance(typeof(Tuple<,,>).MakeGenericType(elementType, elementType, elementType), list[0], list[1], list[2]),
            4 => Activator.CreateInstance(typeof(Tuple<,,,>).MakeGenericType(elementType, elementType, elementType, elementType), list[0], list[1], list[2], list[3]),
            5 => Activator.CreateInstance(typeof(Tuple<,,,,>).MakeGenericType(elementType, elementType, elementType, elementType, elementType), list[0], list[1], list[2], list[3], list[4]),
            6 => Activator.CreateInstance(typeof(Tuple<,,,,,>).MakeGenericType(elementType, elementType, elementType, elementType, elementType, elementType), list[0], list[1], list[2], list[3], list[4], list[5]),
            7 => Activator.CreateInstance(typeof(Tuple<,,,,,,>).MakeGenericType(elementType, elementType, elementType, elementType, elementType, elementType, elementType), list[0], list[1], list[2], list[3], list[4], list[5], list[6]),
            _ => throw new Exception("Tuples with more than 7 elements are not supported")
        };
        return collection!;
    }

    private static bool IsArrayType(Type t)
    {
        return t.IsArray;
    }

    private static bool IsTuppleType(Type parameterType)
    {
        return parameterType.IsGenericType && parameterType.GetGenericTypeDefinition().Name.StartsWith("Tuple");
    }

    private static bool IsGenericListOrEquivalentType(Type t)
    {
        return t.IsGenericType &&
            (t.GetGenericTypeDefinition() == typeof(List<>) ||
            t.GetGenericTypeDefinition() == typeof(ICollection<>) ||
            t.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
            t.GetGenericTypeDefinition() == typeof(IList<>) ||
            t.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>) ||
            t.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
    }

    private static bool IsNullableType(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private readonly Dictionary<MethodInfo, (AIFunction Function, object? Instance)> _functions = new();
    private readonly Dictionary<string, bool?> _readOnlyFunctions = new();
}

