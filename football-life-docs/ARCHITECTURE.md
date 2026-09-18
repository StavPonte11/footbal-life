# Football Life — Architecture

## 1. Architectural Goals

The architecture must support:

- deterministic simulation
- independent testing
- large-scale career simulation
- Unity presentation
- mobile deployment
- data-driven content
- future expansion
- AI-assisted development
- maintainability

The central architectural principle is:

> The simulation is the source of truth. Unity presents it.

---

## 2. High-Level Architecture

```text
                    UNITY APPLICATION
                           │
                    Presentation Layer
                           │
                    Integration Layer
                           │
                           ▼
                 ┌────────────────────┐
                 │     SIMULATION     │
                 │                    │
                 │ Career             │
                 │ Football           │
                 │ Training           │
                 │ Transfers          │
                 │ Relationships      │
                 │ Economy            │
                 │ Life Events        │
                 │ World              │
                 └─────────┬──────────┘
                           │
                           ▼
                 ┌────────────────────┐
                 │       DOMAIN       │
                 │                    │
                 │ Player             │
                 │ Club               │
                 │ Match              │
                 │ Contract           │
                 │ Relationship       │
                 │ Career             │
                 └────────────────────┘
```

Dependency direction:

```text
Unity
  ↓
Integration
  ↓
Simulation
  ↓
Domain
```

Never reverse this dependency.

---

## 3. Repository Structure

Recommended structure:

```text
football-life/
│
├── AGENTS.md
│
├── docs/
│   ├── GAME_DESIGN.md
│   ├── ARCHITECTURE.md
│   ├── SIMULATION_DESIGN.md
│   ├── MATCH_ENGINE.md
│   ├── UI_UX.md
│   └── ROADMAP.md
│
├── simulation/
│   ├── FootballLife.Domain/
│   ├── FootballLife.Simulation/
│   ├── FootballLife.Simulation.Tests/
│   └── FootballLife.CareerSimulator/
│
├── unity/
│   └── FootballLife/
│
├── content/
│   ├── players/
│   ├── clubs/
│   ├── competitions/
│   ├── events/
│   ├── characters/
│   ├── environments/
│   ├── animations/
│   └── audio/
│
└── tools/
```

---

## 4. Domain Layer

The domain contains game concepts and state.

Examples:

```text
Player
PlayerAttributes
PlayerState
Career
Club
League
Competition
Match
Contract
Relationship
FinanceAccount
LifeEvent
Season
WorldState
```

The domain must:

- be pure C#
- have no Unity dependency
- have no rendering knowledge
- be serializable where appropriate
- contain meaningful invariants

---

## 5. Simulation Layer

The simulation contains systems that operate on domain state.

Examples:

```text
TrainingSystem
MatchSimulation
CareerSystem
TransferSystem
ContractSystem
RelationshipSystem
EconomySystem
LifeEventSystem
WorldSimulation
ProgressionSystem
```

Each system should have a narrow responsibility.

Avoid giant manager classes.

---

## 6. Unity Layer

Unity owns presentation and real-time interaction.

Unity is responsible for:

- scenes
- GameObjects
- prefabs
- cameras
- animations
- rendering
- input
- UI
- audio
- VFX
- real-time physics

Unity should not become the authoritative career database.

---

## 7. Integration Layer

The integration layer translates between simulation concepts and Unity.

Examples:

```text
PlayerView
MatchPresentationController
SimulationEventPresenter
InputCommandAdapter
CareerStatePresenter
```

Example:

```text
Simulation
    ↓
MatchEvent
    ↓
Integration
    ↓
Unity
    ↓
Animation / Camera / UI
```

---

## 8. Commands and Events

Prefer explicit commands for player actions.

Examples:

```text
TrainCommand
RestCommand
AcceptTransferCommand
DeclineTransferCommand
InteractCommand
MatchActionCommand
```

Prefer events for simulation outcomes.

Examples:

```text
GoalScoredEvent
TrainingCompletedEvent
TransferOfferReceivedEvent
RelationshipChangedEvent
SeasonCompletedEvent
```

Commands represent intent.

Events represent things that happened.

---

## 9. Deterministic Randomness

Randomness must be controllable.

Use an explicit random source:

```csharp
SimulationRandom
```

Systems should receive or access randomness through simulation context rather than global random calls.

The goal is:

```text
same initial state
+
same commands
+
same seed
=
same outcome
```

---

## 10. State Ownership

Every important piece of state should have a clear owner.

Example:

```text
Player attributes
→ ProgressionSystem

Player fatigue
→ Recovery / Training / Match systems

Manager trust
→ Career / Match / Training systems

Money
→ EconomySystem

Relationship strength
→ RelationshipSystem
```

UI should not directly own authoritative state.

---

## 11. Data-Driven Design

Separate:

### Static data

Examples:

```text
players.json
clubs.json
leagues.json
positions.json
events.json
items.json
```

### Runtime state

Examples:

```text
career save
player state
relationships
contracts
finances
world state
season state
```

Static data describes what exists.

Runtime state describes what happened.

---

## 12. Save System

The save system should persist the minimum authoritative state required to reconstruct the career.

It should support:

- manual save
- automatic save
- versioning
- migration
- corruption detection where practical

Never rely on Unity scene state as the career save.

---

## 13. Serialization

Serialized domain state should use stable identifiers.

Avoid using Unity instance IDs as persistent identifiers.

Examples:

```text
player_id
club_id
relationship_id
event_id
season_id
```

---

## 14. Content Pipeline

Content should flow approximately as:

```text
Authoring Data
    ↓
Validation
    ↓
Build / Import
    ↓
Runtime Content
```

Content validation should detect:

- duplicate IDs
- invalid references
- impossible values
- missing required fields

---

## 15. Testing Architecture

### Domain tests

Validate invariants.

### Simulation unit tests

Validate systems.

### Integration tests

Validate system interactions.

### Career simulations

Run many careers to detect balance problems.

### Unity tests

Validate presentation/integration.

---

## 16. Career Simulator

The simulation must be runnable outside Unity.

Target usage:

```bash
career-simulator --careers 10000 --seasons 20
```

This tool should eventually report:

- career length
- peak overall
- transfer count
- goals
- assists
- injuries
- salary
- market value
- trophies
- retirement age

This tool is essential for balancing.

---

## 17. Unity Scene Architecture

Avoid one enormous scene.

Potential scenes:

```text
Bootstrap
MainMenu
CareerHub
Training
Match
Home
Career
Life
```

The exact structure can evolve.

Use reusable prefabs and components.

---

## 18. UI Architecture

UI should consume presentation models rather than directly manipulating simulation state.

Example:

```text
Simulation State
      ↓
View Model / Presenter
      ↓
UI
      ↓
User Command
      ↓
Simulation
```

---

## 19. Match Architecture

The match consists of:

```text
Match Simulation
       ↓
Match State
       ↓
Situation / Event
       ↓
Player Input
       ↓
Outcome
       ↓
Presentation
```

Unity should not independently calculate the authoritative match outcome.

---

## 20. AI Architecture

The game's non-player footballers should be simulated by deterministic systems.

Potential layers:

```text
World AI
Team AI
Tactical AI
Player AI
Situation Resolution
```

Do not build sophisticated AI before the basic match loop works.

---

## 21. Performance Architecture

Initial implementation should use conventional C# and Unity systems.

Do not introduce ECS/DOTS solely for theoretical scalability.

Measure first.

Optimize when profiling identifies a bottleneck.

---

## 22. Mobile Architecture

Primary targets:

- Android
- iOS

The architecture must support:

- scalable rendering
- asynchronous loading
- efficient asset management
- automatic saving
- touch input
- suspend/resume
- varying hardware capabilities

---

## 23. External Services

The first playable prototype should not require a backend.

Prefer local/offline operation.

Potential future services:

- cloud saves
- analytics
- leaderboards
- remote content
- AI narrative

These should be optional extensions rather than core dependencies.

---

## 24. AI Coding Agent Constraints

Agents must:

- inspect existing architecture before changes
- reuse existing abstractions
- avoid duplicate systems
- write tests for simulation changes
- avoid Unity dependencies in simulation
- avoid giant manager classes
- avoid unnecessary dependencies
- report validation performed

Agents must not silently change architectural principles.

---

## 25. Architectural Decision Rule

When a new feature does not clearly belong to one layer:

1. identify the state it changes
2. identify who owns that state
3. place authoritative behavior in simulation
4. expose the result to Unity
5. document significant architectural decisions

---

## 26. Core Invariants

The following should remain true:

- Simulation can run without Unity.
- Unity does not own authoritative career state.
- Simulation randomness can be seeded.
- Player actions are explicit commands.
- Simulation outcomes are explicit events.
- Static content is separated from runtime state.
- Major systems are independently testable.
- UI does not directly mutate domain state.
