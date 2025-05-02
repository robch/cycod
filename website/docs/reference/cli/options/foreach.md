---
title: --foreach
description: Create loop variables that expand to multiple commands
---

# --foreach

## Description

The `--foreach` command option allows you to define loop variables with multiple values. CycoD will automatically expand these variables into multiple commands, running each combination of values. This is especially useful for batch processing or running similar queries with different parameters.

## Syntax

```
--foreach var NAME in VALUE1 [VALUE2 VALUE3 ...]
```

## Parameters

- `NAME`: The name of the variable to define (will be referenced as `{NAME}` in subsequent parameters)
- `VALUE1`, `VALUE2`, etc.: The values that the variable should take for each iteration

## Formats

The `--foreach` option supports three main formats for specifying values:

### 1. Literal values

```
--foreach var name in Alice Bob Charlie
```

### 2. Values from a file (one per line)

```
--foreach var name in @FILE
```

### 3. Numeric range

```
--foreach var number in 1..5
```

## Reference in Templates

Variables defined using `--foreach` can be referenced in subsequent command options using curly brace syntax:

```
--input "Hello, {name}!"
```

## Multiple Variables

When multiple `--foreach` variables are used, CycoD will create all possible combinations (Cartesian product) of values:

```
--foreach var language in Python JavaScript Go --foreach var topic in "functions" "loops"
```

This will run 6 commands with all combinations of languages and topics.

## Parallelization

Combine `--foreach` with the `--threads` option to run commands in parallel:

```
--threads 4 --foreach var topic in "sorting" "data structures" "algorithms"
```

## Examples

### Example 1: Simple greeting to multiple people

```
cycod --foreach var name in Alice Bob Charlie --input "Hello, {name}!"
```

This will run three separate commands, with the variable `{name}` replaced with each value.

### Example 2: Multiple language-topic combinations

```
cycod --foreach var language in Python JavaScript Go --foreach var topic in "functions" "loops" --input "Show me how to use {topic} in {language}"
```

This will create all combinations of the language and topic variables, running six commands total.

### Example 3: Using numeric ranges

```
cycod --foreach var day in 1..7 --input "What day of the week is day {day}?"
```

This will run seven commands, with the day variable taking values from 1 to 7.

### Example 4: Parallel processing with threads

```
cycod --threads 4 --foreach var topic in "sorting algorithms" "data structures" "design patterns" "algorithms" --question "Explain {topic} concisely"
```

This will run four commands in parallel (up to 4 at once) using multiple threads.

### Example 5: Reading values from a file

```
cycod --foreach var city in @cities.txt --input "What's the weather like in {city}?"
```

This will process each city name from the file `cities.txt`, where each line contains one city name.

## See Also

- [--var](var.md)
- [--vars](vars.md)
- [--threads](threads.md)
- [--use-templates](use-templates.md)