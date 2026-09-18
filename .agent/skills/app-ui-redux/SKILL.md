---
name: app-ui-redux
description: Use when managing complex game state with Redux unidirectional data flow, slices, reducers, actions, and async thunks in Unity App UI.
allowed-tools:
  - Bash
  - Read
  - Write
  - Edit
---

# App UI Redux State Management

State management guide using Redux patterns in Unity for predictable, replayable game state and UI synchronization.

## When to Use Redux in Games
- Complex multi-screen UI with shared state (e.g. inventory, squad builder, career mode progression, transfer market).
- Undo/redo requirements or match replay states.
- High-frequency event dispatching needing decoupled subscribers.

## Key Building Blocks

1. **State**: Immutable C# record or struct holding snapshot data.
2. **Actions**: Objects describing what happened (`SetSquadTacticAction`, `GoalScoredAction`).
3. **Reducers**: Pure functions computing the next state: `(state, action) => newState`.
4. **Store**: Central coordinator holding state, accepting dispatches, and notifying UI subscribers.
5. **AsyncThunks**: Async workflows (e.g. cloud save, AI simulation, matchmaking) with pending/fulfilled/rejected action dispatches.

## Redux Slice Example

```csharp
using Unity.AppUI.Redux;

public record CareerState
{
    public int SeasonYear { get; init; } = 2026;
    public int Budget { get; init; } = 5000000;
    public int TeamReputation { get; init; } = 75;
}

public record ModifyBudgetAction(int Amount);

public static class CareerReducers
{
    public static CareerState Reduce(CareerState state, object action) => action switch
    {
        ModifyBudgetAction a => state with { Budget = state.Budget + a.Amount },
        _ => state
    };
}
```
