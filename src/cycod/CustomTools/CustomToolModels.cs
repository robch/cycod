using System.ComponentModel;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace CycoDev.CustomTools.Models
{
    /// <summary>
    /// Represents a custom tool definition that can be loaded from YAML.
    /// </summary>
    public class CustomToolDefinition
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        [YamlMember(Alias = "description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the tool.
        /// </summary>
        [YamlMember(Alias = "version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the minimum CYCOD version required.
        /// </summary>
        [YamlMember(Alias = "min-cycod-version")]
        public string MinCycodVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the Bash command to execute.
        /// </summary>
        [YamlMember(Alias = "bash")]
        public string? BashCommand { get; set; }

        /// <summary>
        /// Gets or sets the Windows CMD command to execute.
        /// </summary>
        [YamlMember(Alias = "cmd")]
        public string? CmdCommand { get; set; }

        /// <summary>
        /// Gets or sets the PowerShell command to execute.
        /// </summary>
        [YamlMember(Alias = "pwsh")]
        public string? PowerShellCommand { get; set; }

        /// <summary>
        /// Gets or sets the direct command to execute.
        /// </summary>
        [YamlMember(Alias = "run")]
        public string? RunCommand { get; set; }

        /// <summary>
        /// Gets or sets the script content.
        /// </summary>
        [YamlMember(Alias = "script")]
        public string? Script { get; set; }

        /// <summary>
        /// Gets or sets the shell to use with the script.
        /// </summary>
        [YamlMember(Alias = "shell")]
        public string? Shell { get; set; }

        /// <summary>
        /// Gets or sets the steps for multi-step tools.
        /// </summary>
        [YamlMember(Alias = "steps")]
        public List<CustomToolStep>? Steps { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the tool.
        /// </summary>
        [YamlMember(Alias = "parameters")]
        public Dictionary<string, CustomToolParameter> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the timeout in milliseconds.
        /// </summary>
        [YamlMember(Alias = "timeout")]
        public int Timeout { get; set; } = 60000;

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        [YamlMember(Alias = "working-directory")]
        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the platforms supported by this tool.
        /// </summary>
        [YamlMember(Alias = "platforms")]
        public List<string> Platforms { get; set; } = new();

        /// <summary>
        /// Gets or sets the tags for categorization and security.
        /// </summary>
        [YamlMember(Alias = "tags")]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to ignore errors.
        /// </summary>
        [YamlMember(Alias = "ignore-errors")]
        public bool IgnoreErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets the data to pass via stdin.
        /// </summary>
        [YamlMember(Alias = "input")]
        public string? Input { get; set; }

        /// <summary>
        /// Gets or sets environment variables for the tool.
        /// </summary>
        [YamlMember(Alias = "environment")]
        public CustomToolEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets function calling schema configuration.
        /// </summary>
        [YamlMember(Alias = "function-calling")]
        public CustomToolFunctionCalling? FunctionCalling { get; set; }

        /// <summary>
        /// Gets or sets security configuration for the tool.
        /// </summary>
        [YamlMember(Alias = "security")]
        public CustomToolSecurity? Security { get; set; }

        /// <summary>
        /// Gets or sets file path configuration for the tool.
        /// </summary>
        [YamlMember(Alias = "file-paths")]
        public CustomToolFilePaths? FilePaths { get; set; }

        /// <summary>
        /// Gets or sets metadata for categorization and discovery.
        /// </summary>
        [YamlMember(Alias = "metadata")]
        public CustomToolMetadata? Metadata { get; set; }

        /// <summary>
        /// Gets or sets tests for the tool.
        /// </summary>
        [YamlMember(Alias = "tests")]
        public List<CustomToolTest>? Tests { get; set; }

        /// <summary>
        /// Gets or sets resource constraints for the tool.
        /// </summary>
        [YamlMember(Alias = "resources")]
        public CustomToolResources? Resources { get; set; }

        /// <summary>
        /// Gets or sets whether this tool is interactive.
        /// </summary>
        [YamlMember(Alias = "interactive")]
        public bool Interactive { get; set; } = false;

        /// <summary>
        /// Gets or sets interactive options for the tool.
        /// </summary>
        [YamlMember(Alias = "interactive-options")]
        public CustomToolInteractiveOptions? InteractiveOptions { get; set; }

        /// <summary>
        /// Gets or sets whether this tool is an alias.
        /// </summary>
        [YamlMember(Alias = "type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the base tool for an alias.
        /// </summary>
        [YamlMember(Alias = "base-tool")]
        public string? BaseTool { get; set; }

        /// <summary>
        /// Gets or sets default parameters for an alias.
        /// </summary>
        [YamlMember(Alias = "default-parameters")]
        public Dictionary<string, string>? DefaultParameters { get; set; }

        /// <summary>
        /// Gets or sets the changelog for the tool.
        /// </summary>
        [YamlMember(Alias = "changelog")]
        public List<CustomToolChangelogEntry>? Changelog { get; set; }

        /// <summary>
        /// Validates the tool definition.
        /// </summary>
        /// <param name="errorMessage">The error message if validation fails.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public bool Validate(out string? errorMessage)
        {
            errorMessage = null;

            // Validate required fields
            if (string.IsNullOrEmpty(Name))
            {
                errorMessage = "Tool name is required.";
                return false;
            }

            if (string.IsNullOrEmpty(Description))
            {
                errorMessage = "Tool description is required.";
                return false;
            }

            // Validate that at least one command type is specified
            bool hasCommand = !string.IsNullOrEmpty(BashCommand) ||
                              !string.IsNullOrEmpty(CmdCommand) ||
                              !string.IsNullOrEmpty(PowerShellCommand) ||
                              !string.IsNullOrEmpty(RunCommand) ||
                              !string.IsNullOrEmpty(Script) ||
                              (Steps != null && Steps.Count > 0);

            if (!hasCommand)
            {
                errorMessage = "Tool must have at least one command or steps.";
                return false;
            }

            // If script is specified, shell must also be specified
            if (!string.IsNullOrEmpty(Script) && string.IsNullOrEmpty(Shell))
            {
                errorMessage = "Shell must be specified when using a script.";
                return false;
            }

            // Validate steps
            if (Steps != null)
            {
                foreach (var step in Steps)
                {
                    if (string.IsNullOrEmpty(step.Name))
                    {
                        errorMessage = "All steps must have a name.";
                        return false;
                    }

                    bool hasStepCommand = !string.IsNullOrEmpty(step.BashCommand) ||
                                         !string.IsNullOrEmpty(step.CmdCommand) ||
                                         !string.IsNullOrEmpty(step.PowerShellCommand) ||
                                         !string.IsNullOrEmpty(step.RunCommand) ||
                                         !string.IsNullOrEmpty(step.UseTool);

                    if (!hasStepCommand)
                    {
                        errorMessage = $"Step '{step.Name}' must have a command.";
                        return false;
                    }

                    // If using another tool, validate that the with parameters match
                    if (!string.IsNullOrEmpty(step.UseTool) && (step.With == null || step.With.Count == 0))
                    {
                        // This is just a warning, not an error
                        Console.WriteLine($"Warning: Step '{step.Name}' uses tool '{step.UseTool}' but doesn't provide any parameters.");
                    }
                }
            }

            // Check for duplicate step names
            if (Steps != null)
            {
                var stepNames = new HashSet<string>();
                foreach (var step in Steps)
                {
                    if (!stepNames.Add(step.Name))
                    {
                        errorMessage = $"Duplicate step name: '{step.Name}'.";
                        return false;
                    }
                }
            }

            // Set parameter names and validate parameters
            foreach (var param in Parameters)
            {
                param.Value.Name = param.Key;

                if (string.IsNullOrEmpty(param.Value.Description))
                {
                    errorMessage = $"Parameter '{param.Key}' must have a description.";
                    return false;
                }

                // Validate parameter type
                string paramType = param.Value.Type.ToLowerInvariant();
                if (paramType != "string" && paramType != "number" && paramType != "boolean" && paramType != "array" && paramType != "object")
                {
                    errorMessage = $"Parameter '{param.Key}' has invalid type: '{param.Value.Type}'. Must be one of: string, number, boolean, array, object.";
                    return false;
                }

                // Validate default value against parameter type if provided
                if (param.Value.Default != null)
                {
                    if (!param.Value.Validate(param.Value.Default, out string? paramError))
                    {
                        errorMessage = paramError;
                        return false;
                    }
                }
            }

            // Validate parameter references in commands
            HashSet<string> missingParams = new HashSet<string>();

            // Check parameters in main commands
            CheckParameterReferences(BashCommand, missingParams);
            CheckParameterReferences(CmdCommand, missingParams);
            CheckParameterReferences(PowerShellCommand, missingParams);
            CheckParameterReferences(RunCommand, missingParams);
            CheckParameterReferences(Script, missingParams);

            // Check parameters in step commands
            if (Steps != null)
            {
                foreach (var step in Steps)
                {
                    CheckParameterReferences(step.BashCommand, missingParams);
                    CheckParameterReferences(step.CmdCommand, missingParams);
                    CheckParameterReferences(step.PowerShellCommand, missingParams);
                    CheckParameterReferences(step.RunCommand, missingParams);
                }
            }

            // Check for missing parameters
            if (missingParams.Count > 0)
            {
                errorMessage = $"The following parameters are referenced but not defined: {string.Join(", ", missingParams)}";
                return false;
            }

            return true;
        }

        private void CheckParameterReferences(string? command, HashSet<string> missingParams)
        {
            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            // Find parameter references like {PARAM_NAME}
            var matches = System.Text.RegularExpressions.Regex.Matches(command, @"\{([^}]+)\}");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string paramName = match.Groups[1].Value;

                // Skip step references
                if (paramName.Contains("."))
                {
                    continue;
                }

                // Skip parameter exists check for environment variables and other special parameters
                if (paramName.StartsWith("ENV_") || 
                    paramName == "WORKSPACE" || 
                    paramName == "TOOL_NAME" || 
                    paramName == "INPUT_PARAM")
                {
                    continue;
                }

                // Check if parameter is defined
                if (!Parameters.ContainsKey(paramName))
                {
                    missingParams.Add(paramName);
                }
            }
        }
    }

    /// <summary>
    /// Represents a parameter for a custom tool.
    /// </summary>
    public class CustomToolParameter
    {
        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        [YamlMember(Alias = "type")]
        public string Type { get; set; } = "string";

        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        [YamlMember(Alias = "description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the parameter is required.
        /// </summary>
        [YamlMember(Alias = "required")]
        public bool Required { get; set; } = false;

        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        [YamlMember(Alias = "default")]
        public object? Default { get; set; }

        /// <summary>
        /// Gets or sets validation rules for the parameter.
        /// </summary>
        [YamlMember(Alias = "validation")]
        public Dictionary<string, object>? Validation { get; set; }

        /// <summary>
        /// Gets or sets the transform function to apply to the parameter.
        /// </summary>
        [YamlMember(Alias = "transform")]
        public string? Transform { get; set; }

        /// <summary>
        /// Gets or sets the format string for the parameter.
        /// </summary>
        [YamlMember(Alias = "format")]
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets examples for the parameter.
        /// </summary>
        [YamlMember(Alias = "examples")]
        public List<string>? Examples { get; set; }

        /// <summary>
        /// Gets or sets detailed help text for the parameter.
        /// </summary>
        [YamlMember(Alias = "detailed-help")]
        public string? DetailedHelp { get; set; }

        /// <summary>
        /// Gets or sets security options for the parameter.
        /// </summary>
        [YamlMember(Alias = "security")]
        public CustomToolParameterSecurity? Security { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        [YamlIgnore]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Validates a parameter value against the parameter definition.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="errorMessage">The error message if validation fails.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public bool Validate(object? value, out string? errorMessage)
        {
            errorMessage = null;

            // Check if required
            if (Required && value == null)
            {
                errorMessage = $"Parameter '{Name}' is required.";
                return false;
            }

            // If null and not required, validation passes
            if (value == null)
            {
                return true;
            }

            // Validate by type
            switch (Type?.ToLowerInvariant())
            {
                case "string":
                    return ValidateString(value.ToString(), out errorMessage);
                case "number":
                    return ValidateNumber(value, out errorMessage);
                case "boolean":
                    return ValidateBoolean(value, out errorMessage);
                case "array":
                    return ValidateArray(value, out errorMessage);
                case "object":
                    return ValidateObject(value, out errorMessage);
                default:
                    // Default to string validation if type is not specified
                    return ValidateString(value.ToString(), out errorMessage);
            }
        }

        private bool ValidateString(string? value, out string? errorMessage)
        {
            errorMessage = null;

            if (value == null)
            {
                return true;
            }

            if (Validation != null)
            {
                // Validate string length
                if (Validation.ContainsKey("minLength"))
                {
                    if (int.TryParse(Validation["minLength"].ToString(), out int minLength))
                    {
                        if (value.Length < minLength)
                        {
                            errorMessage = $"Parameter '{Name}' must be at least {minLength} characters long.";
                            return false;
                        }
                    }
                }

                if (Validation.ContainsKey("maxLength"))
                {
                    if (int.TryParse(Validation["maxLength"].ToString(), out int maxLength))
                    {
                        if (value.Length > maxLength)
                        {
                            errorMessage = $"Parameter '{Name}' must be no more than {maxLength} characters long.";
                            return false;
                        }
                    }
                }

                // Validate string pattern
                if (Validation.ContainsKey("pattern"))
                {
                    string pattern = Validation["pattern"].ToString();
                    try
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                        {
                            errorMessage = $"Parameter '{Name}' does not match the required pattern: {pattern}.";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Invalid pattern for parameter '{Name}': {ex.Message}";
                        return false;
                    }
                }

                // Validate enum values
                if (Validation.ContainsKey("enum"))
                {
                    if (Validation["enum"] is List<object> enumValues)
                    {
                        bool found = false;
                        foreach (var enumValue in enumValues)
                        {
                            if (value.Equals(enumValue.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            errorMessage = $"Parameter '{Name}' must be one of: {string.Join(", ", enumValues)}.";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool ValidateNumber(object value, out string? errorMessage)
        {
            errorMessage = null;

            // Try to convert to decimal for validation
            if (!decimal.TryParse(value.ToString(), out decimal numericValue))
            {
                errorMessage = $"Parameter '{Name}' must be a valid number.";
                return false;
            }

            if (Validation != null)
            {
                // Validate minimum value
                if (Validation.ContainsKey("minimum"))
                {
                    if (decimal.TryParse(Validation["minimum"].ToString(), out decimal minimum))
                    {
                        if (numericValue < minimum)
                        {
                            errorMessage = $"Parameter '{Name}' must be at least {minimum}.";
                            return false;
                        }
                    }
                }

                // Validate maximum value
                if (Validation.ContainsKey("maximum"))
                {
                    if (decimal.TryParse(Validation["maximum"].ToString(), out decimal maximum))
                    {
                        if (numericValue > maximum)
                        {
                            errorMessage = $"Parameter '{Name}' must be no more than {maximum}.";
                            return false;
                        }
                    }
                }

                // Validate multiple of
                if (Validation.ContainsKey("multipleOf"))
                {
                    if (decimal.TryParse(Validation["multipleOf"].ToString(), out decimal multipleOf))
                    {
                        if (multipleOf != 0 && numericValue % multipleOf != 0)
                        {
                            errorMessage = $"Parameter '{Name}' must be a multiple of {multipleOf}.";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool ValidateBoolean(object value, out string? errorMessage)
        {
            errorMessage = null;

            // Try to convert to boolean for validation
            string strValue = value.ToString().ToLowerInvariant();
            if (strValue != "true" && strValue != "false" && 
                strValue != "1" && strValue != "0" &&
                strValue != "yes" && strValue != "no")
            {
                errorMessage = $"Parameter '{Name}' must be a valid boolean (true/false, 1/0, yes/no).";
                return false;
            }

            return true;
        }

        private bool ValidateArray(object value, out string? errorMessage)
        {
            errorMessage = null;

            // Check if the value is actually an array
            if (!(value is System.Collections.IEnumerable enumerable) || value is string)
            {
                errorMessage = $"Parameter '{Name}' must be an array.";
                return false;
            }

            if (Validation != null)
            {
                // Count items for validation
                int count = 0;
                foreach (var _ in enumerable)
                {
                    count++;
                }

                // Validate minimum items
                if (Validation.ContainsKey("minItems"))
                {
                    if (int.TryParse(Validation["minItems"].ToString(), out int minItems))
                    {
                        if (count < minItems)
                        {
                            errorMessage = $"Parameter '{Name}' must have at least {minItems} items.";
                            return false;
                        }
                    }
                }

                // Validate maximum items
                if (Validation.ContainsKey("maxItems"))
                {
                    if (int.TryParse(Validation["maxItems"].ToString(), out int maxItems))
                    {
                        if (count > maxItems)
                        {
                            errorMessage = $"Parameter '{Name}' must have no more than {maxItems} items.";
                            return false;
                        }
                    }
                }

                // Validate unique items
                if (Validation.ContainsKey("uniqueItems"))
                {
                    if (bool.TryParse(Validation["uniqueItems"].ToString(), out bool uniqueItems) && uniqueItems)
                    {
                        var items = new HashSet<string>();
                        foreach (var item in enumerable)
                        {
                            string itemStr = item?.ToString() ?? string.Empty;
                            if (!items.Add(itemStr))
                            {
                                errorMessage = $"Parameter '{Name}' must have unique items.";
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool ValidateObject(object value, out string? errorMessage)
        {
            errorMessage = null;

            // For now, basic validation that it's a dictionary or similar
            if (!(value is System.Collections.IDictionary))
            {
                errorMessage = $"Parameter '{Name}' must be an object.";
                return false;
            }

            // Additional object validation could be added here
            return true;
        }

        /// <summary>
        /// Transforms a parameter value according to the transformation rules.
        /// </summary>
        /// <param name="value">The value to transform.</param>
        /// <returns>The transformed value.</returns>
        public object? TransformValue(object? value)
        {
            if (value == null || string.IsNullOrEmpty(Transform))
            {
                return value;
            }

            string strValue = value.ToString();

            // Apply transformation based on the transform expression
            switch (Transform.ToLowerInvariant())
            {
                case "lowercase":
                case "tolowercase":
                case "tolower":
                    return strValue.ToLowerInvariant();

                case "uppercase":
                case "touppercase":
                case "toupper":
                    return strValue.ToUpperInvariant();

                case "trim":
                    return strValue.Trim();

                case "int":
                case "integer":
                case "floor":
                case "math.floor":
                    if (decimal.TryParse(strValue, out decimal decValue))
                    {
                        return Math.Floor(decValue);
                    }
                    return value;

                case "ceil":
                case "ceiling":
                case "math.ceiling":
                case "math.ceil":
                    if (decimal.TryParse(strValue, out decimal ceilValue))
                    {
                        return Math.Ceiling(ceilValue);
                    }
                    return value;

                case "round":
                case "math.round":
                    if (decimal.TryParse(strValue, out decimal roundValue))
                    {
                        return Math.Round(roundValue);
                    }
                    return value;

                case "boolean":
                case "bool":
                    string loweredValue = strValue.ToLowerInvariant();
                    return loweredValue == "true" || loweredValue == "1" || loweredValue == "yes";

                default:
                    // If it's a custom transformation, return as is for now
                    // A more sophisticated transformation system could be implemented later
                    return value;
            }
        }
    }

    /// <summary>
    /// Represents a step in a multi-step custom tool.
    /// </summary>
    public class CustomToolStep
    {
        /// <summary>
        /// Gets or sets the name of the step.
        /// </summary>
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Bash command to execute.
        /// </summary>
        [YamlMember(Alias = "bash")]
        public string? BashCommand { get; set; }

        /// <summary>
        /// Gets or sets the Windows CMD command to execute.
        /// </summary>
        [YamlMember(Alias = "cmd")]
        public string? CmdCommand { get; set; }

        /// <summary>
        /// Gets or sets the PowerShell command to execute.
        /// </summary>
        [YamlMember(Alias = "pwsh")]
        public string? PowerShellCommand { get; set; }

        /// <summary>
        /// Gets or sets the direct command to execute.
        /// </summary>
        [YamlMember(Alias = "run")]
        public string? RunCommand { get; set; }

        /// <summary>
        /// Gets or sets the name of a tool to use.
        /// </summary>
        [YamlMember(Alias = "use-tool")]
        public string? UseTool { get; set; }

        /// <summary>
        /// Gets or sets parameters for the used tool.
        /// </summary>
        [YamlMember(Alias = "with")]
        public Dictionary<string, string>? With { get; set; }

        /// <summary>
        /// Gets or sets whether to continue on error.
        /// </summary>
        [YamlMember(Alias = "continue-on-error")]
        public bool ContinueOnError { get; set; } = false;

        /// <summary>
        /// Gets or sets the condition for when to run this step.
        /// </summary>
        [YamlMember(Alias = "run-condition")]
        public string? RunCondition { get; set; }

        /// <summary>
        /// Gets or sets whether to run this step in parallel.
        /// </summary>
        [YamlMember(Alias = "parallel")]
        public bool Parallel { get; set; } = false;

        /// <summary>
        /// Gets or sets steps to wait for before running this step.
        /// </summary>
        [YamlMember(Alias = "wait-for")]
        public List<string>? WaitFor { get; set; }

        /// <summary>
        /// Gets or sets error handling configuration for this step.
        /// </summary>
        [YamlMember(Alias = "error-handling")]
        public CustomToolStepErrorHandling? ErrorHandling { get; set; }

        /// <summary>
        /// Gets or sets output configuration for this step.
        /// </summary>
        [YamlMember(Alias = "output")]
        public CustomToolStepOutput? Output { get; set; }
    }

    /// <summary>
    /// Represents security configuration for a parameter.
    /// </summary>
    public class CustomToolParameterSecurity
    {
        /// <summary>
        /// Gets or sets whether to escape shell metacharacters.
        /// </summary>
        [YamlMember(Alias = "escape-shell")]
        public bool EscapeShell { get; set; } = false;
    }

    /// <summary>
    /// Represents error handling configuration for a step.
    /// </summary>
    public class CustomToolStepErrorHandling
    {
        /// <summary>
        /// Gets or sets retry configuration.
        /// </summary>
        [YamlMember(Alias = "retry")]
        public CustomToolStepRetry? Retry { get; set; }

        /// <summary>
        /// Gets or sets the fallback command.
        /// </summary>
        [YamlMember(Alias = "fallback")]
        public string? Fallback { get; set; }
    }

    /// <summary>
    /// Represents retry configuration for error handling.
    /// </summary>
    public class CustomToolStepRetry
    {
        /// <summary>
        /// Gets or sets the number of retry attempts.
        /// </summary>
        [YamlMember(Alias = "attempts")]
        public int Attempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay between retries in milliseconds.
        /// </summary>
        [YamlMember(Alias = "delay")]
        public int Delay { get; set; } = 1000;
    }

    /// <summary>
    /// Represents output configuration for a step.
    /// </summary>
    public class CustomToolStepOutput
    {
        /// <summary>
        /// Gets or sets the maximum size of output to capture.
        /// </summary>
        [YamlMember(Alias = "max-size")]
        public string? MaxSize { get; set; }

        /// <summary>
        /// Gets or sets whether to truncate output if it exceeds max-size.
        /// </summary>
        [YamlMember(Alias = "truncation")]
        public bool Truncation { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to stream output rather than buffering.
        /// </summary>
        [YamlMember(Alias = "streaming")]
        public bool Streaming { get; set; } = false;

        /// <summary>
        /// Gets or sets the streaming mode.
        /// </summary>
        [YamlMember(Alias = "mode")]
        public string? Mode { get; set; }

        /// <summary>
        /// Gets or sets the buffer limit when buffering.
        /// </summary>
        [YamlMember(Alias = "buffer-limit")]
        public string? BufferLimit { get; set; }

        /// <summary>
        /// Gets or sets where to stream output.
        /// </summary>
        [YamlMember(Alias = "stream-callback")]
        public string? StreamCallback { get; set; }
    }

    /// <summary>
    /// Represents environment configuration for a tool.
    /// </summary>
    public class CustomToolEnvironment
    {
        /// <summary>
        /// Gets or sets environment variables.
        /// </summary>
        [YamlMember(Alias = "variables")]
        public Dictionary<string, string> Variables { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to inherit parent process environment.
        /// </summary>
        [YamlMember(Alias = "inherit")]
        public bool Inherit { get; set; } = true;
    }

    /// <summary>
    /// Represents function calling configuration for a tool.
    /// </summary>
    public class CustomToolFunctionCalling
    {
        /// <summary>
        /// Gets or sets schema generation configuration.
        /// </summary>
        [YamlMember(Alias = "schema-generation")]
        public CustomToolSchemaGeneration? SchemaGeneration { get; set; }
    }

    /// <summary>
    /// Represents schema generation configuration for function calling.
    /// </summary>
    public class CustomToolSchemaGeneration
    {
        /// <summary>
        /// Gets or sets parameter mapping configuration.
        /// </summary>
        [YamlMember(Alias = "parameter-mapping")]
        public Dictionary<string, string> ParameterMapping { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to include descriptions in schema.
        /// </summary>
        [YamlMember(Alias = "include-descriptions")]
        public bool IncludeDescriptions { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include defaults in schema.
        /// </summary>
        [YamlMember(Alias = "include-defaults")]
        public bool IncludeDefaults { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to generate examples.
        /// </summary>
        [YamlMember(Alias = "example-generation")]
        public bool ExampleGeneration { get; set; } = true;
    }

    /// <summary>
    /// Represents security configuration for a tool.
    /// </summary>
    public class CustomToolSecurity
    {
        /// <summary>
        /// Gets or sets the execution privilege level.
        /// </summary>
        [YamlMember(Alias = "execution-privilege")]
        public string ExecutionPrivilege { get; set; } = "same-as-user";

        /// <summary>
        /// Gets or sets the isolation level.
        /// </summary>
        [YamlMember(Alias = "isolation")]
        public string Isolation { get; set; } = "process";

        /// <summary>
        /// Gets or sets required permissions.
        /// </summary>
        [YamlMember(Alias = "required-permissions")]
        public List<string> RequiredPermissions { get; set; } = new();

        /// <summary>
        /// Gets or sets justification for permissions.
        /// </summary>
        [YamlMember(Alias = "justification")]
        public string? Justification { get; set; }
    }

    /// <summary>
    /// Represents file path configuration for a tool.
    /// </summary>
    public class CustomToolFilePaths
    {
        /// <summary>
        /// Gets or sets whether to normalize paths.
        /// </summary>
        [YamlMember(Alias = "normalize")]
        public bool Normalize { get; set; } = true;

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        [YamlMember(Alias = "working-directory")]
        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the temporary directory.
        /// </summary>
        [YamlMember(Alias = "temp-directory")]
        public string? TempDirectory { get; set; }

        /// <summary>
        /// Gets or sets cross-platform path configuration.
        /// </summary>
        [YamlMember(Alias = "cross-platform")]
        public CustomToolCrossPlatformPaths? CrossPlatform { get; set; }
    }

    /// <summary>
    /// Represents cross-platform path configuration.
    /// </summary>
    public class CustomToolCrossPlatformPaths
    {
        /// <summary>
        /// Gets or sets the Windows path separator.
        /// </summary>
        [YamlMember(Alias = "windows-separator")]
        public string WindowsSeparator { get; set; } = "\\";

        /// <summary>
        /// Gets or sets the Unix path separator.
        /// </summary>
        [YamlMember(Alias = "unix-separator")]
        public string UnixSeparator { get; set; } = "/";

        /// <summary>
        /// Gets or sets whether to automatically convert paths.
        /// </summary>
        [YamlMember(Alias = "auto-convert")]
        public bool AutoConvert { get; set; } = true;
    }

    /// <summary>
    /// Represents metadata for a tool.
    /// </summary>
    public class CustomToolMetadata
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        [YamlMember(Alias = "category")]
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the subcategory.
        /// </summary>
        [YamlMember(Alias = "subcategory")]
        public string? Subcategory { get; set; }

        /// <summary>
        /// Gets or sets tags for categorization.
        /// </summary>
        [YamlMember(Alias = "tags")]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Gets or sets search keywords.
        /// </summary>
        [YamlMember(Alias = "search-keywords")]
        public List<string> SearchKeywords { get; set; } = new();
    }

    /// <summary>
    /// Represents a test for a tool.
    /// </summary>
    public class CustomToolTest
    {
        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the test.
        /// </summary>
        [YamlMember(Alias = "description")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the test.
        /// </summary>
        [YamlMember(Alias = "parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets the expected results.
        /// </summary>
        [YamlMember(Alias = "expected")]
        public CustomToolTestExpected Expected { get; set; } = new();

        /// <summary>
        /// Gets or sets cleanup commands to run after the test.
        /// </summary>
        [YamlMember(Alias = "cleanup")]
        public List<string> Cleanup { get; set; } = new();
    }

    /// <summary>
    /// Represents expected results for a test.
    /// </summary>
    public class CustomToolTestExpected
    {
        /// <summary>
        /// Gets or sets the expected exit code.
        /// </summary>
        [YamlMember(Alias = "exit-code")]
        public int? ExitCode { get; set; } = 0;

        /// <summary>
        /// Gets or sets text that should be in the output.
        /// </summary>
        [YamlMember(Alias = "output-contains")]
        public string? OutputContains { get; set; }

        /// <summary>
        /// Gets or sets a file that should exist.
        /// </summary>
        [YamlMember(Alias = "file-exists")]
        public string? FileExists { get; set; }
        
        /// <summary>
        /// Gets or sets a directory that should exist.
        /// </summary>
        [YamlMember(Alias = "directory-exists")]
        public string? DirectoryExists { get; set; }
    }

    /// <summary>
    /// Represents resource constraints for a tool.
    /// </summary>
    public class CustomToolResources
    {
        /// <summary>
        /// Gets or sets the timeout in milliseconds.
        /// </summary>
        [YamlMember(Alias = "timeout")]
        public int Timeout { get; set; } = 60000;

        /// <summary>
        /// Gets or sets the maximum memory usage.
        /// </summary>
        [YamlMember(Alias = "max-memory")]
        public string? MaxMemory { get; set; }

        /// <summary>
        /// Gets or sets cleanup configuration.
        /// </summary>
        [YamlMember(Alias = "cleanup")]
        public List<Dictionary<string, object>> Cleanup { get; set; } = new();

        /// <summary>
        /// Gets or sets environment variables.
        /// </summary>
        [YamlMember(Alias = "environment-variables")]
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

        /// <summary>
        /// Gets or sets the maximum size of output to capture.
        /// </summary>
        [YamlMember(Alias = "max-size")]
        public int? MaxSize { get; set; }

        /// <summary>
        /// Gets or sets whether to truncate output if it exceeds max-size.
        /// </summary>
        [YamlMember(Alias = "truncation")]
        public bool Truncation { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to stream output rather than buffering.
        /// </summary>
        [YamlMember(Alias = "streaming")]
        public bool Streaming { get; set; } = false;
    }

    /// <summary>
    /// Represents interactive options for a tool.
    /// </summary>
    public class CustomToolInteractiveOptions
    {
        /// <summary>
        /// Gets or sets the timeout for user input in milliseconds.
        /// </summary>
        [YamlMember(Alias = "timeout")]
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the default response if no input is provided.
        /// </summary>
        [YamlMember(Alias = "default-response")]
        public string? DefaultResponse { get; set; }
    }

    /// <summary>
    /// Represents a changelog entry for a tool.
    /// </summary>
    public class CustomToolChangelogEntry
    {
        /// <summary>
        /// Gets or sets the version of the entry.
        /// </summary>
        [YamlMember(Alias = "version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the changes in this version.
        /// </summary>
        [YamlMember(Alias = "changes")]
        public string Changes { get; set; } = string.Empty;
    }
}