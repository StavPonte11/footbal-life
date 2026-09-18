---
name: simulation
description: Core simulation engine development, domain models, career progression, world simulation, and deterministic logic.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Simulation Development Skill

Use this skill whenever modifying the Football Life simulation.

## Goal

Build a deterministic, modular football-life simulation that can run independently of Unity.

## Core rules

Simulation code must not reference Unity APIs.

Avoid:

- MonoBehaviour
- GameObject
- Transform
- Animator
- UnityEngine

Simulation should be ordinary C#.

## Systems

Prefer explicit systems:

```text
TrainingSystem
MatchSimulation
CareerSystem
TransferSystem
RelationshipSystem
EconomySystem
LifeEventSystem
WorldSimulation
```

A system should have a clear responsibility.

## State changes

Prefer explicit operations:

```csharp
trainingSystem.Train(player, session);
```

over arbitrary mutations from UI code.

## Randomness

Use explicit seeded randomness.

Never rely on uncontrolled global random state.

## Data

Separate:

- static configuration
- runtime state

Static configuration can come from JSON or equivalent data sources.

Runtime career state should be serializable and persistable.

## Testing

Every new system should have tests covering:

- normal behavior
- edge cases
- invalid inputs
- deterministic behavior
- interactions with existing systems

## Simulation quality

When implementing mechanics, ask:

- Does this produce believable outcomes?
- Can this create interesting career trajectories?
- Does this interact with existing systems?
- Can it create unintended runaway effects?

Avoid systems that always reward the player.

Failure and setbacks are valid outcomes.

## Multi-career validation

When practical, run many simulated careers and inspect distributions.

Look for:

- runaway progression
- impossible attribute growth
- excessive injuries
- excessive transfers
- unrealistic salaries
- unrealistic retirement ages
- players becoming stars too easily
- players becoming permanently stuck

The goal is believable distributions, not identical careers.