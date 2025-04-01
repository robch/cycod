# Templates

ChatX provides a powerful templating system that allows you to create dynamic content with variables, conditionals, and expressions. This document explains how to use templates effectively in your interactions with the AI.

## Overview

Templates in ChatX allow you to:

1. Insert variables into text
2. Use conditional statements to include or exclude sections
3. Perform calculations and evaluations in-line
4. Process environment variables dynamically

## Variable Substitution

The simplest use of templates is variable substitution, which allows you to insert the value of a variable into text.

### Basic Syntax

```
Hello, {name}!
```

If the variable `name` has the value "John", this would output:

```
Hello, John!
```

### Variable Sources

Variables can come from:

1. Command line arguments (using `--var NAME=VALUE`)
2. Foreach loop variables (using `--foreach var NAME in VALUES...`)
3. Environment variables
4. Special built-in variables
5. Variables set within templates

### Special Variables

The following special variables are available:

| Variable | Description |
|----------|-------------|
| `os` | Operating system string |
| `osname` | Operating system platform name |
| `osversion` | Operating system version |
| `date` | Current date (YYYY-MM-DD format) |
| `time` | Current time (HH:MM:SS format) |
| `datetime` | Current date and time |
| `year` | Current year |
| `month` | Current month |
| `day` | Current day |
| `hour` | Current hour |
| `minute` | Current minute |
| `second` | Current second |
| `random` | Random number between 0 and 999 |

## Conditionals

Templates support conditional statements to include or exclude sections based on conditions.

### Basic Syntax

```
{{if condition}}
  Content to include if condition is true
{{else}}
  Content to include if condition is false
{{endif}}
```

### Multiple Conditions

```
{{if condition1}}
  Content for condition1
{{else if condition2}}
  Content for condition2
{{else}}
  Default content
{{endif}}
```

### Inline Conditionals

You can also use conditionals inline:

```
User: {{if isAdmin}}Admin{{else}}Regular User{{endif}} {name} logged in at {time}.
```

## Setting Variables

You can set variables within templates:

```
{{set x = 10 * 5}}
The result is {x}.
```

This would output:

```
The result is 50.
```

## Expressions

The template system includes a powerful expression evaluator that supports:

### Mathematical Operations

- Addition: `+`
- Subtraction: `-`
- Multiplication: `*`
- Division: `/`
- Modulo: `%` or `MOD`
- Integer Division: `DIV`
- Exponentiation: `^` or `**`

### Example

```
{{set a = 5}}
{{set b = 3}}
{{set c = a * (b + 2)}}

The result of {a} * ({b} + 2) is {c}.
```

Would output:

```
The result of 5 * (3 + 2) is 25.
```

### Comparison Operations

- Equal: `==`
- Not equal: `!=`
- Less than: `<`
- Less than or equal: `<=`
- Greater than: `>`
- Greater than or equal: `>=`

### Logical Operations

- AND: `&&`
- OR: `||`
- NOT: `!`

### Bitwise Operations

- AND: `&`
- OR: `|`
- NOT: `~`

### String Operations

Templates also support string operations through built-in functions:

- `TOLOWER(string)`: Convert a string to lowercase
- `TOUPPER(string)`: Convert a string to uppercase
- `EQUALS(string1, string2)`: Check if two strings are equal
- `CONTAINS(string, substring)`: Check if a string contains a substring
- `STARTSWITH(string, prefix)`: Check if a string starts with a prefix
- `ENDSWITH(string, suffix)`: Check if a string ends with a suffix
- `ISEMPTY(string)`: Check if a string is empty

### String Example

```
{{if CONTAINS(osname, "Windows")}}
  You are running on Windows.
{{else}}
  You are not running on Windows.
{{endif}}
```

### Math Functions

Many mathematical functions are available:

