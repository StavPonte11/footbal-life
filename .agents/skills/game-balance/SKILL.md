---
name: game-balance
description: Game balance, economy tuning, attribute curves, progression curves, and difficulty balance for Football Life.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Game Balance Skill

## Goal

Ensure the simulation produces believable and varied football careers.

## Never balance from one career

One career is not evidence.

Use large samples.

Example:

```bash
career-simulator --careers 10000 --seasons 20
```

## Measure

Track distributions such as:

- peak overall
- career length
- retirement age
- transfers
- injuries
- goals
- assists
- salary
- club progression
- national team appearances
- trophies
- manager trust

## Desired behavior

The simulation should create multiple viable career outcomes.

Examples:

```text
early superstar
solid professional
late bloomer
journeyman
lower-league career
injury-disrupted career
one-club career
international star
```

## Avoid runaway systems

Watch for:

```text
positive feedback loops
```

Example:

```text
good performance
→ confidence
→ better performance
→ more playing time
→ more development
→ even better performance
```

Some feedback is good.

Unbounded feedback is not.

## Player agency

Player choices should matter.

But the player should not always receive the optimal outcome.

## Balance changes

When changing important mechanics:

1. record baseline
2. make change
3. simulate large sample
4. compare distributions
5. inspect major deviations