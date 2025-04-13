---
hide:
- toc
icon: material/ray-vertex
---

--8<-- "snippets/ai-generated.md"

# Parallel Processing

ChatX supports parallel processing with the `--foreach` option, which allows you to run the same command multiple times with different variable values in parallel.

??? question "Why use parallel processing?"

    Parallel processing is useful when you need to perform the same operation on multiple items, such as:
    
    - Analyzing multiple documents or code files
    - Generating variations of content
    - Processing a batch of similar requests
    
    Instead of running commands sequentially, parallel processing saves time by executing multiple tasks simultaneously.

## Basic Usage

``` { .bash .cli-command title="Process multiple items in parallel" }
chatx --foreach var FILE in file1.txt file2.txt file3.txt --input "Summarize the content of {FILE}"
```

``` { .bash .cli-command title="Generate multiple variations with numeric range" }
chatx --foreach var NUM in 1..5 --input "Generate version {NUM} of a product description for a smart watch"
```

## Syntax

The `--foreach` option uses the following syntax:

```bash
--foreach var NAME in VALUE1 VALUE2 VALUE3 ...
```

For numeric sequences, you can use the range notation:

```bash
--foreach var NUMBER in START..END
```

## Controlling Thread Count

By default, ChatX determines an appropriate number of parallel threads. You can control this with the `--threads` option:

``` { .bash .cli-command title="Limit to 4 parallel processes" }
chatx --threads 4 --foreach var NAME in VALUE1 VALUE2 VALUE3 ... [command options]
```

## Examples

``` { .bash .cli-command title="Process multiple files with the same prompt" }
chatx --foreach var LANG in Python JavaScript TypeScript --input "Write a function to reverse a string in {LANG}"
```

``` { .bash .cli-command title="Generate multiple variations for content" }
chatx --foreach var TONE in professional casual humorous technical --input "Write a product description for a coffee maker in a {TONE} tone"
```

``` { .bash .cli-command title="Combine with file inputs" }
# Create some files first
echo "What's the weather like in Paris?" > city1.txt
echo "What's the weather like in London?" > city2.txt
echo "What's the weather like in Tokyo?" > city3.txt

# Process them in parallel
chatx --foreach var CITY in city1.txt city2.txt city3.txt --input @{CITY}
```

``` { .bash .cli-command title="Use with other options" }
chatx --system-prompt "You are a technical writer" --foreach var NUM in 1..3 --input "Write paragraph {NUM} of a user guide for a smart thermostat"
```

## Combining with Chat History

You can combine parallel processing with chat history to branch conversations:

``` { .bash .cli-command title="Create a base conversation" }
chatx --input "Let's discuss smartphone features" --output-chat-history smartphone-base.jsonl
```

``` { .bash .cli-command title="Branch into parallel conversations" }
chatx --input-chat-history smartphone-base.jsonl --foreach var FEATURE in camera battery display processor --input "Tell me about the {FEATURE} technology in modern smartphones"
```