- `ABS(x)`: Absolute value
- `SQRT(x)`: Square root
- `SIN(x)`, `COS(x)`, `TAN(x)`: Trigonometric functions
- `LOG(x)`, `LOG10(x)`: Logarithmic functions
- `FLOOR(x)`, `CEIL(x)`, `TRUNCATE(x)`: Rounding functions
- `MIN(x, y)`, `MAX(x, y)`: Minimum and maximum
- And many more

### Mathematical Constants

- `PI`: 3.14159265358979
- `E`: 2.71828182845905

### Example with Math Function

```
The square root of 16 is {{set result = SQRT(16)}}{result}.
```

## File Content in Variables

You can include the content of files in your templates:

```
Here's my code:

{@code.py}
```

This will replace `{@code.py}` with the content of the file `code.py`.

## Example Use Cases

### Conditional System Prompts

```
{{if language == "python"}}
You are a Python expert who provides clear, PEP8 compliant code examples.
{{else if language == "javascript"}}
You are a JavaScript expert who provides modern ES6+ code examples.
{{else}}
You are a helpful programming assistant.
{{endif}}

Please help the user with their coding questions.
```

### Dynamic Greetings

```
Hello! Today is {date} and the time is {time}.

{{if hour < 12}}
Good morning!
{{else if hour < 18}}
Good afternoon!
{{else}}
Good evening!
{{endif}}
```

### Setting Configuration Variables

```
{{set debug = true}}
{{set indent = 4}}
{{set theme = "dark"}}

{{if debug}}
Debug mode is ON. Verbose output will be displayed.
{{endif}}
```

### Custom Functions Using Math Expressions

```
{{set fahrenheitToCelsius = (f - 32) * 5 / 9}}
{{set f = 98.6}}
{{set c = fahrenheitToCelsius}}

Body temperature:
{f}°F = {c}°C
```

## Using Templates with ChatX

### In System Prompts

```bash
chatx --system-prompt "You are helping a user on {os}. Today is {date}." --var name=Alice
```

### In Input Files

You can use templates in input files:

```bash
chatx --input @prompt-template.txt --var language=python --var feature=generators
```

Where `prompt-template.txt` might contain:

```
Explain {language} {feature} with examples.

{{if language == "python"}}
Please include type hints in your examples.
{{endif}}
```

### In Aliases

Templates are particularly useful in aliases:

```
# my-coding-assistant.alias
--system-prompt "{{if language == \"python\"}}You are a Python expert.{{else if language == \"javascript\"}}You are a JavaScript expert.{{else}}You are a coding expert.{{endif}}"
```

Then use it with:

```bash
chatx --my-coding-assistant --var language=python
```

## Loop Variables with Foreach

You can define loop variables that expand a command into multiple commands, each with a different value:

```bash
chatx --foreach var x in 1 2 3 --input "The value of x is {x}"
```

This will execute three separate chat commands, each with a different value of `x`.

You can also read values from a file (one per line):

```bash
chatx --foreach var name in @names.txt --input "Hello, {name}!"
```

When multiple foreach variables are defined, they create a Cartesian product (all combinations):

```bash
chatx --foreach var x in 1 2 3 --foreach var y in a b c --input "{x} + {y}"
```

This will execute 9 separate commands with all combinations of x and y: (1,a), (1,b), (1,c), (2,a), etc.

Loop variables can be used in all template contexts, including:

- System prompts
- User prompts
- Input instructions
- File paths

## Technical Details

- Templates are processed before being sent to the AI
- Variable substitution is case-insensitive
- Environment variables are automatically available as template variables
- Nested conditionals are supported (within reasonable limits)
- Expression evaluation is handled by an embedded calculator with support for math, string, and logical operations

## Best Practices

1. **Keep templates readable**: Use indentation and spacing in your templates to keep them readable
2. **Default values**: Always consider providing defaults for important variables
3. **Document variables**: If sharing templates, document what variables are expected
4. **Test complex templates**: For complex templates, test with different variable values
5. **Watch for escaping**: Be careful with quotes and special characters
6. **Avoid over-complication**: While powerful, overly complex templates can be hard to maintain