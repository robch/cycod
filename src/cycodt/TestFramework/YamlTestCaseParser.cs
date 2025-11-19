using System.IO;
using System;
using System.Security.Cryptography;

using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using YamlDotNet.RepresentationModel;

public partial class YamlTestCaseParser
{
    public static IEnumerable<TestCase> TestCasesFromYaml(string source, FileInfo file)
    {
        var defaultTags = YamlTagHelpers.FindAndGetDefaultTags(file.Directory!);

        var workingDirectory = GetScalarString(null, defaultTags, "workingDirectory");
        workingDirectory = UpdateWorkingDirectory(file.Directory!.FullName, workingDirectory);

        var context = new YamlTestCaseParseContext() {
            Source = source,
            File = file,
            Area = GetRootArea(file),
            Class = GetScalarString(null, defaultTags, "class", defaultClassName)!,
            Tags = defaultTags,
            Environment = YamlEnvHelpers.GetDefaultEnvironment(true, workingDirectory),
            WorkingDirectory = workingDirectory,
            Matrix = YamlTestCaseMatrixHelpers.GetNewMatrix()
        };

        var parsed = YamlHelpers.ParseYamlStream(file.FullName);
        return TestCasesFromYamlStream(context, parsed).ToList();
    }

    #region private methods

    private static IEnumerable<TestCase> TestCasesFromYamlStream(YamlTestCaseParseContext context, YamlStream parsed)
    {
        var tests = new List<TestCase>();

        var docs = parsed?.Documents;
        if (docs == null) return tests;

        foreach (var document in docs)
        {
            var fromDocument = TestCasesFromYamlDocumentRootNode(context, document.RootNode);
            if (fromDocument != null)
            {
                tests.AddRange(fromDocument);
            }
        }

        return tests;
    }

    private static IEnumerable<TestCase>? TestCasesFromYamlDocumentRootNode(YamlTestCaseParseContext context, YamlNode node)
    {
        return node is YamlMappingNode
            ? TestCasesFromYamlMapping(context, (node as YamlMappingNode)!)
            : TestCasesFromYamlSequence(context, node as YamlSequenceNode);
    }

    private static IEnumerable<TestCase>? TestCasesFromYamlMapping(YamlTestCaseParseContext context, YamlMappingNode mapping)
    {
        context.Matrix = CheckUpdateExpandMatrix(context, mapping);

        var children = CheckForChildren(context, mapping);
        if (children != null)
        {
            return children;
        }

        var test = GetTestFromNode(context, mapping);
        if (test != null)
        {
            return new[] { test };
        }

        return null;
    }

    private static List<Dictionary<string, string>> CheckUpdateExpandMatrix(YamlTestCaseParseContext context, YamlMappingNode mapping)
    {
        var fileName = GetScalarString(mapping, "matrix-file");
        var fileNameOk = !string.IsNullOrEmpty(fileName);
        if (fileNameOk)
        {
            context.Matrix = UpdateExpandMatrixFromFile(context, fileName!);
        }

        var matrixNodeOk = mapping.Children.ContainsKey("matrix");
        if (matrixNodeOk)
        {
            var matrixNode = mapping.Children["matrix"];
            context.Matrix = UpdateExpandMatrixFromNode(context, matrixNode);
        }

        return context.Matrix;
    }

    private static List<Dictionary<string, string>> UpdateExpandMatrixFromFile(YamlTestCaseParseContext context, string fileName)
    {
        fileName = PathHelpers.Combine(context.File.Directory!.FullName, fileName)!;
        
        var fi = new FileInfo(fileName);
        if (fi.Exists)
        {
            var parsed = YamlHelpers.ParseYamlStream(fileName);
            var matrixNode = parsed.Documents[0].RootNode;

            context.File = fi;
            return UpdateExpandMatrixFromNode(context, matrixNode);
        }
        else
        {
            TestLogger.LogInfo($"YamlTestCaseParser.UpdateExpandMatrixFromFile: File not found '{fileName}'");
            var check = PropertyInterpolationHelpers.Interpolate(fileName, context.Matrix.Last());
            if (File.Exists(check))
            {
                TestLogger.LogInfo($"YamlTestCaseParser.UpdateExpandMatrixFromFile: File found '{check}'");
                return UpdateExpandMatrixFromFile(context, check);
            }
            else
            {
                TestLogger.LogError($"YamlTestCaseParser.UpdateExpandMatrixFromFile: File not found '{check}'");
            }
        }

        return context.Matrix;
    }

