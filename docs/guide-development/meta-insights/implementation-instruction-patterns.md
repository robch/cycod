# Implementation Instruction Patterns

## Overview

Micro-insights from AI-human collaboration on complex task implementation, specifically patterns for giving AI instructions that involve timing, simulation, or multi-step coordination.

## Core Learning: AI Instruction Specificity for Complex Tasks

### The Pattern Discovered

**❌ Fails**: Asking AI to coordinate timing or simulate abstract concepts  
**✅ Works**: Providing explicit commands and letting AI execute them sequentially

### Examples from Test Development

#### Process Self-Restart Simulation

**❌ Failed Approach**:
```yaml
run: cycod --input "Use StartNamedProcess to start a process that simulates component restart during operation"
```
*Result: AI timeout trying to figure out what "simulates component restart" means*

**✅ Working Approach**:
```yaml
run: cycod --input "Use StartNamedProcess to start a process named 'restart-proc' that runs 'bash -c \"echo Starting && sleep 1 && echo Restarting && sleep 1 && echo Restarted\"'"
```
*Result: Reliable execution with clear, verifiable behavior*

#### Process Termination While Busy

**❌ Failed Approach**:
```yaml
run: cycod --input "Create a busy process. Wait a few seconds for it to get busy, then terminate it while it's actively working."
```
*Result: AI timeout trying to coordinate timing*

**✅ Working Approach**:
```yaml
run: cycod --input "Use StartNamedProcess to create a process named 'busy-proc' that runs 'bash -c \"for i in {1..100}; do echo Busy line $i; sleep 0.2; done\"'. Then use TerminateShellOrProcess to terminate the busy-proc process."
```
*Result: Successful execution*

## Meta-Patterns

### Concreteness Over Conceptual Coordination

**Principle**: Give AI concrete commands to execute rather than asking it to coordinate abstract timing or simulation.

**Why This Works**:
- AI excels at following explicit sequential instructions
- AI struggles with subjective timing decisions ("wait until busy")
- Explicit commands are repeatable and debuggable

### Command Clarity Hierarchy

1. **Most Effective**: Exact bash commands with explicit parameters
2. **Effective**: Specific function calls with named parameters  
3. **Problematic**: Abstract descriptions requiring interpretation
4. **Fails**: Coordination tasks requiring subjective timing

## Application Guidelines

### When Writing AI Instructions

**Do**:
- Specify exact commands and parameters
- Use concrete examples rather than abstract descriptions
- Break complex tasks into explicit sequential steps
- Provide specific names for processes/shells/files

**Don't**:
- Ask AI to "wait until [condition]" without explicit timing
- Use abstract terms like "simulate" without concrete implementation
- Rely on AI to coordinate timing between steps
- Assume AI will interpret domain-specific jargon consistently

### For Complex Scenarios

**Strategy**: Design the concrete implementation first, then ask AI to execute it.

**Example Process**:
1. **Design**: "I want to test process restart behavior"
2. **Implement**: "What bash command would show restart-like output?"
3. **Specify**: "echo Starting && sleep 1 && echo Restarting && sleep 1 && echo Restarted"  
4. **Instruct**: "Use StartNamedProcess with this exact command"

## Connection to Broader Collaboration Patterns

### Human-AI Division of Labor

**Human Strength**: Conceptual design, domain knowledge, reality-checking  
**AI Strength**: Systematic execution, explicit instruction following, pattern application

**Optimal Pattern**: Human designs concrete approach, AI executes systematically.

### Reality-Check Integration

This pattern connects to the reality-checking insights from rubric-driven development: test abstract concepts through concrete implementation before scaling up.

## Future Applications

### Beyond Test Development

This pattern likely applies to any domain where AI needs to coordinate complex, timing-dependent, or simulation-based tasks:

- **Deployment orchestration**: Specify exact commands vs. asking for "graceful deployment"
- **Performance testing**: Define explicit load patterns vs. asking for "realistic load"
- **Data processing**: Specify transformation steps vs. asking for "clean the data"

### Meta-Learning

**Insight**: The boundary between "AI can handle this" and "needs human specification" seems related to the concreteness and objectivity of the task requirements.

**Question**: Are there systematic ways to predict when instructions need to be more concrete?

## Evolution Notes

Discovered through practical test implementation failures. Connects to broader themes about AI capabilities and human-AI collaboration patterns. The meta-insight is about finding the optimal abstraction level for AI instructions in collaborative technical work.