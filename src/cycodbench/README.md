# CycodBench

CycodBench is a benchmark runner and evaluation tool for the cycod agent against SWE-bench problems.

## Overview

This tool provides a comprehensive workflow for:
- Downloading and managing SWE-bench problem datasets
- Solving software engineering problems with the cycod agent
- Evaluating and analyzing solutions
- Generating detailed reports on benchmark performance

## Command Structure

CycodBench follows a noun-verb command structure:

```
cycodbench <noun> <verb> [options]
```

### Main Commands

- `problems` - Problems download and management
  - `download` - Download a problem dataset
  - `list` - List available problems
  - `merge` - Merge multiple datasets
  - `shard` - Filter and split problems into shards
  - `solve` - Generate solutions for problems

- `container` - Container lifecycle management
  - `init` - Start/initialize a container
  - `copy` - Copy files to/from a container
  - `exec` - Execute a command in a container
  - `list` - List running containers
  - `stop` - Stop and remove a container

- `solutions` - Solution management
  - `list` - List solutions
  - `merge` - Merge multiple solutions files
  - `pick` - Select the best solution from candidates
  - `eval` - Evaluate solutions against problems

- `results` - Results management
  - `list` - List evaluation results
  - `merge` - Merge multiple results files
  - `report` - Generate a report from results

- `run` - Run the complete benchmark workflow

## Typical Workflow

1. Download a problem dataset:
   ```
   cycodbench problems download verified
   ```

2. List available problems:
   ```
   cycodbench problems list verified
   ```

3. Solve problems with the agent:
   ```
   cycodbench problems solve verified --parallel 4
   ```

4. Evaluate solutions:
   ```
   cycodbench solutions eval solutions.json
   ```

5. Generate a report:
   ```
   cycodbench results report results.json --output report.md
   ```

Alternatively, use the integrated workflow:
```
cycodbench run verified --parallel 4 --output results.json
```

## Building and Running

Build the application:
```
dotnet build src/cycodbench
```

Run the application:
```
dotnet run --project src/cycodbench [arguments]
```

## Documentation

For detailed documentation on each command and its options, run:
```
cycodbench help
```

Or for specific command help:
```
cycodbench help <command>
```