    private static List<Dictionary<string, string>> UpdateExpandMatrixFromNode(YamlTestCaseParseContext context, YamlNode matrixNode)
    {
        var asMapping = matrixNode as YamlMappingNode;
        var asSequence = asMapping != null && asMapping.Children.ContainsKey("foreach")
            ? asMapping.Children["foreach"] as YamlSequenceNode
            : matrixNode as YamlSequenceNode;
        
        if (asMapping != null)
        {
            context.Matrix = UpdateExpandMatrixWithMapping(context, asMapping);
        }

        if (asSequence != null)
        {
            context.Matrix = UpdateExpandMatrixWithSequence(context, asSequence);
        }

        return context.Matrix;
    }

    private static List<Dictionary<string, string>> UpdateExpandMatrixWithMapping(YamlTestCaseParseContext context, YamlMappingNode mapping)
    {
        foreach (var kvp in mapping.Children)
        {
            var key = (kvp.Key as YamlScalarNode)?.Value;
            if (key == null)
            {
                TestLogger.LogError($"Error parsing YAML: expected scalar key at at {context.File.FullName}({kvp.Key.Start.Line})");
                continue;
            }

            if (key == "foreach") continue;

            var value = (kvp.Value as YamlScalarNode)?.Value;
            var newMatrix = new List<Dictionary<string, string>>();

            if (value != null)
            {
                foreach (var existingMatrixItem in context.Matrix)
                {
                    var newMatrixItem = YamlTestCaseMatrixHelpers.GetNewMatrixItemDictionary(existingMatrixItem);
                    newMatrixItem[key] = value;
                    newMatrix.Add(newMatrixItem);
                }

                context.Matrix = newMatrix;
                continue;
            }

            var asSequence = kvp.Value as YamlSequenceNode;
            if (asSequence == null)
            {
                TestLogger.LogError($"Error parsing YAML: expected string or sequence at at {context.File.FullName}({kvp.Value.Start.Line})");
                continue;
            }

            foreach (var item in asSequence.Children)
            {
                value = (item as YamlScalarNode)?.Value;
                if (value == null)
                {
                    TestLogger.LogError($"Error parsing YAML: expected scalar value at at {context.File.FullName}({item.Start.Line})");
                    continue;
                }

                foreach (var existingMatrixItem in context.Matrix)
                {
                    var newMatrixItem = YamlTestCaseMatrixHelpers.GetNewMatrixItemDictionary(existingMatrixItem);
                    newMatrixItem[key] = value;
                    newMatrix.Add(newMatrixItem);
                }
            }

            context.Matrix = newMatrix;
        }

        return context.Matrix;
    }

    private static List<Dictionary<string, string>> UpdateExpandMatrixWithSequence(YamlTestCaseParseContext context, YamlSequenceNode sequenceOfMaps)
    {
        var newMatrix = new List<Dictionary<string, string>>();
        foreach (var item in sequenceOfMaps.Children)
        {
            var asMapping = item as YamlMappingNode;
            if (asMapping == null)
            {
                TestLogger.LogError($"Error parsing YAML: expected mapping at at {context.File.FullName}({item.Start.Line})");
                continue;
            }

            foreach (var existingMatrixItem in context.Matrix)
            {
                var newMatrixItem = YamlTestCaseMatrixHelpers.GetNewMatrixItemDictionary(existingMatrixItem);
                foreach (var kvp in asMapping.Children)
                {
                    var key = (kvp.Key as YamlScalarNode)?.Value;
                    if (key == null)
                    {
                        TestLogger.LogError($"Error parsing YAML: expected scalar key at at {context.File.FullName}({kvp.Key.Start.Line})");
                        continue;
                    }

                    var value = (kvp.Value as YamlScalarNode)?.Value;
                    if (value == null)
                    {
                        TestLogger.LogError($"Error parsing YAML: expected scalar value at at {context.File.FullName}({kvp.Value.Start.Line})");
                        continue;
                    }

                    newMatrixItem[key] = value;
                }

                newMatrix.Add(newMatrixItem);
            }
        }

        return newMatrix;
    }

