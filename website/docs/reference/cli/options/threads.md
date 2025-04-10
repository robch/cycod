---
title: --threads
description: Control parallel execution of multiple commands
---

# --threads

## Description

The `--threads` option allows you to specify the number of parallel threads to use when processing multiple commands. This is especially useful when combined with the [`--foreach`](foreach.md) option to run multiple variations of a command simultaneously.

## Syntax

```
--threads COUNT
```

## Parameters

- `COUNT`: The maximum number of parallel threads to use

## Default Value

By default, if not specified, ChatX will use a number of threads equal to the CPU core count on the current machine.

## Examples

### Example 1: Process 4 commands in parallel

```
chatx --threads 4 --foreach var topic in "sorting algorithms" "data structures" "design patterns" "algorithms" --question "Explain {topic} concisely"
```

This will run up to 4 commands simultaneously, each asking about a different topic.

### Example 2: Single-threaded execution

```
chatx --threads 1 --foreach var name in Alice Bob Charlie --input "Hello, {name}!"
```

Setting `--threads 1` ensures commands are processed one at a time in sequence.

## Notes

1. Setting too many threads might not always improve performance, as it depends on:
   - Your network bandwidth
   - API rate limits of the AI provider you're using
   - Your machine's resources

2. For deterministic order of outputs, use `--threads 1`

## See Also

- [--foreach](foreach.md)