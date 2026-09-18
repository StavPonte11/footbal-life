---
name: Feature Task
about: Specify a discrete simulation, domain, or presentation task
title: '[FEAT] '
labels: ['feature']
assignees: ''
---

## 1. Goal & Context
<!-- What capability is being added or improved? Reference roadmap milestone. -->

## 2. Architecture Layer
<!-- Domain (Pure C#) / Simulation (Pure C# Deterministic) / Unity (Presentation) -->
- [ ] Domain (`FootballLife.Domain`)
- [ ] Simulation (`FootballLife.Simulation`)
- [ ] Unity Presentation (`FootballLife.Unity`)

## 3. Specification & Constraints
<!-- Detail the data structures, methods, deterministic rules, and zero-allocation requirements. -->

## 4. Acceptance Criteria
- [ ] Deterministic behavior under identical `SimulationRandom(seed)`
- [ ] Zero GC allocations in hot paths / ticks
- [ ] No `UnityEngine` references in Domain or Simulation
- [ ] Automated xUnit / NUnit test coverage (>90% for domain logic)
- [ ] Headless execution verified

## 5. Verification Plan
<!-- How will this change be tested and verified? -->
- Tests: `dotnet test simulation/FootballLife.slnx`
- Multi-career simulation (if applicable): `dotnet run --project simulation/FootballLife.CareerSimulator`