    private static IEnumerable<TestCase>? TestCasesFromYamlSequence(YamlTestCaseParseContext context, YamlSequenceNode? sequence)
    {
        var tests = new List<TestCase>();
        if (sequence == null) return tests;

        foreach (var node in sequence.Children)
        {
            var mapping = node as YamlMappingNode;
            if (mapping == null)
            {
                var message = $"Error parsing YAML: expected mapping at {context.File.FullName}({node.Start.Line})";
                TestLogger.LogError(message);
                return null;
            }

            var fromMapping = TestCasesFromYamlMapping(context, mapping);
            if (fromMapping != null)
            {
                tests.AddRange(fromMapping);
            }
        }

        return tests;
    }

    private static TestCase? GetTestFromNode(YamlTestCaseParseContext context, YamlMappingNode? mapping, int stepNumber = 0)
    {
        if (mapping == null) return null;

        var cli = GetScalarString(mapping, context.Tags, "cli");
        var parallelize = GetScalarString(mapping, context.Tags, "parallelize");
        var skipOnFailure = GetScalarString(mapping, context.Tags, "skipOnFailure");
        string workingDirectory = UpdateWorkingDirectory(mapping!, context.WorkingDirectory);

        var runProcess = GetScalarString(mapping, "run");

        var script = GetScalarString(mapping, "script");
        var shell = GetScalarString(mapping, "shell");

        var bash = GetScalarString(mapping, "bash");
        var cmd = GetScalarString(mapping, "cmd");
        var powershell = GetScalarString(mapping, "powershell");
        var pwsh = GetScalarString(mapping, "pwsh");

        if (!string.IsNullOrEmpty(bash))
        {
            script = bash;
            shell = "bash";
        }
        else if (!string.IsNullOrEmpty(cmd))
        {
            script = cmd;
            shell = "cmd";
        }
        else if (!string.IsNullOrEmpty(pwsh))
        {
            script = pwsh;
            shell = "pwsh";
        }
        else if (!string.IsNullOrEmpty(powershell))
        {
            script = powershell;
            shell = "powershell";
        }

        var fullyQualifiedName = runProcess == null && script == null
            ? GetFullyQualifiedNameAndCommandFromShortForm(mapping, context.Area, context.Class, ref runProcess, stepNumber)
            : GetFullyQualifiedName(mapping, context.Area, context.Class, stepNumber);
        fullyQualifiedName ??= GetFullyQualifiedName(context.Area, context.Class, $"Expected YAML node ('name') at {context.File.FullName}({mapping.Start.Line})", 0);

        var neitherOrBoth = (runProcess == null) == (script == null);
        if (neitherOrBoth)
        {
            var message = $"Error parsing YAML: expected/unexpected key ('name', 'run', 'script', 'shell', 'bash', 'pwsh', 'powershell', 'cmd', 'arguments') at {context.File.FullName}({mapping.Start.Line})";
            TestLogger.LogWarning(message);
            return null;
        }

        TestLogger.Log($"YamlTestCaseParser.GetTests(): new TestCase('{fullyQualifiedName}')");
        var test = new TestCase(fullyQualifiedName, new Uri(YamlTestFramework.FakeExecutor), context.Source)
        {
            CodeFilePath = context.File.FullName,
            LineNumber = (int)mapping.Start.Line
        };

        AppendDeterministicUniquenessSuffix(test);

        SetTestCaseProperty(test, "cli", cli);
        SetTestCaseProperty(test, "run", runProcess);

        SetTestCaseProperty(test, "script", script);
        SetTestCaseProperty(test, "shell", shell);
        SetTestCaseProperty(test, "bash", bash);
        SetTestCaseProperty(test, "cmd", cmd);
        SetTestCaseProperty(test, "powershell", powershell);
        SetTestCaseProperty(test, "pwsh", pwsh);

        SetTestCaseProperty(test, "parallelize", parallelize);
        SetTestCaseProperty(test, "skipOnFailure", skipOnFailure);

        var timeout = GetScalarString(mapping, context.Tags, "timeout", YamlTestFramework.DefaultTimeout);
        SetTestCaseProperty(test, "timeout", timeout);

        SetTestCaseProperty(test, "working-directory", workingDirectory);

        var processEnv = YamlEnvHelpers.GetCurrentProcessEnvironment();
        var testEnv = YamlEnvHelpers.UpdateCopyEnvironment(context.Environment, mapping);
        testEnv = YamlEnvHelpers.GetNewAndUpdatedEnvironmentVariables(processEnv, testEnv);
        SetTestCasePropertyMap(test, "env", testEnv);

        var matrix = JsonHelpers.GetJsonArrayText(context.Matrix);
        SetTestCaseProperty(test, "matrix", matrix);

        SetTestCasePropertyMap(test, "foreach", mapping, "foreach", workingDirectory);
        SetTestCasePropertyMap(test, "arguments", mapping, "arguments", workingDirectory);
        SetTestCasePropertyMap(test, "input", mapping, "input", workingDirectory);

        SetTestCaseProperty(test, "expect", mapping, "expect");
        SetTestCaseProperty(test, "expect-regex", mapping, "expect-regex");
        SetTestCaseProperty(test, "not-expect-regex", mapping, "not-expect-regex");
        SetTestCaseProperty(test, "expect-exit-code", mapping, "expect-exit-code");

        SetTestCaseTagsAsTraits(test, YamlTagHelpers.UpdateCopyTags(context.Tags, mapping));

        CheckInvalidTestCaseNodes(context, mapping, test);
        return test;
    }

