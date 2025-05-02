---
hide:
- toc
icon: material/variable
---

--8<-- "snippets/ai-generated.md"

# Variable Substitution

CycoD supports variable substitution in your inputs, prompts, and commands.

## Using Variables

``` { .bash .cli-command title="Define and use a simple variable" }
cycod --var name=Alice --input "Hello, {name}!"
```

``` { .plaintext .cli-output }
User: Hello, Alice!
```

``` { .bash .cli-command title="Define multiple variables at once" }
cycod --var name=Alice --var job="software developer" --input "Hello, I'm {name} and I work as a {job}."
```

``` { .plaintext .cli-output }
User: Hello, I'm Alice and I work as a software developer.
```

## Multiple Variables with --vars

You can also define multiple variables at once using the `--vars` option.

``` { .bash .cli-command title="Define multiple variables with --vars" }
cycod --vars name=Bob age=30 city="New York" --input "Hi, I'm {name}, {age} years old, from {city}."
```

``` { .plaintext .cli-output }
User: Hi, I'm Bob, 30 years old, from New York.
```

## Variable Sources

Variables can come from multiple sources:

1. Command-line definitions (shown above)
2. Configuration settings
3. Environment variables
4. Special built-in variables

``` { .bash .cli-command title="Use built-in time/date variables" }
cycod --input "Today is {date} and the time is {time}."
```

``` { .plaintext .cli-output }
User: Today is 2025-04-12 and the time is 08:51:46.
```

## Special Variables

CycoD provides several built-in special variables:

``` { .bash .cli-command title="Use date/time variables" }
cycod --input "Year: {year}, Month: {month}, Day: {day}"
```

``` { .plaintext .cli-output }
User: Year: 2025, Month: 4, Day: 12
```

??? info "Available special variables"

    **Date and Time**:
    - `{date}` - Current date (yyyy-MM-dd)
    - `{time}` - Current time (HH:mm:ss)
    - `{datetime}` - Current date and time (yyyy-MM-dd HH:mm:ss)
    - `{year}`, `{month}`, `{day}`, `{hour}`, `{minute}`, `{second}` - Individual time components
    
    **OS Information**:
    - `{os}` - Full OS information
    - `{osname}` - OS platform name
    - `{osversion}` - OS version
    
    **Other**:
    - `{random}` - Random number between 0 and 999

## Using Variables with --foreach

You can iterate over multiple values using the `--foreach` option.

``` { .bash .cli-command title="Iterate over multiple values" }
cycod --input "Hello, {name}!" --foreach var name in Alice Bob Charlie
```

This will run three separate commands with different values for the `{name}` variable:

``` { .plaintext .cli-output }
User: Hello, Alice!

User: Hello, Bob!

User: Hello, Charlie!
```

## Using Variables in System Prompts

Variables are especially useful in system prompts. See [System Prompts](system-prompts.md) for more examples.

``` { .bash .cli-command title="Use variables in system prompts" }
cycod --system-prompt "You are an expert in {topic}." --var topic="artificial intelligence" --input "What are the latest developments?"
```

## Variables in File Names

You can also use variables in file names for chat history and trajectories.

``` { .bash .cli-command title="Use variables in file names" }
cycod --output-chat-history "chat-{topic}-{date}.jsonl" --var topic="AI" --input "Tell me about machine learning"
```

## Advanced: Range Variables

You can create numeric range variables using the `..` syntax:

``` { .bash .cli-command title="Create a range of values" }
cycod --input "Number: {num}" --foreach var num in 1..5
```

This will run five separate commands with values 1, 2, 3, 4, and 5.