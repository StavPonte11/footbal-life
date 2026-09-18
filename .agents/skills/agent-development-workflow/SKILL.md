---
name: agent-development-workflow
description: Agent development workflow, task decomposition, change validation, and architecture preservation.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# Agent Development Workflow

## Before coding

Read:

1. AGENTS.md
2. relevant architecture documentation
3. relevant skill
4. current implementation

Inspect existing code before creating new files.

## For small tasks

Implement directly if the architecture is obvious.

## For medium/large tasks

First produce:

```text
Goal
Current architecture
Proposed changes
Files affected
Tests
Risks
```

Then implement.

## Never

- rewrite unrelated code
- create duplicate systems
- silently change architecture
- remove tests to make them pass
- disable warnings/errors instead of fixing them
- introduce dependencies without justification
- implement unrelated future features

## After implementation

Run:

1. formatter/static analysis if configured
2. unit tests
3. integration tests where relevant
4. simulation validation where relevant
5. Unity compilation/tests where relevant

## Failure handling

If tests fail:

1. inspect failure
2. determine root cause
3. fix implementation
4. rerun tests

Do not modify tests simply to make failures disappear.

## Git

Keep changes logically grouped.

Prefer small commits with meaningful messages.

## Communication

Final report:

```text
Implemented:
...

Tests:
...

Validation:
...

Known limitations:
...

Next recommended step:
...
```

Do not claim validation that was not actually performed.