    private static IEnumerable<TestCase>? CheckForChildren(YamlTestCaseParseContext context, YamlMappingNode mapping)
    {
        if (mapping.Children.ContainsKey("steps") && mapping.Children["steps"] is YamlSequenceNode stepsSequence)
        {
            context.Class = GetScalarString(mapping, "class", context.Class)!;
            context.Area = UpdateArea(mapping, context.Area);
            context.Tags = YamlTagHelpers.UpdateCopyTags(context.Tags, mapping);
            context.Environment = YamlEnvHelpers.UpdateCopyEnvironment(context.Environment, mapping);
            context.WorkingDirectory = UpdateWorkingDirectory(mapping, context.WorkingDirectory);

            return TestCasesFromYamlSequenceOfSteps(context, stepsSequence);
        }

        if (mapping.Children.ContainsKey("tests") && mapping.Children["tests"] is YamlSequenceNode testsSequence)
        {
            context.Class = GetScalarString(mapping, "class", context.Class)!;
            context.Area = UpdateArea(mapping, context.Area);
            context.Tags = YamlTagHelpers.UpdateCopyTags(context.Tags, mapping);
            context.Environment = YamlEnvHelpers.UpdateCopyEnvironment(context.Environment, mapping);
            context.WorkingDirectory = UpdateWorkingDirectory(mapping, context.WorkingDirectory);

            return TestCasesFromYamlSequence(context, testsSequence)?.ToList();
        }

        return null;
    }

