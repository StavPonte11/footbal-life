---
name: game-testing
description: Unit testing, simulation tests, balance validation, and automated test runners for Football Life.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Testing Skill

## Test levels

Use:

### Unit tests

For individual systems.

### Integration tests

For interactions between systems.

### Simulation tests

For complete career/season behavior.

### Unity tests

For presentation/integration behavior.

## Determinism

Given:

```text
same state
same inputs
same seed
```

the simulation should produce:

```text
same result
```

where deterministic behavior is expected.

## Regression testing

When fixing a bug:

1. reproduce
2. write a regression test
3. fix
4. run full relevant suite

## Simulation invariants

Test important invariants.

Examples:

```text
age never decreases
money does not change without a transaction
player cannot play while unavailable due to injury
contract cannot end before its start
attribute stays within valid range
```

## Do not over-test implementation details

Test behavior and outcomes.

Avoid tests that break merely because internal implementation changes.