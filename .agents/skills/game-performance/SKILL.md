---
name: game-performance
description: Game performance, zero GC allocations, memory budgeting, mobile frame rates, and profiling for Football Life.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Performance Skill

## Principle

Make it correct first.

Profile before optimizing.

## Mobile priorities

Watch:

- frame time
- memory
- battery
- load time
- asset size
- draw calls
- animation cost

## Simulation

The simulation should be efficient enough to run many careers for balance testing.

The career simulator should eventually be able to run thousands of careers without Unity.

## Unity

Avoid unnecessary per-frame work.

Prefer event-driven updates where appropriate.

Cache expensive references.

Avoid unnecessary allocations in hot loops.

## Graphics

Use scalable quality levels.

At minimum:

```text
Low
Medium
High
```

Do not assume every device can render the same scene at the same quality.

## ECS/DOTS

Do not introduce ECS/DOTS without evidence.

Use standard C# and Unity architecture first.

Introduce data-oriented systems only when profiling demonstrates a need.