    private static IEnumerable<TestCase> TestCasesFromYamlSequenceOfSteps(YamlTestCaseParseContext context, YamlSequenceNode sequence)
    {
        var tests = new List<TestCase>();
        for (int i = 0; i < sequence.Children.Count; i++)
        {
            var mapping = sequence.Children[i] as YamlMappingNode;
            var test = GetTestFromNode(context, mapping, i + 1);
            if (test != null)
            {
                tests.Add(test);
            }
        }

        for (int i = 0; i < tests.Count; i++)
        {
            if (i > 0)
            {
                SetTestCaseProperty(tests[i - 1], "nextTestCaseId", tests[i].Id.ToString());
                SetTestCaseProperty(tests[i], "afterTestCaseId", tests[i - 1].Id.ToString());
            }

            SetTestCaseProperty(tests[i], "parallelize", "true");
        }

        return tests;
    }

    private static void CheckInvalidTestCaseNodes(YamlTestCaseParseContext context, YamlMappingNode mapping, TestCase test)
    {
        foreach (YamlScalarNode key in mapping.Children.Keys)
        {
            if (key.Value == null) continue;
            if (!IsValidTestCaseNode(key.Value) && !test.DisplayName.EndsWith(key.Value))
            {
                var error = $"Error parsing YAML: Unexpected YAML key/value ('{key.Value}', '{test.DisplayName}') in {context.File.FullName}({mapping[key].Start.Line})";
                test.DisplayName = error;
                TestLogger.LogError(error);
            }
        }
    }

    private static bool IsValidTestCaseNode(string? value)
    {
        return !string.IsNullOrEmpty(value) && ";area;class;name;cli;run;script;shell;bash;pwsh;powershell;cmd;timeout;foreach;arguments;input;expect;expect-regex;not-expect-regex;expect-exit-code;parallelize;skipOnFailure;tag;tags;matrix;matrix-file;workingDirectory;env;sanitize;optional;".IndexOf($";{value};") >= 0;
    }

    private static void SetTestCaseProperty(TestCase test, string propertyName, YamlMappingNode mapping, string mappingName)
    {
        var value = GetScalarString(mapping, mappingName);
        SetTestCaseProperty(test, propertyName, value);
    }

    private static void SetTestCaseProperty(TestCase test, string propertyName, string? value)
    {
        if (value != null)
        {
            YamlTestProperties.Set(test, propertyName, value);
        }
    }

    private static void SetTestCasePropertyMap(TestCase test, string propertyName, IDictionary<string, string> map)
    {
        var sb = new StringBuilder();
        foreach (var key in map.Keys)
        {
            sb.Append($"{key}={map[key]}\n");
        }

        SetTestCaseProperty(test, propertyName, sb.ToString());
    }

    private static void SetTestCasePropertyMap(TestCase test, string propertyName, YamlMappingNode testNode, string mappingName, string workingDirectory)
    {
        var ok = testNode.Children.ContainsKey(mappingName);
        if (!ok) return;

        var argumentsNode = testNode.Children[mappingName];
        if (argumentsNode == null) return;

        if (argumentsNode is YamlScalarNode asScalar)
        {
            var value = asScalar.Value;
            SetTestCaseProperty(test, propertyName, $"\"{value}\"");
        }
        else if (argumentsNode is YamlMappingNode asMapping)
        {
            SetTestCasePropertyMap(test, propertyName, asMapping
                .Select(x => NormalizeToScalarKeyValuePair(test, x, workingDirectory)));
        }
        else if (argumentsNode is YamlSequenceNode asSequence)
        {
            SetTestCasePropertyMap(test, propertyName, asSequence
                .Select(mapping => (mapping as YamlMappingNode)?
                    .Select(x => NormalizeToScalarKeyValuePair(test, x, workingDirectory))));
        }
    }

