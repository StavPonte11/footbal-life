---
trigger: always_on
---

# Architecture Rules

## 1. Two Strictly Separated Layers

```text
       ┌──────────────┐
       │    DOMAIN    │ (Pure C# models: Player, Club, Match, Contract, Item)
       └──────┬───────┘
              │ (Domain state)
       ┌──────▼───────┐
       │  SIMULATION  │ (Pure C# systems: Training, MatchSimulation, Transfers, LifeEvents)
       └──────┬───────┘
              │ (State snapshots & events)
       ┌──────▼───────┐
       │    UNITY     │ (Presentation: URP, UI Toolkit/App UI, 3D GameObjects, Physics)
       └──────────────┘
```

- **Domain**: Pure C# domain models (no `UnityEngine` dependencies).
- **Simulation**: Pure C# systems that modify domain state deterministically via `SimulationRandom(seed)`. Must be completely executable and testable without launching Unity.
- **Unity**: Presentation and interaction layer (GameObjects, UXML/USS, Input, Audio, VFX, Cameras). Translates user touch input into simulation actions and renders simulation state.
- **Dependency Rule**: `Domain` ↑ `Simulation` ↑ `Unity`. Never allow Domain or Simulation to depend on Unity.

## 2. Determinism
- Simulation systems must be deterministic given identical initial state, inputs, and `SimulationRandom(seed)`.
- No global uncontrolled randomness (never call `System.Random` or `UnityEngine.Random` inside Domain or Simulation).

## 3. Simulation Before Presentation
- When implementing a new mechanic:
  1. Define domain model.
  2. Define simulation behavior.
  3. Write tests.
  4. Validate simulation.
  5. Only then implement Unity presentation.
- Never build UI for a mechanic whose underlying simulation does not exist.

## 4. No God Objects
- Avoid monolithic managers (`GameManager`, `CareerManager`, `PlayerManager`).
- Build small, decoupled systems: `TrainingSystem`, `TransferSystem`, `ContractSystem`, `RelationshipSystem`.

## 5. Data-Driven Content
- All clubs, player attributes, items, dilemmas, and events must be data-driven.
- Avoid hardcoded logic based on string comparisons (e.g. `if (club.Name == "Arsenal")`).
