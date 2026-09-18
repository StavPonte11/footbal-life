---
trigger: always_on
---

# Simulation Rules & Principles

## 1. Time Advancement
The world advances through explicit time steps:
```text
Day → Week → Month → Season → Career
```
- The player interacts at the day/week level.
- Simulation operates deterministically on discrete ticks.

## 2. Player State Separation
Strictly separate player attributes:
- **Abilities** (Long-term capability): finishing, passing, dribbling, pace, stamina, vision.
- **Potential**: Probabilistic ceiling, nonlinear development curve by age.
- **Current State** (Temporary condition): fatigue, confidence, form, happiness, motivation, morale.
- **Career State**: club, position, squad status, manager trust, reputation, wage, contract, market value.
- Never collapse these into a single generic rating.

## 3. Football & Personal Life Interaction
- Personal life directly impacts football:
  - Sleep / rest affects physical recovery and fatigue.
  - Social activities increase happiness but cost energy/fatigue.
  - Moving clubs affects relationships and lifestyle.
  - Salary unlocks lifestyle tiers and investments.
  - Manager trust directly determines starting vs bench vs reserve status.

## 4. Emergent Stories & LLM Authority
- Prefer systemic emergent stories over scripted linear plots.
- **LLM Boundary**: LLMs may generate flavour text, press conference commentary, or newspaper headlines from simulation snapshots, but must **never** decide game outcomes or mutate state directly.

## 5. Automated Validation & Headless Execution
- Every simulation system must have automated tests.
- Support running large-scale simulation tests headless:
  ```bash
  dotnet run --project simulation/FootballLife.CareerSimulator -- --careers 10000 --seasons 20
  ```
