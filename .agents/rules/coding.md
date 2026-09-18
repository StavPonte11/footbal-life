---
trigger: always_on
---

# C# Coding Standards

## 1. Naming Conventions
- `PascalCase` for classes, structs, enums, records, public methods, and public properties (`MatchController`, `GoalScoredEvent`).
- `camelCase` with leading underscore for private/protected fields: `_playerSpeed`, `_ballRigidbody`.
- `camelCase` for method parameters and local variables: `kickPower`, `targetPosition`.
- Serialized private fields:
  ```csharp
  [SerializeField] private float _movementSpeed = 6.5f;
  ```

## 2. Nullable Reference Types & Safety
- Enable nullable reference types (`<Nullable>enable</Nullable>`).
- Check references explicitly before dereferencing: `if (_animator != null) ...` or null-conditional `_onScoreChanged?.Invoke()`.

## 3. Asynchronous Programming
- Prefer `UniTask` or `Task` with proper `CancellationToken` support tied to `destroyCancellationToken` or `GetCancellationTokenOnDestroy()`.
- Avoid `async void` except for top-level UI event handlers.

## 4. Assembly Definitions (Asmdef)
- Partition codebase into distinct assemblies to optimize compilation time and enforce dependency direction:
  - `FootballLife.Domain` (Pure C#, zero dependencies)
  - `FootballLife.Simulation` (Pure C#, references Domain)
  - `FootballLife.Simulation.Tests` (xUnit/NUnit tests)
  - `FootballLife.CareerSimulator` (CLI tool)
  - `FootballLife.Unity.Core` (MonoBehaviours, camera rigs, input, audio)
  - `FootballLife.Unity.UI` (UI Toolkit / App UI views and viewmodels)
  - `FootballLife.Unity.Editor` (custom inspectors, import postprocessors, MCP tools)
