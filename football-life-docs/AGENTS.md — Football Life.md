# Football Life — Agent Instructions

## 1. Project

Football Life is a mobile-first football career and life simulation game inspired by New Star Soccer, expanded with:

- modern 3D football gameplay
- player position selection
- career progression
- training
- transfers
- contracts
- money
- relationships
- personal life
- home/lifestyle
- reputation
- media
- social interactions
- long-term world simulation

Core fantasy:

> You don't manage a football club. You live the life of a footballer.

The game should create emergent stories rather than following a predefined storyline.

---

# 2. Primary Development Principle

Build the game as two strongly separated layers:

```text
SIMULATION
    ↓
GAME STATE
    ↓
UNITY PRESENTATION
```

The simulation is the source of truth.

Unity presents and interacts with the simulation.

Never reverse this dependency.

---

# 3. Architecture

The project consists of:

### Domain

Pure C# domain models.

Examples:

- Player
- Club
- League
- Competition
- Contract
- Career
- Relationship
- Match
- Event
- Item
- Property

The domain must not depend on Unity.

### Simulation

Pure C# systems that modify domain state.

Examples:

- TrainingSystem
- CareerSystem
- MatchSimulation
- TransferSystem
- RelationshipSystem
- EconomySystem
- LifeEventSystem
- WorldSimulation

The simulation must not depend on Unity.

### Unity

Unity is the presentation/runtime layer.

Unity owns:

- GameObjects
- Scenes
- Prefabs
- Animators
- Cameras
- UI
- input
- audio
- VFX
- rendering
- real-time physics

Unity should translate between player input / simulation state and presentation.

---

# 4. Dependency Rule

The dependency direction must always be:

```text
Domain
   ↑
Simulation
   ↑
Unity
```

Never:

```text
Domain → Unity
Simulation → Unity
```

The simulation must be executable without launching Unity.

---

# 5. Determinism

Simulation systems should be deterministic when given:

- the same initial state
- the same inputs
- the same random seed

Do not use uncontrolled global randomness.

Prefer explicit RNG/context objects.

Example:

```csharp
var rng = new SimulationRandom(seed);
```

This allows:

- debugging
- reproducibility
- automated testing
- career replay
- balance testing

---

# 6. Simulation Before Presentation

When implementing a new mechanic:

1. Define the domain model.
2. Define the simulation behavior.
3. Write tests.
4. Validate the simulation.
5. Only then implement Unity presentation.

Do not start by building UI for a mechanic whose underlying behavior does not exist.

---

# 7. No God Objects

Do not create giant classes such as:

```text
GameManager
UltimateGameManager
CareerManager
PlayerManager
WorldManager
```

with hundreds or thousands of lines.

Prefer small focused systems.

For example:

```text
TrainingSystem
CareerSystem
ContractSystem
TransferSystem
RelationshipSystem
```

---

# 8. No Hard-Coded Game Content

Game content should be data-driven whenever practical.

Avoid:

```csharp
if (club.Name == "Arsenal")
{
    ...
}
```

Prefer data/configuration.

Events, clubs, players, items and similar content should be representable as data.

---

# 9. Player Model

Separate:

### Abilities

Long-term player capability.

Examples:

- finishing
- passing
- dribbling
- pace
- stamina
- vision

from:

### State

Current temporary condition.

Examples:

- confidence
- fatigue
- happiness
- motivation
- form
- morale

Do not combine these into a single generic rating system.

---

# 10. Football Philosophy

The player controls one footballer.

The player does NOT directly control the entire team.

The simulation controls:

- teammates
- opponents
- tactics
- positioning
- ball movement
- substitutions
- manager decisions

The player's interaction occurs during relevant football situations.

Different positions must produce different gameplay opportunities.

---

# 11. Personal Life Philosophy

Personal life is not a cosmetic side feature.

It must interact with football.

Examples:

- sleep affects recovery
- social activities affect happiness/fatigue
- relationships affect wellbeing
- moving clubs can affect relationships
- fame affects social life
- salary affects lifestyle
- poor performances affect confidence
- manager relationships affect playing time

The systems should interact through explicit state changes.

---

# 12. Emergent Stories

Prefer systems that create stories naturally.

Avoid hard-coded career narratives.

Example:

```text
Poor performance
→ manager confidence decreases
→ reduced playing time
→ frustration
→ transfer request
→ new club
→ new country
→ relationship changes
→ new career trajectory
```

The simulation generates the story.

The UI presents it.

---

# 13. LLM Rule

LLMs may eventually generate narrative presentation.

They must NOT be the authoritative source of game mechanics.

Allowed:

```text
Simulation event
    ↓
LLM
    ↓
News article
```

Not allowed:

```text
LLM
    ↓
decides player transferred
```

Game state must remain deterministic and authoritative.

---

# 14. Testing

Every simulation system requires automated tests.

When adding a mechanic:

- add unit tests
- run the complete test suite
- run deterministic tests
- run multi-career simulation where appropriate

The career simulator should eventually support:

```bash
career-simulator --careers 10000 --seasons 20
```

and report distributions useful for balance analysis.

---

# 15. Performance

Do not optimize prematurely.

First write clear code.

Profile before making architectural performance changes.

Do not introduce ECS/DOTS merely because the game contains many entities.

Use standard C# first.

Move to data-oriented architecture only when profiling demonstrates a real need.

---

# 16. Mobile First

The primary target is:

- iOS
- Android

PC is secondary.

Design for:

- touch
- short sessions
- fast loading
- automatic saves
- scalable graphics
- battery-conscious behavior

Do not assume keyboard/mouse/controller as the primary input.

---

# 17. Development Workflow

For every task:

### Step 1 — Understand

Read:

- AGENTS.md
- relevant architecture docs
- relevant skills
- existing implementation

### Step 2 — Inspect

Understand the current code before modifying it.

Do not recreate existing systems.

### Step 3 — Plan

For non-trivial tasks, describe:

- files to change
- systems affected
- dependencies
- tests required
- potential architectural risks

### Step 4 — Implement

Make the smallest coherent change.

### Step 5 — Test

Run relevant tests.

### Step 6 — Validate

For simulation changes, run simulations where useful.

For Unity changes, verify:

- compilation
- console errors
- relevant scene
- relevant gameplay

### Step 7 — Report

Explain:

- what changed
- why
- tests executed
- known limitations
- recommended next step

---

# 18. Scope Control

Do not implement future features unless explicitly requested.

The project contains many future systems.

Current priority always comes from ROADMAP.md.

Do not expand a task from:

> "Implement training"

into:

> "Implement training + injuries + contracts + transfer market + UI + AI."

Keep changes focused.

---

# 19. When Requirements Are Ambiguous

Do not immediately ask unnecessary questions.

Use reasonable assumptions when they do not affect architecture.

If an ambiguity materially affects architecture or gameplay design:

1. state the assumption
2. explain the consequence
3. ask for clarification if necessary

Never silently make a major architectural decision.

---

# 20. Definition of Done

A feature is not complete merely because the code compiles.

A feature is complete when:

- architecture is respected
- tests exist
- existing tests pass
- simulation behavior is coherent
- content is data-driven where appropriate
- Unity presentation works when applicable
- no obvious regressions exist

---

# 21. Current Product Priority

Always optimize for:

```text
FUN
↓
COHERENT SIMULATION
↓
GOOD UX
↓
VISUAL POLISH
↓
SCALE
```

Do not optimize for feature count.

The primary product question is:

> Is it fun to live through a footballer's career?