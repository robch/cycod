---
hide:
- toc
icon: material/format-line-spacing
---

--8<-- "snippets/ai-generated.md"

# Line Options

Filter and format lines in your markdown output using line-specific options.

??? question "When should I use line filtering?"
    
    Line filtering is useful when you need to focus on specific parts of files, such as:
    
    - Finding error messages in logs
    - Extracting TODO comments from code
    - Focusing on specific code patterns
    - Excluding boilerplate or generated content

## Line Filtering

Use these options to control which lines appear in your output.

``` { .bash .cli-command title="Find lines containing a pattern" }
cycodmd "**/*.cs" --contains "TODO"
```

``` { .bash .cli-command title="Filter lines but keep all matching files" }
cycodmd "**/*.cs" --line-contains "public class"
```

``` { .bash .cli-command title="Remove unwanted lines" }
cycodmd "**/*.cs" --remove-all-lines "^\s*//"
```

## Context Lines

Control how many surrounding lines appear around matches.

``` { .bash .cli-command title="Include context before and after matches" }
cycodmd "**/*.cs" --line-contains "TODO" --lines 3
```

``` { .bash .cli-command title="Show lines before matches" }
cycodmd "**/*.js" --line-contains "function" --lines-before 2
```

``` { .bash .cli-command title="Show lines after matches" }
cycodmd "**/*.log" --line-contains "ERROR" --lines-after 5
```

## Line Numbers

Add line numbers to your output for reference.

``` { .bash .cli-command title="Include line numbers in output" }
cycodmd "**/*.cs" --line-numbers
```

``` { .bash .cli-command title="Combine with filtering" }
cycodmd "**/*.cs" --line-contains "class" --line-numbers
```

## Common Patterns

### Finding Code Patterns

``` { .bash .cli-command title="Find class definitions" }
cycodmd "**/*.cs" --line-contains "public class \w+" --lines 2 --line-numbers
```

``` { .bash .cli-command title="Find API endpoints" }
cycodmd "**/*.cs" --line-contains "\[Route\(.*\)\]" --lines 5
```

``` { .bash .cli-command title="Find SQL queries" }
cycodmd "**/*.cs" --line-contains "SELECT.*FROM" --lines 3
```

### Cleaning Output

``` { .bash .cli-command title="Remove comments" }
cycodmd "**/*.js" --remove-all-lines "^\s*//.*$" --line-numbers
```

``` { .bash .cli-command title="Remove empty lines" }
cycodmd "**/*.md" --remove-all-lines "^\s*$"
```

### Combining Options

``` { .bash .cli-command title="Find TODOs with context and line numbers" }
cycodmd "**/*.cs" --line-contains "TODO" --lines 2 --line-numbers
```

``` { .bash .cli-command title="Show functions without comments" }
cycodmd "**/*.js" --line-contains "function" --remove-all-lines "^\\s*//" --line-numbers
```

``` { .bash .cli-command title="Show errors with context, excluding debug messages" }
cycodmd "**/*.log" --line-contains "ERROR" --remove-all-lines "DEBUG" --lines-after 10
```