    private static void SetTestCasePropertyMap(TestCase test, string propertyName, IEnumerable<IEnumerable<KeyValuePair<YamlNode, YamlNode>>?> kvss)
    {
        // flatten the kvs
        var kvs = kvss.Where(x => x != null).SelectMany(x => x!);

        // ensure all keys are unique, if not, transform appropriately
        var keys = kvs
            .GroupBy(kv => (kv.Key as YamlScalarNode)?.Value)
            .Where(g => g?.Key != null)
            .Select(g => g.Key!)
            .ToArray();
        if (keys.Length < kvs.Count())
        {
            TestLogger.Log($"keys.Length={keys.Length}, kvs.Count={kvs.Count()}");
            TestLogger.Log($"keys='{string.Join(",", keys)}'");

            var values = new List<string>();
            foreach (var items in kvss)
            {
                var map = new YamlMappingNode(items!);
                var tsv = map.ConvertScalarMapToTsvString(keys);
                if (tsv != null) values.Add(tsv);
            }

            var combinedKey = new YamlScalarNode(string.Join("\t", keys));
            var combinedValue = new YamlScalarNode(string.Join("\n", values));
            var combinedKv = new KeyValuePair<YamlNode, YamlNode>(combinedKey, combinedValue);
            kvs = new List<KeyValuePair<YamlNode, YamlNode>>(new[] { combinedKv });
        }

        SetTestCasePropertyMap(test, propertyName, kvs);
    }

    private static void SetTestCasePropertyMap(TestCase test, string propertyName, IEnumerable<KeyValuePair<YamlNode, YamlNode>> kvs)
    {
        var newMap = new YamlMappingNode(kvs);
        SetTestCaseProperty(test, propertyName, newMap.ToJsonString());
    }

    private static KeyValuePair<YamlNode, YamlNode> NormalizeToScalarKeyValuePair(TestCase test, KeyValuePair<YamlNode, YamlNode> item, string workingDirectory)
    {
        var key = item.Key;
        var keyOk = key is YamlScalarNode;
        var value = item.Value;
        var valueOk = value is YamlScalarNode;
        if (keyOk && valueOk) return item;

        string[]? keys = null;
        if (!keyOk)
        {
            var text = key.ConvertScalarSequenceToTsvString();
            if (text == null)
            {
                text = $"Invalid key at {test.CodeFilePath}({key.Start.Line},{key.Start.Column})";
                TestLogger.Log(text);
            }
            else if (text.Contains('\t'))
            {
                keys = text.Split('\t');
            }
            key = new YamlScalarNode(text);
        }

        if (!valueOk)
        {
            value = value.ConvertScalarSequenceToMultiLineTsvScalarNode(test, keys!);
        }
        else
        {
            var scalarValue = value.ToJsonString().Trim('\"');
            if (TryGetFileContentFromScalar(scalarValue, workingDirectory, out string fileContent))
            {
                value = fileContent;
                if (!(value is YamlScalarNode))
                {
                    value = value.ConvertScalarSequenceToMultiLineTsvScalarNode(test, keys!);
                }
            }
        }

        TestLogger.Log($"YamlTestCaseParser.NormalizeToScalarKeyValuePair: key='{(key as YamlScalarNode)?.Value}', value='{(value as YamlScalarNode)?.Value}'");
        return new KeyValuePair<YamlNode, YamlNode>(key, value);
    }

    private static bool TryGetFileContentFromScalar(string scalar, string workingDirectory, out string fileContent)
    {
        // Treat this scalar value as file if it starts with '@' and does not have InvalidFileNameChars
        if (scalar.StartsWith("@") && Path.GetFileName(scalar).IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
        {
            var fileName = scalar.Substring(1);

            // check if the file already exists
            var filePath = fileName;
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(workingDirectory, fileName);
            }

            TestLogger.Log($"YamlTestCaseParser.TryGetFileContentFromScalar: Read file contents from {filePath}");
            if (File.Exists(filePath))
            {
                fileContent = File.ReadAllText(filePath);
                return true;
            }
        }

        fileContent = "";
        return false;
    }

    private static string? GetScalarString(YamlMappingNode? mapping, Dictionary<string, List<string>> tags, string mappingName, string? defaultValue = null)
    {
        var value = GetScalarString(mapping, mappingName, null);
        if (value != null) return value;

        if (tags.ContainsKey(mappingName))
        {
            value = tags[mappingName].Last();
        }

        return value ?? defaultValue;
    }

