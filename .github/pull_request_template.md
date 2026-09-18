## Summary
<!-- Concise explanation of changes and their architectural intent. -->
Closes #<!-- Issue Number -->

## Architecture Compliance
- [ ] **Layer separation maintained**: Pure C# (no `UnityEngine` in Domain/Simulation).
- [ ] **Determinism guaranteed**: Uses `SimulationRandom(seed)` instead of global randomness.
- [ ] **Zero GC in hot paths**: No allocations (`new`, LINQ, string formatting) in tick / update loops.
- [ ] **Decoupled design**: No god objects, single-responsibility systems.
- [ ] **Data-driven**: No hard-coded logic tied to magic strings.

## Changes Implemented
- 

## Verification & Automated Test Results
<!-- Paste dotnet test or headless simulation run output -->
```text
dotnet test results
```

## Reviewer Notes
<!-- Highlighting edge cases, tradeoffs, or specific lines requiring scrutiny. -->
