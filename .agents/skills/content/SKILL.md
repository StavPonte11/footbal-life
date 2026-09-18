---
name: content
description: Game content design, events, life situations, dilemmas, relationships, items, and narrative content.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Game Content Skill

## Philosophy

Game content should be data-driven.

The code should define how content behaves.

Data should define what content exists.

## Life events

Events should contain:

- id
- conditions
- weight
- cooldown
- choices
- effects
- narrative

Example:

```text
teammate_invites_you_out
```

should not require a new C# class.

## Conditions

Prefer composable conditions.

Examples:

```text
age >= 18
relationship.teammates > 50
fatigue < 60
is_starting_player == true
```

## Effects

Effects should be explicit.

Examples:

```text
confidence +5
fatigue +10
relationship +4
money -50
```

## Content quality

Events should:

- create meaningful choices
- interact with existing systems
- avoid repetitive outcomes
- have contextual conditions

Avoid hundreds of meaningless events.

## Narrative

Narrative should reflect actual game state.

Never write narrative that contradicts simulation state.