    private static string? GetScalarString(YamlMappingNode? mapping, string mappingName, string? defaultValue = null)
    {
        var ok = mapping != null && mapping.Children.ContainsKey(mappingName);
        if (!ok) return defaultValue;

        var node = mapping!.Children[mappingName] as YamlScalarNode;
        var value = node?.Value;

        return value ?? defaultValue;
    }

    private static string GetRootArea(FileInfo file)
    {
        return $"{file.Extension.TrimStart('.')}.{Path.GetFileNameWithoutExtension(file.Name)}";
    }



    private static string UpdateArea(YamlMappingNode mapping, string area)
    {
        var subArea = GetScalarString(mapping, "area");
        return string.IsNullOrEmpty(subArea)
            ? area
            : $"{area}.{subArea}";
    }

    private static string? GetFullyQualifiedName(YamlMappingNode mapping, string area, string @class, int stepNumber)
    {
        var name = GetScalarString(mapping, "name");
        if (name == null) return null;

        area = UpdateArea(mapping, area);
        @class = GetScalarString(mapping, "class", @class)!;

        return GetFullyQualifiedName(area, @class, name, stepNumber);
    }

    private static string? GetFullyQualifiedNameAndCommandFromShortForm(YamlMappingNode mapping, string area, string @class, ref string? runProcess, int stepNumber)
    {
        // if there's only one invalid mapping node, we'll treat it's key as "name" and value as "run"
        var invalid = mapping.Children.Keys.Where(key => !IsValidTestCaseNode((key as YamlScalarNode)?.Value));
        if (invalid.Count() == 1 && runProcess == null)
        {
            var name = (invalid.FirstOrDefault() as YamlScalarNode)?.Value;
            if (name == null) return null;

            runProcess = GetScalarString(mapping, name);
            area = UpdateArea(mapping, area);
            @class = GetScalarString(mapping, "class", @class)!;

            return GetFullyQualifiedName(area, @class, name, stepNumber);
        }

        return null;
    }

    private static string GetFullyQualifiedName(string area, string @class, string name, int stepNumber)
    {
        return stepNumber > 0
            ? $"{area}.{@class}::{stepNumber:D2}.{name}"
            : $"{area}.{@class}::{name}";
    }

    private static void SetTestCaseTagsAsTraits(TestCase test, Dictionary<string, List<string>> tags)
    {
        foreach (var tag in tags)
        {
            foreach (var value in tag.Value)
            {
                test.Traits.Add(tag.Key, value);
            }
        }
    }
    


    private static string UpdateWorkingDirectory(YamlMappingNode mapping, string currentWorkingDirectory)
    {
        var workingDirectory = GetScalarString(mapping, "workingDirectory");
        return UpdateWorkingDirectory(currentWorkingDirectory, workingDirectory);
    }

    private static string UpdateWorkingDirectory(string currentWorkingDirectory, string? workingDirectory)
    {
        if (string.IsNullOrEmpty(workingDirectory)) return currentWorkingDirectory;
        return PathHelpers.Combine(currentWorkingDirectory, workingDirectory) ?? currentWorkingDirectory;
    }

    private static void AppendDeterministicUniquenessSuffix(TestCase test)
    {
        try
        {
            if (string.IsNullOrEmpty(test.CodeFilePath)) return;

            var fileInfo = new FileInfo(test.CodeFilePath);
            var rel = fileInfo.FullName.Replace('\\', '/').ToLowerInvariant();
            var seed = $"{rel}:{test.LineNumber}";
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(seed));
            var h6 = BitConverter.ToString(bytes).Replace("-", string.Empty).Substring(0, 6).ToLowerInvariant();
            if (!test.FullyQualifiedName.EndsWith("@" + h6, StringComparison.Ordinal))
            {
                test.FullyQualifiedName += "@" + h6;
            }
        }
        catch (Exception ex)
        {
            TestLogger.LogWarning($"AppendDeterministicUniquenessSuffix failed: {ex.Message}");
        }
    }


    private const string defaultClassName = "TestCases";

    #endregion
}
